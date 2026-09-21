using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace UbisamBase.Launcher;

/// <summary>
/// 실행할 때 깃 저장소의 업데이트 파일과 지금 깔린 플랫폼(D:\UbisamPlatform\bin)을 비교해,
/// 새 버전이면 받아서 덮어쓴다. 순서는 이렇다.
///
/// <code>
/// 설정에서 켜져 있나? → 인터넷 연결됐나? → 저장소에 새 커밋이 있나? → 그 안의 업데이트 파일이
/// 지금 것보다 새 버전인가? → 자동 업데이트면 바로 받기 / 아니면 물어보고 "예"일 때만 받기
/// </code>
///
/// 어느 단계에서 막히든(설정 꺼짐 · 네트워크 없음 · git 없음 · 저장소 접근 실패) 조용히 넘어가고
/// 기존 파일로 그대로 실행한다 — 업데이트 때문에 프로그램이 안 켜지는 일은 없어야 한다.
///
/// 설정 파일 스키마는 Core의 PlatformUpdateSettings와 같다. Launcher가 Core를 참조하면 갱신 대상인
/// Core.dll을 먼저 물고 있게 되어 덮어쓸 수 없으므로, 여기서는 참조 없이 같은 파일을 직접 읽고 쓴다.
/// </summary>
internal static class PlatformUpdater
{
    private const string SettingsPath = @"D:\UbisamPlatform\update-settings.json";
    // 업데이트 확인은 조용히 지나가는 것이 기본이라, 왜 안 떴는지 볼 방법이 하나는 있어야 한다.
    // 실행할 때마다 덮어쓰므로 파일이 쌓이지 않는다.
    private const string LogPath = @"D:\UbisamPlatform\update-log.txt";
    private const string VersionFileName = "platform-version.json";
    private const int LsRemoteTimeoutMs = 10000;
    private const int CloneTimeoutMs = 180000;

    private static readonly StringBuilder LogBuffer = new StringBuilder();

    /// <summary>확인 과정을 한 줄씩 남긴다(D:\UbisamPlatform\update-log.txt). 토큰이나 응답 내용은 남기지 않는다.</summary>
    private static void Note(string message)
        => LogBuffer.AppendLine(DateTime.Now.ToString("HH:mm:ss") + "  " + message);

    private static void FlushLog()
    {
        try
        {
            // 메모장·PowerShell에서 한글이 깨지지 않도록 BOM을 붙여 쓴다.
            File.WriteAllText(LogPath, LogBuffer.ToString(), new UTF8Encoding(true));
        }
        catch
        {
            // 로그를 못 써도 실행에는 지장이 없다.
        }
    }

    /// <summary>플랫폼 배포 폴더를 최신으로 만든다. 실패해도 예외를 밖으로 내보내지 않는다.</summary>
    public static void TryUpdate(string platformDir)
    {
        try
        {
            Run(platformDir);
        }
        catch (Exception ex)
        {
            Note("예외 : " + ex.Message);
            MessageBox.Show(
                "플랫폼 업데이트를 확인하는 중 문제가 발생했습니다:\n" + ex.Message + "\n\n기존 버전으로 계속 실행합니다.",
                "UbisamBase 업데이트", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            FlushLog();
        }
    }

    private static void Run(string platformDir)
    {
        var settings = Settings.Load(SettingsPath);
        Note($"시작 — 사용:{settings.Enabled} 자동:{settings.AutoUpdate} 주소:{(string.IsNullOrWhiteSpace(settings.RepositoryUrl) ? "(없음)" : settings.RepositoryUrl)}");

        if (!settings.Enabled || string.IsNullOrWhiteSpace(settings.RepositoryUrl))
        {
            Note("설정이 꺼져 있거나 주소가 비어 있어 건너뜀");
            return;
        }

        // 1) 인터넷 연결 확인 — 연결이 없으면 깃을 부르지도 않는다(오프라인 현장 PC).
        if (!NetworkInterface.GetIsNetworkAvailable())
        {
            Note("인터넷 연결 없음 — 건너뜀");
            return;
        }

        if (!TryRunGit("--version", null, LsRemoteTimeoutMs, out _))
        {
            Note("git을 찾지 못함 — 건너뜀");
            return; // git이 깔려 있지 않은 PC — 업데이트 기능만 조용히 쉰다.
        }

        // 2) 업데이트 할 게 있는가 — 원격 브랜치의 최신 커밋이 마지막에 설치한 커밋과 같으면 끝.
        var branch = string.IsNullOrWhiteSpace(settings.Branch) ? "main" : settings.Branch.Trim();
        if (!TryRunGit($"ls-remote \"{settings.RepositoryUrl}\" \"refs/heads/{branch}\"", null, LsRemoteTimeoutMs, out var lsRemote))
        {
            Note("원격 확인 실패(주소·권한·네트워크) — 건너뜀");
            return; // 주소가 틀렸거나 접근 권한이 없다 — 실행을 막지 않는다.
        }

        var remoteCommit = FirstToken(lsRemote);
        Note($"원격 커밋:{Short(remoteCommit)} / 설치된 커밋:{Short(settings.InstalledCommit)}");

        if (string.IsNullOrEmpty(remoteCommit) || remoteCommit == settings.InstalledCommit)
        {
            Note("새 커밋 없음 — 끝");
            return;
        }

        var tempDir = Path.Combine(Path.GetTempPath(), "UbisamPlatformUpdate_" + Guid.NewGuid().ToString("N"));
        try
        {
            var distName = string.IsNullOrWhiteSpace(settings.DistPath) ? "dist" : settings.DistPath.Trim().Replace('\\', '/');

            // 업데이트 파일만 얕게 받는다 — 소스 전체 이력을 받지 않는다.
            if (!TryRunGit(
                    $"clone --depth 1 --branch \"{branch}\" --filter=blob:none --sparse \"{settings.RepositoryUrl}\" \"{tempDir}\"",
                    null, CloneTimeoutMs, out _))
            {
                Note("업데이트 파일을 내려받지 못함(clone 실패) — 건너뜀");
                return;
            }

            if (!TryRunGit($"sparse-checkout set \"{distName}\"", tempDir, CloneTimeoutMs, out _))
            {
                Note("업데이트 폴더를 꺼내지 못함(sparse-checkout 실패) — 건너뜀");
                return;
            }

            var remoteDist = Path.Combine(tempDir, distName.Replace('/', '\\'));
            if (!Directory.Exists(remoteDist))
            {
                Note($"저장소에 {distName} 폴더가 없음 — 커밋만 기록");
                // 저장소에 아직 업데이트 파일이 없다 — 이 커밋은 확인했다고 기록만 하고 넘어간다.
                settings.InstalledCommit = remoteCommit;
                settings.Save(SettingsPath);
                return;
            }

            var remoteVersion = ReadVersion(Path.Combine(remoteDist, VersionFileName));
            var localVersion = ReadVersion(Path.Combine(platformDir, VersionFileName));

            // 버전은 배포 시각("yyyy-MM-dd HH:mm:ss")이라 문자열 비교로 앞뒤가 가려진다.
            // 원격이 더 새것일 때만 업데이트한다 — 소스만 바뀐 커밋이면 확인 기록만 남긴다.
            Note($"원격 버전:{remoteVersion} / 지금 버전:{localVersion}");

            if (string.IsNullOrEmpty(remoteVersion) || string.CompareOrdinal(remoteVersion, localVersion) <= 0)
            {
                Note("더 새 버전이 아님 — 커밋만 기록");
                settings.InstalledCommit = remoteCommit;
                settings.Save(SettingsPath);
                return;
            }

            // 3) 자동 업데이트가 아니면 물어본다.
            if (!settings.AutoUpdate && !UpdateDialog.Ask(localVersion, remoteVersion))
            {
                Note("사용자가 \"나중에\"를 선택 — 다음 실행 때 다시 물어본다");
                // 기록을 남기지 않는다 — 다음에 켤 때 다시 물어본다.
                return;
            }

            var failed = CopyAll(remoteDist, platformDir);
            Note($"업데이트 완료 — 바꾸지 못한 파일:{failed}개");
            settings.InstalledCommit = remoteCommit;
            settings.InstalledVersion = remoteVersion;
            settings.Save(SettingsPath);

            if (failed > 0)
            {
                MessageBox.Show(
                    $"업데이트 중 {failed}개 파일을 바꾸지 못했습니다(다른 프로그램이 사용 중일 수 있습니다).\n" +
                    "그 프로그램들을 모두 종료한 뒤 다시 실행하면 나머지도 적용됩니다.",
                    "UbisamBase 업데이트", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        finally
        {
            TryDeleteDirectory(tempDir);
        }
    }

    private static string Short(string commit)
        => string.IsNullOrEmpty(commit) ? "(없음)" : commit.Substring(0, Math.Min(7, commit.Length));

    private static string FirstToken(string text)
    {
        var trimmed = (text ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        var index = trimmed.IndexOfAny(new[] { '\t', ' ', '\r', '\n' });
        return index < 0 ? trimmed : trimmed.Substring(0, index);
    }

    /// <summary>platform-version.json의 version 값. 파일이 없으면 빈 문자열(= 가장 오래된 것으로 취급).</summary>
    private static string ReadVersion(string path)
    {
        try
        {
            return File.Exists(path) ? Json.ReadString(File.ReadAllText(path), "version") : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>폴더 내용을 통째로 덮어쓴다. 바꾸지 못한(잠긴) 파일 개수를 돌려준다.</summary>
    private static int CopyAll(string sourceDir, string destDir)
    {
        var failed = 0;
        Directory.CreateDirectory(destDir);

        foreach (var sourceFile in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relative = sourceFile.Substring(sourceDir.Length).TrimStart('\\', '/');
            var destFile = Path.Combine(destDir, relative);
            var destSubDir = Path.GetDirectoryName(destFile);

            if (!string.IsNullOrEmpty(destSubDir))
            {
                Directory.CreateDirectory(destSubDir);
            }

            try
            {
                File.Copy(sourceFile, destFile, overwrite: true);
            }
            catch (IOException)
            {
                failed++;
            }
            catch (UnauthorizedAccessException)
            {
                failed++;
            }
        }

        return failed;
    }

    private static bool TryRunGit(string arguments, string? workingDirectory, int timeoutMs, out string output)
    {
        output = string.Empty;

        var info = new ProcessStartInfo("git", arguments)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            // 자격 증명 창이 떠서 실행이 멈추는 일이 없게 한다 — 물어봐야 하는 저장소면 그냥 실패시킨다.
            Environment = { ["GIT_TERMINAL_PROMPT"] = "0", ["GCM_INTERACTIVE"] = "never" }
        };

        if (!string.IsNullOrEmpty(workingDirectory))
        {
            info.WorkingDirectory = workingDirectory;
        }

        try
        {
            using var process = Process.Start(info);
            if (process == null)
            {
                return false;
            }

            var stdout = process.StandardOutput.ReadToEnd();
            process.StandardError.ReadToEnd();

            if (!process.WaitForExit(timeoutMs))
            {
                try
                {
                    process.Kill();
                }
                catch
                {
                    // 이미 끝났으면 무시한다.
                }

                return false;
            }

            output = stdout;
            return process.ExitCode == 0;
        }
        catch
        {
            return false; // git이 없거나 실행이 막힌 환경
        }
    }

    private static void TryDeleteDirectory(string dir)
    {
        try
        {
            if (!Directory.Exists(dir))
            {
                return;
            }

            // git이 만든 .git 안의 파일은 읽기 전용이라 그냥 지우면 실패한다.
            foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
            {
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                }
                catch
                {
                    // 무시한다.
                }
            }

            Directory.Delete(dir, recursive: true);
        }
        catch
        {
            // 임시 폴더가 남는 것은 실행에 지장이 없다.
        }
    }

    /// <summary>설정 파일(D:\UbisamPlatform\update-settings.json)의 내용. Core의 PlatformUpdateSettings와 같은 스키마.</summary>
    private sealed class Settings
    {
        public bool Enabled;
        public bool AutoUpdate;
        public string RepositoryUrl = string.Empty;
        public string Branch = "main";
        public string DistPath = "dist";
        public string InstalledVersion = string.Empty;
        public string InstalledCommit = string.Empty;

        public static Settings Load(string path)
        {
            var settings = new Settings();

            try
            {
                if (!File.Exists(path))
                {
                    return settings;
                }

                var json = File.ReadAllText(path);
                settings.Enabled = Json.ReadBool(json, nameof(Enabled));
                settings.AutoUpdate = Json.ReadBool(json, nameof(AutoUpdate));
                settings.RepositoryUrl = Json.ReadString(json, nameof(RepositoryUrl));
                settings.Branch = Json.ReadString(json, nameof(Branch));
                settings.DistPath = Json.ReadString(json, nameof(DistPath));
                settings.InstalledVersion = Json.ReadString(json, nameof(InstalledVersion));
                settings.InstalledCommit = Json.ReadString(json, nameof(InstalledCommit));
            }
            catch
            {
                // 손상된 파일은 무시한다 — 기본값(꺼짐)이라 아무것도 하지 않게 된다.
            }

            return settings;
        }

        public void Save(string path)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var text = new StringBuilder()
                    .AppendLine("{")
                    .AppendLine($"  \"{nameof(Enabled)}\": {(Enabled ? "true" : "false")},")
                    .AppendLine($"  \"{nameof(AutoUpdate)}\": {(AutoUpdate ? "true" : "false")},")
                    .AppendLine($"  \"{nameof(RepositoryUrl)}\": \"{Json.Escape(RepositoryUrl)}\",")
                    .AppendLine($"  \"{nameof(Branch)}\": \"{Json.Escape(Branch)}\",")
                    .AppendLine($"  \"{nameof(DistPath)}\": \"{Json.Escape(DistPath)}\",")
                    .AppendLine($"  \"{nameof(InstalledVersion)}\": \"{Json.Escape(InstalledVersion)}\",")
                    .AppendLine($"  \"{nameof(InstalledCommit)}\": \"{Json.Escape(InstalledCommit)}\"")
                    .AppendLine("}")
                    .ToString();

                File.WriteAllText(path, text);
            }
            catch
            {
                // 기록에 실패해도 실행은 계속한다(다음 실행 때 다시 확인할 뿐이다).
            }
        }
    }

    /// <summary>평평한 JSON 객체에서 값 하나만 꺼내는 최소 도우미 — Launcher에 JSON 라이브러리를 들이지 않기 위한 것.</summary>
    private static class Json
    {
        public static string ReadString(string json, string name)
        {
            var match = Regex.Match(json, "\"" + Regex.Escape(name) + "\"\\s*:\\s*\"((?:[^\"\\\\]|\\\\.)*)\"");
            return match.Success ? Unescape(match.Groups[1].Value) : string.Empty;
        }

        public static bool ReadBool(string json, string name)
        {
            var match = Regex.Match(json, "\"" + Regex.Escape(name) + "\"\\s*:\\s*(true|false)", RegexOptions.IgnoreCase);
            return match.Success && string.Equals(match.Groups[1].Value, "true", StringComparison.OrdinalIgnoreCase);
        }

        public static string Escape(string value)
            => (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");

        private static string Unescape(string value)
            => value.Replace("\\\"", "\"").Replace("\\\\", "\\");
    }
}

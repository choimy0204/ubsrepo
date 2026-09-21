using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace UbisamBase.Launcher;

/// <summary>
/// 소비 프로젝트는 UbisamBase.Shell.exe를 직접 실행하지 않고 이걸 실행한다(StartProgram).
///
/// 실행할 때마다 D:\UbisamPlatform\bin의 최신 내용을 자기가 있는 폴더로 복사해 온 뒤, 그 폴더의
/// Shell을 <b>같은 프로세스 안에서</b> 로드해 띄운다 - 예전 방식(빌드할 때만 복사)은 빌드를 다시
/// 해야만 플랫폼 갱신이 반영됐는데, 소스 없이 배포되는 환경(사용자 PC)에는 "빌드"라는 절차 자체가
/// 없어서 이 갱신이 영영 반영될 방법이 없었다. 실행할 때마다 복사하면 빌드 여부와 무관하게 항상
/// 최신으로 뜬다.
///
/// <para>
/// Shell을 <b>별도 프로세스로 띄우지 않는</b> 이유 - Visual Studio는 여기(Launcher)에 디버거를
/// 붙인다. Process.Start로 Shell을 따로 띄우면 그 새 프로세스에는 디버거가 안 붙고, Launcher는
/// 곧바로 끝나버려서 디버그 세션이 그 자리에서 종료된다. 그러면 소비 프로젝트(dll)에 찍은
/// 중단점이 하나도 안 걸린다. 같은 프로세스에서 로드하면 Shell·Core·소비 프로젝트 dll이 전부
/// 디버거가 붙어 있는 이 프로세스에 올라오므로 중단점이 정상 동작한다.
/// </para>
/// </summary>
internal static class Program
{
    private const string PlatformSourceDir = @"D:\UbisamPlatform\bin";

    [STAThread]
    private static int Main()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;

        // 배포 폴더를 복사해 오기 전에, 깃 저장소에 더 새 버전이 있는지 먼저 확인한다
        // (설정에서 꺼져 있거나 인터넷이 없으면 아무것도 하지 않고 넘어간다).
        PlatformUpdater.TryUpdate(PlatformSourceDir);

        try
        {
            CopyPlatformFiles(PlatformSourceDir, baseDir);
        }
        catch (Exception ex)
        {
            // 복사가 통째로 실패해도(예: D드라이브 자체가 없음) 이미 있던 파일로라도 실행을 시도한다.
            MessageBox.Show(
                $"플랫폼 파일을 최신으로 갱신하는 중 문제가 발생했습니다:\n{ex.Message}\n\n기존 파일로 계속 실행합니다.",
                "UbisamBase 실행", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        var shellPath = Path.Combine(baseDir, "UbisamBase.Shell.exe");
        if (!File.Exists(shellPath))
        {
            MessageBox.Show(
                $"UbisamBase.Shell.exe를 찾을 수 없습니다:\n{shellPath}\n\n{PlatformSourceDir} 가 존재하는지 확인하세요.",
                "UbisamBase 실행", MessageBoxButton.OK, MessageBoxImage.Error);
            return 1;
        }

        var shell = Assembly.LoadFrom(shellPath);

        // Application.ResourceAssembly는 건드리지 않는다 - WPF가 이미 정한 뒤라 바꾸려 하면
        // InvalidOperationException으로 죽는다. Shell의 pack URI는 전부 어셈블리를 명시하고
        // 있어서(/UbisamBase.Shell;component/..., /UbisamBase.Core;component/...) 기준 어셈블리가
        // 무엇이든 그대로 해석된다.
        var entry = shell.EntryPoint
            ?? throw new InvalidOperationException($"{shellPath} 에 진입점(Main)이 없습니다.");

        // 생성된 WPF 진입점은 보통 Main() 이지만, Main(string[]) 인 경우도 받아준다.
        // 인자는 넘기지 않아도 된다 - 같은 프로세스라 Environment.GetCommandLineArgs()가
        // 이 Launcher가 받은 인자를 그대로 돌려준다(탐색기 우클릭 · git mergetool 경로).
        var parameters = entry.GetParameters().Length == 0 ? null : new object[] { GetArgsWithoutExePath() };

        var result = entry.Invoke(null, parameters);
        return result is int exitCode ? exitCode : 0;
    }

    private static string[] GetArgsWithoutExePath()
    {
        var all = Environment.GetCommandLineArgs();
        if (all.Length <= 1) return Array.Empty<string>();

        var args = new string[all.Length - 1];
        Array.Copy(all, 1, args, 0, args.Length);
        return args;
    }

    private static void CopyPlatformFiles(string sourceDir, string destDir)
    {
        if (!Directory.Exists(sourceDir))
            throw new DirectoryNotFoundException($"플랫폼 배포 폴더를 찾을 수 없습니다: {sourceDir}");

        // 지금 실행 중인 자기 자신(Launcher.exe)은 파일이 잠겨 있어 덮어쓸 수 없다 - 건너뛴다.
        // Launcher는 자주 안 바뀌는 얇은 부트스트랩 코드라, 새 버전은 다음에 다시 빌드/배포할 때
        // 프로그램이 꺼져 있는 상태로 정상 복사된다.
        var launcherFileName = Path.GetFileName(typeof(Program).Assembly.Location);

        foreach (var sourceFile in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relative = sourceFile.Substring(sourceDir.Length).TrimStart('\\', '/');
            var destFile = Path.Combine(destDir, relative);

            if (string.Equals(Path.GetFileName(destFile), launcherFileName, StringComparison.OrdinalIgnoreCase))
                continue;

            var destSubDir = Path.GetDirectoryName(destFile);
            if (!string.IsNullOrEmpty(destSubDir) && !Directory.Exists(destSubDir))
                Directory.CreateDirectory(destSubDir);

            try
            {
                File.Copy(sourceFile, destFile, overwrite: true);
            }
            catch (IOException)
            {
                // 다른 파일이 잠겨 있어도(예: 같은 프로그램의 다른 인스턴스가 실행 중) 나머지는 계속 복사한다.
            }
        }
    }
}

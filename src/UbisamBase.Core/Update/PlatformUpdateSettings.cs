using System;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UbisamBase.Core.Update;

/// <summary>
/// 플랫폼 자동 업데이트 설정. 실행할 때 Launcher가 이 파일을 읽어 깃 저장소와 비교하고,
/// UI Setting 화면에서 사용자가 켜고 끈다.
///
/// 저장 위치가 배포 폴더(D:\UbisamPlatform\bin) <b>바깥</b>인 이유 — 업데이트는 그 폴더를 통째로
/// 덮어쓰므로, 설정을 안에 두면 업데이트할 때마다 날아간다.
///
/// 이 파일은 Launcher도 읽고 쓴다(UbisamBase.Launcher\PlatformUpdater.cs). Launcher는 Core를
/// 참조하지 않으므로(참조하면 갱신 대상인 Core.dll을 먼저 물고 있게 되어 덮어쓸 수 없다)
/// 같은 스키마를 각자 들고 있다. 속성 이름을 바꿀 때는 양쪽을 같이 고쳐야 한다.
/// </summary>
public partial class PlatformUpdateSettings : ObservableObject
{
    public const string FilePath = @"D:\UbisamPlatform\update-settings.json";

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static PlatformUpdateSettings? current;

    /// <summary>앱 전체가 공유하는 인스턴스. 처음 쓰는 시점에 파일에서 읽어온다.</summary>
    public static PlatformUpdateSettings Current => current ??= Load();

    /// <summary>업데이트 확인 기능 자체를 쓸지. 끄면 실행할 때 아무것도 확인하지 않는다.</summary>
    [ObservableProperty]
    private bool enabled;

    /// <summary>true면 새 버전이 있을 때 묻지 않고 바로 받는다. false면 받을지 물어본다.</summary>
    [ObservableProperty]
    private bool autoUpdate;

    /// <summary>플랫폼 소스와 업데이트 파일(dist)이 올라가는 깃 저장소 주소. 비어 있으면 확인하지 않는다.</summary>
    [ObservableProperty]
    private string repositoryUrl = string.Empty;

    /// <summary>비교 대상 브랜치.</summary>
    [ObservableProperty]
    private string branch = "main";

    /// <summary>저장소 안에서 업데이트 파일이 들어 있는 폴더.</summary>
    [ObservableProperty]
    private string distPath = "dist";

    /// <summary>마지막으로 설치한 버전/커밋 — Launcher가 기록한다. 화면에서는 보여주기만 한다.</summary>
    [ObservableProperty]
    private string installedVersion = string.Empty;

    [ObservableProperty]
    private string installedCommit = string.Empty;

    public static PlatformUpdateSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var loaded = JsonSerializer.Deserialize<PlatformUpdateSettings>(File.ReadAllText(FilePath), JsonOptions);
                if (loaded != null)
                {
                    return loaded;
                }
            }
        }
        catch
        {
            // 손상된 파일은 무시하고 기본값으로 시작한다.
        }

        return new PlatformUpdateSettings();
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(FilePath, JsonSerializer.Serialize(this, JsonOptions));
    }

    /// <summary>디스크에 저장된 값으로 되돌린다 — UI Setting의 "새로고침"이 호출한다.</summary>
    public void Reload()
    {
        var loaded = Load();
        Enabled = loaded.Enabled;
        AutoUpdate = loaded.AutoUpdate;
        RepositoryUrl = loaded.RepositoryUrl;
        Branch = loaded.Branch;
        DistPath = loaded.DistPath;
        InstalledVersion = loaded.InstalledVersion;
        InstalledCommit = loaded.InstalledCommit;
    }

    /// <summary>지금 D:\UbisamPlatform\bin에 깔려 있는 플랫폼 버전(배포할 때 찍힌 값). 없으면 빈 문자열.</summary>
    public static string ReadInstalledPlatformVersion()
    {
        try
        {
            var path = Path.Combine(@"D:\UbisamPlatform\bin", "platform-version.json");
            if (!File.Exists(path))
            {
                return string.Empty;
            }

            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            return doc.RootElement.TryGetProperty("version", out var value) ? value.GetString() ?? string.Empty : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}

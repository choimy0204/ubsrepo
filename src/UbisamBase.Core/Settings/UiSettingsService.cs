using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;

namespace UbisamBase.Core.Settings;

public enum ThemeMode
{
    System,
    Light,
    Dark
}

public enum NavPosition
{
    Left,
    Right,
    Bottom
}

// TopRight를 0번으로 둔다 — 기존 UiSettingsService.json(ToastPosition 필드가 없는 이전 버전)을
// 역직렬화하면 누락된 필드는 enum 기본값(0)이 되므로, 그 기본값이 실제 기본 위치(우상단)와 같아야 한다.
public enum ToastPosition
{
    TopRight,
    TopLeft,
    BottomLeft,
    BottomRight
}

/// <summary>
/// 테마, 화면 배율, 네비게이션 위치 등 "지금 이 화면"에 대한 UI 설정을 담당하는 싱글턴 서비스.
/// UI Setting 화면에서 값을 바꾸면 ShellWindow가 즉시 반영한다.
/// </summary>
public partial class UiSettingsService : ObservableObject
{
    /// <summary>
    /// 실제 적용되는 ChromeScale 배율 중 "100%"로 표시되는 기준값.
    /// UI Setting 화면의 배율 슬라이더는 이 값을 기준으로 상대 퍼센트를 보여준다
    /// (예: 여기 값이 1.2면 실제 ChromeScale 1.2가 "100%", 1.44가 "120%").
    /// </summary>
    public const double BaselineChromeScale = 1.2;

    [ObservableProperty]
    private ThemeMode themeMode = ThemeMode.System;

    [ObservableProperty]
    private double chromeScale = BaselineChromeScale;

    [ObservableProperty]
    private NavPosition navPosition = NavPosition.Bottom;

    [ObservableProperty]
    private ToastPosition toastPosition = ToastPosition.TopRight;

    // "펼침"이 기본이라 "접힘"을 true로 둔다 — TopBarCollapsed 필드가 없는 이전 버전 json을
    // 역직렬화하면 누락된 bool은 false가 되는데, 그 값이 곧 기본 상태(상단 바 보임)여야 한다.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TopBarVisible))]
    private bool topBarCollapsed;

    /// <summary>XAML에서 쓰기 편하게 뒤집어 둔 값 — 상단 바를 보여야 하면 true.</summary>
    public bool TopBarVisible => !TopBarCollapsed;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private string? filePath;

    /// <summary>
    /// Settings/UiSettingsService.json에서 이전 값을 불러오고(없으면 기본값으로 새로 만들어두고),
    /// 테마를 적용한다.
    /// </summary>
    public void Initialize(string basePath)
    {
        var dir = Path.Combine(basePath, "Settings");
        Directory.CreateDirectory(dir);
        filePath = Path.Combine(dir, $"{nameof(UiSettingsService)}.json");

        if (File.Exists(filePath))
        {
            try
            {
                var snapshot = JsonSerializer.Deserialize<Snapshot>(File.ReadAllText(filePath), JsonOptions);
                if (snapshot != null)
                {
                    ThemeMode = snapshot.ThemeMode;
                    ChromeScale = snapshot.ChromeScale;
                    NavPosition = snapshot.NavPosition;
                    ToastPosition = snapshot.ToastPosition;
                    TopBarCollapsed = snapshot.TopBarCollapsed;
                }
            }
            catch
            {
                // 손상된 파일은 무시하고 기본값을 그대로 쓴다.
            }
        }
        else
        {
            Save();
        }

        ApplyTheme(ThemeMode);
    }

    /// <summary>현재 값을 Settings/UiSettingsService.json에 기록한다. UI Setting 화면의 "저장" 버튼이 호출한다.</summary>
    public void Save()
    {
        if (filePath == null)
        {
            return;
        }

        var snapshot = new Snapshot
        {
            ThemeMode = ThemeMode,
            ChromeScale = ChromeScale,
            NavPosition = NavPosition,
            ToastPosition = ToastPosition,
            TopBarCollapsed = TopBarCollapsed
        };
        File.WriteAllText(filePath, JsonSerializer.Serialize(snapshot, JsonOptions));
    }

    /// <summary>디스크에 저장된 값으로 다시 불러온다 — UI Setting 화면의 "새로고침" 버튼이 호출한다.</summary>
    public void Reload()
    {
        if (filePath == null || !File.Exists(filePath))
        {
            return;
        }

        try
        {
            var snapshot = JsonSerializer.Deserialize<Snapshot>(File.ReadAllText(filePath), JsonOptions);
            if (snapshot != null)
            {
                ThemeMode = snapshot.ThemeMode;
                ChromeScale = snapshot.ChromeScale;
                NavPosition = snapshot.NavPosition;
                ToastPosition = snapshot.ToastPosition;
                TopBarCollapsed = snapshot.TopBarCollapsed;
            }
        }
        catch
        {
            // 손상된 파일은 무시하고 현재 값을 유지한다.
        }
    }

    partial void OnThemeModeChanged(ThemeMode value) => ApplyTheme(value);

    private sealed class Snapshot
    {
        public ThemeMode ThemeMode { get; set; }
        public double ChromeScale { get; set; }
        public NavPosition NavPosition { get; set; }
        public ToastPosition ToastPosition { get; set; }
        public bool TopBarCollapsed { get; set; }
    }

    private static void ApplyTheme(ThemeMode mode)
    {
        var isLight = mode switch
        {
            ThemeMode.Light => true,
            ThemeMode.Dark => false,
            _ => IsSystemLightTheme()
        };

        var themeFile = isLight ? "Colors.Light.xaml" : "Colors.Dark.xaml";
        var dict = new ResourceDictionary
        {
            Source = new Uri($"pack://application:,,,/UbisamBase.Core;component/Themes/{themeFile}")
        };

        var merged = Application.Current.Resources.MergedDictionaries;
        for (var i = merged.Count - 1; i >= 0; i--)
        {
            if (merged[i].Source?.OriginalString.Contains("/Themes/Colors.") == true)
            {
                merged.RemoveAt(i);
            }
        }

        merged.Add(dict);

        Application.Current.Resources["Ubisam.Logo"] =
            Application.Current.Resources[isLight ? "Ubisam.Logo.Light" : "Ubisam.Logo.Dark"];

        UbisamBase.Core.Charting.ChartTheme.NotifyThemeChanged();
    }

    private static bool IsSystemLightTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

            return key?.GetValue("AppsUseLightTheme") is not int value || value != 0;
        }
        catch
        {
            return true;
        }
    }
}

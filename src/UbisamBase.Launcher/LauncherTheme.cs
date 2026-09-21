using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace UbisamBase.Launcher;

/// <summary>
/// 런처가 띄우는 창(진행 창·업데이트 안내창)의 밝기를 정한다.
///
/// 런처는 Core를 참조할 수 없어서(갱신 대상인 Core.dll을 물게 된다) UiSettingsService를 쓸 수 없지만,
/// 그 서비스가 저장해 둔 파일은 그대로 읽을 수 있다. 사용자가 앱에서 다크로 해뒀는데 런처 창만
/// 윈도우 설정(밝게)을 따라가면 혼자 하얗게 떠서 튄다 — 그래서 앱 설정을 먼저 보고,
/// "시스템 설정 따르기"이거나 파일이 없을 때만 윈도우 설정을 본다.
/// </summary>
internal static class LauncherTheme
{
    // 설정 폴더는 Core의 UbisamAppBuilder.ConfigRoot와 같은 고정 경로다(앱이 달라도 같은 폴더를 쓴다).
    private const string UiSettingsPath = @"D:\UbisamConfig\Settings\UiSettingsService.json";

    /// <summary>지금 밝은 테마로 그려야 하는가.</summary>
    public static bool IsLight()
    {
        var mode = ReadThemeMode();

        return mode switch
        {
            ThemeMode.Light => true,
            ThemeMode.Dark => false,
            _ => IsSystemLightTheme()
        };
    }

    /// <summary>Core의 UbisamBase.Core.Settings.ThemeMode와 값·순서가 같아야 한다(파일에 숫자로 저장된다).</summary>
    private enum ThemeMode
    {
        System = 0,
        Light = 1,
        Dark = 2
    }

    private static ThemeMode ReadThemeMode()
    {
        try
        {
            if (!File.Exists(UiSettingsPath))
            {
                return ThemeMode.System;
            }

            var json = File.ReadAllText(UiSettingsPath);

            // 숫자로 저장된 경우(지금 방식)
            var number = Regex.Match(json, "\"ThemeMode\"\\s*:\\s*(\\d+)");
            if (number.Success && int.TryParse(number.Groups[1].Value, out var value) &&
                Enum.IsDefined(typeof(ThemeMode), value))
            {
                return (ThemeMode)value;
            }

            // 이름으로 저장된 경우(나중에 문자열 변환기를 붙이더라도 깨지지 않게)
            var name = Regex.Match(json, "\"ThemeMode\"\\s*:\\s*\"(\\w+)\"");
            if (name.Success && Enum.TryParse<ThemeMode>(name.Groups[1].Value, ignoreCase: true, out var parsed))
            {
                return parsed;
            }
        }
        catch
        {
            // 읽지 못하면 시스템 설정을 따른다.
        }

        return ThemeMode.System;
    }

    /// <summary>윈도우가 밝은 테마인지. Core의 UiSettingsService.IsSystemLightTheme과 같은 판정이다.</summary>
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

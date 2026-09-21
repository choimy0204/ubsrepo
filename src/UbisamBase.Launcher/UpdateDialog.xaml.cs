using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace UbisamBase.Launcher;

/// <summary>
/// "새 버전이 있습니다 — 지금 받을까요?" 대화상자. 플랫폼(Core)의 MessageDialog와 같은 생김새지만,
/// Launcher는 Core를 참조하지 않으므로(참조하면 갱신 대상인 Core.dll을 물고 있게 된다) 색을 직접 넣는다.
///
/// 테마는 윈도우 설정(밝게/어둡게)을 따라간다 — 이 시점에는 아직 앱의 UI 설정을 읽을 수 없기 때문이다.
/// 색 값은 Core의 Colors.Dark.xaml / Colors.Light.xaml에서 같은 이름으로 가져온 것이다.
/// </summary>
public partial class UpdateDialog : Window
{
    private UpdateDialog(string currentVersion, string newVersion)
    {
        InitializeComponent();

        CurrentValue.Text = string.IsNullOrEmpty(currentVersion) ? "(알 수 없음)" : currentVersion;
        NewValue.Text = newVersion;

        ApplyTheme(IsSystemLightTheme());
    }

    /// <summary>대화상자를 띄우고 "업데이트"를 눌렀는지 돌려준다.</summary>
    public static bool Ask(string currentVersion, string newVersion)
        => new UpdateDialog(currentVersion, newVersion).ShowDialog() == true;

    private void ApplyTheme(bool light)
    {
        // Colors.Light.xaml / Colors.Dark.xaml 과 같은 값.
        var background = Color(light ? "#FFF2F2F3" : "#FF23262B");
        var foreground = Color(light ? "#FF1D1F20" : "#FFE8EAED");
        var panel = Color(light ? "#FFFFFFFF" : "#FF2A2E34");
        var border = Color(light ? "#FFD6D6D9" : "#FF3A3F47");
        var muted = Color(light ? "#FF98989B" : "#FF8A9099");
        var accentFill = Color(light ? "#FF1D82E8" : "#FF1E7FD4");
        var onAccent = Color(light ? "#FFFFFFFF" : "#FFF2F7FC");

        Background = background;
        Foreground = foreground;
        RootBorder.BorderBrush = border;

        TitleBar.Background = panel;
        TitleBar.BorderBrush = border;
        TitleText.Foreground = foreground;

        ButtonBar.Background = panel;
        ButtonBar.BorderBrush = border;

        IconPath.Stroke = foreground;
        HeadlineText.Foreground = foreground;
        CurrentLabel.Foreground = muted;
        NewLabel.Foreground = muted;
        CurrentValue.Foreground = muted;
        NewValue.Foreground = foreground;
        FootnoteText.Foreground = muted;

        YesButton.Background = accentFill;
        YesButton.BorderBrush = accentFill;
        YesButton.Foreground = onAccent;

        NoButton.Background = Brushes.Transparent;
        NoButton.BorderBrush = border;
        NoButton.Foreground = foreground;
    }

    private static SolidColorBrush Color(string hex)
        => new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));

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

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    private void Yes_Click(object sender, RoutedEventArgs e) => DialogResult = true;

    private void No_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
        }
    }
}

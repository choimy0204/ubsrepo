using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace UbisamBase.Launcher;

/// <summary>
/// "새 버전이 준비됐습니다 — 지금 받을까요?" 대화상자.
///
/// 플랫폼(Core)의 색·글꼴을 따르되 리소스는 공유하지 않는다 — Launcher가 Core를 참조하면 갱신
/// 대상인 Core.dll을 물고 있게 되어 덮어쓸 수 없기 때문이다. 그래서 색을 여기서 직접 넣는다.
/// 테마는 윈도우 설정(밝게/어둡게)을 따른다. 이 시점에는 아직 앱의 UI 설정을 읽을 수 없다.
/// </summary>
public partial class UpdateDialog : Window
{
    private UpdateDialog(string currentVersion, string newVersion)
    {
        InitializeComponent();

        CurrentValue.Text = string.IsNullOrEmpty(currentVersion) ? "(알 수 없음)" : currentVersion;
        NewValue.Text = newVersion;

        ApplyTheme(LauncherTheme.IsLight());
    }

    /// <summary>보조 버튼에 마우스를 올렸을 때 깔리는 색. 템플릿에서 바인딩해 쓴다.</summary>
    public Brush QuietHoverBrush { get; private set; } = Brushes.Transparent;

    /// <summary>대화상자를 띄우고 "지금 업데이트"를 눌렀는지 돌려준다.</summary>
    public static bool Ask(string currentVersion, string newVersion)
        => new UpdateDialog(currentVersion, newVersion).ShowDialog() == true;

    private void ApplyTheme(bool light)
    {
        // Colors.Light.xaml / Colors.Dark.xaml 과 같은 값.
        var background = Brush(light ? "#FFFFFFFF" : "#FF2A2E34");
        var foreground = Brush(light ? "#FF1D1F20" : "#FFE8EAED");
        var border = Brush(light ? "#FFD6D6D9" : "#FF3A3F47");
        var muted = Brush(light ? "#FF98989B" : "#FF8A9099");
        var accent = Brush(light ? "#FF2196F3" : "#FF4DA6F5");
        var accentFill = Brush(light ? "#FF1D82E8" : "#FF1E7FD4");
        var onAccent = Brush(light ? "#FFFFFFFF" : "#FFF2F7FC");
        var sunken = Brush(light ? "#FFF2F2F3" : "#FF23262B");
        var chip = Brush(light ? "#142196F3" : "#1F4DA6F5");

        Background = background;
        Foreground = foreground;
        RootBorder.BorderBrush = border;
        AccentStrip.Background = accent;

        IconChip.Background = chip;
        IconPath.Stroke = accent;

        HeadlineText.Foreground = foreground;
        SubText.Foreground = muted;

        VersionBox.Background = sunken;
        CurrentLabel.Foreground = muted;
        CurrentValue.Foreground = muted;
        ArrowPath.Stroke = accent;
        NewLabel.Foreground = muted;
        NewValue.Foreground = foreground;
        FootnoteText.Foreground = muted;

        YesButton.Background = accentFill;
        YesButton.Foreground = onAccent;
        NoButton.Foreground = muted;
        QuietHoverBrush = sunken;
    }

    private static SolidColorBrush Brush(string hex)
        => new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));

    /// <summary>제목줄이 없으므로 창 아무 데나 끌어서 옮길 수 있게 한다.</summary>
    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

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

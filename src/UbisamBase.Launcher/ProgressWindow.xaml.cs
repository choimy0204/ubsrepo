using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace UbisamBase.Launcher;

/// <summary>
/// 업데이트를 확인하고 받는 동안 보여주는 창. 런처를 누르자마자 떠서 "지금 뭘 하고 있는지"를 알린다
/// (예전에는 받는 동안 아무것도 안 보여서 프로그램이 안 켜지는 것처럼 보였다).
///
/// 받기가 끝나면 이 창은 닫히고, 설치 여부는 UpdateDialog가 묻는다.
/// 색은 UpdateDialog와 같은 방식으로(윈도우 밝게/어둡게) 직접 넣는다 — Core를 참조할 수 없기 때문.
/// </summary>
public partial class ProgressWindow : Window
{
    private double percent;

    public ProgressWindow()
    {
        InitializeComponent();
        ApplyTheme(LauncherTheme.IsLight());
        SizeChanged += (_, _) => UpdateFillWidth(animate: false);
    }

    /// <summary>제목과 설명을 바꾼다. 어느 스레드에서 불러도 된다.</summary>
    public void SetStage(string headline, string status)
        => Dispatcher.Invoke(() =>
        {
            HeadlineText.Text = headline;
            StatusText.Text = status;
        });

    /// <summary>0~100. 뒤로 가지 않도록 지금 값보다 큰 값만 반영한다 — git이 단계마다 0%부터 다시 세기 때문.</summary>
    public void SetPercent(double value)
        => Dispatcher.Invoke(() =>
        {
            var clamped = Math.Max(0, Math.Min(100, value));
            if (clamped <= percent)
            {
                return;
            }

            percent = clamped;
            PercentText.Text = ((int)percent) + "%";
            UpdateFillWidth(animate: true);
        });

    private void UpdateFillWidth(bool animate)
    {
        var target = Math.Max(0, TrackBorder.ActualWidth * percent / 100.0);

        if (!animate)
        {
            FillBorder.BeginAnimation(WidthProperty, null);
            FillBorder.Width = target;
            return;
        }

        // 숫자가 툭툭 튀지 않도록 짧게 이어 붙인다.
        FillBorder.BeginAnimation(WidthProperty, new DoubleAnimation(target, TimeSpan.FromMilliseconds(220))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            FillBehavior = FillBehavior.HoldEnd
        });
    }

    private void ApplyTheme(bool light)
    {
        var background = Brush(light ? "#FFFFFFFF" : "#FF2A2E34");
        var foreground = Brush(light ? "#FF1D1F20" : "#FFE8EAED");
        var border = Brush(light ? "#FFD6D6D9" : "#FF3A3F47");
        var muted = Brush(light ? "#FF98989B" : "#FF8A9099");
        var accent = Brush(light ? "#FF2196F3" : "#FF4DA6F5");
        var track = Brush(light ? "#FFEDEDEF" : "#FF23262B");
        var chip = Brush(light ? "#142196F3" : "#1F4DA6F5");

        Background = background;
        Foreground = foreground;
        RootBorder.BorderBrush = border;
        AccentStrip.Background = accent;
        IconChip.Background = chip;
        IconPath.Stroke = accent;
        HeadlineText.Foreground = foreground;
        StatusText.Foreground = muted;
        PercentText.Foreground = muted;
        TrackBorder.Background = track;
        FillBorder.Background = accent;
    }

    private static SolidColorBrush Brush(string hex)
        => new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
}

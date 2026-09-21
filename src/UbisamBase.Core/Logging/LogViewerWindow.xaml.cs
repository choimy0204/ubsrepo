using System;
using System.Windows;
using System.Windows.Input;
using UbisamBase.Core.Locking;
using UbisamBase.Core.Shell;

namespace UbisamBase.Core.Logging;

/// <summary>
/// "로그" 버튼을 누르면 뜨는 비모달 창 — 기존 LogViewerView를 그대로 담는다.
/// 이미 열려 있으면 새로 만들지 않고 그 창에 포커스만 준다(ShowOrFocus). 화면 잠금이 걸리면
/// 자동으로 닫힌다(잠금 상태에서 별도 창으로 로그가 노출되는 것을 막기 위함).
/// </summary>
public partial class LogViewerWindow : Window
{
    private static LogViewerWindow? current;

    public LogViewerWindow()
    {
        InitializeComponent();
        Viewer.DataContext = new LogViewerViewModel();

        LockService.Current.PropertyChanged += OnLockChanged;
        Closed += (_, _) =>
        {
            LockService.Current.PropertyChanged -= OnLockChanged;
            if (ReferenceEquals(current, this))
            {
                current = null;
            }
        };
    }

    private void OnLockChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LockService.IsLocked) && LockService.Current.IsLocked)
        {
            Close();
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    /// <summary>이미 열려 있으면 그 창을 앞으로 가져와 포커스만 준다. 없으면 새로 연다(닫는 용도가 아니다).</summary>
    public static void ShowOrFocus(Window owner)
    {
        if (current is { IsLoaded: true })
        {
            if (current.WindowState == WindowState.Minimized)
            {
                current.WindowState = WindowState.Normal;
            }

            current.Activate();
            return;
        }

        current = new LogViewerWindow { Owner = owner };
        current.PlaceOnOwnerMonitor(owner);
        current.Show();
    }

    // 2560x1440 모니터에서 로그를 편하게 읽을 수 있는 크기(사용자가 직접 맞춰 본 크기)를 기본으로 한다.
    // 작은 모니터에서는 화면의 90%를 넘지 않게 줄이고, 셸이 떠 있는 모니터 한가운데에 띄운다.
    private const double PreferredWidth = 1440;
    private const double PreferredHeight = 860;

    private void PlaceOnOwnerMonitor(Window owner)
    {
        var screen = MonitorHelper.GetBounds(owner);
        Width = Math.Max(MinWidth, Math.Min(PreferredWidth, screen.Width * 0.9));
        Height = Math.Max(MinHeight, Math.Min(PreferredHeight, screen.Height * 0.9));
        WindowStartupLocation = WindowStartupLocation.Manual;
        Left = screen.Left + (screen.Width - Width) / 2;
        Top = screen.Top + (screen.Height - Height) / 2;
    }
}

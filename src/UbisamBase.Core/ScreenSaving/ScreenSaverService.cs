using System;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UbisamBase.Core.ScreenSaving;

/// <summary>
/// 특정 동작을 진행하는 동안 사용자 조작을 막기 위한 반투명 오버레이 상태를 들고 있는 싱글턴.
/// 화면 잠금(LockService)과 의미는 비슷하지만 인증이 없다 — 아무 코드나 자유롭게 켜고 끌 수 있다.
/// ShellWindow가 IsActive를 바인딩해 ScreenSaverOverlay를 덮는다.
/// </summary>
public partial class ScreenSaverService : ObservableObject
{
    public static ScreenSaverService Current { get; } = new();

    [ObservableProperty]
    private bool isActive;

    [ObservableProperty]
    private string message = string.Empty;

    private DispatcherTimer? timer;

    /// <summary>Show(seconds:)로 지정한 시간이 지나면 올라간다. 이 서비스가 알아서 끄지 않는다 —
    /// 끌지 말지는 이 이벤트를 구독한 쪽이 정한다.</summary>
    public event Action? TimerElapsed;

    private ScreenSaverService()
    {
    }

    /// <summary>화면 가운데에 message를 띄우고 사용자 조작을 막는다. seconds를 주면 그 시간이 지났을 때
    /// 자동으로 끄는 게 아니라 TimerElapsed 이벤트만 올린다.</summary>
    public void Show(string message, int? seconds = null)
    {
        Message = message;
        IsActive = true;

        timer?.Stop();
        timer = null;

        if (seconds is > 0)
        {
            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(seconds.Value) };
            timer.Tick += (_, _) =>
            {
                timer!.Stop();
                TimerElapsed?.Invoke();
            };
            timer.Start();
        }
    }

    public void Hide()
    {
        timer?.Stop();
        timer = null;
        IsActive = false;
    }
}

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace UbisamBase.Core.Shell;

/// <summary>window가 떠 있는 모니터의 경계를 Win32로 구하는 공용 헬퍼.
/// ScreenshotService(화면 캡처)와 ShellWindow(kiosk 전체화면 배치)가 함께 쓴다.</summary>
internal static class MonitorHelper
{
    private const uint MONITOR_DEFAULTTONEAREST = 2;

    /// <summary>모니터 전체 경계(작업표시줄 포함). WPF Window.Left/Top/Width/Height에 그대로 대입 가능한
    /// 좌표계다 — 이 앱은 DPI 매니페스트가 없는 System DPI Aware 프로세스라, Windows가 GetMonitorInfo
    /// 값을 그 좌표계에 맞춰 돌려준다(혼합 DPI 다중 모니터에서는 소폭 오차 가능 — 알려진 제약).
    /// window가 아직 한 번도 자리를 잡지 못한 상태(막 생성된 직후)면 OS가 임시로 준 위치가 엉뚱한
    /// 모니터와 겹칠 수 있어(멀티 모니터 환경에서 실제로 확인된 문제) 그 경우 주 모니터로 대체한다.</summary>
    public static Rect GetBounds(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        var hMonitor = window.IsLoaded
            ? MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST)
            : MonitorFromPoint(default, MONITOR_DEFAULTTOPRIMARY);

        var info = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
        if (hMonitor != IntPtr.Zero && GetMonitorInfo(hMonitor, ref info))
        {
            var r = info.rcMonitor;
            return new Rect(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
        }

        return new Rect(
            SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenWidth,
            SystemParameters.VirtualScreenHeight);
    }

    /// <summary>창이 실제로 차지하고 있는 사각형을 GetBounds와 같은 좌표계(Win32)로 돌려준다.
    /// WPF의 Left/Top/Width/Height(DIP)로 지정한 크기가 환경에 따라 그대로 반영되지 않는 경우가 있어
    /// (DPI 배율 반올림 등) 실제 값을 재서 보정하는 데 쓴다.</summary>
    public static Rect GetWindowBounds(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero || !GetWindowRect(hwnd, out var r))
        {
            return Rect.Empty;
        }

        return new Rect(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
    }

    private const uint MONITOR_DEFAULTTOPRIMARY = 1;

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }
}

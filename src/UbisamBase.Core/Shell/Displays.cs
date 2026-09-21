using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace UbisamBase.Core.Shell;

/// <summary>
/// 모니터 정보를 WPF 좌표(DIP)로 돌려주는 공용 헬퍼. 발표자 보기·검사 화면처럼 "본 창이 없는 쪽
/// 모니터"에 창을 띄우는 모듈이 쓴다.
///
/// 모듈이 직접 System.Windows.Forms의 Screen을 읽으면 실제 픽셀이 나와서, 배율(DPI)이 걸린
/// 환경에서는 DIP로 환산하지 않으면 창이 엉뚱한 자리에 뜬다. 그 환산까지 여기서 처리한다.
/// </summary>
public static class Displays
{
    /// <summary>모니터가 두 대 이상인가.</summary>
    public static bool HasSecondary => GetMonitors().Count > 1;

    /// <summary>
    /// owner 창이 있는 모니터가 아닌 다른 모니터의 작업 영역(작업표시줄 제외)을 DIP로 돌려준다.
    /// 모니터가 하나뿐이면 null. 세 대 이상이면 owner가 없는 것 중 첫 번째를 준다.
    /// </summary>
    public static Rect? SecondaryWorkArea(Window owner)
    {
        if (owner == null)
        {
            throw new ArgumentNullException(nameof(owner));
        }

        var monitors = GetMonitors();
        if (monitors.Count < 2)
        {
            return null;
        }

        var ownerHandle = MonitorFromWindow(new WindowInteropHelper(owner).Handle, MONITOR_DEFAULTTONEAREST);
        foreach (var monitor in monitors)
        {
            if (monitor.Handle != ownerHandle)
            {
                return ToDip(monitor.Work, owner);
            }
        }

        return null;
    }

    /// <summary>owner 창이 있는 모니터의 작업 영역(DIP).</summary>
    public static Rect WorkArea(Window owner)
    {
        if (owner == null)
        {
            throw new ArgumentNullException(nameof(owner));
        }

        var ownerHandle = MonitorFromWindow(new WindowInteropHelper(owner).Handle, MONITOR_DEFAULTTONEAREST);
        foreach (var monitor in GetMonitors())
        {
            if (monitor.Handle == ownerHandle)
            {
                return ToDip(monitor.Work, owner);
            }
        }

        return SystemParameters.WorkArea;
    }

    /// <summary>실제 픽셀 좌표를 그 창이 쓰는 DIP 좌표로 바꾼다. 배율이 걸린 환경에서 이 변환을
    /// 빠뜨리면 창이 화면 밖으로 밀린다.</summary>
    private static Rect ToDip(Rect device, Window owner)
    {
        var scaleX = 1.0;
        var scaleY = 1.0;

        try
        {
            var dpi = VisualTreeHelper.GetDpi(owner);
            if (dpi.DpiScaleX > 0 && dpi.DpiScaleY > 0)
            {
                scaleX = dpi.DpiScaleX;
                scaleY = dpi.DpiScaleY;
            }
        }
        catch
        {
            // 창이 아직 화면에 붙기 전이면 배율을 못 구한다 — 1:1로 둔다.
        }

        return new Rect(device.Left / scaleX, device.Top / scaleY, device.Width / scaleX, device.Height / scaleY);
    }

    private readonly struct MonitorInfo
    {
        public MonitorInfo(IntPtr handle, Rect work)
        {
            Handle = handle;
            Work = work;
        }

        public IntPtr Handle { get; }

        public Rect Work { get; }
    }

    private static List<MonitorInfo> GetMonitors()
    {
        var list = new List<MonitorInfo>();

        MonitorEnumProc callback = (IntPtr hMonitor, IntPtr hdc, ref RECT rect, IntPtr data) =>
        {
            var info = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
            if (GetMonitorInfo(hMonitor, ref info))
            {
                var w = info.rcWork;
                var work = new Rect(w.Left, w.Top, w.Right - w.Left, w.Bottom - w.Top);

                // 주 모니터를 항상 앞에 둔다 — "다른 쪽"을 고를 때 순서가 매번 같아야 한다.
                if ((info.dwFlags & MONITORINFOF_PRIMARY) != 0)
                {
                    list.Insert(0, new MonitorInfo(hMonitor, work));
                }
                else
                {
                    list.Add(new MonitorInfo(hMonitor, work));
                }
            }

            return true;
        };

        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
        GC.KeepAlive(callback);
        return list;
    }

    private const uint MONITOR_DEFAULTTONEAREST = 2;
    private const uint MONITORINFOF_PRIMARY = 1;

    private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

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

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using UbisamBase.Core.Bootstrap;
using UbisamBase.Core.Logging;
using UbisamBase.Core.Messaging;
using UbisamBase.Core.Shell;

namespace UbisamBase.Core.Capture;

/// <summary>
/// 화면 스크린샷 캡처 — Shell 창이 떠 있는 모니터 전체를 PNG로 저장한다.
/// DI 없이 정적으로 쓴다(ToastService/LogService와 같은 패턴) — 등록을 빠뜨려도 다른 프로그램이
/// 죽지 않도록 하기 위함.
/// </summary>
public static class ScreenshotService
{
    private static readonly Logger Log = new("Screenshot");
    private static readonly string SaveDir = Path.Combine(UbisamAppBuilder.ConfigRoot, "ScreenShot");

    /// <summary>window가 떠 있는 모니터 전체를 캡처해 저장한다. 성공/실패 모두 토스트+로그를 남긴다.</summary>
    public static void Capture(Window window)
    {
        try
        {
            var bounds = GetMonitorBounds(window);
            Directory.CreateDirectory(SaveDir);

            var path = BuildUniquePath(DateTime.Now);

            using var bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
            }

            bitmap.Save(path, ImageFormat.Png);

            MessageUtil.ShowSuccessToast($"화면을 저장했습니다: {Path.GetFileName(path)}", Log);
        }
        catch (Exception ex)
        {
            MessageUtil.ShowErrorToast("화면 저장에 실패했습니다.", Log);
            Log.E("스크린샷 저장 실패", ex);
        }
    }

    /// <summary>같은 초에 여러 번 눌러도 덮어쓰지 않도록 충돌 시 _2, _3 접미사를 붙인다.</summary>
    private static string BuildUniquePath(DateTime now)
    {
        var baseName = now.ToString("yyyyMMdd_HHmmss");
        var path = Path.Combine(SaveDir, $"{baseName}.png");

        var suffix = 2;
        while (File.Exists(path))
        {
            path = Path.Combine(SaveDir, $"{baseName}_{suffix}.png");
            suffix++;
        }

        return path;
    }

    /// <summary>window가 있는 모니터의 전체 화면 경계(작업표시줄 포함)를 물리 픽셀 좌표로 구한다.
    /// Graphics.CopyFromScreen은 항상 물리 픽셀 기준이라, MonitorHelper가 돌려주는 값(WPF DIP 기준)을
    /// 그대로 정수 변환해 쓴다 — System DPI Aware 프로세스라 100% 배율에서는 둘이 같고, 다른 배율에서도
    /// Windows의 DPI 가상화로 일치한다(F2/AC-04 실측 대상).</summary>
    private static Rectangle GetMonitorBounds(Window window)
    {
        var bounds = MonitorHelper.GetBounds(window);
        return new Rectangle((int)bounds.Left, (int)bounds.Top, (int)bounds.Width, (int)bounds.Height);
    }
}

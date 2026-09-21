using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using UbisamBase.Core.Bootstrap;

namespace UbisamBase.Shell;

/// <summary>
/// 고정 호스트 실행파일. 프로젝트별 화면 로직은 전혀 없고, 실행 시 같은 폴더에서
/// IAppSetup을 구현한 dll을 찾아 로드하기만 한다.
/// </summary>
public partial class App : Application
{
    private const string LauncherFileName = "UbisamBase.Launcher.exe";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (RedirectToLauncher())
        {
            return;
        }

        UbisamAppBuilder.Run(AppDomain.CurrentDomain.BaseDirectory);
    }

    /// <summary>
    /// 이 exe를 직접 실행한 경우 런처로 넘긴다.
    ///
    /// 프로그램의 정식 진입점은 <b>항상 런처</b>다 — 런처만이 배포 폴더(D:\UbisamPlatform\bin)를
    /// 최신으로 맞추고 업데이트를 확인한다. 폴더에 exe가 둘 보여서 이걸 직접 누르는 일이 생기는데,
    /// 그러면 갱신 없이 옛 파일로 떠서 "왜 업데이트가 안 되지?"가 된다. 그래서 조용히 런처로 넘긴다.
    ///
    /// 런처는 Shell을 <b>같은 프로세스에서</b> 로드하므로, 런처로 실행된 경우 이 프로세스의 exe
    /// 이름은 런처다 — 그때는 아무 일도 하지 않는다.
    /// </summary>
    private bool RedirectToLauncher()
    {
        try
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var currentExe = Process.GetCurrentProcess().MainModule?.FileName;

            if (string.Equals(Path.GetFileName(currentExe), LauncherFileName, StringComparison.OrdinalIgnoreCase))
            {
                return false; // 런처가 띄운 정상 경로
            }

            var launcherPath = Path.Combine(baseDir, LauncherFileName);
            if (!File.Exists(launcherPath))
            {
                return false; // 런처 없이 배포된 환경 — 그대로 실행한다
            }

            Process.Start(new ProcessStartInfo(launcherPath)
            {
                UseShellExecute = true,
                WorkingDirectory = baseDir
            });

            Shutdown();
            return true;
        }
        catch
        {
            return false; // 넘기지 못하면 그냥 이대로 실행한다 — 프로그램이 안 켜지는 것보다 낫다.
        }
    }
}

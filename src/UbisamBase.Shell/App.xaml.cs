using System;
using System.Windows;
using UbisamBase.Core.Bootstrap;

namespace UbisamBase.Shell;

/// <summary>
/// 고정 호스트 실행파일. 프로젝트별 화면 로직은 전혀 없고, 실행 시 같은 폴더에서
/// IAppSetup을 구현한 dll을 찾아 로드하기만 한다.
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        UbisamAppBuilder.Run(AppDomain.CurrentDomain.BaseDirectory);
    }
}

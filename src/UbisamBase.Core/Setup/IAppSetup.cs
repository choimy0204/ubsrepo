using Microsoft.Extensions.DependencyInjection;
using UbisamBase.Core.Modules;

namespace UbisamBase.Core.Setup;

/// <summary>
/// 소프트웨어 프로젝트가 구현하는 단일 진입점. 각 프로젝트는 exe를 빌드하지 않고
/// 이 인터페이스를 구현한 dll만 빌드하며, UbisamBase.Shell.exe가 실행 시 그 dll을
/// 리플렉션으로 찾아 로드한다.
///
/// 필요에 따라 이 인터페이스에 Register* 메서드를 계속 추가해나가면 된다
/// (지금은 화면 등록만 지원한다).
/// </summary>
public interface IAppSetup
{
    /// <summary>Shell 창 제목 등에 쓰이는 프로그램 이름.</summary>
    string GetAppName();

    /// <summary>메인 탭/서브탭 화면을 등록한다.</summary>
    void RegisterViews(ITabManager manager);

    /// <summary>
    /// View 없이 자동 생성되는 설정 화면을 등록한다. manager.AddSettingsSub&lt;T&gt;(TabManager.SettingsTabKey, "제목")
    /// 형태로 호출하면 플랫폼 고정 "설정" 탭 아래 서브탭으로 자동 편집 UI가 생긴다.
    /// 등록할 설정이 없으면 빈 메서드로 둬도 된다.
    /// </summary>
    void RegisterSettings(ITabManager manager);

    /// <summary>
    /// 화면 어디서든 생성자로 받아쓰고 싶은 공용 객체를 DI 컨테이너에 등록한다.
    /// 여기서 services.AddSingleton&lt;MyContext&gt;()처럼 등록해두면, 이 dll 안의 어떤
    /// View/ViewModel이든 생성자에 MyContext를 선언하기만 하면 Shell이 자동으로 주입해준다
    /// (TabManager가 화면을 만들 때 ActivatorUtilities.CreateInstance로 생성하기 때문).
    /// 등록할 게 없으면 빈 메서드로 둬도 된다.
    /// </summary>
    void RegisterServices(IServiceCollection services);
}

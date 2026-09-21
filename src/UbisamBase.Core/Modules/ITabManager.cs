using System.Windows;

namespace UbisamBase.Core.Modules;

/// <summary>
/// IAppSetup.RegisterViews에 전달되는 탭 등록기.
/// View와 ViewModel을 제네릭으로 함께 넘기면 Shell이 View 인스턴스를 만들고
/// DI 컨테이너로 ViewModel을 생성해서 자동으로 DataContext를 연결한다.
///
/// 사용 예:
/// <code>
/// manager.AddMain&lt;HomeView, HomeViewModel&gt;(Icon.Home, ViewIds.Home, "홈");
/// manager.AddSub&lt;HomeInfoView, HomeInfoViewModel&gt;(ViewIds.Home, "정보");
/// </code>
/// icon은 문자열이 아니라 <see cref="Icon"/> enum이라 오타/잘못된 값이 컴파일 타임에 걸러진다.
/// 아이콘 없이 글자만 쓰려면 Icon.None을 넘긴다.
/// </summary>
public interface ITabManager
{
    /// <summary>메인 탭을 등록한다. key는 AddSub에서 이 탭을 다시 찾을 때 쓰는 고유 식별자다.</summary>
    void AddMain<TView, TViewModel>(Icon icon, string key, string? title = null)
        where TView : FrameworkElement, new()
        where TViewModel : class;

    /// <summary>parentKey로 지정한 메인 탭의 우측 서브탭 바에 화면을 하나 추가한다.</summary>
    void AddSub<TView, TViewModel>(string parentKey, string? title = null)
        where TView : FrameworkElement, new()
        where TViewModel : class;

    /// <summary>
    /// View 없이 설정 클래스의 public 프로퍼티만으로 편집 화면을 자동 생성해 서브탭으로 등록한다.
    /// 프로퍼티 타입에 따라 자동으로 컨트롤이 정해진다:
    /// bool→체크박스, int→정수 전용 텍스트박스, float/double→소수 텍스트박스,
    /// DateTime→날짜 선택, string→일반 텍스트박스, ObservableCollection&lt;T&gt;→그리드.
    /// 값은 JSON으로 자동 저장/복원된다(저장 버튼 클릭 시). RegisterViews가 아니라
    /// IAppSetup.RegisterSettings에서 호출한다.
    ///
    /// 사용 예:
    /// <code>
    /// manager.AddSettingsSub&lt;GeneralSettings&gt;(TabManager.SettingsTabKey, "일반");
    /// </code>
    /// </summary>
    void AddSettingsSub<TSettings>(string parentKey, string? title = null)
        where TSettings : class, new();
}

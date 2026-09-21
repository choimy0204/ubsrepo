using System;
using CommunityToolkit.Mvvm.ComponentModel;
using UbisamBase.Core.Modules;

namespace UbisamBase.Core.Shell;

/// <summary>
/// 하단 탭 바에 실제 콘텐츠 탭처럼 보이지만, 선택되는 순간 화면 전환 대신 동작을 실행하고
/// 원래 탭으로 되돌아가는 항목("바탕화면" 등). Title/Icon 프로퍼티 이름이 MainTabViewModel과
/// 같아서 같은 ItemContainerStyle을 그대로 재사용할 수 있다.
/// ObservableObject인 이유: 전체화면/창모드 토글처럼 상태에 따라 라벨·아이콘이 바뀌어야 하는
/// 항목이 있다 — Title/Icon이 그냥 get-only면 값을 바꿔도 화면(바인딩)이 갱신되지 않는다.
/// </summary>
public sealed partial class NavActionItem : ObservableObject
{
    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private Icon icon;

    public Action Execute { get; }

    public NavActionItem(string title, Icon icon, Action execute)
    {
        this.title = title;
        this.icon = icon;
        Execute = execute;
    }
}

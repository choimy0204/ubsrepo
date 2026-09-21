using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UbisamBase.Core.Modules;

/// <summary>
/// 좌측 사이드바에 표시되는 메인 탭 하나. AddMain으로 등록한 화면이 이 탭 고유의 기본 화면이 되고,
/// AddSub로 추가한 화면들은 하단 서브탭 바에 표시된다. 서브탭을 아무것도 선택하지 않은 기본
/// 상태에서는 메인 탭 자신의 화면(directContent)이 표시된다.
/// </summary>
public partial class MainTabViewModel : ObservableObject
{
    /// <summary>AddMain에 넘긴 key. TabManager.Activate(key)로 이 탭을 찾을 때 쓴다.</summary>
    public string Key { get; }

    public string Title { get; }

    public Icon Icon { get; }

    public int Order { get; }

    public ObservableCollection<SubTabViewModel> SubTabs { get; } = new();

    public bool HasSubTabs => SubTabs.Count > 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentContent))]
    private SubTabViewModel? selectedSubTab;

    private readonly Lazy<object> ownContent;

    public object CurrentContent => SelectedSubTab?.Content ?? ownContent.Value;

    /// <summary>이미 만들어진 경우에만 이 탭 자신의 화면을 돌려준다(아직 한 번도 안 열렸으면 null).
    /// 닫기 확인처럼 "열려 있는 화면에만" 물어봐야 하는 처리에 쓴다 — 여기서 Content를 읽어버리면
    /// 열지도 않은 화면이 그 순간 만들어진다.</summary>
    public object? CreatedContent => ownContent.IsValueCreated ? ownContent.Value : null;

    public MainTabViewModel(string key, string title, Icon icon, Func<object> contentFactory, int order = 0)
    {
        Key = key;
        Title = title;
        Icon = icon;
        Order = order;
        ownContent = new Lazy<object>(contentFactory);
    }

    internal void AddSubTab(SubTabViewModel subTab) => SubTabs.Add(subTab);
}

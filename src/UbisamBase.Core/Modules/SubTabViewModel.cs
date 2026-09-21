using System;

namespace UbisamBase.Core.Modules;

/// <summary>
/// 메인 탭 우측에 표시되는 서브탭 하나. Content는 처음 선택되는 시점에 딱 한 번 생성된다
/// (하드웨어 통신 등을 여는 ViewModel이 있을 수 있으므로 탭을 열기 전까지는 만들지 않는다).
/// </summary>
public class SubTabViewModel
{
    public string Title { get; }

    public int Order { get; }

    private readonly Lazy<object> content;

    public object Content => content.Value;

    /// <summary>이미 만들어진 경우에만 화면을 돌려준다(아직 안 열렸으면 null) — MainTabViewModel.CreatedContent와 같은 용도.</summary>
    public object? CreatedContent => content.IsValueCreated ? content.Value : null;

    public SubTabViewModel(string title, Func<object> contentFactory, int order = 0)
    {
        Title = title;
        Order = order;
        content = new Lazy<object>(contentFactory);
    }
}

using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UbisamBase.Core.Messaging;

public sealed partial class ToastMessage : ObservableObject
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Text { get; }
    public ToastType Type { get; }

    /// <summary>true가 되면 ShellWindow가 화면 밖으로 미끄러지는 퇴장 애니메이션을 재생한 뒤
    /// 컬렉션에서 실제로 제거한다 — ToastService.Show의 타이머가 표시 시간이 끝나면 켠다.</summary>
    [ObservableProperty]
    private bool isClosing;

    public ToastMessage(string text, ToastType type)
    {
        Text = text;
        Type = type;
    }
}

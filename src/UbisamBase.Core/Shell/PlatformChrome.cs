using System;
using System.ComponentModel;
using System.Windows;

namespace UbisamBase.Core.Shell;

/// <summary>
/// 플랫폼이 그리는 껍데기(상단 브랜딩 바 · 네비게이션 탭 바 · 서브탭 줄)를 통째로 보였다 숨겼다 하는 스위치.
/// 발표자 모드용 — <c>PlatformChrome.IsVisible = false;</c> 한 줄이면 모듈 화면만 꽉 차게 남는다.
///
/// UI 설정의 "상단 바 접기"와는 다른 개념이다. 그쪽은 사용자가 저장해 두는 취향이고, 이건 코드에서
/// 잠깐 껐다 켜는 모드라 파일에 저장하지 않는다 — 앱을 다시 켜면 항상 보이는 상태(true)로 시작한다.
/// (껍데기를 숨기면 종료·잠금 버튼도 같이 사라지므로, 다시 켤 방법을 모듈 쪽에 남겨 둬야 한다.)
///
/// 토스트 메시지, 화면 잠금, 스크린 세이버 오버레이는 이 스위치와 무관하게 그대로 뜬다.
/// </summary>
public static class PlatformChrome
{
    private static bool isVisible = true;

    /// <summary>true면 평소대로 전부 보이고, false면 플랫폼 껍데기가 전부 숨는다. 기본값 true.</summary>
    public static bool IsVisible
    {
        get => isVisible;
        set
        {
            if (isVisible == value)
            {
                return;
            }

            isVisible = value;
            // WPF가 정적 프로퍼티 바인딩을 갱신하려면 이 이름(StaticPropertyChanged)의 정적 이벤트여야 한다.
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsVisible)));
            VisibleChanged?.Invoke(null, value);
        }
    }

    /// <summary>보임/숨김을 뒤집는다 — 발표 중 단축키나 버튼에 걸어 쓰기 좋다.</summary>
    public static void Toggle() => IsVisible = !IsVisible;

    /// <summary>
    /// 모듈 화면에 붙이는 첨부 속성. True면 셸이 그 화면 둘레의 여백(26,22)을 주지 않는다.
    ///
    /// 편집기·지도·카메라처럼 화면을 꽉 쓰는 모듈은 자기 안에서 이미 칸을 나누고 각 칸이 자기 여백을
    /// 갖는다. 거기에 바깥 여백이 한 겹 더 붙으면 화면만 좁아지고 테두리처럼 보인다. 반대로 설정
    /// 화면이나 폼은 여백이 있어야 읽기 좋으므로, 기본은 여백 있음이고 필요한 모듈만 끈다.
    ///
    /// <code>&lt;UserControl ... shell:PlatformChrome.FullBleed="True"&gt;</code>
    /// </summary>
    public static readonly DependencyProperty FullBleedProperty =
        DependencyProperty.RegisterAttached(
            "FullBleed",
            typeof(bool),
            typeof(PlatformChrome),
            new PropertyMetadata(false, OnFullBleedChanged));

    public static void SetFullBleed(DependencyObject element, bool value)
        => element.SetValue(FullBleedProperty, value);

    public static bool GetFullBleed(DependencyObject element)
        => element != null && (bool)element.GetValue(FullBleedProperty);

    /// <summary>화면이 떠 있는 동안 값을 바꿔도 셸이 여백을 다시 계산하도록 알린다
    /// (첨부 속성은 셸의 바인딩에서 직접 감시할 수 없다).</summary>
    public static event EventHandler? FullBleedChanged;

    private static void OnFullBleedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => FullBleedChanged?.Invoke(d, EventArgs.Empty);

    /// <summary>보임/숨김이 바뀔 때마다 새 값(true=보임)과 함께 알린다. 모듈이 자기가 바꾸지 않은
    /// 변화(설정의 "상단 바 접기", 다른 모듈, 단축키)에도 반응할 수 있게 하는 통로다 —
    /// 발표 모드를 나갈 때 타이머를 멈추거나 띄워둔 창을 닫는 정리에 쓴다.
    /// 정적 이벤트라 구독을 남기면 객체가 살아남는다. 화면이 사라질 때 반드시 해제할 것.</summary>
    public static event EventHandler<bool>? VisibleChanged;

    public static event EventHandler<PropertyChangedEventArgs>? StaticPropertyChanged;
}

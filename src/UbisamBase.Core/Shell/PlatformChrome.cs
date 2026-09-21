using System;
using System.ComponentModel;

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

    /// <summary>보임/숨김이 바뀔 때마다 새 값(true=보임)과 함께 알린다. 모듈이 자기가 바꾸지 않은
    /// 변화(설정의 "상단 바 접기", 다른 모듈, 단축키)에도 반응할 수 있게 하는 통로다 —
    /// 발표 모드를 나갈 때 타이머를 멈추거나 띄워둔 창을 닫는 정리에 쓴다.
    /// 정적 이벤트라 구독을 남기면 객체가 살아남는다. 화면이 사라질 때 반드시 해제할 것.</summary>
    public static event EventHandler<bool>? VisibleChanged;

    public static event EventHandler<PropertyChangedEventArgs>? StaticPropertyChanged;
}

using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UbisamBase.Core.Locking;

/// <summary>
/// 화면 잠금 상태를 들고 있는 싱글턴. IsLocked를 ShellWindow가 바인딩해 LockOverlay를 덮는다.
/// 잠글 때마다 그 순간 입력한 아이디/비밀번호가 이번 잠금의 해제 키가 된다 —
/// LockConfirmDialog가 Lock() 직전에 Validator를 새로 갈아끼운다.
/// </summary>
public partial class LockService : ObservableObject
{
    public static LockService Current { get; } = new();

    public const int MaxAttempts = 3;

    private int attempts;

    [ObservableProperty]
    private bool isLocked;

    public int AttemptsRemaining => Math.Max(0, MaxAttempts - attempts);

    /// <summary>이번 잠금을 해제할 수 있는 조건. 기본값은 항상 거부 — LockConfirmDialog가
    /// Lock()을 부르기 직전에 실제 검증 델리게이트로 바꿔치기한다.</summary>
    public Func<string, string, bool> Validator { get; set; } = (_, _) => false;

    private LockService()
    {
    }

    public void Lock()
    {
        attempts = 0;
        OnPropertyChanged(nameof(AttemptsRemaining));
        IsLocked = true;
    }

    /// <summary>남은 시도 횟수가 없으면 검증 없이 바로 false. 맞으면 잠금을 풀고 시도 횟수를 초기화한다.</summary>
    public bool TryUnlock(string id, string password)
    {
        if (AttemptsRemaining <= 0)
        {
            return false;
        }

        if (Validator(id, password))
        {
            attempts = 0;
            OnPropertyChanged(nameof(AttemptsRemaining));
            IsLocked = false;
            return true;
        }

        attempts++;
        OnPropertyChanged(nameof(AttemptsRemaining));
        return false;
    }

    /// <summary>시도 횟수 초과로 잠긴 입력을 관리자가 다시 열어줄 때 쓴다.</summary>
    public void ResetAttempts()
    {
        attempts = 0;
        OnPropertyChanged(nameof(AttemptsRemaining));
    }

    /// <summary>비밀번호 확인 없이 즉시 잠금을 해제한다 — LockOverlay 구석의 "강제 잠금 해제" 버튼 전용.</summary>
    public void ForceUnlock()
    {
        attempts = 0;
        OnPropertyChanged(nameof(AttemptsRemaining));
        IsLocked = false;
    }
}

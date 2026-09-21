using System;
using System.Windows;
using System.Windows.Controls;

namespace UbisamBase.Core.Locking;

public partial class LockOverlay : UserControl
{
    public LockOverlay()
    {
        InitializeComponent();
    }

    /// <summary>ShellWindow가 LockService.IsLocked에 Visibility를 바인딩해두므로,
    /// Visible로 바뀌는 순간이 "잠금 걸림" 시점이다 — 입력칸을 비우고 아이디 칸에 포커스를 준다.</summary>
    private void LockOverlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (!IsVisible)
        {
            return;
        }

        IdBox.Text = string.Empty;
        PasswordBoxInput.Password = string.Empty;
        UpdateLockoutState();

        Dispatcher.BeginInvoke(new Action(() => IdBox.Focus()));
    }

    private void Unlock_Click(object sender, RoutedEventArgs e)
    {
        if (LockService.Current.AttemptsRemaining <= 0)
        {
            return;
        }

        if (LockService.Current.TryUnlock(IdBox.Text, PasswordBoxInput.Password))
        {
            return;
        }

        PasswordBoxInput.Password = string.Empty;
        PasswordBoxInput.Focus();
        UpdateLockoutState();
    }

    private void ForceUnlock_Click(object sender, RoutedEventArgs e) => LockService.Current.ForceUnlock();

    private void UpdateLockoutState()
    {
        var remaining = LockService.Current.AttemptsRemaining;

        if (remaining <= 0)
        {
            ErrorText.Text = "시도 횟수를 초과했습니다. 관리자에게 문의하세요.";
            ErrorText.Visibility = Visibility.Visible;
            IdBox.IsEnabled = false;
            PasswordBoxInput.IsEnabled = false;
            UnlockButton.IsEnabled = false;
            return;
        }

        IdBox.IsEnabled = true;
        PasswordBoxInput.IsEnabled = true;
        UnlockButton.IsEnabled = true;

        ErrorText.Visibility = remaining < LockService.MaxAttempts ? Visibility.Visible : Visibility.Collapsed;
        if (ErrorText.Visibility == Visibility.Visible)
        {
            ErrorText.Text = $"비밀번호가 올바르지 않습니다. 남은 시도: {remaining}회";
        }
    }
}

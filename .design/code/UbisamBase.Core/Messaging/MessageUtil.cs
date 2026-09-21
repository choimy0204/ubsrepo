using UbisamBase.Core.Logging;

namespace UbisamBase.Core.Messaging;

/// <summary>
/// 어디서나 바로 쓰는 대화상자 / Toast 헬퍼.
/// 시그니처는 이전과 완전히 동일하고, 대화상자만 OS 기본 MessageBox에서
/// 테마가 적용된 <see cref="MessageDialog"/>로 바뀌었다 — 호출하는 쪽은 고칠 것이 없다.
/// </summary>
public static class MessageUtil
{
    public static void ShowInfo(string title, string message)
        => MessageDialog.Show(title, message, MessageDialog.DialogKind.Info);

    public static void ShowError(string title, string message)
        => MessageDialog.Show(title, message, MessageDialog.DialogKind.Error);

    public static bool ShowYesNo(string title, string message)
        => MessageDialog.Show(title, message, MessageDialog.DialogKind.Confirm);

    public static void ShowSaveSuccessDialog() => ShowInfo("저장", "저장되었습니다.");

    public static void ShowSaveFailedDialog() => ShowError("저장 실패", "저장하지 못했습니다.");

    public static void ShowToast(string message, Logger? logger = null)
    {
        ToastService.Current.Show(message, ToastType.Info);
        logger?.I(message);
    }

    public static void ShowSuccessToast(string message = "완료되었습니다.", Logger? logger = null)
    {
        ToastService.Current.Show(message, ToastType.Success);
        logger?.I(message);
    }

    public static void ShowSavedToast(Logger? logger = null) => ShowSuccessToast("저장되었습니다.", logger);

    public static void ShowWarningToast(string message, Logger? logger = null)
    {
        ToastService.Current.Show(message, ToastType.Warning);
        logger?.W(message);
    }

    public static void ShowErrorToast(string message, Logger? logger = null)
    {
        ToastService.Current.Show(message, ToastType.Error);
        logger?.E(message);
    }
}

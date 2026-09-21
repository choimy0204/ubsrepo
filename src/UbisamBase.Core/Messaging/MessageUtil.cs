using UbisamBase.Core.Logging;

namespace UbisamBase.Core.Messaging;

/// <summary>어디서나 바로 쓰는 대화상자 / Toast 헬퍼. 확인/오류/예아니오는 OS MessageBox 대신
/// 테마 적용된 <see cref="MessageDialog"/>를 띄운다.</summary>
public static class MessageUtil
{
    public static void ShowInfo(string title, string message) => MessageDialog.ShowInfo(title, message);

    public static void ShowError(string title, string message) => MessageDialog.ShowError(title, message);

    public static bool ShowYesNo(string title, string message) => MessageDialog.ShowYesNo(title, message);

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

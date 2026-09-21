using System.Windows;
using System.Windows.Input;

namespace UbisamBase.Core.Shell;

/// <summary>
/// kiosk 전체화면에서 빠져나오는 탈출용 전역 단축키(Ctrl+Shift+F) — Window 타입에 클래스 핸들러로
/// 걸어서, 잠금 오버레이의 입력칸에 포커스가 있거나 모달 대화상자가 떠 있어도 항상 동작한다
/// (ButtonClickLogger와 같은 방식: 특정 창이 아니라 타입 전체에 건다).
/// </summary>
public static class FullscreenHotkeyInstaller
{
    private static bool installed;

    public static void Install()
    {
        if (installed)
        {
            return;
        }

        installed = true;
        EventManager.RegisterClassHandler(typeof(Window), UIElement.PreviewKeyDownEvent,
            new KeyEventHandler(OnPreviewKeyDown));
    }

    private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.F || Keyboard.Modifiers != (ModifierKeys.Control | ModifierKeys.Shift))
        {
            return;
        }

        ShellWindow.Instance?.ToggleFullscreen();
        e.Handled = true;
    }
}

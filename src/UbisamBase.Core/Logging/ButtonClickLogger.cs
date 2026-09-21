using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace UbisamBase.Core.Logging;

/// <summary>
/// 플랫폼 전체(그리고 이 플랫폼으로 만든 모든 프로그램)에서 버튼을 누르면 자동으로 로그를 남긴다.
/// EventManager.RegisterClassHandler로 ButtonBase.Click을 전역으로 가로채므로, 개발자가
/// 버튼마다 따로 로그 코드를 넣을 필요가 없다 — UbisamAppBuilder.Run이 부팅 시 한 번 건다.
/// </summary>
public static class ButtonClickLogger
{
    private static readonly Logger log = new("UI");
    private static bool installed;

    public static void Install()
    {
        if (installed)
        {
            return;
        }

        installed = true;
        EventManager.RegisterClassHandler(typeof(ButtonBase), ButtonBase.ClickEvent, new RoutedEventHandler(OnClick));
    }

    private static void OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        log.I($"버튼 클릭: {Describe(element)}");
    }

    private static string Describe(FrameworkElement element)
    {
        if (element is ContentControl { Content: string text } && !string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var automationName = AutomationProperties.GetName(element);
        if (!string.IsNullOrWhiteSpace(automationName))
        {
            return automationName;
        }

        if (!string.IsNullOrWhiteSpace(element.Name))
        {
            return element.Name;
        }

        return element.GetType().Name;
    }
}

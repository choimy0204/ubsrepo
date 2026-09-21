using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace UbisamBase.Core.Messaging;

/// <summary>
/// OS <see cref="MessageBox"/>를 대체하는 테마 적용 대화상자. WindowStyle="None"이라 OS 크롬이 없고,
/// 다크/라이트 테마에 맞춰 색이 바뀐다. <see cref="MessageUtil"/>이 이 클래스로 위임한다.
/// </summary>
public partial class MessageDialog : Window
{
    private enum DialogKind
    {
        Info,
        Question,
        Error
    }

    private bool result;

    private MessageDialog(string title, string message, DialogKind kind, bool showSecondary)
    {
        InitializeComponent();

        Title = title;
        TitleText.Text = title;
        MessageText.Text = message;

        var (iconKey, strokeKey) = kind switch
        {
            DialogKind.Error => ("Ubisam.Icon.Error", "Ubisam.Brush.Danger"),
            DialogKind.Question => ("Ubisam.Icon.Question", "Ubisam.Brush.ContentForeground"),
            _ => ("Ubisam.Icon.Info", "Ubisam.Brush.ContentForeground")
        };

        IconPath.Data = (Geometry)FindResource(iconKey);
        IconPath.Stroke = (Brush)FindResource(strokeKey);

        SecondaryButton.Visibility = showSecondary ? Visibility.Visible : Visibility.Collapsed;
        PrimaryButton.Content = showSecondary ? "예" : "확인";

        if (Application.Current?.MainWindow is { IsLoaded: true } main && !ReferenceEquals(main, this))
        {
            Owner = main;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        else
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    private void Primary_Click(object sender, RoutedEventArgs e)
    {
        result = true;
        DialogResult = true;
    }

    private void Secondary_Click(object sender, RoutedEventArgs e)
    {
        result = false;
        DialogResult = false;
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        result = false;
        DialogResult = false;
        e.Handled = true;
    }

    public static void ShowInfo(string title, string message)
        => new MessageDialog(title, message, DialogKind.Info, showSecondary: false).ShowDialog();

    public static void ShowError(string title, string message)
        => new MessageDialog(title, message, DialogKind.Error, showSecondary: false).ShowDialog();

    public static bool ShowYesNo(string title, string message)
    {
        var dialog = new MessageDialog(title, message, DialogKind.Question, showSecondary: true);
        dialog.ShowDialog();
        return dialog.result;
    }
}

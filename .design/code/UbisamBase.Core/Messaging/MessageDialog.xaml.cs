using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace UbisamBase.Core.Messaging;

/// <summary>
/// OS 기본 MessageBox 대신 쓰는 테마 적용 대화상자.
/// 직접 생성하지 말고 <see cref="MessageUtil"/>의 Info / Error / Confirm을 쓴다.
/// </summary>
public partial class MessageDialog : Window
{
    public enum DialogKind { Info, Error, Confirm }

    private MessageDialog(string title, string message, DialogKind kind)
    {
        InitializeComponent();

        Title = title;
        MessageText.Text = message;

        // 아이콘과 색은 종류에 따라. 오류만 붉은색을 쓰고 나머지는 accent.
        var iconKey = kind switch
        {
            DialogKind.Error => "Ubisam.Icon.Error",
            DialogKind.Confirm => "Ubisam.Icon.Question",
            _ => "Ubisam.Icon.Info"
        };
        var brushKey = kind == DialogKind.Error ? "Ubisam.Brush.Danger" : "Ubisam.Brush.Accent";

        Icon.Data = TryFindResource(iconKey) as Geometry;
        Icon.Stroke = TryFindResource(brushKey) as Brush;

        if (kind == DialogKind.Confirm)
        {
            OkButton.Content = "예";
            CancelButton.Content = "아니오";
            CancelButton.Visibility = Visibility.Visible;
            CancelButton.IsCancel = true;
        }

        // 소유 창 위에 가운데 정렬. 소유자가 없으면 화면 가운데.
        var owner = Application.Current?.MainWindow;
        if (owner != null && owner.IsLoaded && !ReferenceEquals(owner, this))
        {
            Owner = owner;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        else
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }

    /// <summary>확인 버튼을 눌렀으면 true. Confirm이 아닌 경우 항상 true.</summary>
    public static bool Show(string title, string message, DialogKind kind)
    {
        var dialog = new MessageDialog(title, message, kind);
        return dialog.ShowDialog() == true;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void Dialog_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
        }
    }

    private void Ok_Click(object sender, RoutedEventArgs e) => DialogResult = true;

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}

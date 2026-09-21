using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UbisamBase.Core.Logging;

public partial class LogViewerView : UserControl
{
    private LogEntry? rightClickedEntry;

    public LogViewerView()
    {
        InitializeComponent();
    }

    /// <summary>우클릭한 지점의 행을 기억해둔다(선택 상태로도 표시). ApplicationCommands.Copy의
    /// CanExecute 타이밍에 기대면 "우클릭 직후엔 아직 선택이 안 걸려서 복사가 씹히는" 문제가 있어서,
    /// 복사는 이 필드를 직접 써서 CopyMenuItem_Click에서 처리한다 — 커맨드 상태와 무관하게 항상 먹는다.</summary>
    private void LogGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var element = e.OriginalSource as DependencyObject;
        while (element != null && element is not DataGridRow)
        {
            element = VisualTreeHelper.GetParent(element);
        }

        if (element is DataGridRow row)
        {
            row.IsSelected = true;
            rightClickedEntry = row.Item as LogEntry;
        }
        else
        {
            rightClickedEntry = null;
        }
    }

    private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (rightClickedEntry is not { } entry)
        {
            return;
        }

        var text = $"{entry.Timestamp:HH:mm:ss.fff}\t{entry.Level}\t{entry.Tag}\t{entry.Message}";
        Clipboard.SetText(text);
    }
}

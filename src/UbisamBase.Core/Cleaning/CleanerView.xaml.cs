using System.Windows.Controls;

namespace UbisamBase.Core.Cleaning;

public partial class CleanerView : UserControl
{
    public CleanerView()
    {
        InitializeComponent();
    }

    /// <summary>경로/확장자/폴더삭제 열을 편집하면 그 규칙을 "미적용"으로 되돌린다 — 재검증 없이
    /// 예전 상태 그대로 자동 삭제가 계속되는 걸 막기 위한 안전장치(CleanerViewModel.MarkUnapplied).</summary>
    private void RulesGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.Row.Item is not CleanerRule rule)
        {
            return;
        }

        var header = e.Column.Header as string;
        if (header is "폴더 경로" or "확장자" or "폴더삭제" &&
            DataContext is CleanerViewModel vm)
        {
            vm.MarkUnapplied(rule);
        }
    }
}

using System.Windows;
using System.Windows.Controls;

namespace UbisamBase.Core.Themes;

/// <summary>
/// DataGrid에 붙이면 행 머리(RowHeader)에 1부터 시작하는 행 번호가 자동으로 채워진다.
/// 별도 열을 선언할 필요 없이 Controls.xaml의 DataGrid 기본 스타일에서 항상 켜둔다.
/// </summary>
public static class RowNumberBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(RowNumberBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid grid)
        {
            return;
        }

        grid.LoadingRow -= OnLoadingRow;

        if ((bool)e.NewValue)
        {
            grid.LoadingRow += OnLoadingRow;
        }
    }

    private static void OnLoadingRow(object sender, DataGridRowEventArgs e)
    {
        e.Row.Header = (e.Row.GetIndex() + 1).ToString();
    }
}

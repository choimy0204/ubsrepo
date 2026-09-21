using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace UbisamBase.Core.Themes;

/// <summary>
/// DataGrid에 붙이면 AutoGenerateColumns로 자동 생성되는 컬럼 헤더에 [DisplayName]을 반영한다.
/// WPF DataGrid는 AutoGenerateColumns="True"여도 DisplayNameAttribute를 자동으로 읽지 않고
/// 프로퍼티 이름을 그대로 헤더로 쓰기 때문에, 이 동작을 직접 붙여줘야 한다.
/// 별도 설정 없이 Controls.xaml의 DataGrid 기본 스타일에서 항상 켜둔다.
/// </summary>
public static class ColumnDisplayNameBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(ColumnDisplayNameBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid grid)
        {
            return;
        }

        grid.AutoGeneratingColumn -= OnAutoGeneratingColumn;

        if ((bool)e.NewValue)
        {
            grid.AutoGeneratingColumn += OnAutoGeneratingColumn;
        }
    }

    private static void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        if (e.PropertyDescriptor is not PropertyDescriptor descriptor)
        {
            return;
        }

        if (descriptor.Attributes[typeof(DisplayNameAttribute)] is DisplayNameAttribute { DisplayName: { Length: > 0 } displayName })
        {
            e.Column.Header = displayName;
        }
    }
}

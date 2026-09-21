using System;
using System.Globalization;
using System.Windows.Data;
using UbisamBase.Core.Shell;

namespace UbisamBase.Core.Converters;

/// <summary>하단 네비게이션 항목이 실제 탭(MainTabViewModel)이 아니라 동작형 항목(NavActionItem,
/// "바탕화면"/"종료" 등)인지. NavList의 ItemsPanel(DockPanel)에서 동작형 항목만 오른쪽으로
/// 붙이는 데 쓴다.</summary>
public class IsNavActionItemConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is NavActionItem;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

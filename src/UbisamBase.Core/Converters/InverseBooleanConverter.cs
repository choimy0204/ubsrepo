using System;
using System.Globalization;
using System.Windows.Data;

namespace UbisamBase.Core.Converters;

/// <summary>true/false를 뒤집는다. 두 개짜리 라디오 버튼에서 "아니오" 쪽을 같은 값에 묶을 때 쓴다.</summary>
public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => !(value is bool b && b);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => !(value is bool b && b);
}

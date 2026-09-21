using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UbisamBase.Core.Converters;

/// <summary>true/false에 각각 다른 여백을 준다. 리소스로 만들 때 True/False를 지정해 쓴다
/// (예: 발표자 모드에서 컨텐츠 여백을 0으로 만드는 BoolToContentPadding).</summary>
public class BoolToThicknessConverter : IValueConverter
{
    public Thickness True { get; set; }

    public Thickness False { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? True : False;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

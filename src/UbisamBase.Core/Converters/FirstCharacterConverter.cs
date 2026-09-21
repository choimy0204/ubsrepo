using System;
using System.Globalization;
using System.Windows.Data;

namespace UbisamBase.Core.Converters;

/// <summary>상단 브랜딩 배지에 쓰는, 문자열의 첫 글자를 대문자로 뽑아내는 컨버터.</summary>
public class FirstCharacterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string { Length: > 0 } text)
        {
            return char.ToUpperInvariant(text[0]).ToString();
        }

        return "?";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using UbisamBase.Core.Messaging;

namespace UbisamBase.Core.Converters;

/// <summary>토스트 종류 → 상태색. 색을 하드코딩하지 않고 현재 테마의 브러시 리소스를 그때그때 읽는다
/// (테마를 바꾸면 다음에 뜨는 토스트부터 반영).</summary>
public class ToastTypeToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var key = (value as ToastType?) switch
        {
            ToastType.Success => "Ubisam.Brush.Success",
            ToastType.Warning => "Ubisam.Brush.Warning",
            ToastType.Error => "Ubisam.Brush.Danger",
            _ => "Ubisam.Brush.Accent"
        };

        return Application.Current.Resources[key];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

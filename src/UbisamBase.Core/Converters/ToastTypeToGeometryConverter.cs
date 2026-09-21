using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using UbisamBase.Core.Messaging;

namespace UbisamBase.Core.Converters;

/// <summary>토스트 종류 → 아이콘. 색만으로 구분하면 색약 사용자가 성공/실패를 구별할 수 없어
/// 종류마다 도형도 다르게 쓴다.</summary>
public class ToastTypeToGeometryConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var key = (value as ToastType?) switch
        {
            ToastType.Success => "Ubisam.Icon.Pass",
            ToastType.Warning => "Ubisam.Icon.Warning",
            ToastType.Error => "Ubisam.Icon.Error",
            _ => "Ubisam.Icon.Info"
        };

        return (Geometry)Application.Current.Resources[key];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

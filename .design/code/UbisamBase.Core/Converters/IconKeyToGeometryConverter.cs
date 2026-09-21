using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace UbisamBase.Core.Converters;

/// <summary>
/// MainTabViewModel.Icon 문자열("Home", "Monitor" 등)을 Themes/Icons.xaml의
/// Geometry 리소스("Ubisam.Icon.Home")로 바꿔준다.
/// 값이 비어 있거나 없는 키면 null을 돌려주고, 하단 탭은 라벨만 표시한다.
/// </summary>
public sealed class IconKeyToGeometryConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string key || string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        var resourceKey = key.StartsWith("Ubisam.Icon.", StringComparison.Ordinal)
            ? key
            : "Ubisam.Icon." + key;

        return Application.Current?.TryFindResource(resourceKey) as Geometry;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

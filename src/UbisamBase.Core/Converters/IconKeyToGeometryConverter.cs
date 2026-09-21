using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using UbisamBase.Core.Modules;

namespace UbisamBase.Core.Converters;

/// <summary>
/// MainTabViewModel.Icon(Icon enum, 예: Icon.Home)을 Themes/Icons.xaml의
/// Geometry 리소스("Ubisam.Icon.Home")로 바꿔준다.
/// Icon.None이면 null을 돌려주고, 하단 탭은 라벨만 표시한다.
/// </summary>
public sealed class IconKeyToGeometryConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Icon icon || icon == Icon.None)
        {
            return null;
        }

        return Application.Current?.TryFindResource("Ubisam.Icon." + icon) as Geometry;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using UbisamBase.Core.Shell;

namespace UbisamBase.Core.Converters;

/// <summary>
/// 셸 컨텐츠 영역의 여백을 정한다. 값 두 개를 받는다 — (1) 플랫폼 껍데기가 보이는지,
/// (2) 지금 올라와 있는 모듈 화면.
///
/// 발표자 모드(껍데기 숨김)이거나 그 화면이 <see cref="PlatformChrome.FullBleedProperty"/>를 켜 뒀으면
/// 여백 0, 그 외에는 <see cref="Default"/>.
/// </summary>
public class ContentPaddingConverter : IMultiValueConverter
{
    /// <summary>평소 여백. ShellStyles.xaml에서 지정한다.</summary>
    public Thickness Default { get; set; }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var chromeVisible = values.Length > 0 && values[0] is bool visible && visible;
        if (!chromeVisible)
        {
            return new Thickness(0);
        }

        var content = values.Length > 1 ? values[1] as DependencyObject : null;
        return content != null && PlatformChrome.GetFullBleed(content) ? new Thickness(0) : Default;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

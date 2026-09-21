using System;
using System.Windows;
using System.Windows.Media;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;

namespace UbisamBase.Core.Charting;

/// <summary>
/// 차트(축·시리즈)를 만들어주는 정적 팩토리 — 색을 코드에 박지 않고 현재 테마의 브러시 리소스에서
/// 매번 읽는다. 화면마다 색을 다시 지정하지 말고 이 클래스만 써야 라이트/다크 전환에도 톤이 유지된다.
/// 테마가 바뀐 뒤에는 이미 만들어진 시리즈의 색은 그대로이므로, <see cref="NotifyThemeChanged"/>를
/// 구독해 시리즈를 다시 만들어야 한다.
/// </summary>
public static class ChartTheme
{
    private static readonly TimeSpan AnimationSpeed = TimeSpan.FromMilliseconds(260);

    public static event Action? ThemeChanged;

    public static void NotifyThemeChanged() => ThemeChanged?.Invoke();

    public static SKColor Accent => ReadColor("Ubisam.Brush.Accent");
    public static SKColor AccentFill => ReadColor("Ubisam.Brush.Accent.Fill");
    public static SKColor GridLine => ReadColor("Ubisam.Brush.Table.GridLine");
    public static SKColor Muted => ReadColor("Ubisam.Brush.Muted");
    public static SKColor Surface => ReadColor("Ubisam.Brush.Sidebar.Background");
    public static SKColor Text => ReadColor("Ubisam.Brush.ContentForeground");

    private static SKColor ReadColor(string resourceKey)
    {
        var color = ((SolidColorBrush)Application.Current.Resources[resourceKey]).Color;
        return new SKColor(color.R, color.G, color.B, color.A);
    }

    private static SKTypeface BodyTypeface => SKTypeface.FromFamilyName("Barlow") ?? SKTypeface.Default;

    private static SKTypeface HeadingTypeface => SKTypeface.FromFamilyName("Barlow Condensed") ?? SKTypeface.Default;

    /// <summary>측정값 선 — accent 밝은 단계, 두께 2, 보간 없음(LineSmoothness=0), 점 표시 없음, 면적 12%.</summary>
    public static ISeries Line(double[] values, string name) => new LineSeries<double>
    {
        Values = values,
        Name = name,
        Stroke = new SolidColorPaint(Accent, 2),
        Fill = new SolidColorPaint(Accent.WithAlpha(0x1F)),
        GeometrySize = 0,
        LineSmoothness = 0,
        AnimationsSpeed = AnimationSpeed
    };

    /// <summary>기준·목표선 — 회색 파선, 면적 없음, hover 비활성. 측정선보다 먼저 시리즈에 넣어야 아래에 깔린다.</summary>
    public static ISeries ReferenceLine(double[] values, string name) => new LineSeries<double>
    {
        Values = values,
        Name = name,
        Stroke = new SolidColorPaint(Muted, 1) { PathEffect = new DashEffect(new float[] { 5, 4 }) },
        Fill = null,
        GeometrySize = 0,
        LineSmoothness = 0,
        IsHoverable = false,
        AnimationsSpeed = AnimationSpeed
    };

    /// <summary>막대 — accent 깊은 단계 채움 85%, 각진 모서리.</summary>
    public static ISeries Column(double[] values, string name) => new ColumnSeries<double>
    {
        Values = values,
        Name = name,
        Fill = new SolidColorPaint(AccentFill.WithAlpha(217)),
        Rx = 0,
        Ry = 0,
        AnimationsSpeed = AnimationSpeed
    };

    /// <summary>0~100 가동률 도넛 게이지 — 값 조각(accent 채움) + 잔여 조각(격자선 색).</summary>
    public static ISeries[] Gauge(double value) => new ISeries[]
    {
        new PieSeries<double>
        {
            Values = new[] { value },
            Fill = new SolidColorPaint(AccentFill),
            InnerRadius = 40,
            MaxRadialColumnWidth = 46,
            AnimationsSpeed = AnimationSpeed
        },
        new PieSeries<double>
        {
            Values = new[] { 100 - value },
            Fill = new SolidColorPaint(GridLine),
            InnerRadius = 40,
            MaxRadialColumnWidth = 46,
            IsHoverable = false,
            AnimationsSpeed = AnimationSpeed
        }
    };

    /// <summary>가로축 — 세로 격자선 없음(SeparatorsPaint 미지정), 라벨 11pt Muted Barlow.</summary>
    public static Axis XAxis(Func<double, string>? labeler = null) => new Axis
    {
        Labeler = labeler != null ? new Func<double, string>(v => labeler(v)) : null!,
        TextSize = 11,
        LabelsPaint = new SolidColorPaint(Muted) { SKTypeface = BodyTypeface },
        SeparatorsPaint = null!
    };

    /// <summary>세로축 — 가로 격자선(GridLine, 헤어라인), 라벨 11pt Muted Barlow, 자릿수 고정 포맷.</summary>
    public static Axis YAxis(string format = "0", double? min = null, double? max = null) => new Axis
    {
        Labeler = v => v.ToString(format),
        MinLimit = min,
        MaxLimit = max,
        TextSize = 11,
        LabelsPaint = new SolidColorPaint(Muted) { SKTypeface = BodyTypeface },
        SeparatorsPaint = new SolidColorPaint(GridLine, 1)
    };
}

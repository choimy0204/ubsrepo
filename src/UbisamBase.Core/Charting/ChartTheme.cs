using System;
using System.Windows;
using System.Windows.Media;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
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

    // ─────────────────────────────────────────────────────────────────────────
    // 시리즈 색
    // 단일 계열 차트는 지금까지처럼 accent 하나만 쓴다. 여러 계열을 한 그래프에 그릴 때만
    // 아래 팔레트를 순서대로 쓴다 — 색을 화면마다 고르면 프로그램마다 톤이 달라진다.
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>계열 순번(0부터)에 해당하는 색. 6개를 넘어가면 처음으로 돌아간다.</summary>
    public static SKColor SeriesColor(int index)
        => ReadColor("Ubisam.Brush.Chart.Series" + ((Math.Abs(index) % 6) + 1));

    /// <summary>꺾은선 — 여러 계열용. 색만 팔레트에서 가져오고 나머지는 단일 계열과 같다(두께 2, 보간 없음).</summary>
    public static ISeries Line(double[] values, string name, int seriesIndex)
    {
        var color = SeriesColor(seriesIndex);
        return new LineSeries<double>
        {
            Values = values,
            Name = name,
            Stroke = new SolidColorPaint(color, 2),
            Fill = null,
            GeometrySize = 0,
            LineSmoothness = 0,
            AnimationsSpeed = AnimationSpeed
        };
    }

    /// <summary>부드러운 꺾은선 — 추세용. 공정 측정값처럼 "튄 지점"이 중요한 데이터에는 쓰지 말 것
    /// (보간이 실제로 없던 중간값을 지어낸다).</summary>
    public static ISeries SmoothLine(double[] values, string name, int seriesIndex = 0)
    {
        var color = SeriesColor(seriesIndex);
        return new LineSeries<double>
        {
            Values = values,
            Name = name,
            Stroke = new SolidColorPaint(color, 2),
            Fill = null,
            GeometrySize = 0,
            LineSmoothness = 0.65,
            AnimationsSpeed = AnimationSpeed
        };
    }

    /// <summary>면적 — 누적량·점유량처럼 "쌓인 양"을 보여줄 때. 겹쳐 그릴 수 있도록 채움을 24%로 둔다.</summary>
    public static ISeries Area(double[] values, string name, int seriesIndex = 0)
    {
        var color = SeriesColor(seriesIndex);
        return new LineSeries<double>
        {
            Values = values,
            Name = name,
            Stroke = new SolidColorPaint(color, 2),
            Fill = new SolidColorPaint(color.WithAlpha(0x3D)),
            GeometrySize = 0,
            LineSmoothness = 0,
            AnimationsSpeed = AnimationSpeed
        };
    }

    /// <summary>계단선 — ON/OFF나 단계값처럼 "다음 값까지 그 상태가 유지되는" 신호용.
    /// 이런 데이터를 꺾은선으로 그리면 없는 중간값이 있는 것처럼 보인다.</summary>
    public static ISeries StepLine(double[] values, string name, int seriesIndex = 0)
    {
        var color = SeriesColor(seriesIndex);
        return new StepLineSeries<double>
        {
            Values = values,
            Name = name,
            Stroke = new SolidColorPaint(color, 2),
            Fill = null,
            GeometrySize = 0,
            AnimationsSpeed = AnimationSpeed
        };
    }

    /// <summary>세로 막대 — 여러 계열용. 각진 모서리는 단일 계열과 같다.</summary>
    public static ISeries Column(double[] values, string name, int seriesIndex)
        => new ColumnSeries<double>
        {
            Values = values,
            Name = name,
            Fill = new SolidColorPaint(SeriesColor(seriesIndex).WithAlpha(217)),
            Rx = 0,
            Ry = 0,
            AnimationsSpeed = AnimationSpeed
        };

    /// <summary>누적 막대 — 전체 대비 구성비를 볼 때. 같은 차트에 넣은 누적 막대끼리 쌓인다.</summary>
    public static ISeries StackedColumn(double[] values, string name, int seriesIndex)
        => new StackedColumnSeries<double>
        {
            Values = values,
            Name = name,
            Fill = new SolidColorPaint(SeriesColor(seriesIndex).WithAlpha(217)),
            Rx = 0,
            Ry = 0,
            AnimationsSpeed = AnimationSpeed
        };

    /// <summary>가로 막대 — 항목 이름이 길어 세로 막대에서 라벨이 겹칠 때(설비명·불량 유형 등).</summary>
    public static ISeries Bar(double[] values, string name, int seriesIndex = 0)
        => new RowSeries<double>
        {
            Values = values,
            Name = name,
            Fill = new SolidColorPaint(SeriesColor(seriesIndex).WithAlpha(217)),
            Rx = 0,
            Ry = 0,
            AnimationsSpeed = AnimationSpeed
        };

    /// <summary>원형 — 구성비. 조각은 팔레트 순서대로, 경계는 판 색으로 얇게 갈라 서로 붙어 보이지 않게 한다.
    /// 조각이 6개를 넘으면 색이 돌아오므로 그때는 가로 막대(Bar)가 낫다.</summary>
    public static ISeries[] Pie(double[] values, string[] names = null)
        => BuildPie(values, names, innerRadius: 0);

    /// <summary>도넛 — 원형과 같되 가운데를 비워 총계·제목을 얹을 수 있다.</summary>
    public static ISeries[] Donut(double[] values, string[] names = null)
        => BuildPie(values, names, innerRadius: 52);

    private static ISeries[] BuildPie(double[] values, string[] names, double innerRadius)
    {
        var series = new ISeries[values.Length];

        for (var i = 0; i < values.Length; i++)
        {
            series[i] = new PieSeries<double>
            {
                Values = new[] { values[i] },
                Name = names != null && i < names.Length ? names[i] : string.Empty,
                Fill = new SolidColorPaint(SeriesColor(i)),
                Stroke = new SolidColorPaint(Surface, 2),
                InnerRadius = innerRadius,
                AnimationsSpeed = AnimationSpeed
            };
        }

        return series;
    }

    /// <summary>산점 — 두 값의 상관을 볼 때. 점이 겹쳐도 개수를 셀 수 있도록 판 색 테두리를 두른다.</summary>
    public static ISeries Scatter(ObservablePoint[] points, string name, int seriesIndex = 0)
    {
        var color = SeriesColor(seriesIndex);
        return new ScatterSeries<ObservablePoint>
        {
            Values = points,
            Name = name,
            Fill = new SolidColorPaint(color.WithAlpha(0xC8)),
            Stroke = new SolidColorPaint(Surface, 1),
            GeometrySize = 9,
            AnimationsSpeed = AnimationSpeed
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 툴팁 · 범례 — 차트 컨트롤에 직접 넣어야 하는 값들.
    // 지정하지 않으면 LiveCharts 기본(밝은 바탕 + 검은 글자)이 나와서 다크 테마에서 튄다.
    // ─────────────────────────────────────────────────────────────────────────

    public static SolidColorPaint TooltipTextPaint => new SolidColorPaint(Text) { SKTypeface = BodyTypeface };

    public static SolidColorPaint TooltipBackgroundPaint => new SolidColorPaint(Surface);

    public static SolidColorPaint LegendTextPaint => new SolidColorPaint(Muted) { SKTypeface = BodyTypeface };

    public static SolidColorPaint LegendBackgroundPaint => new SolidColorPaint(Surface);

    /// <summary>툴팁·범례 색을 이 차트에 입힌다. 차트를 만든 뒤와 테마가 바뀐 뒤에 한 번씩 불러주면 된다.</summary>
    public static void Apply(CartesianChart chart)
    {
        if (chart == null)
        {
            return;
        }

        chart.TooltipTextPaint = TooltipTextPaint;
        chart.TooltipBackgroundPaint = TooltipBackgroundPaint;
        chart.TooltipTextSize = 12;
        chart.LegendTextPaint = LegendTextPaint;
        chart.LegendBackgroundPaint = LegendBackgroundPaint;
        chart.LegendTextSize = 12;
    }

    /// <summary>원형·도넛 차트에도 같은 처리.</summary>
    public static void Apply(PieChart chart)
    {
        if (chart == null)
        {
            return;
        }

        chart.TooltipTextPaint = TooltipTextPaint;
        chart.TooltipBackgroundPaint = TooltipBackgroundPaint;
        chart.TooltipTextSize = 12;
        chart.LegendTextPaint = LegendTextPaint;
        chart.LegendBackgroundPaint = LegendBackgroundPaint;
        chart.LegendTextSize = 12;
    }
}

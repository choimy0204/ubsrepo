# 패치 10 — LiveCharts2 차트 테마

시안 <a>11a</a>(라인 트렌드 · 막대 · 게이지)를 LiveCharts2로 옮기는 코드입니다.
색을 코드에 박지 않고 **현재 테마의 브러시 리소스에서 읽으므로** 라이트/다크 전환을 그대로 따라갑니다.

## 0. 패키지

WPF용 LiveCharts2 하나만 있으면 됩니다 (SkiaSharp는 의존성으로 따라옵니다).

```
dotnet add src/UbisamBase.Core package LiveChartsCore.SkiaSharpView.WPF
```

`.NET Framework 4.8` 타깃이므로 LiveCharts2가 net48을 지원하는 버전인지 확인하세요. 안 되면 `net8.0-windows`로 올리거나, 차트만 별도 프로젝트로 분리하는 방법이 있습니다 — 어느 쪽인지 알려주시면 그에 맞춰 정리해 드립니다.

## 1. 새 파일

| 이 프로젝트 | 리포지토리 |
| --- | --- |
| `code/UbisamBase.Core/Charting/ChartTheme.cs` | `src/UbisamBase.Core/Charting/ChartTheme.cs` (신규) |

축·시리즈를 만들어주는 정적 팩토리입니다. 화면마다 색을 다시 지정하지 마세요 — 이 클래스만 쓰면 톤이 유지됩니다.

## 2. 디자인 규칙 (코드에 반영된 것)

| 요소 | 규칙 |
| --- | --- |
| 격자 | **가로만** 헤어라인 1px (`Ubisam.Brush.Table.GridLine`). 세로 격자는 그리지 않음 |
| 데이터 선 | accent 밝은 단계, 두께 2, `LineSmoothness = 0` — 값을 매끄럽게 왜곡하지 않음 |
| 면적 | accent 12% (`0x1F` 알파) |
| 점 | 기본은 표시하지 않음 (`GeometrySize = 0`) — 밀도가 낮을 때만 켜세요 |
| 기준·목표선 | 회색 파선 `5 4`, 면적 없음, hover 비활성 |
| 막대 | 깊은 단계 채움 85%, **각진 모서리** (`Rx = Ry = 0`) |
| 축 라벨 | 11pt, `Ubisam.Brush.Muted`, Barlow |
| 값 라벨 | Barlow Condensed, 자릿수 고정 포맷 |
| 애니메이션 | 260ms — 탭 인디케이터와 같은 속도 |

색을 늘리지 않고 굵기와 불투명도로 구분하는 것이 이 시스템의 규칙입니다. 시리즈가 여러 개 필요하면 알려주세요 — 색을 더 만들지 않고 구분하는 방법을 따로 정리하겠습니다.

## 3. 라인 트렌드

```xml
<lvc:CartesianChart Series="{Binding TrendSeries}"
                    XAxes="{Binding TrendXAxes}"
                    YAxes="{Binding TrendYAxes}"
                    TooltipPosition="Top"
                    Height="240"/>
```

```csharp
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using UbisamBase.Core.Charting;

public ISeries[] TrendSeries { get; }
public Axis[] TrendXAxes { get; }
public Axis[] TrendYAxes { get; }

public MonitorViewModel()
{
    var measured = new double[] { 121, 123, 119, 128, 126, 132, 130, 134 };
    var baseline = Enumerable.Repeat(125d, measured.Length).ToArray();

    TrendSeries = new ISeries[]
    {
        ChartTheme.ReferenceLine(baseline, "기준"),
        ChartTheme.Line(measured, "측정")
    };

    TrendXAxes = new[] { ChartTheme.XAxis(labeler: v => $"-{(7 - v) * 15:0}s") };
    TrendYAxes = new[] { ChartTheme.YAxis(format: "0", min: 100, max: 140) };
}
```

기준선을 먼저 넣어야 측정선이 위에 그려집니다.

## 4. 막대

```csharp
ProductionSeries = new ISeries[] { ChartTheme.Column(counts, "생산") };
ProductionXAxes = new[] { ChartTheme.XAxis(labeler: v => $"{8 + (int)v:00}") };
ProductionYAxes = new[] { ChartTheme.YAxis(format: "0", min: 0) };
```

시안처럼 최고값 막대만 진하게 하려면 `ColumnSeries`의 `Fill`을 하나로 두는 대신 값별 페인트를 주는 방식이 필요합니다 — 필요하시면 따로 만들어 드립니다.

## 5. 게이지

```xml
<lvc:PieChart Series="{Binding UtilizationSeries}"
              InitialRotation="-180"
              MaxAngle="180"
              MinValue="0"
              MaxValue="100"
              Height="150"/>
```

```csharp
UtilizationSeries = ChartTheme.Gauge(82.4);
```

**목표선(파선)은 LiveCharts2가 제공하지 않습니다.** 시안의 75% 틱은 차트 위에 `Path`를 하나 얹어서 그려야 합니다. 반지름 r, 중심 (cx, cy), 목표 p(0~1)일 때 끝점은

```
x = cx + r · cos(180° − p · 180°)   →  p=0.75 이면 cx + r·cos(45°)
y = cy − r · sin(180° − p · 180°)
```

시안 값(cx=95, cy=100, r=77, p=0.75)으로는 (149.4, 45.6)입니다. 필요하시면 이 계산을 하는 작은 UserControl로 만들어 드립니다.

## 6. 테마 전환 대응

`ChartTheme`은 호출 시점의 브러시를 읽습니다. 테마를 바꾼 뒤 이미 만들어진 차트는 색이 그대로이므로, `UiSettingsService`가 테마를 교체한 직후 시리즈를 다시 만들어야 합니다.

```csharp
// 테마 적용 코드 끝에
ChartTheme.NotifyThemeChanged();
```

차트를 가진 ViewModel에서 이 이벤트를 구독해 `TrendSeries` 등을 다시 만들고 `OnPropertyChanged`를 올리세요.

## 7. 툴팁·범례

LiveCharts2 기본 툴팁은 밝은 판이라 다크에서 튑니다. 차트마다 지정하세요.

```xml
<lvc:CartesianChart TooltipBackgroundPaint="{Binding TooltipBackground}"
                    TooltipTextPaint="{Binding TooltipText}"
                    LegendTextPaint="{Binding LegendText}"
                    LegendPosition="Top"/>
```

```csharp
public IPaint<SkiaSharpDrawingContext> TooltipBackground => new SolidColorPaint(ChartTheme.Surface);
public IPaint<SkiaSharpDrawingContext> TooltipText => new SolidColorPaint(ChartTheme.Text);
public IPaint<SkiaSharpDrawingContext> LegendText => new SolidColorPaint(ChartTheme.Muted);
```

시안처럼 범례를 직접 그리는 편이 더 깔끔합니다 — 차트 위에 선 샘플 + 라벨을 `StackPanel`로 두면 됩니다.

## 8. 확인

1. 라이트에서 격자가 가로만, 헤어라인으로 보이는지
2. 다크로 바꾸고 차트를 다시 만들었을 때 선이 #4DA6F5, 격자가 #383D44인지
3. 막대 모서리가 각진지 (둥글면 `Rx`/`Ry`가 안 먹은 것)
4. 측정선이 기준 파선 위에 그려지는지
5. 게이지 숫자가 Barlow Condensed로 크게 나오는지
6. 툴팁 배경이 테마에 맞는지

## 주의

- `LineSmoothness = 0`을 유지하세요. 곡선 보간은 측정값을 실제와 다르게 보이게 합니다.
- Barlow가 대상 PC에 없으면 SkiaSharp도 기본 서체로 폴백합니다 — 폰트 임베드를 하시려면 알려주세요.
- LiveCharts2는 버전에 따라 속성명이 조금씩 다릅니다(`IPaint`/`Paint`, `TicksPaint` 등). 빌드 오류가 나면 해당 줄과 패키지 버전을 알려주시면 맞춰 드립니다.

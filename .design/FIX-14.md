# 패치 14 — 차트가 흰 판 위에 그려지는 문제

적용 화면을 보니 차트 컨트롤 자체가 흰 배경으로 렌더되고 있습니다. 여기서 세 가지 증상이 한꺼번에 나옵니다.

| 증상 | 원인 |
| --- | --- |
| 다크 테마인데 차트 판만 흰색 | `CartesianChart` / `PieChart`의 기본 배경이 흰색이고, 테마 브러시를 상속하지 않음 |
| 격자선·게이지 남은 구간이 검게 보임 | 다크용 토큰(`#383D44`)을 흰 판에 그려서 — 색은 맞지만 판이 틀림 |
| 게이지 가운데 숫자(82.4)가 안 보임 | 값 라벨이 다크용 본문색(`#E8EAED`)인데 판이 흰색이라 묻힘 |

`ChartTheme`은 축·시리즈만 칠합니다. 컨트롤의 배경은 XAML에서 지정해야 합니다.

## 1. 차트에 배경을 지정하세요

```xml
<lvc:CartesianChart Series="{Binding TrendSeries}"
                    XAxes="{Binding TrendXAxes}"
                    YAxes="{Binding TrendYAxes}"
                    Background="Transparent"
                    Height="240"/>
```

`Background="Transparent"`가 핵심입니다. `PieChart`(게이지)도 같습니다.

## 2. 판은 셸의 도면 프레임으로 감싸세요

시안(<a>11a</a>)의 차트 패널은 흰 채움이 아니라 **선 드로잉**입니다 — 1px 헤어라인 + 네 모서리 등록 마크, 배경은 컨텐츠 바탕색 그대로.

```xml
<Border BorderBrush="{DynamicResource Ubisam.Brush.Input.Border}"
        BorderThickness="1"
        Background="Transparent"
        Padding="20,18,20,12">
    <StackPanel>
        <TextBlock Text="라인 트렌드"
                   FontFamily="{StaticResource Ubisam.Font.Heading}"
                   FontSize="19" FontWeight="SemiBold"
                   Foreground="{DynamicResource Ubisam.Brush.ContentForeground}"
                   Margin="0,0,0,16"/>
        <lvc:CartesianChart … Background="Transparent" Height="200"/>
    </StackPanel>
</Border>
```

등록 마크까지 넣으려면 `Ubisam.Style.Tag`처럼 스타일로 빼는 게 편합니다 — 필요하시면 `Ubisam.Style.BlueprintPanel`로 만들어 드립니다.

## 3. 테마를 바꾼 뒤에는 시리즈를 다시 만들어야 합니다

막대 색이 진한 파랑(`#1E7FD4`)이 아니라 밝은 파랑으로 보이는 것은, 차트가 만들어진 시점의 브러시를 그대로 들고 있기 때문입니다. `ChartTheme`은 **호출 시점**에 리소스를 읽습니다.

테마 적용 코드 끝에:

```csharp
ChartTheme.NotifyThemeChanged();
```

차트를 가진 ViewModel에서 구독해 다시 만드세요:

```csharp
public MonitorViewModel()
{
    Rebuild();
    ChartTheme.ThemeChanged += Rebuild;
}

private void Rebuild()
{
    TrendSeries = new ISeries[] { ChartTheme.ReferenceLine(baseline), ChartTheme.Line(measured) };
    TrendXAxes = new[] { ChartTheme.XAxis() };
    TrendYAxes = new[] { ChartTheme.YAxis(format: "0", min: 100, max: 140) };
    OnPropertyChanged(nameof(TrendSeries));
    OnPropertyChanged(nameof(TrendXAxes));
    OnPropertyChanged(nameof(TrendYAxes));
}
```

`ThemeChanged -= Rebuild`를 뷰가 사라질 때 해제하는 것을 잊지 마세요.

## 4. 툴팁·범례도 판을 따라야 합니다

LiveCharts2 기본 툴팁은 밝은 판입니다.

```xml
<lvc:CartesianChart TooltipBackgroundPaint="{Binding TooltipBackground}"
                    TooltipTextPaint="{Binding TooltipText}"
                    LegendTextPaint="{Binding LegendText}"
                    LegendPosition="Top"/>
```

## 5. 게이지에서 확인할 것

- 판이 어두워지면 남은 구간(`GridLine`)이 검정이 아니라 회색으로 읽힙니다
- 가운데 숫자가 나타납니다. 안 보이면 `DataLabelsPaint`가 `Text` 토큰인지 확인하세요
- 목표선(파선)은 LiveCharts2가 그려주지 않습니다 — 시안의 75% 틱은 차트 위에 `Path`를 얹어야 합니다. 계산식은 `FIX-10.md` §5에 있습니다

## 6. 확인

1. 다크 테마에서 차트 판이 배경과 같은 어두운 색인지
2. 격자선이 검은 실선이 아니라 헤어라인으로 보이는지
3. 막대가 진한 파랑(`#1E7FD4`)인지
4. 게이지 가운데 82.4가 보이는지
5. 라이트 테마로 바꾸고 화면을 다시 열었을 때 색이 라이트 토큰으로 바뀌는지
6. 차트 패널 테두리가 셸의 다른 판과 같은 1px 헤어라인인지

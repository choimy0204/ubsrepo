# 패치 03 — 하단 탭 모양 정리 + 선택 인디케이터 슬라이드 애니메이션

첨부하신 첫 번째 사진(아이콘 위 / 라벨 아래 / 라벨 아래 밑줄만) 기준으로 맞추고, 탭을 전환하면 그 밑줄이 선택한 탭으로 **미끄러져 이동**합니다. 화면 컨텐츠는 건드리지 않고 하단 탭 바에만 애니메이션이 들어갑니다.

이 패치는 패치 01·02를 포함해 대체합니다 — `ShellStyles.xaml`의 하단 탭 관련 두 스타일을 통째로 교체합니다.

## 1. 모양 (사진 1 기준)

- 칸 사이 세로 구분선 **제거**
- 선택 시 상단 3px 바 + 옅은 accent 판 → **바닥 3px 밑줄만**
- 선택 배경 틴트 **제거**, 선택된 탭은 글자·아이콘 색만 accent
- 마우스 오버는 선택되지 않은 탭에만 옅은 틴트
- 탭은 가운데 모임(균등 분할 아님), 아이템 패딩 `30,13,30,14`

## 2. `src/UbisamBase.Core/Themes/ShellStyles.xaml`

`Ubisam.Style.SidebarItem.Horizontal`과 `Ubisam.Style.SidebarBar.Horizontal`을 아래 두 스타일로 교체하세요.
`SidebarBar.Horizontal`에는 `ControlTemplate`이 새로 들어갑니다 — 그 안의 `SelectionIndicator` / `SelectionIndicatorShift` 이름을 코드비하인드가 찾습니다. **이름을 바꾸지 마세요.**

```xml
<Style x:Key="Ubisam.Style.SidebarItem.Horizontal" TargetType="ListBoxItem">
    <Setter Property="Padding" Value="30,13,30,14"/>
    <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Sidebar.Foreground}"/>
    <Setter Property="FontFamily" Value="{StaticResource Ubisam.Font.Body}"/>
    <Setter Property="FontSize" Value="13.5"/>
    <Setter Property="FontWeight" Value="Medium"/>
    <Setter Property="Cursor" Value="Hand"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="ListBoxItem">
                <Border x:Name="Bd" Background="Transparent" Padding="{TemplateBinding Padding}">
                    <StackPanel Orientation="Vertical" HorizontalAlignment="Center" VerticalAlignment="Center">
                        <Path x:Name="Ico"
                              Data="{Binding Icon, Converter={StaticResource IconToGeometry}}"
                              Stroke="{Binding Foreground, RelativeSource={RelativeSource TemplatedParent}}"
                              StrokeThickness="1.5"
                              StrokeStartLineCap="Round" StrokeEndLineCap="Round" StrokeLineJoin="Round"
                              Width="22" Height="22" Stretch="Uniform"
                              Margin="0,0,0,7" HorizontalAlignment="Center"/>
                        <TextBlock Text="{Binding Title}" HorizontalAlignment="Center" TextTrimming="CharacterEllipsis"/>
                    </StackPanel>
                </Border>
                <ControlTemplate.Triggers>
                    <DataTrigger Binding="{Binding Icon, Converter={StaticResource IconToGeometry}}" Value="{x:Null}">
                        <Setter TargetName="Ico" Property="Visibility" Value="Collapsed"/>
                    </DataTrigger>
                    <Trigger Property="IsSelected" Value="True">
                        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Sidebar.SelectedForeground}"/>
                    </Trigger>
                    <MultiTrigger>
                        <MultiTrigger.Conditions>
                            <Condition Property="IsMouseOver" Value="True"/>
                            <Condition Property="IsSelected" Value="False"/>
                        </MultiTrigger.Conditions>
                        <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.HoverBackground}"/>
                    </MultiTrigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>

<Style x:Key="Ubisam.Style.SidebarBar.Horizontal" TargetType="ListBox">
    <Setter Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.Background}"/>
    <Setter Property="BorderThickness" Value="0,1,0,0"/>
    <Setter Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.SubTabBar.Border}"/>
    <Setter Property="Padding" Value="0"/>
    <Setter Property="ItemContainerStyle" Value="{StaticResource Ubisam.Style.SidebarItem.Horizontal}"/>
    <Setter Property="ScrollViewer.HorizontalScrollBarVisibility" Value="Disabled"/>
    <Setter Property="ScrollViewer.VerticalScrollBarVisibility" Value="Disabled"/>
    <Setter Property="ItemsPanel">
        <Setter.Value>
            <ItemsPanelTemplate>
                <StackPanel Orientation="Horizontal" HorizontalAlignment="Center"/>
            </ItemsPanelTemplate>
        </Setter.Value>
    </Setter>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="ListBox">
                <Border Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}">
                    <Grid>
                        <ItemsPresenter/>
                        <Border x:Name="SelectionIndicator"
                                Height="3" Width="0"
                                HorizontalAlignment="Left" VerticalAlignment="Bottom"
                                Background="{DynamicResource Ubisam.Brush.Accent}">
                            <Border.RenderTransform>
                                <TranslateTransform x:Name="SelectionIndicatorShift" X="0"/>
                            </Border.RenderTransform>
                        </Border>
                    </Grid>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

## 3. `src/UbisamBase.Core/Shell/ShellWindow.xaml.cs` — 애니메이션

using 추가:

```csharp
using System.Windows.Media.Animation;
```

`ShellWindow` 생성자 마지막에 이벤트 두 개를 붙이세요.

```csharp
NavList.SelectionChanged += (_, _) => MoveTabIndicator(animate: true);
NavList.Loaded += (_, _) => MoveTabIndicator(animate: false);
NavList.SizeChanged += (_, _) => MoveTabIndicator(animate: false);
```

그리고 메서드를 추가하세요.

```csharp
/// <summary>하단 탭의 선택 인디케이터를 선택된 탭 아래로 옮긴다. 좌/우 네비게이션일 때는 인디케이터가 없어 무시된다.</summary>
private void MoveTabIndicator(bool animate)
{
    if (NavList.Template?.FindName("SelectionIndicator", NavList) is not Border indicator ||
        NavList.Template.FindName("SelectionIndicatorShift", NavList) is not TranslateTransform shift)
    {
        return;
    }

    if (NavList.SelectedItem == null ||
        NavList.ItemContainerGenerator.ContainerFromItem(NavList.SelectedItem) is not ListBoxItem container ||
        !container.IsArrangeValid)
    {
        indicator.Width = 0;
        return;
    }

    var origin = container.TranslatePoint(new Point(0, 0), NavList);
    var targetX = origin.X;
    var targetWidth = container.ActualWidth;

    if (!animate)
    {
        shift.BeginAnimation(TranslateTransform.XProperty, null);
        indicator.BeginAnimation(FrameworkElement.WidthProperty, null);
        shift.X = targetX;
        indicator.Width = targetWidth;
        return;
    }

    var duration = new Duration(TimeSpan.FromMilliseconds(260));
    var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

    shift.BeginAnimation(TranslateTransform.XProperty,
        new DoubleAnimation(targetX, duration) { EasingFunction = ease, FillBehavior = FillBehavior.HoldEnd });
    indicator.BeginAnimation(FrameworkElement.WidthProperty,
        new DoubleAnimation(targetWidth, duration) { EasingFunction = ease, FillBehavior = FillBehavior.HoldEnd });
}
```

`ApplyNavPosition`에서 위치를 바꾼 뒤에도 한 번 호출해 주세요 (좌/우 → 하단 전환 시 위치 재계산).

```csharp
NavList.Width = isHorizontal ? double.NaN : 140;
NavList.Height = double.NaN;

if (isHorizontal)
{
    Dispatcher.BeginInvoke(new Action(() => MoveTabIndicator(animate: false)));
}
```

## 4. 확인

1. 탭을 누르면 밑줄이 260ms에 걸쳐 미끄러져 오고, 너비도 새 탭 폭에 맞게 함께 변하는지
2. 앱을 처음 켰을 때는 애니메이션 없이 첫 탭 아래에 바로 놓이는지
3. 창 크기를 바꾸거나 최대화할 때 밑줄이 어긋나지 않는지
4. 설정에서 네비게이션 위치를 좌/우로 바꿨다가 하단으로 되돌렸을 때 밑줄이 정상 위치인지
5. 권한에 따라 탭이 필터링(`navView.Refresh()`)된 뒤에도 밑줄이 맞는지 — 어긋나면 `OnAuthChanged` 끝에 `MoveTabIndicator(animate: false)`를 한 줄 추가하세요

## 참고

- 260ms가 느리면 `TimeSpan.FromMilliseconds(160)`으로 줄이세요.
- 밑줄을 라벨 폭만큼만 짧게 하고 싶으면 `targetWidth`를 `container.ActualWidth - 40` 정도로 줄이고 `targetX`에 `20`을 더하면 됩니다.

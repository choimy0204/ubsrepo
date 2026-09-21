# 다크모드 색상 + 입력 컨트롤 디자인 — 통합 지시서

이 문서 전체를 Claude Code에 붙여넣고 "이 지시서대로 적용해줘"라고 요청하세요.
**패치 04 ~ 08을 하나로 합친 것입니다** — 이것만 적용하면 됩니다.

---

당신은 `UbisamBase` WPF 솔루션(.NET Framework 4.8, MVVM + CommunityToolkit.Mvvm)에서 작업합니다.
아래 §3의 파일은 전문이 주어져 있으니 **그대로 덮어쓰거나 새로 만드세요.** 값을 임의로 재해석하지 마세요.

## 1. 다크모드 색상 규칙

검정 바탕이 아니라 **차가운 회색 3단 층**입니다. 층이 밝아지는 순서가 구조를 만듭니다.

| 층 | 값 | 무엇 |
| --- | --- | --- |
| 바탕 | `#23262B` | 컨텐츠 영역 |
| 판 | `#2A2E34` | 상단바 · 하단 탭 바 · 서브탭 바 |
| 입력 | `#2F343A` | TextBox · ComboBox · 표 헤더 |
| 경계 | `#3A3F47` (셸) / `#454B54` (입력) | 헤어라인 1px |
| 본문 글자 | `#E8EAED` · 보조 `#C2C7CE` · 흐린 `#8A9099` | |

**accent는 두 단계로 씁니다.** 이 구분이 핵심입니다.

| 역할 | 다크 | 라이트 | 쓰는 곳 |
| --- | --- | --- | --- |
| `Ubisam.Brush.Accent` | `#4DA6F5` | `#2196F3` | 선 · 글자 · 탭 밑줄 · 포커스 테두리 — 바탕 위에 얹히는 것 |
| `Ubisam.Brush.Accent.Fill` | `#1E7FD4` | `#1D82E8` | **글자를 얹는 채움면** — 주 버튼, 로고 타일, 선택된 항목, 체크박스 |
| `Ubisam.Brush.Accent.OnFill` | `#F2F7FC` | `#FFFFFF` | 그 채움면 위의 글자 · 체크 표시 |

밝은 단계(`#4DA6F5`)에 검은 글자도, 흰 글자도 얹지 마세요 — 대비가 부족합니다. 글자가 올라가는 면은 항상 깊은 단계(`#1E7FD4`, 흰 글자와 약 4.2:1)를 씁니다.

## 2. 컨트롤 디자인 규칙

셸과 같은 도면 톤입니다.

- **각진 모서리** — 라운드 없음. 알약형 세그먼트, 둥근 버튼 금지
- **1px 헤어라인 경계** — 두꺼운 테두리나 그림자로 층을 만들지 않음
- **포커스** = accent 테두리 + 옅은 글로우(BlurRadius 6, 55%). 브라우저/OS 기본 포커스 링 사용 금지
- **비활성** = 45% 불투명도 또는 어두운 배경 + 흐린 글자
- **높이 34** 통일 (TextBox · ComboBox · Button)
- **상태 열은 태그로** — 판정·레벨 같은 열을 맨 텍스트로 두면 예외 행이 안 보입니다
- **수치 열은 우측 정렬** + 자릿수 고정(`tabular-nums`)

컨트롤별로:

| 컨트롤 | 디자인 |
| --- | --- |
| TextBox | 어두운 입력면, 포커스 시 accent 테두리 + 글로우, 읽기 전용은 어두운 배경 |
| ComboBox | 각진 닫힘 상태 + 우측 화살표(열리면 뒤집힘 + accent), 드롭다운은 입력창 **바로 아래에 붙고** 위 테두리 없음, 선택 항목은 `Accent.Fill` + 흰 글자 |
| CheckBox | 17×17 각진 박스, 체크 시 `Accent.Fill` + 흰 체크 |
| Button | 암시적 = 보조(외곽선). 주 동작은 `Ubisam.Style.Button.Primary`(`Accent.Fill` 채움), 세 번째는 `.Ghost` |
| ListView + GridView | 읽기 전용 표. 헤더 회색 판 + 세로 구분선, 행 hover/선택 accent 틴트 |
| DataGrid | 편집 가능한 격자. 정렬 열은 accent 글자 + 화살표, 행 머리(26px)가 선택 시 accent 바, 편집 중인 셀만 accent 테두리, 짝수 행 옅은 줄무늬 |
| Slider | 얇은 홈 트랙 + 지나온 구간만 accent, 썸은 각진 10×20 세로 막대(둥근 다이얼 아님) |
| ScrollBar | 폭 10, 트랙 비움, 썸만 회색 → hover 시 accent, 화살표 버튼 없음 |

전부 **암시적 스타일**(`x:Key` 없음)이라 각 View를 고칠 필요가 없습니다. 예외를 두려면 그 컨트롤에 `Style="{x:Null}"`을 주세요.

## 3. 파일 전문


### `src/UbisamBase.Core/Themes/Colors.Dark.xaml`

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- 다크: 검정이 아니라 차가운 회색 바탕 + 회사 하늘색.
         판(#2A2E34)이 바탕(#23262B)보다 한 단계 밝아 층이 구분된다.
         accent는 두 단계로 쓴다 — 선·글자·밑줄은 밝은 #4DA6F5, 글자를 얹는 채움면은
         깊은 #1E7FD4 + 흰 글자(#F2F7FC). 밝은 단계에 검은 글자를 얹지 않는다. -->
    <SolidColorBrush x:Key="Ubisam.Brush.ContentBackground" Color="#FF23262B"/>
    <SolidColorBrush x:Key="Ubisam.Brush.ContentForeground" Color="#FFE8EAED"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Accent" Color="#FF4DA6F5"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Accent.Fill" Color="#FF1E7FD4"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Accent.OnFill" Color="#FFF2F7FC"/>

    <SolidColorBrush x:Key="Ubisam.Brush.Sidebar.Background" Color="#FF2A2E34"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Sidebar.Foreground" Color="#FFC2C7CE"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Sidebar.HoverBackground" Color="#16FFFFFF"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Sidebar.SelectedBackground" Color="#1A4DA6F5"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Sidebar.SelectedForeground" Color="#FF4DA6F5"/>

    <SolidColorBrush x:Key="Ubisam.Brush.SubTabBar.Background" Color="#FF2A2E34"/>
    <SolidColorBrush x:Key="Ubisam.Brush.SubTabBar.Border" Color="#FF3A3F47"/>
    <SolidColorBrush x:Key="Ubisam.Brush.SubTabBar.Foreground" Color="#FFC2C7CE"/>
    <SolidColorBrush x:Key="Ubisam.Brush.SubTabBar.HoverBackground" Color="#16FFFFFF"/>
    <SolidColorBrush x:Key="Ubisam.Brush.SubTabBar.SelectedBackground" Color="#FF1E7FD4"/>
    <SolidColorBrush x:Key="Ubisam.Brush.SubTabBar.SelectedForeground" Color="#FFF2F7FC"/>

    <!-- 입력 컨트롤 -->
    <SolidColorBrush x:Key="Ubisam.Brush.Input.Background" Color="#FF2F343A"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Input.Border" Color="#FF454B54"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Input.BorderHover" Color="#FF5A616B"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Input.DisabledBackground" Color="#FF292D32"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Input.DisabledBorder" Color="#FF383D44"/>
    <Color x:Key="Ubisam.Color.AccentGlow">#FF4DA6F5</Color>

    <!-- 표 -->
    <SolidColorBrush x:Key="Ubisam.Brush.Table.HeaderBackground" Color="#FF2F343A"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Table.RowBorder" Color="#FF383D44"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Table.GridLine" Color="#FF383D44"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Table.AltRowBackground" Color="#FF333840"/>

    <!-- 상태 태그 -->
    <SolidColorBrush x:Key="Ubisam.Brush.Tag.AccentBackground" Color="#FF123A5C"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Tag.AccentForeground" Color="#FFA8D4FA"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Tag.NeutralBackground" Color="#FF383D44"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Tag.NeutralForeground" Color="#FFC2C7CE"/>

    <SolidColorBrush x:Key="Ubisam.Brush.Slot.Hatch" Color="#1A4DA6F5"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Muted" Color="#FF8A9099"/>

</ResourceDictionary>
```

### `src/UbisamBase.Core/Themes/Colors.Light.xaml`

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- 라이트: 기술 도면 톤의 밝은 바탕 + 회사 하늘색 accent -->
    <SolidColorBrush x:Key="Ubisam.Brush.ContentBackground" Color="#FFF2F2F3"/>
    <SolidColorBrush x:Key="Ubisam.Brush.ContentForeground" Color="#FF1D1F20"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Accent" Color="#FF2196F3"/>
    <!-- 글자를 얹는 채움면용 깊은 단계 + 그 위 글자색 (다크와 키를 공유) -->
    <SolidColorBrush x:Key="Ubisam.Brush.Accent.Fill" Color="#FF1D82E8"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Accent.OnFill" Color="#FFFFFFFF"/>

    <!-- 상단 브랜딩 바 = 흰 판, 하단 탭 바와 같은 계열 -->
    <SolidColorBrush x:Key="Ubisam.Brush.Sidebar.Background" Color="#FFFFFFFF"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Sidebar.Foreground" Color="#FF5D5D60"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Sidebar.HoverBackground" Color="#14000000"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Sidebar.SelectedBackground" Color="#122196F3"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Sidebar.SelectedForeground" Color="#FF2196F3"/>

    <SolidColorBrush x:Key="Ubisam.Brush.SubTabBar.Background" Color="#FFFFFFFF"/>
    <SolidColorBrush x:Key="Ubisam.Brush.SubTabBar.Border" Color="#FFD6D6D9"/>
    <SolidColorBrush x:Key="Ubisam.Brush.SubTabBar.Foreground" Color="#FF5D5D60"/>
    <SolidColorBrush x:Key="Ubisam.Brush.SubTabBar.HoverBackground" Color="#14000000"/>
    <SolidColorBrush x:Key="Ubisam.Brush.SubTabBar.SelectedBackground" Color="#FF2196F3"/>
    <SolidColorBrush x:Key="Ubisam.Brush.SubTabBar.SelectedForeground" Color="#FFFFFFFF"/>

    <!-- 입력 컨트롤 -->
    <SolidColorBrush x:Key="Ubisam.Brush.Input.Background" Color="#FFFFFFFF"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Input.Border" Color="#FFC9C9CC"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Input.BorderHover" Color="#FFA8A8AC"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Input.DisabledBackground" Color="#FFEDEDEF"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Input.DisabledBorder" Color="#FFDEDEE1"/>
    <Color x:Key="Ubisam.Color.AccentGlow">#FF2196F3</Color>

    <!-- 표 -->
    <SolidColorBrush x:Key="Ubisam.Brush.Table.HeaderBackground" Color="#FFEDEDEF"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Table.RowBorder" Color="#FFEDEDEF"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Table.GridLine" Color="#FFDEDEE1"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Table.AltRowBackground" Color="#FFF8F8F9"/>

    <!-- 상태 태그 -->
    <SolidColorBrush x:Key="Ubisam.Brush.Tag.AccentBackground" Color="#FFDCEBFA"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Tag.AccentForeground" Color="#FF0B4C7E"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Tag.NeutralBackground" Color="#FFEDEDEF"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Tag.NeutralForeground" Color="#FF3F3F42"/>

    <!-- 빈 뷰 슬롯의 사선 해치 색 -->
    <SolidColorBrush x:Key="Ubisam.Brush.Slot.Hatch" Color="#172196F3"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Muted" Color="#FF98989B"/>

</ResourceDictionary>
```

### `src/UbisamBase.Core/Themes/Controls.xaml`

```xml
<!-- 입력 컨트롤 · 표 스타일. 전부 암시적 스타일(x:Key 없음)이라
     각 View를 고치지 않아도 셸 안의 모든 TextBox/ComboBox/CheckBox/Button/ListBox/ListView에 적용된다.
     특정 위치만 예외로 두고 싶으면 그 컨트롤에 Style="{x:Null}"을 주면 된다. -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- ══ TextBox ══ -->
    <Style TargetType="TextBox">
        <Setter Property="MinHeight" Value="34"/>
        <Setter Property="Padding" Value="10,0"/>
        <Setter Property="FontFamily" Value="{DynamicResource Ubisam.Font.Body}"/>
        <Setter Property="FontSize" Value="13.5"/>
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.ContentForeground}"/>
        <Setter Property="CaretBrush" Value="{DynamicResource Ubisam.Brush.Accent}"/>
        <Setter Property="SelectionBrush" Value="{DynamicResource Ubisam.Brush.Accent}"/>
        <Setter Property="VerticalContentAlignment" Value="Center"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="TextBox">
                    <Border x:Name="Bd"
                            Background="{DynamicResource Ubisam.Brush.Input.Background}"
                            BorderBrush="{DynamicResource Ubisam.Brush.Input.Border}"
                            BorderThickness="1"
                            SnapsToDevicePixels="True">
                        <ScrollViewer x:Name="PART_ContentHost"
                                      Margin="{TemplateBinding Padding}"
                                      VerticalAlignment="Center"
                                      Focusable="False"
                                      HorizontalScrollBarVisibility="Hidden"
                                      VerticalScrollBarVisibility="Hidden"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="Bd" Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.Input.BorderHover}"/>
                        </Trigger>
                        <Trigger Property="IsKeyboardFocusWithin" Value="True">
                            <Setter TargetName="Bd" Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.Accent}"/>
                            <Setter TargetName="Bd" Property="Effect">
                                <Setter.Value>
                                    <DropShadowEffect Color="{DynamicResource Ubisam.Color.AccentGlow}"
                                                      BlurRadius="6" ShadowDepth="0" Opacity="0.55"/>
                                </Setter.Value>
                            </Setter>
                        </Trigger>
                        <MultiTrigger>
                            <MultiTrigger.Conditions>
                                <Condition Property="IsEnabled" Value="False"/>
                            </MultiTrigger.Conditions>
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Input.DisabledBackground}"/>
                            <Setter TargetName="Bd" Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.Input.DisabledBorder}"/>
                            <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Muted}"/>
                        </MultiTrigger>
                        <Trigger Property="IsReadOnly" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Input.DisabledBackground}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- ══ ComboBox ══ -->
    <Style x:Key="Ubisam.Style.ComboToggle" TargetType="ToggleButton">
        <Setter Property="Focusable" Value="False"/>
        <Setter Property="ClickMode" Value="Press"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="ToggleButton">
                    <Border x:Name="Bd"
                            Background="{DynamicResource Ubisam.Brush.Input.Background}"
                            BorderBrush="{DynamicResource Ubisam.Brush.Input.Border}"
                            BorderThickness="1"
                            SnapsToDevicePixels="True">
                        <Path x:Name="Arrow"
                              Data="M 0,0 L 5,5 L 10,0"
                              Stroke="{DynamicResource Ubisam.Brush.Sidebar.Foreground}"
                              StrokeThickness="1.5"
                              StrokeStartLineCap="Round"
                              StrokeEndLineCap="Round"
                              StrokeLineJoin="Round"
                              HorizontalAlignment="Right"
                              VerticalAlignment="Center"
                              Margin="0,0,11,0"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="Bd" Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.Input.BorderHover}"/>
                        </Trigger>
                        <Trigger Property="IsChecked" Value="True">
                            <Setter TargetName="Bd" Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.Accent}"/>
                            <Setter TargetName="Arrow" Property="Stroke" Value="{DynamicResource Ubisam.Brush.Accent}"/>
                            <Setter TargetName="Arrow" Property="RenderTransformOrigin" Value="0.5,0.5"/>
                            <Setter TargetName="Arrow" Property="RenderTransform">
                                <Setter.Value>
                                    <ScaleTransform ScaleY="-1"/>
                                </Setter.Value>
                            </Setter>
                        </Trigger>
                        <Trigger Property="IsEnabled" Value="False">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Input.DisabledBackground}"/>
                            <Setter TargetName="Bd" Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.Input.DisabledBorder}"/>
                            <Setter TargetName="Arrow" Property="Stroke" Value="{DynamicResource Ubisam.Brush.Muted}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style TargetType="ComboBoxItem">
        <Setter Property="Padding" Value="11,8"/>
        <Setter Property="FontFamily" Value="{DynamicResource Ubisam.Font.Body}"/>
        <Setter Property="FontSize" Value="13.5"/>
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.ContentForeground}"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="ComboBoxItem">
                    <Border x:Name="Bd" Background="Transparent" Padding="{TemplateBinding Padding}">
                        <ContentPresenter VerticalAlignment="Center"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.SelectedBackground}"/>
                            <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Sidebar.SelectedForeground}"/>
                        </Trigger>
                        <Trigger Property="IsSelected" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Accent.Fill}"/>
                            <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Accent.OnFill}"/>
                            <Setter Property="FontWeight" Value="SemiBold"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style TargetType="ComboBox">
        <Setter Property="MinHeight" Value="34"/>
        <Setter Property="Padding" Value="10,0,30,0"/>
        <Setter Property="FontFamily" Value="{DynamicResource Ubisam.Font.Body}"/>
        <Setter Property="FontSize" Value="13.5"/>
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.ContentForeground}"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="ComboBox">
                    <Grid>
                        <ToggleButton x:Name="ToggleButton"
                                      Style="{StaticResource Ubisam.Style.ComboToggle}"
                                      IsChecked="{Binding IsDropDownOpen, Mode=TwoWay, RelativeSource={RelativeSource TemplatedParent}}"/>
                        <ContentPresenter x:Name="ContentSite"
                                          Content="{TemplateBinding SelectionBoxItem}"
                                          ContentTemplate="{TemplateBinding SelectionBoxItemTemplate}"
                                          ContentStringFormat="{TemplateBinding SelectionBoxItemStringFormat}"
                                          Margin="{TemplateBinding Padding}"
                                          VerticalAlignment="Center"
                                          HorizontalAlignment="Left"
                                          IsHitTestVisible="False"/>
                        <TextBox x:Name="PART_EditableTextBox"
                                 Style="{x:Null}"
                                 Visibility="Collapsed"
                                 Margin="{TemplateBinding Padding}"
                                 Background="Transparent"
                                 BorderThickness="0"/>
                        <Popup x:Name="Popup"
                               Placement="Bottom"
                               IsOpen="{TemplateBinding IsDropDownOpen}"
                               AllowsTransparency="True"
                               Focusable="False"
                               PopupAnimation="Fade">
                            <Border MinWidth="{TemplateBinding ActualWidth}"
                                    MaxHeight="{TemplateBinding MaxDropDownHeight}"
                                    Background="{DynamicResource Ubisam.Brush.Input.Background}"
                                    BorderBrush="{DynamicResource Ubisam.Brush.Input.Border}"
                                    BorderThickness="1,0,1,1"
                                    Effect="{DynamicResource Ubisam.Effect.Popup}">
                                <ScrollViewer>
                                    <ItemsPresenter/>
                                </ScrollViewer>
                            </Border>
                        </Popup>
                    </Grid>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsEditable" Value="True">
                            <Setter TargetName="PART_EditableTextBox" Property="Visibility" Value="Visible"/>
                            <Setter TargetName="ContentSite" Property="Visibility" Value="Collapsed"/>
                        </Trigger>
                        <Trigger Property="IsEnabled" Value="False">
                            <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Muted}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- ══ CheckBox ══ -->
    <Style TargetType="CheckBox">
        <Setter Property="FontFamily" Value="{DynamicResource Ubisam.Font.Body}"/>
        <Setter Property="FontSize" Value="13.5"/>
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.ContentForeground}"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="CheckBox">
                    <StackPanel Orientation="Horizontal">
                        <Border x:Name="Box"
                                Width="17" Height="17"
                                Background="{DynamicResource Ubisam.Brush.Input.Background}"
                                BorderBrush="{DynamicResource Ubisam.Brush.Input.Border}"
                                BorderThickness="1"
                                SnapsToDevicePixels="True"
                                VerticalAlignment="Center">
                            <Path x:Name="Check"
                                  Data="M 1,6 L 5,10 L 12,2"
                                  Stroke="{DynamicResource Ubisam.Brush.Accent.OnFill}"
                                  StrokeThickness="2"
                                  StrokeStartLineCap="Round"
                                  StrokeEndLineCap="Round"
                                  StrokeLineJoin="Round"
                                  Visibility="Collapsed"
                                  HorizontalAlignment="Center"
                                  VerticalAlignment="Center"/>
                        </Border>
                        <ContentPresenter Margin="9,0,0,0" VerticalAlignment="Center"/>
                    </StackPanel>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="Box" Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.Input.BorderHover}"/>
                        </Trigger>
                        <Trigger Property="IsChecked" Value="True">
                            <Setter TargetName="Box" Property="Background" Value="{DynamicResource Ubisam.Brush.Accent.Fill}"/>
                            <Setter TargetName="Box" Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.Accent.Fill}"/>
                            <Setter TargetName="Check" Property="Visibility" Value="Visible"/>
                        </Trigger>
                        <Trigger Property="IsEnabled" Value="False">
                            <Setter TargetName="Box" Property="Background" Value="{DynamicResource Ubisam.Brush.Input.DisabledBackground}"/>
                            <Setter TargetName="Box" Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.Input.DisabledBorder}"/>
                            <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Muted}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- ══ Button ══
         암시적 = 보조(외곽선). 주 동작에는 Style="{StaticResource Ubisam.Style.Button.Primary}" -->
    <Style TargetType="Button">
        <Setter Property="MinHeight" Value="34"/>
        <Setter Property="Padding" Value="20,8"/>
        <Setter Property="FontFamily" Value="{DynamicResource Ubisam.Font.Body}"/>
        <Setter Property="FontSize" Value="13.5"/>
        <Setter Property="FontWeight" Value="Medium"/>
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Sidebar.Foreground}"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border x:Name="Bd"
                            Background="{DynamicResource Ubisam.Brush.Input.Background}"
                            BorderBrush="{DynamicResource Ubisam.Brush.Input.Border}"
                            BorderThickness="1"
                            Padding="{TemplateBinding Padding}"
                            SnapsToDevicePixels="True">
                        <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.SelectedBackground}"/>
                            <Setter TargetName="Bd" Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.Accent}"/>
                            <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Sidebar.SelectedForeground}"/>
                        </Trigger>
                        <Trigger Property="IsPressed" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.HoverBackground}"/>
                        </Trigger>
                        <Trigger Property="IsEnabled" Value="False">
                            <Setter TargetName="Bd" Property="Opacity" Value="0.45"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style x:Key="Ubisam.Style.Button.Primary" TargetType="Button" BasedOn="{StaticResource {x:Type Button}}">
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Accent.OnFill}"/>
        <Setter Property="FontWeight" Value="SemiBold"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border x:Name="Bd"
                            Background="{DynamicResource Ubisam.Brush.Accent.Fill}"
                            BorderThickness="0"
                            Padding="{TemplateBinding Padding}"
                            SnapsToDevicePixels="True">
                        <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="Bd" Property="Opacity" Value="0.88"/>
                        </Trigger>
                        <Trigger Property="IsPressed" Value="True">
                            <Setter TargetName="Bd" Property="Opacity" Value="0.75"/>
                        </Trigger>
                        <Trigger Property="IsEnabled" Value="False">
                            <Setter TargetName="Bd" Property="Opacity" Value="0.45"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style x:Key="Ubisam.Style.Button.Ghost" TargetType="Button" BasedOn="{StaticResource {x:Type Button}}">
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border x:Name="Bd" Background="Transparent" Padding="{TemplateBinding Padding}">
                        <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.HoverBackground}"/>
                            <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Sidebar.SelectedForeground}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- ══ 일반 ListBox (설정 키 목록 등) ══ -->
    <Style TargetType="ListBoxItem">
        <Setter Property="Padding" Value="11,8"/>
        <Setter Property="FontFamily" Value="{DynamicResource Ubisam.Font.Body}"/>
        <Setter Property="FontSize" Value="13.5"/>
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.ContentForeground}"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="HorizontalContentAlignment" Value="Stretch"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="ListBoxItem">
                    <Border x:Name="Bd"
                            Background="Transparent"
                            BorderThickness="2,0,0,0"
                            BorderBrush="Transparent"
                            Padding="{TemplateBinding Padding}">
                        <ContentPresenter VerticalAlignment="Center"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.HoverBackground}"/>
                        </Trigger>
                        <Trigger Property="IsSelected" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.SelectedBackground}"/>
                            <Setter TargetName="Bd" Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.Accent}"/>
                            <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Sidebar.SelectedForeground}"/>
                            <Setter Property="FontWeight" Value="SemiBold"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style TargetType="ListBox">
        <Setter Property="Background" Value="{DynamicResource Ubisam.Brush.Input.Background}"/>
        <Setter Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.Input.Border}"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="Padding" Value="0,4"/>
    </Style>

    <!-- ══ ListView + GridView (로그 표) ══ -->
    <Style TargetType="GridViewColumnHeader">
        <Setter Property="Padding" Value="11,9"/>
        <Setter Property="FontFamily" Value="{DynamicResource Ubisam.Font.Body}"/>
        <Setter Property="FontSize" Value="11"/>
        <Setter Property="FontWeight" Value="SemiBold"/>
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Sidebar.Foreground}"/>
        <Setter Property="HorizontalContentAlignment" Value="Left"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="GridViewColumnHeader">
                    <Border x:Name="Bd"
                            Background="{DynamicResource Ubisam.Brush.Table.HeaderBackground}"
                            BorderBrush="{DynamicResource Ubisam.Brush.Input.Border}"
                            BorderThickness="0,0,1,1"
                            Padding="{TemplateBinding Padding}">
                        <ContentPresenter VerticalAlignment="Center"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.HoverBackground}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style x:Key="Ubisam.Style.TableRow" TargetType="ListViewItem">
        <Setter Property="Padding" Value="0"/>
        <Setter Property="FontFamily" Value="{DynamicResource Ubisam.Font.Body}"/>
        <Setter Property="FontSize" Value="12.5"/>
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.ContentForeground}"/>
        <Setter Property="HorizontalContentAlignment" Value="Stretch"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="ListViewItem">
                    <Border x:Name="Bd"
                            Background="Transparent"
                            BorderBrush="{DynamicResource Ubisam.Brush.Table.RowBorder}"
                            BorderThickness="0,0,0,1"
                            Padding="0,4">
                        <GridViewRowPresenter Content="{TemplateBinding Content}"
                                              Columns="{TemplateBinding GridView.ColumnCollection}"
                                              VerticalAlignment="Center"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.HoverBackground}"/>
                        </Trigger>
                        <Trigger Property="IsSelected" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.SelectedBackground}"/>
                            <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Sidebar.SelectedForeground}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style TargetType="ListView">
        <Setter Property="Background" Value="{DynamicResource Ubisam.Brush.Input.Background}"/>
        <Setter Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.Input.Border}"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="ItemContainerStyle" Value="{StaticResource Ubisam.Style.TableRow}"/>
        <Setter Property="ScrollViewer.HorizontalScrollBarVisibility" Value="Auto"/>
    </Style>

    <!-- ══ DataGrid ══
         ListView+GridView가 읽기 전용 표라면 DataGrid는 편집 가능한 격자다.
         셀 구분선은 세로만 그리고(가로는 행 경계가 담당), 선택은 행 머리의 accent 바 + 행 틴트로 알린다. -->

    <!-- 정렬 화살표 -->
    <Geometry x:Key="Ubisam.Geometry.SortAsc">M1,6 L5.5,1 L10,6</Geometry>
    <Geometry x:Key="Ubisam.Geometry.SortDesc">M1,1 L5.5,6 L10,1</Geometry>

    <Style TargetType="DataGridColumnHeader">
        <Setter Property="Padding" Value="11,10"/>
        <Setter Property="FontFamily" Value="{DynamicResource Ubisam.Font.Body}"/>
        <Setter Property="FontSize" Value="11"/>
        <Setter Property="FontWeight" Value="SemiBold"/>
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Sidebar.Foreground}"/>
        <Setter Property="HorizontalContentAlignment" Value="Left"/>
        <Setter Property="VerticalContentAlignment" Value="Center"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="DataGridColumnHeader">
                    <Grid>
                        <Border x:Name="Bd"
                                Background="{DynamicResource Ubisam.Brush.Table.HeaderBackground}"
                                BorderBrush="{DynamicResource Ubisam.Brush.Table.GridLine}"
                                BorderThickness="0,0,1,0"
                                Padding="{TemplateBinding Padding}">
                            <Grid>
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="*"/>
                                    <ColumnDefinition Width="Auto"/>
                                </Grid.ColumnDefinitions>
                                <ContentPresenter Grid.Column="0"
                                                  VerticalAlignment="Center"
                                                  HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"/>
                                <Path x:Name="SortArrow"
                                      Grid.Column="1"
                                      Margin="7,0,0,0"
                                      Data="{StaticResource Ubisam.Geometry.SortDesc}"
                                      Stroke="{DynamicResource Ubisam.Brush.Accent}"
                                      StrokeThickness="2"
                                      StrokeStartLineCap="Round"
                                      StrokeEndLineCap="Round"
                                      StrokeLineJoin="Round"
                                      VerticalAlignment="Center"
                                      Visibility="Collapsed"/>
                            </Grid>
                        </Border>
                        <Thumb x:Name="PART_RightHeaderGripper"
                               HorizontalAlignment="Right"
                               Width="6"
                               Cursor="SizeWE"
                               Opacity="0">
                            <Thumb.Template>
                                <ControlTemplate TargetType="Thumb">
                                    <Border Background="Transparent"/>
                                </ControlTemplate>
                            </Thumb.Template>
                        </Thumb>
                    </Grid>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.HoverBackground}"/>
                        </Trigger>
                        <Trigger Property="SortDirection" Value="Ascending">
                            <Setter TargetName="SortArrow" Property="Visibility" Value="Visible"/>
                            <Setter TargetName="SortArrow" Property="Data" Value="{StaticResource Ubisam.Geometry.SortAsc}"/>
                            <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Sidebar.SelectedForeground}"/>
                        </Trigger>
                        <Trigger Property="SortDirection" Value="Descending">
                            <Setter TargetName="SortArrow" Property="Visibility" Value="Visible"/>
                            <Setter TargetName="SortArrow" Property="Data" Value="{StaticResource Ubisam.Geometry.SortDesc}"/>
                            <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Sidebar.SelectedForeground}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- 행 머리: 폭 26, 선택 시 accent 바가 채워진다 -->
    <Style TargetType="DataGridRowHeader">
        <Setter Property="Width" Value="26"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="DataGridRowHeader">
                    <Border x:Name="Bd"
                            Background="Transparent"
                            BorderBrush="{DynamicResource Ubisam.Brush.Table.GridLine}"
                            BorderThickness="0,0,1,0"/>
                    <ControlTemplate.Triggers>
                        <DataTrigger Binding="{Binding IsSelected, RelativeSource={RelativeSource AncestorType=DataGridRow}}" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Accent}"/>
                        </DataTrigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style TargetType="DataGridCell">
        <Setter Property="Padding" Value="11,9"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.ContentForeground}"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="DataGridCell">
                    <Border x:Name="Bd"
                            Background="Transparent"
                            BorderBrush="{DynamicResource Ubisam.Brush.Table.RowBorder}"
                            BorderThickness="0,0,1,0"
                            Padding="{TemplateBinding Padding}">
                        <ContentPresenter VerticalAlignment="Center"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <!-- 편집 중인 셀만 accent 테두리 + 내부 여백을 줄여 입력칸을 보여준다 -->
                        <Trigger Property="IsEditing" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Input.Background}"/>
                            <Setter TargetName="Bd" Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.Accent}"/>
                            <Setter TargetName="Bd" Property="BorderThickness" Value="1"/>
                            <Setter TargetName="Bd" Property="Padding" Value="6,4"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style TargetType="DataGridRow">
        <Setter Property="Background" Value="Transparent"/>
        <Setter Property="FontFamily" Value="{DynamicResource Ubisam.Font.Body}"/>
        <Setter Property="FontSize" Value="12.5"/>
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.ContentForeground}"/>
        <Style.Triggers>
            <Trigger Property="AlternationIndex" Value="1">
                <Setter Property="Background" Value="{DynamicResource Ubisam.Brush.Table.AltRowBackground}"/>
            </Trigger>
            <Trigger Property="IsMouseOver" Value="True">
                <Setter Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.HoverBackground}"/>
            </Trigger>
            <Trigger Property="IsSelected" Value="True">
                <Setter Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.SelectedBackground}"/>
                <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Sidebar.SelectedForeground}"/>
            </Trigger>
        </Style.Triggers>
    </Style>

    <Style TargetType="DataGrid">
        <Setter Property="Background" Value="{DynamicResource Ubisam.Brush.Input.Background}"/>
        <Setter Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.Input.Border}"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="RowBackground" Value="Transparent"/>
        <Setter Property="AlternatingRowBackground" Value="{DynamicResource Ubisam.Brush.Table.AltRowBackground}"/>
        <Setter Property="RowHeight" Value="34"/>
        <Setter Property="ColumnHeaderHeight" Value="34"/>
        <Setter Property="GridLinesVisibility" Value="Horizontal"/>
        <Setter Property="HorizontalGridLinesBrush" Value="{DynamicResource Ubisam.Brush.Table.RowBorder}"/>
        <Setter Property="VerticalGridLinesBrush" Value="{DynamicResource Ubisam.Brush.Table.RowBorder}"/>
        <Setter Property="HeadersVisibility" Value="All"/>
        <Setter Property="SelectionUnit" Value="FullRow"/>
        <Setter Property="GridLinesVisibility" Value="All"/>
        <Setter Property="AutoGenerateColumns" Value="False"/>
        <Setter Property="CanUserAddRows" Value="False"/>
        <Setter Property="CanUserDeleteRows" Value="False"/>
        <Setter Property="CanUserResizeRows" Value="False"/>
        <Setter Property="EnableRowVirtualization" Value="True"/>
    </Style>

    <!-- 수치 열에 붙일 셀 스타일: 우측 정렬 + 자릿수 고정 -->
    <Style x:Key="Ubisam.Style.NumericCell" TargetType="DataGridCell" BasedOn="{StaticResource {x:Type DataGridCell}}">
        <Setter Property="HorizontalAlignment" Value="Stretch"/>
        <Setter Property="HorizontalContentAlignment" Value="Right"/>
        <Setter Property="TextBlock.TextAlignment" Value="Right"/>
    </Style>

    <!-- ══ 상태 태그 ══
         표의 판정·레벨 같은 상태 열은 맨 텍스트가 아니라 태그로 그린다 (디자인 시스템 규칙).
         DataGridTemplateColumn / GridViewColumn의 CellTemplate 안에서 쓴다. -->
    <Style x:Key="Ubisam.Style.Tag" TargetType="Border">
        <Setter Property="Background" Value="{DynamicResource Ubisam.Brush.Tag.NeutralBackground}"/>
        <Setter Property="CornerRadius" Value="3"/>
        <Setter Property="Padding" Value="10,3"/>
        <Setter Property="HorizontalAlignment" Value="Left"/>
        <Setter Property="VerticalAlignment" Value="Center"/>
    </Style>

    <Style x:Key="Ubisam.Style.TagText" TargetType="TextBlock">
        <Setter Property="FontFamily" Value="{DynamicResource Ubisam.Font.Body}"/>
        <Setter Property="FontSize" Value="11"/>
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Tag.NeutralForeground}"/>
    </Style>

    <!-- 정상·완료: accent 옅은 채움 -->
    <Style x:Key="Ubisam.Style.Tag.Accent" TargetType="Border" BasedOn="{StaticResource Ubisam.Style.Tag}">
        <Setter Property="Background" Value="{DynamicResource Ubisam.Brush.Tag.AccentBackground}"/>
    </Style>
    <Style x:Key="Ubisam.Style.TagText.Accent" TargetType="TextBlock" BasedOn="{StaticResource Ubisam.Style.TagText}">
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Tag.AccentForeground}"/>
    </Style>

    <!-- 주의·재검: 외곽선만 (예외 행이 한눈에 걸리도록) -->
    <Style x:Key="Ubisam.Style.Tag.Outline" TargetType="Border" BasedOn="{StaticResource Ubisam.Style.Tag}">
        <Setter Property="Background" Value="Transparent"/>
        <Setter Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.Accent}"/>
        <Setter Property="BorderThickness" Value="1"/>
    </Style>
    <Style x:Key="Ubisam.Style.TagText.Outline" TargetType="TextBlock" BasedOn="{StaticResource Ubisam.Style.TagText}">
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Accent}"/>
    </Style>

    <!-- ══ Slider ══
         트랙은 얇은 홈, 채워진 구간만 accent. 썸은 각진 세로 막대(다이얼이 아니라 계기 슬라이드). -->
    <Style x:Key="Ubisam.Style.SliderThumb" TargetType="Thumb">
        <Setter Property="Focusable" Value="False"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Thumb">
                    <Border x:Name="Bd"
                            Width="10" Height="20"
                            Background="{DynamicResource Ubisam.Brush.Accent.Fill}"
                            BorderBrush="{DynamicResource Ubisam.Brush.Accent}"
                            BorderThickness="1"
                            SnapsToDevicePixels="True"/>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Accent}"/>
                        </Trigger>
                        <Trigger Property="IsDragging" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Accent}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style TargetType="Slider">
        <Setter Property="MinHeight" Value="24"/>
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Accent}"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Slider">
                    <Grid VerticalAlignment="Center">
                        <!-- 트랙 홈 -->
                        <Border Height="4"
                                Background="{DynamicResource Ubisam.Brush.Input.DisabledBackground}"
                                BorderBrush="{DynamicResource Ubisam.Brush.Input.Border}"
                                BorderThickness="1"
                                VerticalAlignment="Center"
                                SnapsToDevicePixels="True"/>
                        <Track x:Name="PART_Track">
                            <Track.DecreaseRepeatButton>
                                <RepeatButton Command="Slider.DecreaseLarge"
                                              Focusable="False"
                                              Height="4"
                                              VerticalAlignment="Center">
                                    <RepeatButton.Template>
                                        <ControlTemplate TargetType="RepeatButton">
                                            <!-- 채워진 구간 -->
                                            <Border Background="{DynamicResource Ubisam.Brush.Accent}"
                                                    Height="4"
                                                    VerticalAlignment="Center"
                                                    SnapsToDevicePixels="True"/>
                                        </ControlTemplate>
                                    </RepeatButton.Template>
                                </RepeatButton>
                            </Track.DecreaseRepeatButton>
                            <Track.IncreaseRepeatButton>
                                <RepeatButton Command="Slider.IncreaseLarge" Focusable="False">
                                    <RepeatButton.Template>
                                        <ControlTemplate TargetType="RepeatButton">
                                            <Border Background="Transparent"/>
                                        </ControlTemplate>
                                    </RepeatButton.Template>
                                </RepeatButton>
                            </Track.IncreaseRepeatButton>
                            <Track.Thumb>
                                <Thumb Style="{StaticResource Ubisam.Style.SliderThumb}"/>
                            </Track.Thumb>
                        </Track>
                    </Grid>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsEnabled" Value="False">
                            <Setter Property="Opacity" Value="0.45"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- ══ ScrollBar ══
         폭 10의 얇은 막대. 트랙은 비우고 썸만 회색 — hover에서 accent로. 화살표 버튼은 없앤다. -->
    <Style x:Key="Ubisam.Style.ScrollThumb" TargetType="Thumb">
        <Setter Property="Focusable" Value="False"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Thumb">
                    <Border x:Name="Bd"
                            Background="{DynamicResource Ubisam.Brush.Input.BorderHover}"
                            SnapsToDevicePixels="True"/>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Accent}"/>
                        </Trigger>
                        <Trigger Property="IsDragging" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Accent}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style TargetType="ScrollBar">
        <Setter Property="Background" Value="Transparent"/>
        <Setter Property="Width" Value="10"/>
        <Setter Property="MinWidth" Value="10"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="ScrollBar">
                    <Grid Background="{TemplateBinding Background}">
                        <Track x:Name="PART_Track" IsDirectionReversed="True">
                            <Track.DecreaseRepeatButton>
                                <RepeatButton Command="ScrollBar.PageUpCommand" Focusable="False">
                                    <RepeatButton.Template>
                                        <ControlTemplate TargetType="RepeatButton">
                                            <Border Background="Transparent"/>
                                        </ControlTemplate>
                                    </RepeatButton.Template>
                                </RepeatButton>
                            </Track.DecreaseRepeatButton>
                            <Track.IncreaseRepeatButton>
                                <RepeatButton Command="ScrollBar.PageDownCommand" Focusable="False">
                                    <RepeatButton.Template>
                                        <ControlTemplate TargetType="RepeatButton">
                                            <Border Background="Transparent"/>
                                        </ControlTemplate>
                                    </RepeatButton.Template>
                                </RepeatButton>
                            </Track.IncreaseRepeatButton>
                            <Track.Thumb>
                                <Thumb Style="{StaticResource Ubisam.Style.ScrollThumb}" Margin="3,0"/>
                            </Track.Thumb>
                        </Track>
                    </Grid>
                    <ControlTemplate.Triggers>
                        <Trigger Property="Orientation" Value="Horizontal">
                            <Setter Property="Height" Value="10"/>
                            <Setter Property="MinHeight" Value="10"/>
                            <Setter Property="Width" Value="Auto"/>
                            <Setter Property="MinWidth" Value="0"/>
                            <Setter TargetName="PART_Track" Property="IsDirectionReversed" Value="False"/>
                            <Setter TargetName="PART_Track" Property="Orientation" Value="Horizontal"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

</ResourceDictionary>
```

## 4. `src/UbisamBase.Core/Themes/ShellStyles.xaml` — 머지 추가

`Controls.xaml`을 머지 목록에 넣고, ComboBox 드롭다운 그림자를 정의하세요.

```xml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="pack://application:,,,/UbisamBase.Core;component/Themes/Icons.xaml"/>
    <ResourceDictionary Source="pack://application:,,,/UbisamBase.Core;component/Themes/Controls.xaml"/>
</ResourceDictionary.MergedDictionaries>
```

```xml
<DropShadowEffect x:Key="Ubisam.Effect.Popup" BlurRadius="12" ShadowDepth="3" Direction="270" Opacity="0.22" Color="#000000"/>
```

**중요 — 암시적 스타일이 안 걸릴 때:** `Controls.xaml`을 `ShellWindow.Resources`에만 머지하면, 리소스 조회 범위 밖의 `UserControl`에는 적용되지 않습니다. 콤보박스가 여전히 WPF 기본 크롬(둥근 모서리 + 밝은 회색)으로 보이면 `App.xaml`의 `Application.Resources`에 같은 머지를 넣으세요.

## 5. `src/UbisamBase.Core/Shell/ShellWindow.xaml` — 로고 타일

로고 타일은 글자를 얹는 채움면이므로 밝은 단계가 아니라 채움 단계를 씁니다.

```xml
<Border Width="30" Height="30" Background="{DynamicResource Ubisam.Brush.Accent.Fill}">
    <TextBlock Text="{Binding Brand, Converter={StaticResource FirstCharConverter}}"
               Foreground="{DynamicResource Ubisam.Brush.Accent.OnFill}"
               FontFamily="{StaticResource Ubisam.Font.Heading}"
               FontWeight="SemiBold" FontSize="17"
               HorizontalAlignment="Center" VerticalAlignment="Center"/>
</Border>
```

반대로 `ShellStyles.xaml`의 **좌측 네비 선택 테두리**와 **하단 탭 인디케이터 바**는 글자를 얹지 않는 선이므로 `Accent`(밝은 단계)가 맞습니다 — 그대로 두세요.

## 6. 상태 태그 쓰는 법

```xml
<DataGridTemplateColumn Header="판정" Width="96">
    <DataGridTemplateColumn.CellTemplate>
        <DataTemplate>
            <Border Style="{StaticResource Ubisam.Style.Tag.Accent}">
                <TextBlock Text="{Binding Verdict}" Style="{StaticResource Ubisam.Style.TagText.Accent}"/>
            </Border>
        </DataTemplate>
    </DataGridTemplateColumn.CellTemplate>
</DataGridTemplateColumn>
```

| 스타일 쌍 | 쓰는 곳 |
| --- | --- |
| `Ubisam.Style.Tag.Accent` + `TagText.Accent` | 정상 · 완료 (양품, Info) |
| `Ubisam.Style.Tag.Outline` + `TagText.Outline` | 주의 · 예외 (재검, Warn) — 외곽선만이라 눈에 걸립니다 |
| `Ubisam.Style.Tag` + `TagText` | 중립 · 대기 |

## 7. DataGrid 쓰는 법

```xml
<DataGrid ItemsSource="{Binding Measurements}" SelectedItem="{Binding Selected}">
    <DataGrid.Columns>
        <DataGridTextColumn Header="시각" Width="96"
                            Binding="{Binding Timestamp, StringFormat=HH:mm:ss}" IsReadOnly="True"/>
        <DataGridTextColumn Header="레시피" Width="*" Binding="{Binding RecipeName}" IsReadOnly="True"/>
        <DataGridTextColumn Header="측정값" Width="118"
                            Binding="{Binding Value, StringFormat=N1}"
                            CellStyle="{StaticResource Ubisam.Style.NumericCell}"/>
    </DataGrid.Columns>
</DataGrid>
```

수치 열에는 반드시 `CellStyle="{StaticResource Ubisam.Style.NumericCell}"`을 주세요.

## 8. 확인

1. 다크에서 콤보박스가 **어두운 배경 + 각진 모서리**인지 — 둥근 밝은 회색이면 §4의 머지 문제입니다
2. 콤보박스를 열면 드롭다운이 입력창 바로 아래에 붙고, 선택 항목이 진한 파랑 + 흰 글자인지
3. 입력창을 클릭하면 accent 테두리 + 옅은 글로우가 생기는지
4. 주 버튼("저장")이 진한 파랑(#1E7FD4) + 흰 글자인지
5. 체크박스 체크 표시가 흰색인지 — 검정이면 파일이 갱신되지 않은 것입니다
6. 화면 배율 슬라이더의 왼쪽 구간만 파랗게 차고, 썸이 각진 세로 막대인지
7. 스크롤바가 폭 10의 얇은 막대이고 hover에서 accent로 바뀌는지
8. 표의 수치 열이 모두 우측 정렬인지
9. 바탕(#23262B) → 판(#2A2E34) → 입력(#2F343A) 순으로 밝아지는 층이 눈에 보이는지
10. 라이트 테마가 이전과 동일한지 — 새 키 2개가 추가됐을 뿐입니다

## 9. 하지 말 것

- 모듈 View의 내용은 건드리지 마세요 — 테마와 스타일만 바꿉니다
- 라운드 모서리를 되살리지 마세요 (`CornerRadius`는 태그의 3px만 예외)
- `Controls.xaml`의 폰트 참조를 `StaticResource`로 바꾸지 마세요 — 머지 시점 때문에 로드가 실패합니다
- `TabManager`, `EventBus`, `ToastService`, `AuthService`, `LogService`의 구조나 시그니처를 바꾸지 마세요

# UbisamBase 셸 UI 개편 지시서

이 문서 전체를 Claude Code에 붙여넣고 "이 지시서대로 적용해줘"라고 요청하세요.

---

당신은 `UbisamBase` WPF 솔루션(.NET Framework 4.8, MVVM + CommunityToolkit.Mvvm)에서 작업합니다.
플랫폼 셸의 화면 디자인을 아래대로 교체하세요. **아래 §2의 파일 내용은 전문이 주어져 있으니 그대로 덮어쓰거나 새로 만드세요** — 임의로 재해석하거나 값을 바꾸지 마세요.

## 1. 무엇을 바꾸는가

- **좌상단**: 회사 로고(accent 사각형) + 회사명 `Brand` + 세로 구분선 + 장비명 `Title`(= `IAppSetup.GetAppName()`)
- **우상단**: 작은 날짜 + 큰 시계(25px). 기존 시계 타이머가 날짜도 함께 갱신
- **하단 탭**: 좌측 사이드바 대신 **하단 네비게이션이 기본**. 등록된 메인 탭 수만큼 `UniformGrid Rows="1"`로 균등 분할, 각 칸은 아이콘(위) + 라벨(아래), 선택 시 상단 3px accent 바 + 옅은 accent 판
- **메인 컨텐츠**: 기존 `Margin="32"` 제거 — 영역 전체를 모듈 View가 사용. 셸은 컨텐츠 제목줄을 그리지 않음(선택된 하단 탭과 중복). 모듈이 없을 때는 사선 해치 브러시로 빈 슬롯 표시
- **아이콘**: Segoe Fluent 글리프 대신 **벡터 Geometry 24종**. `AddMain`의 `icon` 인자에 글리프가 아니라 아이콘 이름("Home", "Monitor" 등)을 넘김. 생략하면 라벨만 표시
- **모양 규칙**: 라운드 제거(서브탭 알약 → 각진 세그먼트, ChromeButton CornerRadius 0), 헤어라인 경계
- **색**: 라이트 = 바탕 #F2F2F3 / 판 #FFFFFF / accent #2196F3. 다크 = 바탕 #121317 / 판 #16171C / 경계 #2A2C33 / accent #22D3EE
- **폰트**: `Ubisam.Font.Heading` = "Barlow Condensed, Segoe UI", `Ubisam.Font.Body` = "Barlow, Segoe UI" (Barlow 미설치 시 Segoe UI 폴백)

## 2. 파일 전문


### `src/UbisamBase.Core/Themes/Colors.Light.xaml`

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- 라이트: 기술 도면 톤의 밝은 바탕 + 회사 하늘색 accent -->
    <SolidColorBrush x:Key="Ubisam.Brush.ContentBackground" Color="#FFF2F2F3"/>
    <SolidColorBrush x:Key="Ubisam.Brush.ContentForeground" Color="#FF1D1F20"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Accent" Color="#FF2196F3"/>

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

    <!-- 빈 뷰 슬롯의 사선 해치 색 -->
    <SolidColorBrush x:Key="Ubisam.Brush.Slot.Hatch" Color="#172196F3"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Muted" Color="#FF98989B"/>

</ResourceDictionary>
```

### `src/UbisamBase.Core/Themes/Colors.Dark.xaml`

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- 다크: 라이트의 반전 — 검은 바탕 + 시안 accent -->
    <SolidColorBrush x:Key="Ubisam.Brush.ContentBackground" Color="#FF121317"/>
    <SolidColorBrush x:Key="Ubisam.Brush.ContentForeground" Color="#FFE6E6E9"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Accent" Color="#FF22D3EE"/>

    <SolidColorBrush x:Key="Ubisam.Brush.Sidebar.Background" Color="#FF16171C"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Sidebar.Foreground" Color="#FFB8BCC4"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Sidebar.HoverBackground" Color="#18FFFFFF"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Sidebar.SelectedBackground" Color="#1A22D3EE"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Sidebar.SelectedForeground" Color="#FF22D3EE"/>

    <SolidColorBrush x:Key="Ubisam.Brush.SubTabBar.Background" Color="#FF16171C"/>
    <SolidColorBrush x:Key="Ubisam.Brush.SubTabBar.Border" Color="#FF2A2C33"/>
    <SolidColorBrush x:Key="Ubisam.Brush.SubTabBar.Foreground" Color="#FFB8BCC4"/>
    <SolidColorBrush x:Key="Ubisam.Brush.SubTabBar.HoverBackground" Color="#18FFFFFF"/>
    <SolidColorBrush x:Key="Ubisam.Brush.SubTabBar.SelectedBackground" Color="#FF22D3EE"/>
    <SolidColorBrush x:Key="Ubisam.Brush.SubTabBar.SelectedForeground" Color="#FF0B0C0F"/>

    <SolidColorBrush x:Key="Ubisam.Brush.Slot.Hatch" Color="#1A22D3EE"/>
    <SolidColorBrush x:Key="Ubisam.Brush.Muted" Color="#FF6A6F79"/>

</ResourceDictionary>
```

### `src/UbisamBase.Core/Themes/Icons.xaml`

```xml
<!-- Ubisam Base 아이콘 세트 — 24×24 그리드, 스트로크 렌더링(StrokeThickness 1.5).
     Path의 Fill은 비우고 Stroke만 쓴다. 색은 상위 Foreground를 상속시키려면
     Stroke="{Binding Foreground, RelativeSource={RelativeSource AncestorType=ListBoxItem}}" 처럼 묶으면 된다.
     아이콘을 추가할 때는 Lucide(https://lucide.dev)에서 같은 24px 그리드/스트로크 1.5 규칙으로 가져와
     여기에 Geometry 한 줄로 등록하고, AddMain의 icon 인자에 키 뒷부분("Home" 등)을 넘긴다. -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- 현재 탭 5개 -->
    <Geometry x:Key="Ubisam.Icon.Home">M3,10.5 L12,3 L21,10.5 L21,20 A1,1 0 0 1 20,21 L4,21 A1,1 0 0 1 3,20 Z M9.5,21 L9.5,14 L14.5,14 L14.5,21</Geometry>
    <Geometry x:Key="Ubisam.Icon.Monitor">M22,12 L18,12 L15,20 L11,4 L8,12 L2,12</Geometry>
    <Geometry x:Key="Ubisam.Icon.Chart">M3,3 L3,21 L21,21 M7.5,17.5 L7.5,12.5 M12,17.5 L12,8 M16.5,17.5 L16.5,9.5</Geometry>
    <Geometry x:Key="Ubisam.Icon.Maintenance">M15,4 L17,4 A1,1 0 0 1 18,5 L18,20 A1,1 0 0 1 17,21 L7,21 A1,1 0 0 1 6,20 L6,5 A1,1 0 0 1 7,4 L9,4 M9,2.5 L15,2.5 L15,6.5 L9,6.5 Z M9.5,14 L11.5,16 L15.5,11.5</Geometry>
    <Geometry x:Key="Ubisam.Icon.Settings">M3,7 L21,7 M3,12 L21,12 M3,17 L21,17 M13.3,7 A2.2,2.2 0 0 1 17.7,7 A2.2,2.2 0 0 1 13.3,7 Z M6.8,12 A2.2,2.2 0 0 1 11.2,12 A2.2,2.2 0 0 1 6.8,12 Z M14.8,17 A2.2,2.2 0 0 1 19.2,17 A2.2,2.2 0 0 1 14.8,17 Z</Geometry>

    <!-- 예비 아이콘 — 탭이 늘어날 때 사용 -->
    <Geometry x:Key="Ubisam.Icon.Alarm">M18,16.5 L18,11 A6,6 0 0 0 6,11 L6,16.5 L4.5,19.5 L19.5,19.5 Z M10,22 L14,22</Geometry>
    <Geometry x:Key="Ubisam.Icon.Log">M8,6 L20,6 M8,12 L20,12 M8,18 L20,18 M3.1,6 A1.1,1.1 0 0 1 5.3,6 A1.1,1.1 0 0 1 3.1,6 Z M3.1,12 A1.1,1.1 0 0 1 5.3,12 A1.1,1.1 0 0 1 3.1,12 Z M3.1,18 A1.1,1.1 0 0 1 5.3,18 A1.1,1.1 0 0 1 3.1,18 Z</Geometry>
    <Geometry x:Key="Ubisam.Icon.Document">M14,3 L7,3 A1,1 0 0 0 6,4 L6,20 A1,1 0 0 0 7,21 L17,21 A1,1 0 0 0 18,20 L18,7 Z M14,3 L14,7 L18,7 M9,12.5 L15,12.5 M9,16.5 L15,16.5</Geometry>
    <Geometry x:Key="Ubisam.Icon.Data">M4,6 A8,3 0 0 1 20,6 A8,3 0 0 1 4,6 Z M4,6 L4,18 C4,19.7 7.6,21 12,21 C16.4,21 20,19.7 20,18 L20,6 M4,12 C4,13.7 7.6,15 12,15 C16.4,15 20,13.7 20,12</Geometry>
    <Geometry x:Key="Ubisam.Icon.Power">M12,3 L12,12 M6.8,7.3 A8,8 0 1 0 17.2,7.3</Geometry>
    <Geometry x:Key="Ubisam.Icon.Temperature">M14,14 L14,5 A2,2 0 1 0 10,5 L10,14 A4,4 0 1 0 14,14 Z</Geometry>
    <Geometry x:Key="Ubisam.Icon.Gauge">M4,18 A9,9 0 1 1 20,18 M12,14 L16,10</Geometry>
    <Geometry x:Key="Ubisam.Icon.Run">M8,5 L19,12 L8,19 Z</Geometry>
    <Geometry x:Key="Ubisam.Icon.Stop">M6.5,6.5 L17.5,6.5 L17.5,17.5 L6.5,17.5 Z</Geometry>
    <Geometry x:Key="Ubisam.Icon.Reset">M20,12 A8,8 0 1 1 16.8,5.6 M20,4.5 L20,9.5 L15,9.5</Geometry>
    <Geometry x:Key="Ubisam.Icon.Account">M8,8 A4,4 0 0 1 16,8 A4,4 0 0 1 8,8 Z M4.5,21 A7.5,7.5 0 0 1 19.5,21</Geometry>
    <Geometry x:Key="Ubisam.Icon.Lock">M4.5,11 L19.5,11 L19.5,21 L4.5,21 Z M8,11 L8,7.5 A4,4 0 0 1 16,7.5 L16,11</Geometry>
    <Geometry x:Key="Ubisam.Icon.Export">M12,3.5 L12,14.5 M7,10 L12,14.5 L17,10 M4,20.5 L20,20.5</Geometry>
    <Geometry x:Key="Ubisam.Icon.Import">M12,14.5 L12,3.5 M7,8 L12,3.5 L17,8 M4,20.5 L20,20.5</Geometry>
    <Geometry x:Key="Ubisam.Icon.Search">M4,11 A7,7 0 0 1 18,11 A7,7 0 0 1 4,11 Z M16.2,16.2 L20.5,20.5</Geometry>
    <Geometry x:Key="Ubisam.Icon.Calendar">M3.5,5.5 L20.5,5.5 L20.5,20.5 L3.5,20.5 Z M3.5,10.5 L20.5,10.5 M8,3 L8,7 M16,3 L16,7</Geometry>
    <Geometry x:Key="Ubisam.Icon.Clock">M3,12 A9,9 0 0 1 21,12 A9,9 0 0 1 3,12 Z M12,7 L12,12.5 L15.8,14.5</Geometry>
    <Geometry x:Key="Ubisam.Icon.Link">M9.5,14.5 L14.5,9.5 M13.5,6.5 L14.5,5.5 A4.6,4.6 0 0 1 21,12 L20,13 M10.5,17.5 L9.5,18.5 A4.6,4.6 0 0 1 3,12 L4,11</Geometry>
    <Geometry x:Key="Ubisam.Icon.Warning">M12,4 L21,20 L3,20 Z M12,10 L12,14.5 M12,17.4 L12,17.6</Geometry>
    <Geometry x:Key="Ubisam.Icon.Pass">M3,12 A9,9 0 0 1 21,12 A9,9 0 0 1 3,12 Z M8,12.5 L11,15.5 L16,9.5</Geometry>
    <Geometry x:Key="Ubisam.Icon.Folder">M3.5,8 L3.5,6 A1,1 0 0 1 4.5,5 L9,5 L11,8 L19.5,8 A1,1 0 0 1 20.5,9 L20.5,19 A1,1 0 0 1 19.5,20 L4.5,20 A1,1 0 0 1 3.5,19 Z</Geometry>
    <Geometry x:Key="Ubisam.Icon.Vision">M3,8 L7,8 L9,5 L15,5 L17,8 L21,8 L21,20 L3,20 Z M8.2,13.5 A3.8,3.8 0 0 1 15.8,13.5 A3.8,3.8 0 0 1 8.2,13.5 Z</Geometry>
    <Geometry x:Key="Ubisam.Icon.Control">M5.5,5.5 L18.5,5.5 L18.5,18.5 L5.5,18.5 Z M9.5,9.5 L14.5,9.5 L14.5,14.5 L9.5,14.5 Z M12,2 L12,5.5 M12,18.5 L12,22 M2,12 L5.5,12 M18.5,12 L22,12</Geometry>
    <Geometry x:Key="Ubisam.Icon.Recipe">M12,3.5 L3,8.5 L12,13.5 L21,8.5 Z M3,13.5 L12,18.5 L21,13.5</Geometry>
    <Geometry x:Key="Ubisam.Icon.Modules">M4,4 L11,4 L11,11 L4,11 Z M13,4 L20,4 L20,11 L13,11 Z M4,13 L11,13 L11,20 L4,20 Z M13,13 L20,13 L20,20 L13,20 Z</Geometry>

</ResourceDictionary>
```

### `src/UbisamBase.Core/Themes/ShellStyles.xaml`

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:conv="clr-namespace:UbisamBase.Core.Converters"
                    xmlns:sys="clr-namespace:System;assembly=mscorlib">

    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="pack://application:,,,/UbisamBase.Core;component/Themes/Icons.xaml"/>
    </ResourceDictionary.MergedDictionaries>

    <conv:BoolToVisibilityConverter x:Key="BoolToVis"/>
    <conv:FirstCharacterConverter x:Key="FirstCharConverter"/>
    <conv:ToastTypeToBrushConverter x:Key="ToastTypeToBrush"/>
    <conv:IconKeyToGeometryConverter x:Key="IconToGeometry"/>

    <!-- 타이포: Barlow가 설치돼 있으면 쓰고, 없으면 Segoe UI로 폴백 -->
    <FontFamily x:Key="Ubisam.Font.Heading">Barlow Condensed, Segoe UI</FontFamily>
    <FontFamily x:Key="Ubisam.Font.Body">Barlow, Segoe UI</FontFamily>

    <!-- 좌/우측 네비게이션 아이템: 아이콘 위 + 라벨 아래, 선택 시 좌측 accent 바 -->
    <Style x:Key="Ubisam.Style.SidebarItem" TargetType="ListBoxItem">
        <Setter Property="Padding" Value="14,20"/>
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Sidebar.Foreground}"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="HorizontalContentAlignment" Value="Center"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="ListBoxItem">
                    <Border x:Name="Bd"
                            Background="Transparent"
                            BorderThickness="3,0,0,0"
                            BorderBrush="Transparent"
                            Padding="{TemplateBinding Padding}">
                        <StackPanel Orientation="Vertical" HorizontalAlignment="Center">
                            <TextBlock Text="{Binding Icon}"
                                       FontFamily="Segoe Fluent Icons, Segoe MDL2 Assets"
                                       FontSize="26"
                                       HorizontalAlignment="Center"/>
                            <TextBlock Text="{Binding Title}"
                                       FontSize="13"
                                       Margin="0,8,0,0"
                                       HorizontalAlignment="Center"
                                       TextWrapping="Wrap"
                                       TextAlignment="Center"
                                       MaxWidth="100"/>
                        </StackPanel>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsSelected" Value="True">
                            <Setter TargetName="Bd" Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.Accent}"/>
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.SelectedBackground}"/>
                            <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Sidebar.SelectedForeground}"/>
                            <Setter Property="FontWeight" Value="SemiBold"/>
                        </Trigger>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.HoverBackground}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style x:Key="Ubisam.Style.SidebarBar.Vertical" TargetType="ListBox">
        <Setter Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.Background}"/>
        <Setter Property="BorderThickness" Value="0,0,1,0"/>
        <Setter Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.SubTabBar.Border}"/>
        <Setter Property="Padding" Value="0,20"/>
        <Setter Property="ItemContainerStyle" Value="{StaticResource Ubisam.Style.SidebarItem}"/>
        <Setter Property="ScrollViewer.HorizontalScrollBarVisibility" Value="Disabled"/>
        <Setter Property="ScrollViewer.VerticalScrollBarVisibility" Value="Auto"/>
        <Setter Property="ItemsPanel">
            <Setter.Value>
                <ItemsPanelTemplate>
                    <StackPanel Orientation="Vertical"/>
                </ItemsPanelTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- 하단 네비게이션 아이템: 균등 분할 칸, 아이콘 위 + 라벨 아래.
         선택 시 상단 3px accent 바 + 옅은 accent 판. icon 인자가 비면 라벨만 나온다. -->
    <Style x:Key="Ubisam.Style.SidebarItem.Horizontal" TargetType="ListBoxItem">
        <Setter Property="Padding" Value="10,12,10,13"/>
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Sidebar.Foreground}"/>
        <Setter Property="FontFamily" Value="{StaticResource Ubisam.Font.Body}"/>
        <Setter Property="FontSize" Value="13.5"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="HorizontalContentAlignment" Value="Stretch"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="ListBoxItem">
                    <Grid>
                        <!-- 칸 구분선 -->
                        <Border BorderThickness="1,0,0,0"
                                BorderBrush="{DynamicResource Ubisam.Brush.SubTabBar.Border}"/>
                        <Border x:Name="Bd"
                                Background="Transparent"
                                BorderThickness="0,3,0,0"
                                BorderBrush="Transparent"
                                Padding="{TemplateBinding Padding}">
                            <StackPanel Orientation="Vertical" HorizontalAlignment="Center" VerticalAlignment="Center">
                                <Path Data="{Binding Icon, Converter={StaticResource IconToGeometry}}"
                                      Stroke="{Binding Foreground, RelativeSource={RelativeSource TemplatedParent}}"
                                      StrokeThickness="1.5"
                                      StrokeStartLineCap="Round"
                                      StrokeEndLineCap="Round"
                                      StrokeLineJoin="Round"
                                      Width="22" Height="22"
                                      Stretch="Uniform"
                                      Margin="0,0,0,7"
                                      HorizontalAlignment="Center"/>
                                <TextBlock Text="{Binding Title}"
                                           HorizontalAlignment="Center"
                                           TextTrimming="CharacterEllipsis"/>
                            </StackPanel>
                        </Border>
                    </Grid>
                    <ControlTemplate.Triggers>
                        <!-- icon 인자를 생략한 탭은 Path를 접어 라벨만 남긴다 -->
                        <DataTrigger Binding="{Binding Icon, Converter={StaticResource IconToGeometry}}" Value="{x:Null}">
                            <Setter Property="Padding" Value="10,16,10,15"/>
                        </DataTrigger>
                        <Trigger Property="IsSelected" Value="True">
                            <Setter TargetName="Bd" Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.Accent}"/>
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.SelectedBackground}"/>
                            <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Sidebar.SelectedForeground}"/>
                            <Setter Property="FontWeight" Value="SemiBold"/>
                        </Trigger>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.HoverBackground}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- 하단 탭 바: 등록된 탭 수만큼 균등 분할(UniformGrid Rows=1) -->
    <Style x:Key="Ubisam.Style.SidebarBar.Horizontal" TargetType="ListBox">
        <Setter Property="Background" Value="{DynamicResource Ubisam.Brush.Sidebar.Background}"/>
        <Setter Property="BorderThickness" Value="0,1,0,0"/>
        <Setter Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.SubTabBar.Border}"/>
        <Setter Property="Padding" Value="0"/>
        <Setter Property="HorizontalContentAlignment" Value="Stretch"/>
        <Setter Property="ItemContainerStyle" Value="{StaticResource Ubisam.Style.SidebarItem.Horizontal}"/>
        <Setter Property="ScrollViewer.HorizontalScrollBarVisibility" Value="Disabled"/>
        <Setter Property="ScrollViewer.VerticalScrollBarVisibility" Value="Disabled"/>
        <Setter Property="ItemsPanel">
            <Setter.Value>
                <ItemsPanelTemplate>
                    <UniformGrid Rows="1"/>
                </ItemsPanelTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- 하단 서브탭: 각진 세그먼트(라운드 제거) -->
    <Style x:Key="Ubisam.Style.SubTabItem" TargetType="ListBoxItem">
        <Setter Property="Padding" Value="22,11"/>
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="FontFamily" Value="{StaticResource Ubisam.Font.Body}"/>
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.SubTabBar.Foreground}"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="ListBoxItem">
                    <Border x:Name="Bd"
                            Background="Transparent"
                            BorderThickness="1,0,0,0"
                            BorderBrush="{DynamicResource Ubisam.Brush.SubTabBar.Border}"
                            Padding="{TemplateBinding Padding}">
                        <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsSelected" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.SubTabBar.SelectedBackground}"/>
                            <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.SubTabBar.SelectedForeground}"/>
                            <Setter Property="FontWeight" Value="SemiBold"/>
                        </Trigger>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="{DynamicResource Ubisam.Brush.SubTabBar.HoverBackground}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style x:Key="Ubisam.Style.SubTabBar" TargetType="ListBox">
        <Setter Property="Background" Value="{DynamicResource Ubisam.Brush.SubTabBar.Background}"/>
        <Setter Property="BorderThickness" Value="0,1,0,0"/>
        <Setter Property="BorderBrush" Value="{DynamicResource Ubisam.Brush.SubTabBar.Border}"/>
        <Setter Property="Padding" Value="0"/>
        <Setter Property="ItemContainerStyle" Value="{StaticResource Ubisam.Style.SubTabItem}"/>
        <Setter Property="ScrollViewer.HorizontalScrollBarVisibility" Value="Auto"/>
        <Setter Property="ScrollViewer.VerticalScrollBarVisibility" Value="Disabled"/>
        <Setter Property="ItemsPanel">
            <Setter.Value>
                <ItemsPanelTemplate>
                    <StackPanel Orientation="Horizontal"/>
                </ItemsPanelTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- 상단바의 계정/바탕화면/종료 같은 유틸리티 버튼: 각진 헤어라인 박스 -->
    <Style x:Key="Ubisam.Style.ChromeButton" TargetType="Button">
        <Setter Property="Padding" Value="14,7"/>
        <Setter Property="FontSize" Value="13"/>
        <Setter Property="FontFamily" Value="{StaticResource Ubisam.Font.Body}"/>
        <Setter Property="Foreground" Value="{DynamicResource Ubisam.Brush.Sidebar.Foreground}"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border x:Name="Bd"
                            Background="Transparent"
                            CornerRadius="0"
                            BorderThickness="1"
                            BorderBrush="{DynamicResource Ubisam.Brush.SubTabBar.Border}"
                            Padding="{TemplateBinding Padding}">
                        <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
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

    <!-- 빈 뷰 슬롯 배경(사선 해치). 모듈이 마운트되기 전 영역 표시용. -->
    <DrawingBrush x:Key="Ubisam.Brush.SlotHatch"
                  TileMode="Tile"
                  Viewport="0,0,9,9"
                  ViewportUnits="Absolute">
        <DrawingBrush.Drawing>
            <GeometryDrawing>
                <GeometryDrawing.Pen>
                    <Pen Thickness="1" Brush="{DynamicResource Ubisam.Brush.Slot.Hatch}"/>
                </GeometryDrawing.Pen>
                <GeometryDrawing.Geometry>
                    <LineGeometry StartPoint="0,9" EndPoint="9,0"/>
                </GeometryDrawing.Geometry>
            </GeometryDrawing>
        </DrawingBrush.Drawing>
    </DrawingBrush>

</ResourceDictionary>
```

### `src/UbisamBase.Core/Shell/ShellWindow.xaml`

```xml
<Window x:Class="UbisamBase.Core.Shell.ShellWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:messaging="clr-namespace:UbisamBase.Core.Messaging"
        Title="{Binding Title}"
        Height="760" Width="1200"
        WindowStartupLocation="CenterScreen"
        WindowState="Maximized"
        Background="{DynamicResource Ubisam.Brush.ContentBackground}">

    <Window.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="pack://application:,,,/UbisamBase.Core;component/Themes/ShellStyles.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Window.Resources>

    <Grid>
        <DockPanel TextElement.Foreground="{DynamicResource Ubisam.Brush.ContentForeground}"
                   TextElement.FontFamily="{StaticResource Ubisam.Font.Body}">

            <!-- 상단 브랜딩 바: 좌측 = 로고 + 회사명 + 장비명, 우측 = 날짜 + 시계, 그 뒤 유틸리티 버튼.
                 플랫폼 고정 요소 — 모듈이 건드리지 않는다. -->
            <Border DockPanel.Dock="Top"
                    Background="{DynamicResource Ubisam.Brush.Sidebar.Background}"
                    BorderBrush="{DynamicResource Ubisam.Brush.SubTabBar.Border}"
                    BorderThickness="0,0,0,1"
                    Padding="26,14">
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="Auto"/>
                        <ColumnDefinition Width="Auto"/>
                        <ColumnDefinition Width="Auto"/>
                    </Grid.ColumnDefinitions>

                    <StackPanel Grid.Column="0" Orientation="Horizontal" VerticalAlignment="Center">
                        <!-- 로고 자리: 이미지 파일이 준비되면 이 Border를 <Image Source="..."/>로 교체 -->
                        <Border Width="30" Height="30" Background="{DynamicResource Ubisam.Brush.Accent}">
                            <TextBlock Text="{Binding Brand, Converter={StaticResource FirstCharConverter}}"
                                       Foreground="{DynamicResource Ubisam.Brush.SubTabBar.SelectedForeground}"
                                       FontFamily="{StaticResource Ubisam.Font.Heading}"
                                       FontWeight="SemiBold"
                                       FontSize="17"
                                       HorizontalAlignment="Center"
                                       VerticalAlignment="Center"/>
                        </Border>

                        <TextBlock Text="{Binding Brand}"
                                   FontFamily="{StaticResource Ubisam.Font.Heading}"
                                   FontWeight="SemiBold"
                                   FontSize="17"
                                   VerticalAlignment="Center"
                                   Margin="14,0,0,0">
                            <TextBlock.LayoutTransform>
                                <ScaleTransform ScaleX="1" ScaleY="1"/>
                            </TextBlock.LayoutTransform>
                        </TextBlock>

                        <Border Width="1" Height="20" Margin="14,0"
                                Background="{DynamicResource Ubisam.Brush.SubTabBar.Border}"/>

                        <!-- 장비명: IAppSetup.GetAppName()이 돌려주는 값 -->
                        <TextBlock Text="{Binding Title}"
                                   FontSize="14.5"
                                   Foreground="{DynamicResource Ubisam.Brush.Sidebar.Foreground}"
                                   VerticalAlignment="Center"/>
                    </StackPanel>

                    <StackPanel Grid.Column="1" Orientation="Horizontal" VerticalAlignment="Center" Margin="0,0,20,0">
                        <TextBlock Text="{Binding CurrentDate}"
                                   FontSize="11"
                                   Foreground="{DynamicResource Ubisam.Brush.Muted}"
                                   VerticalAlignment="Bottom"
                                   Margin="0,0,12,2"/>
                        <TextBlock Text="{Binding CurrentTime}"
                                   FontFamily="{StaticResource Ubisam.Font.Heading}"
                                   FontWeight="SemiBold"
                                   FontSize="25"
                                   VerticalAlignment="Bottom"/>
                    </StackPanel>

                    <Button Grid.Column="2"
                            Content="{Binding Auth.UserDisplay}"
                            Style="{StaticResource Ubisam.Style.ChromeButton}"
                            VerticalAlignment="Center"
                            Margin="0,0,8,0"
                            Click="Account_Click"/>

                    <StackPanel Grid.Column="3" Orientation="Horizontal" VerticalAlignment="Center">
                        <Button Content="바탕화면"
                                Style="{StaticResource Ubisam.Style.ChromeButton}"
                                Click="ShowDesktop_Click"/>
                        <Button Content="종료"
                                Style="{StaticResource Ubisam.Style.ChromeButton}"
                                Margin="8,0,0,0"
                                Click="Exit_Click"/>
                    </StackPanel>
                </Grid>
            </Border>

            <!-- 네비게이션: 기본은 하단(NavPosition.Bottom). 좌/우 전환은 코드비하인드가 처리 -->
            <ListBox x:Name="NavList"
                      DockPanel.Dock="Bottom"
                      Style="{StaticResource Ubisam.Style.SidebarBar.Horizontal}"
                      Height="56"
                      ItemsSource="{Binding MainTabs}"
                      SelectedItem="{Binding SelectedMainTab, Mode=TwoWay}">
                <ListBox.LayoutTransform>
                    <ScaleTransform ScaleX="{Binding UiSettings.ChromeScale}" ScaleY="{Binding UiSettings.ChromeScale}"/>
                </ListBox.LayoutTransform>
            </ListBox>

            <!-- 컨텐츠 + 하단 서브탭. 컨텐츠는 여백 없이 영역 전체를 쓴다 —
                 내부 여백은 각 모듈 View가 직접 정한다. -->
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <!-- 모듈이 마운트되기 전에는 사선 해치가 빈 슬롯을 표시 -->
                <Border Grid.Row="0" Background="{DynamicResource Ubisam.Brush.SlotHatch}">
                    <ContentControl Content="{Binding SelectedMainTab.CurrentContent}"/>
                </Border>

                <ListBox Grid.Row="1"
                          Style="{StaticResource Ubisam.Style.SubTabBar}"
                          DisplayMemberPath="Title"
                          ItemsSource="{Binding SelectedMainTab.SubTabs}"
                          SelectedItem="{Binding SelectedMainTab.SelectedSubTab, Mode=TwoWay}"
                          Visibility="{Binding SelectedMainTab.HasSubTabs, Converter={StaticResource BoolToVis}}">
                    <ListBox.LayoutTransform>
                        <ScaleTransform ScaleX="{Binding UiSettings.ChromeScale}" ScaleY="{Binding UiSettings.ChromeScale}"/>
                    </ListBox.LayoutTransform>
                </ListBox>
            </Grid>
        </DockPanel>

        <!-- 토스트 오버레이 -->
        <ItemsControl HorizontalAlignment="Right"
                      VerticalAlignment="Top"
                      Margin="0,80,20,0"
                      IsHitTestVisible="False">
            <ItemsControl.ItemsSource>
                <Binding Path="Messages" Source="{x:Static messaging:ToastService.Current}"/>
            </ItemsControl.ItemsSource>
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <Border Background="{Binding Type, Converter={StaticResource ToastTypeToBrush}}"
                            Padding="16,12"
                            Margin="0,0,0,8"
                            MaxWidth="360">
                        <TextBlock Text="{Binding Text}" Foreground="White" TextWrapping="Wrap" FontSize="13"/>
                    </Border>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </Grid>
</Window>
```

### `src/UbisamBase.Core/Shell/ShellViewModel.cs`

```csharp
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using UbisamBase.Core.Auth;
using UbisamBase.Core.Modules;
using UbisamBase.Core.Settings;
using UbisamBase.Core.Setup;

namespace UbisamBase.Core.Shell;

public partial class ShellViewModel : ObservableObject
{
    /// <summary>좌상단에 표기되는 회사명. 장비명(Title)과 구분선으로 나란히 놓인다.</summary>
    public string Brand { get; } = "UBISAM";

    /// <summary>좌상단 장비명. IAppSetup.GetAppName()이 돌려주는 값.</summary>
    public string Title { get; }

    public UiSettingsService UiSettings { get; }

    public AuthService Auth { get; }

    public ObservableCollection<MainTabViewModel> MainTabs { get; } = new();

    [ObservableProperty]
    private MainTabViewModel? selectedMainTab;

    [ObservableProperty]
    private string currentTime = DateTime.Now.ToString("HH:mm:ss");

    [ObservableProperty]
    private string currentDate = FormatDate(DateTime.Now);

    private readonly DispatcherTimer clockTimer;

    public ShellViewModel(IServiceProvider serviceProvider, IAppSetup appSetup, TabManager tabManager, UiSettingsService uiSettings, AuthService auth)
    {
        Title = appSetup.GetAppName();
        UiSettings = uiSettings;
        Auth = auth;

        foreach (var registration in tabManager.Registrations.OrderBy(m => m.Order))
        {
            var mainTab = new MainTabViewModel(
                registration.Title,
                registration.Icon,
                () => registration.ContentFactory(serviceProvider),
                registration.Order);

            foreach (var sub in registration.SubTabs.OrderBy(s => s.Order))
            {
                mainTab.AddSubTab(new SubTabViewModel(sub.Title, () => sub.ContentFactory(serviceProvider), sub.Order));
            }

            MainTabs.Add(mainTab);
        }

        SelectedMainTab = MainTabs.FirstOrDefault();

        clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        clockTimer.Tick += (_, _) =>
        {
            var now = DateTime.Now;
            CurrentTime = now.ToString("HH:mm:ss");
            CurrentDate = FormatDate(now);
        };
        clockTimer.Start();
    }

    private static string FormatDate(DateTime value)
    {
        var day = value.DayOfWeek switch
        {
            DayOfWeek.Sunday => "일",
            DayOfWeek.Monday => "월",
            DayOfWeek.Tuesday => "화",
            DayOfWeek.Wednesday => "수",
            DayOfWeek.Thursday => "목",
            DayOfWeek.Friday => "금",
            _ => "토"
        };

        return $"{value:yyyy.MM.dd} {day}";
    }
}
```

### `src/UbisamBase.Core/Converters/IconKeyToGeometryConverter.cs`

```csharp
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
```

## 3. 직접 수정해야 하는 기존 파일 2곳

### `src/UbisamBase.Core/Shell/ShellWindow.xaml.cs`

`ApplyNavPosition`에서 하단 바 높이를 100 → 72로 바꾸세요 (아이콘 + 라벨 두 줄 높이).

```csharp
NavList.Width = isHorizontal ? double.NaN : 140;
NavList.Height = isHorizontal ? 72 : double.NaN;
```

### `src/UbisamBase.Core/Settings/UiSettingsService.cs`

기본 네비게이션 위치를 하단으로 바꾸세요.

```csharp
[ObservableProperty]
private NavPosition navPosition = NavPosition.Bottom;
```

## 4. 탭 등록 예시 (`src/UbisamBase.App/AppSetup.cs`)

`icon` 인자는 이제 아이콘 이름입니다. 기존 등록에 이름을 채워 넣으세요.

```csharp
manager.AddMain<HomeView, HomeViewModel>(ViewIds.Home, "홈", "Home");
manager.AddMain<SettingsView, SettingsViewModel>(ViewIds.Settings, "설정", "Settings");
manager.AddSub<GeneralSettingsView, GeneralSettingsViewModel>(ViewIds.Settings, "일반");
```

사용 가능한 아이콘 이름 30종: Home, Monitor, Chart, Maintenance, Settings, Alarm, Log, Document, Data, Power, Temperature, Gauge, Run, Stop, Reset, Account, Lock, Export, Import, Search, Calendar, Clock, Link, Warning, Pass, Folder, Vision, Control, Recipe, Modules.

아이콘을 더 늘릴 때는 Lucide(https://lucide.dev)에서 **24×24 그리드 / 스트로크 1.5** 규칙으로 가져와 `Icons.xaml`에 `Geometry` 한 줄로 등록하세요. SVG의 `<circle>`/`<rect>`는 WPF Geometry 문법(호 `A` 두 개 / `M…L…Z`)으로 옮겨야 합니다.

## 5. 완료 후 확인할 것

1. 빌드가 통과하는지 (`dotnet build` 또는 Visual Studio)
2. `Icons.xaml`이 빌드에 포함되는지 — SDK 스타일 csproj + `UseWPF`라 자동 포함되지만, 리소스 조회 실패 시 `Page`로 명시 추가
3. 하단 탭이 창 폭에 균등 분할되고, 선택된 탭에 상단 accent 바가 보이는지
4. 설정 화면에서 테마를 Light/Dark로 바꿀 때 상단바·하단 탭·아이콘 색이 함께 전환되는지
5. 화면 배율(ChromeScale) 슬라이더를 움직였을 때 아이콘이 깨지지 않고 확대되는지
6. 아이콘 이름을 넘기지 않은 탭이 라벨만으로 정상 표시되는지

## 6. 하지 말 것

- 모듈 View(`HomeView`, `SettingsView`, `GeneralSettingsView` 등)의 내용은 건드리지 마세요 — 셸만 바꿉니다.
- 컨텐츠 영역에 제목/헤더를 다시 넣지 마세요.
- 기존 `TabManager`, `EventBus`, `ToastService`, `AuthService`, `LogService`의 구조나 시그니처를 바꾸지 마세요. `ShellViewModel`은 `Brand`/`CurrentDate` 추가 외에는 동일합니다.

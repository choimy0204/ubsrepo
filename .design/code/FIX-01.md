# 패치 01 — 적용 후 발견된 3가지 수정

첫 적용 후 실행 화면에서 나온 문제를 고치는 패치입니다. 이 문서를 Claude Code에 붙여넣고 "이 패치 적용해줘"라고 요청하세요.
`CLAUDE_CODE_PROMPT.md`를 이미 적용한 상태를 전제로 합니다.

## 문제 1 — 하단 탭 바가 잘린다

`ShellWindow.xaml`에서 `NavList`에 `Height="56"`이 박혀 있는데, 아이콘(22) + 간격(7) + 라벨 + 상하 패딩을 합치면 56을 넘겨서 라벨이 창 밖으로 밀립니다. **높이 지정을 지우고 내용 높이에 맡깁니다.**

`ShellWindow.xaml`의 `NavList`에서 이 줄을 삭제:

```xml
Height="56"
```

그리고 `ShellWindow.xaml.cs`의 `ApplyNavPosition`도 하단일 때 높이를 강제하지 않도록:

```csharp
NavList.Width = isHorizontal ? double.NaN : 140;
NavList.Height = double.NaN;
```

## 문제 2 — 모듈이 떠 있는데도 사선 해치가 배경에 깔린다

해치는 **빈 슬롯 표시용**이라 모듈이 마운트되면 사라져야 합니다. `ShellWindow.xaml`의 컨텐츠 `Border`를 아래로 교체하세요 — `CurrentContent`가 null일 때만 해치가 됩니다.

```xml
<Border Grid.Row="0" Padding="26,22">
    <Border.Style>
        <Style TargetType="Border">
            <Setter Property="Background" Value="{DynamicResource Ubisam.Brush.ContentBackground}"/>
            <Style.Triggers>
                <DataTrigger Binding="{Binding SelectedMainTab.CurrentContent}" Value="{x:Null}">
                    <Setter Property="Background" Value="{DynamicResource Ubisam.Brush.SlotHatch}"/>
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </Border.Style>
    <ContentControl Content="{Binding SelectedMainTab.CurrentContent}"/>
</Border>
```

## 문제 3 — 컨텐츠가 화면 왼쪽 끝에 붙는다

위 `Border`의 `Padding="26,22"`가 이 문제를 함께 해결합니다. 상단 헤더(`Padding="26,14"`)와 세로선이 맞습니다.

`Margin`이 아니라 `Padding`인 게 중요합니다 — 배경(해치/바탕색)은 영역 전체를 채우고, 안의 모듈 컨텐츠만 26px 안으로 들어옵니다. 모듈 View는 계속 여백 없이 작성해도 됩니다.

## 문제 4 (덤) — 아이콘 없는 탭도 아이콘 높이를 차지한다

`ShellStyles.xaml`의 `Ubisam.Style.SidebarItem.Horizontal`에서 `Path`에 `x:Name="Ico"`를 주고, 트리거를 패딩 변경이 아니라 **Path 숨김**으로 바꾸세요.

```xml
<Path x:Name="Ico"
      Data="{Binding Icon, Converter={StaticResource IconToGeometry}}"
      ... />
```

```xml
<DataTrigger Binding="{Binding Icon, Converter={StaticResource IconToGeometry}}" Value="{x:Null}">
    <Setter TargetName="Ico" Property="Visibility" Value="Collapsed"/>
</DataTrigger>
```

## 참고 — 이번 화면에서 아이콘이 안 보인 이유

`AppSetup.cs`의 `AddMain`에 `icon` 인자를 아직 넘기지 않아서 라벨만 나온 상태입니다. 정상 동작입니다. 아이콘을 켜려면:

```csharp
manager.AddMain<HomeView, HomeViewModel>(ViewIds.Home, "홈", "Home");
manager.AddMain<ToolView, ToolViewModel>(ViewIds.Tool, "도구", "Control");
manager.AddMain<SettingsView, SettingsViewModel>(ViewIds.Settings, "설정", "Settings");
```

## 확인

1. 하단 탭 바가 잘리지 않고 라벨(과 아이콘)이 온전히 보이는지
2. 설정 화면처럼 모듈이 떠 있을 때 배경이 해치가 아닌 단색인지
3. 컨텐츠 좌측이 상단 로고와 세로로 맞는지
4. 화면 배율을 1.6까지 올렸을 때 하단 탭 바가 같이 커지고 잘리지 않는지

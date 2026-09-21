# 패치 02 — 하단 탭 바가 너무 넓다

하단 탭이 화면 폭을 균등 분할해서 칸이 과하게 커졌습니다. **가운데에 자기 크기만큼 모이도록** 바꿉니다
(디자인 시안 3a와 같은 방식). 바 자체는 폭 전체를 쓰고 상단 경계선도 그대로 이어집니다.

`src/UbisamBase.Core/Themes/ShellStyles.xaml` 두 곳만 고치면 됩니다.

## 1. `Ubisam.Style.SidebarBar.Horizontal`

ItemsPanel을 `UniformGrid`에서 가운데 정렬 `StackPanel`로 바꾸고, ListBox의 `HorizontalContentAlignment="Stretch"` Setter를 삭제하세요.

```xml
<Setter Property="ItemsPanel">
    <Setter.Value>
        <ItemsPanelTemplate>
            <StackPanel Orientation="Horizontal" HorizontalAlignment="Center"/>
        </ItemsPanelTemplate>
    </Setter.Value>
</Setter>
```

## 2. `Ubisam.Style.SidebarItem.Horizontal`

칸 너비를 라벨 길이 + 좌우 패딩으로 정하도록, 패딩을 키우고 아이템의 `HorizontalContentAlignment="Stretch"` Setter를 삭제하세요.

```xml
<Setter Property="Padding" Value="34,12,34,13"/>
```

## 확인

- 탭 3~5개가 화면 가운데에 붙어 있고, 각 칸이 라벨 길이에 맞게 좁아졌는지
- 상단 경계선(1px)은 여전히 화면 폭 끝까지 이어지는지
- 탭이 늘어나 폭을 넘기면 좌우로 스크롤되지 않고 잘리는지 — 그런 경우 알려주세요. `ScrollViewer.HorizontalScrollBarVisibility`를 `Auto`로 되돌리면 됩니다.
- 여전히 넓게 느껴지면 패딩 34 → 24로 줄이면 됩니다.

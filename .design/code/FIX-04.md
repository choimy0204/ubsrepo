# 패치 04 — 입력 컨트롤 · 표 스타일

TextBox / ComboBox / CheckBox / Button / ListBox / ListView(GridView) 를 셸과 같은 도면 톤으로 맞춥니다.
각진 모서리 · 1px 헤어라인 · 포커스는 accent 테두리 + 옅은 글로우. 라이트/다크 모두 토큰으로 전환됩니다.

전부 **암시적 스타일**(`x:Key` 없음)이라 각 View를 고칠 필요가 없습니다 — 셸 안의 모든 해당 컨트롤에 자동 적용됩니다.
예외를 두려면 그 컨트롤에 `Style="{x:Null}"`을 주세요.

## 1. 새 파일

`src/UbisamBase.Core/Themes/Controls.xaml` — 이 프로젝트의 `code/UbisamBase.Core/Themes/Controls.xaml`을 그대로 복사하세요.

## 2. `Themes/ShellStyles.xaml`

머지 목록에 Controls.xaml을 추가하고, ComboBox 드롭다운 그림자를 정의합니다.

```xml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="pack://application:,,,/UbisamBase.Core;component/Themes/Icons.xaml"/>
    <ResourceDictionary Source="pack://application:,,,/UbisamBase.Core;component/Themes/Controls.xaml"/>
</ResourceDictionary.MergedDictionaries>
```

```xml
<DropShadowEffect x:Key="Ubisam.Effect.Popup" BlurRadius="12" ShadowDepth="3" Direction="270" Opacity="0.22" Color="#000000"/>
```

## 3. `Themes/Colors.Light.xaml` / `Colors.Dark.xaml`

입력·표 토큰을 추가합니다 (`code/`의 두 파일에 이미 들어 있습니다).

| 키 | 라이트 | 다크 |
| --- | --- | --- |
| `Ubisam.Brush.Input.Background` | #FFFFFF | #1B1D23 |
| `Ubisam.Brush.Input.Border` | #C9C9CC | #33363E |
| `Ubisam.Brush.Input.BorderHover` | #A8A8AC | #4A4E57 |
| `Ubisam.Brush.Input.DisabledBackground` | #EDEDEF | #191A1F |
| `Ubisam.Brush.Input.DisabledBorder` | #DEDEE1 | #26282F |
| `Ubisam.Color.AccentGlow` (Color) | #2196F3 | #22D3EE |
| `Ubisam.Brush.Table.HeaderBackground` | #EDEDEF | #1B1D23 |
| `Ubisam.Brush.Table.RowBorder` | #EDEDEF | #1F2127 |

## 4. 버튼 종류

암시적 Button은 **보조(외곽선)** 입니다. 주 동작에는 키를 지정하세요.

```xml
<Button Content="저장" Command="{Binding SaveCommand}" Style="{StaticResource Ubisam.Style.Button.Primary}"/>
<Button Content="되돌리기" Style="{StaticResource Ubisam.Style.Button.Ghost}"/>
```

`AllSettingsView.xaml`의 "저장" 버튼에 Primary를 적용하고, 거기 붙어 있는 `Padding="14,6"`은 지우세요(스타일이 정합니다).

## 5. 확인

1. `설정 → 설정값` 화면의 TextBox/ComboBox/CheckBox가 각진 모양으로 바뀌고, 클릭 시 테두리가 accent로 변하는지
2. ComboBox를 열면 드롭다운이 입력창 아래에 붙고, 선택 항목이 accent 채움인지
3. `로그` 화면 표의 헤더가 회색 판 + 1px 경계, 행 hover/선택이 accent 틴트인지
4. 다크 테마로 바꿨을 때 입력창 배경이 `#1B1D23`으로 함께 바뀌는지
5. 비활성 컨트롤이 흐리게(45%) 보이는지

## 주의

- `Controls.xaml`은 폰트 리소스를 `DynamicResource`로 참조합니다. `StaticResource`로 바꾸면 머지 시점 때문에 로드가 실패합니다.
- 암시적 `ListBox`/`ListBoxItem` 스타일이 추가되지만, 하단 탭과 서브탭 바는 `Style`/`ItemContainerStyle`을 명시적으로 지정하고 있어 영향받지 않습니다.
- `ScrollBar`와 `Slider`는 이번 패치에 없습니다 — 필요하면 알려주세요.

## 6. 아이콘 세트 갱신 (Icons.xaml 재발행)

`Themes/Icons.xaml`을 Lucide 원본 경로 기준으로 다시 만들었습니다. 이전 버전은 손으로 근사한 도형이라 실제 Lucide와 미세하게 달랐습니다 — `code/UbisamBase.Core/Themes/Icons.xaml`을 그대로 덮어쓰세요.

- 좌표계·스트로크 규칙은 동일(24×24, 1.5, 둥근 캡)해서 XAML 다른 곳은 고칠 필요가 없습니다.
- 리소스 키(`Ubisam.Icon.Home` 등)와 `AddMain`에 넘기는 이름도 그대로입니다.
- SVG 원본은 `icons/` 폴더에 있고, 각 아이콘의 Lucide 이름은 `icons/readme.md` 표에 적어뒀습니다. 새 아이콘은 그 이름으로 lucide.dev에서 찾아 추가하세요.

# 2a 시안 적용 — 변경 파일과 수동 편집 2곳

`Ubisam Base Platform.dc.html`의 **2a**(도면 콘솔 · 하단 5분할 탭 · 여백 없는 메인)를 실제 셸에 반영한 파일들입니다.
`code/` 아래 경로는 리포지토리의 `src/` 아래 경로와 1:1 대응합니다. 그대로 덮어쓰면 됩니다.

| 이 프로젝트의 파일 | 리포지토리 경로 |
| --- | --- |
| `code/UbisamBase.Core/Themes/Colors.Light.xaml` | `src/UbisamBase.Core/Themes/Colors.Light.xaml` |
| `code/UbisamBase.Core/Themes/Colors.Dark.xaml` | `src/UbisamBase.Core/Themes/Colors.Dark.xaml` |
| `code/UbisamBase.Core/Themes/ShellStyles.xaml` | `src/UbisamBase.Core/Themes/ShellStyles.xaml` |
| `code/UbisamBase.Core/Themes/Icons.xaml` | `src/UbisamBase.Core/Themes/Icons.xaml` (신규) |
| `code/UbisamBase.Core/Converters/IconKeyToGeometryConverter.cs` | `src/UbisamBase.Core/Converters/IconKeyToGeometryConverter.cs` (신규) |
| `code/UbisamBase.Core/Shell/ShellWindow.xaml` | `src/UbisamBase.Core/Shell/ShellWindow.xaml` |
| `code/UbisamBase.Core/Shell/ShellViewModel.cs` | `src/UbisamBase.Core/Shell/ShellViewModel.cs` |

## 무엇이 바뀌었나

**Colors.Light.xaml** — 바탕을 흰색(#FFFFFF)에서 도면 톤 밝은 회색(#F2F2F3)으로, 상단바·하단 탭 바를 흰 판으로 분리했습니다. accent는 회사 하늘색 #2196F3 그대로. 하늘색 틴트 배경(#EAF4FD 계열)은 선택 상태에만 남기고 판 전체에서는 뺐습니다.

**Colors.Dark.xaml** — 기존 값 유지, 라이트의 반전 관계만 정리(#121317 바탕 / #16171C 판 / #2A2C33 경계 / #22D3EE accent).

**ShellStyles.xaml**
- `SidebarBar.Horizontal`의 ItemsPanel을 `StackPanel` → `UniformGrid Rows="1"`로 바꿔 하단 탭이 **등록된 탭 수만큼 균등 분할**됩니다.
- 하단 탭 아이템은 아이콘+라벨 가로 배치 → **라벨만, 중앙 정렬, 선택 시 상단 3px accent 바**. 칸 사이 세로 헤어라인 추가.
- 서브탭의 알약(CornerRadius 20) → **각진 세그먼트**, ChromeButton도 CornerRadius 0.
- `Ubisam.Font.Heading` / `Ubisam.Font.Body` 폰트 리소스 추가 (Barlow → 없으면 Segoe UI 폴백).
- 빈 슬롯 표시용 `Ubisam.Brush.SlotHatch` DrawingBrush 추가.

**ShellWindow.xaml**
- 좌상단: 로고 사각형 + 회사명(`Brand`) + 구분선 + **장비명**(`Title`). 로고 이미지가 준비되면 해당 `Border`를 `<Image Source="..."/>`로 교체하세요.
- 우상단: 날짜(작게) + **시계 25px**.
- `NavList`의 기본 Dock을 `Left` → `Bottom`, Style을 Horizontal, Height 56.
- 컨텐츠 `Margin="32"` → **여백 제거**. 내부 여백은 각 모듈 View가 정합니다.
- 컨텐츠 영역에 제목줄은 두지 않습니다 — 선택된 하단 탭과 중복이라 뺐습니다.

**ShellViewModel.cs** — `Brand`(회사명, 기본 "UBISAM")와 `CurrentDate` 추가. 시계 타이머가 날짜도 같이 갱신합니다.

**Icons.xaml (신규)** — 탭 아이콘 24종을 24×24 그리드 / 스트로크 1.5 규칙으로 `Geometry` 리소스로 등록했습니다(`Ubisam.Icon.Home` … `Ubisam.Icon.Modules`). 아이콘 폰트 글리프 대신 벡터라 배율(ChromeScale)에 깨지지 않습니다.

**IconKeyToGeometryConverter.cs (신규)** — `AddMain`의 `icon` 인자를 글리프 코드가 아니라 **아이콘 이름**으로 받게 해줍니다. 등록은 이렇게:

```csharp
manager.AddMain<HomeView, HomeViewModel>(ViewIds.Home, "홈", "Home");
manager.AddMain<MonitorView, MonitorViewModel>(ViewIds.Monitor, "모니터링·그래프", "Monitor");
manager.AddMain<ReportView, ReportViewModel>(ViewIds.Report, "통계·리포트", "Chart");
manager.AddMain<MaintenanceView, MaintenanceViewModel>(ViewIds.Maintenance, "유지보수", "Maintenance");
manager.AddMain<SettingsView, SettingsViewModel>(ViewIds.Settings, "설정", "Settings");
```

`icon`을 생략하면 그 탭은 라벨만 표시됩니다. 쓸 수 있는 이름: Home, Monitor, Chart, Maintenance, Settings, Alarm, Log, Document, Data, Power, Temperature, Gauge, Run, Stop, Reset, Account, Lock, Export, Import, Search, Calendar, Clock, Link, Warning, Pass, Folder, Vision, Control, Recipe, Modules.

아이콘을 더 늘릴 때는 Lucide(https://lucide.dev)에서 같은 24px/1.5 규칙으로 가져와 `Icons.xaml`에 `Geometry` 한 줄 추가하면 끝입니다. `Icons.xaml`은 `.csproj`에 `Page`로 포함되어야 합니다(다른 Themes/*.xaml과 동일 처리).

## 직접 고쳐야 하는 2곳

1. `src/UbisamBase.Core/Shell/ShellWindow.xaml.cs` — `ApplyNavPosition` 마지막 줄의 하단 바 높이를 100 → 72로(아이콘+라벨 두 줄):

```csharp
NavList.Width = isHorizontal ? double.NaN : 140;
NavList.Height = isHorizontal ? 72 : double.NaN;
```

2. `src/UbisamBase.Core/Settings/UiSettingsService.cs` — 기본 네비게이션 위치를 하단으로:

```csharp
[ObservableProperty]
private NavPosition navPosition = NavPosition.Bottom;
```

## 확인 안 된 것

- Barlow / Barlow Condensed가 대상 PC에 설치돼 있는지 — 없으면 Segoe UI로 폴백되어 도면 느낌이 약해집니다. 설치하거나 프로젝트에 폰트를 임베드할지 알려주세요.
- 회사 로고 이미지 파일 — 아직 없어서 accent 사각형 + 첫 글자로 두었습니다.
- 빌드는 이 환경에서 돌려보지 못했습니다(WPF/.NET Framework 4.8). 빌드 후 하단 탭 정렬과 다크 전환만 눈으로 확인해 주세요.
- 아이콘 Geometry는 SVG 경로를 WPF 문법으로 옮긴 것입니다. 원·타원은 호(A) 두 개로 근사했으니, 렌더링 후 Data/Power/Account 아이콘의 곡선만 확인해 주세요.

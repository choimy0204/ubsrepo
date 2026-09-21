# UbisamBase 플랫폼 사용 가이드 (AI/개발자 참고용)

이 문서는 다른 AI 세션이나 개발자가 이 플랫폼으로 새 프로그램을 만들거나 기존 코드를 이식할 때
빠르게 참고할 수 있도록 정리한 기술 레퍼런스입니다. 사람이 읽는 슬라이드 버전은
`doc/UbisamBase_플랫폼_사용_매뉴얼.pptx`를 참고하세요.

## 0. 구조

```
UbisamBase.Core      공통 서비스 + Shell UI 라이브러리 (여기 손대면 플랫폼 전체가 바뀜)
UbisamBase.Shell     고정 호스트. 같은 폴더의 IAppSetup 구현 dll을 리플렉션으로 로드
UbisamBase.Launcher  실행 진입점. 실행할 때마다 D:\UbisamPlatform\bin을 자기 폴더로 복사한 뒤
                     Shell을 같은 프로세스 안에서 띄운다 (F5도, 사용자 실행도 이걸로 시작)
소비 프로젝트          UbisamBase.Core를 참조하고 IAppSetup만 구현하는 실제 프로그램
```

**저장소 위치**: Core/Shell 소스는 `D:\업무\03_개발\Source\용접기\08_UbisamPlatform`
저장소에 있다. 소비 프로젝트(예제 `UbisamBase.App`)는 `08_UbisamBase` 저장소에 있다.
Core/Shell을 참조하는 방식(모든 프로그램이 D드라이브 배포 폴더 하나를 본다)의 전체 설명은 10-2 참고 — 아래는
그 요약이자, 새 프로젝트를 만드는 순간부터 바로 따라 할 수 있는 실전 절차다.

---

## 0-1. 새 소비 프로젝트 만들기 — 생성 직후 플랫폼 연결

### 프로젝트 생성 버전 / 설정

이 플랫폼은 **.NET Framework 4.8(`net48`) 전용**이다. Visual Studio의 "새 프로젝트 만들기"
마법사로 시작한다면:

| 선택 항목 | 값 |
| --- | --- |
| 대상 프레임워크 | **.NET Framework 4.8** (netcoreapp/net6+ 아님) |
| 프로젝트 형식 | **클래스 라이브러리** (WPF 앱 템플릿 아님 — 실행은 `UbisamBase.Launcher.exe`가 하고, 이 프로젝트는 그 안에서 Shell이 리플렉션으로 로드하는 dll일 뿐이다) |
| 플랫폼 | **Any CPU만** (`<Platforms>`에 x64를 넣지 않는다 — 출력 폴더가 `bin\x64\...`로 바뀌어 F5 실행 경로와 어긋난다) |
| SDK 스타일 | 새 SDK 스타일 csproj (`<Project Sdk="Microsoft.NET.Sdk">`) |

VS의 클래스 라이브러리(.NET Framework) 템플릿에는 WPF 활성화 옵션이 따로 없고, 아래에 정리한
`CopyPlatformRuntime` 타겟 등 이 플랫폼 특유의 설정도 마법사가 만들어주지 않는다. 그래서
실무적으로는 둘 중 하나를 쓴다:

- **(권장) 기존 소비 프로젝트를 복사해서 시작** — `08_UbisamBase\src\UbisamBase.App` 폴더
  전체를 복사하고 폴더/파일 이름, `AssemblyName`/`RootNamespace`만 바꾼다. 아래 csproj의
  모든 설정이 이미 정답 상태로 들어있다.
- **처음부터 만들기** — 빈 폴더에 아래 `.csproj`를 그대로 붙여넣는다.

### `.csproj` 템플릿

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Library</OutputType>
    <TargetFramework>net48</TargetFramework>
    <UseWPF>true</UseWPF>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <RootNamespace>MyApp</RootNamespace>
    <AssemblyName>MyApp</AssemblyName>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>

    <!-- 플랫폼 배포 폴더. 소스 코드가 없는 컴파일된 dll/exe만 있다(.pdb/.cs 없음).
         플랫폼 원본(08_UbisamPlatform)을 빌드하면 여기에 자동 반영된다. 모든 프로그램이 이 값을
         그대로 쓴다 - 플랫폼을 고치는 개발자도 마찬가지(10-2 참고). -->
    <UbisamPlatformDir>D:\UbisamPlatform\bin\</UbisamPlatformDir>

    <!-- F5(디버그 시작) 시 Launcher.exe가 실행되도록 지정 — 이 프로젝트 자체는 dll이라 직접 실행 불가.
         Launcher는 실행할 때마다 D:\UbisamPlatform\bin의 최신 내용을 이 폴더로 복사한 뒤 Shell을
         같은 프로세스에서 띄운다. 그래서 다시 빌드하지 않아도 최신 플랫폼으로 뜨고, 이 프로젝트에
         찍은 중단점도 정상으로 걸린다.
         주의: 여기서 $(TargetDir)를 쓰면 안 된다. 이 시점엔 아직 비어 있어서 VS가
         "UbisamBase.Launcher.exe 프로그램을 시작할 수 없습니다 / 파일을 찾을 수 없습니다"로 실패한다. -->
    <StartAction>Program</StartAction>
    <StartProgram>$(MSBuildProjectDirectory)\bin\$(Configuration)\$(TargetFramework)\UbisamBase.Launcher.exe</StartProgram>
    <StartWorkingDirectory>$(MSBuildProjectDirectory)\bin\$(Configuration)\$(TargetFramework)\</StartWorkingDirectory>

    <!-- VS의 "빠른 최신 상태 확인"이 아래 CopyPlatformRuntime 복사를 추적 못 해서
         소스 변경 없으면 빌드를 건너뛰려 하는 문제를 막는다 -->
    <DisableFastUpToDateCheck>true</DisableFastUpToDateCheck>
  </PropertyGroup>

  <ItemGroup>
    <!-- 프로젝트 참조가 아니라 배포된 dll을 그대로 가져다 쓰는 파일 참조 -->
    <Reference Include="UbisamBase.Core">
      <HintPath>$(UbisamPlatformDir)UbisamBase.Core.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <!-- IAppSetup.RegisterServices(IServiceCollection) 구현에 필요 -->
    <Reference Include="Microsoft.Extensions.DependencyInjection.Abstractions">
      <HintPath>$(UbisamPlatformDir)Microsoft.Extensions.DependencyInjection.Abstractions.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <!-- [ObservableProperty]/[RelayCommand]를 쓰려면 반드시 PackageReference로 받아야 한다
         (소스 제너레이터가 필요해서 dll 파일 참조만으로는 안 됨) -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.2" />
  </ItemGroup>

  <!-- 차트를 쓴다면 이 4개도 같은 방식(HintPath, Private=false)으로 추가:
       LiveChartsCore, LiveChartsCore.SkiaSharpView, LiveChartsCore.SkiaSharpView.WPF, SkiaSharp -->

  <!-- 플랫폼 런타임 전체(Shell.exe + Core.dll + nuget 런타임)를 이 프로젝트 출력 폴더로 복사 -->
  <Target Name="CopyPlatformRuntime" BeforeTargets="Build">
    <ItemGroup>
      <_PlatformFiles Include="$(UbisamPlatformDir)**\*.*" />
    </ItemGroup>
    <MakeDir Directories="$(TargetDir)" Condition="!Exists('$(TargetDir)')" />
    <Copy SourceFiles="@(_PlatformFiles)" DestinationFiles="@(_PlatformFiles->'$(TargetDir)%(RecursiveDir)%(Filename)%(Extension)')" SkipUnchangedFiles="true" />
  </Target>

</Project>
```

### 확인 순서

1. `dotnet build`(또는 VS 빌드) — 출력 폴더 `bin\Debug\net48\`(※ `bin\x64\...`가 아니어야 함)에
   `UbisamBase.Launcher.exe`, `UbisamBase.Launcher.exe.config`, `UbisamBase.Shell.exe`,
   `UbisamBase.Core.dll`, 이 프로젝트의 dll이 같이 생겼는지 확인.
2. 아직 `IAppSetup`을 구현하지 않았으면 F5 시 "IAppSetup을 구현한 dll을 … 찾지 못했습니다"
   예외로 멈춘다 — 이 메시지가 나오면 참조·복사·실행 경로 연결은 성공한 것이다.
3. 다음(0-2)에서 `IAppSetup`을 구현하면 실제 화면이 붙는다.
4. F5로 뜬 상태에서 작업 관리자를 보면 **`UbisamBase.Launcher` 프로세스 하나만** 있어야 한다.
   `UbisamBase.Shell`이 별도 프로세스로 따로 떠 있으면 옛 Launcher(별도 프로세스 방식)라서
   디버거가 안 붙고 중단점이 안 걸린다 → `D:\UbisamPlatform\bin`을 최신 배포본으로 교체.
5. (선택) 이 저장소에 `.slnx` 솔루션이 있다면 `<Project Path="src/MyApp/MyApp.csproj" />`를
   추가해 VS 솔루션 탐색기에 노출시킨다. 이때 `<Platform Name="x64" />` 같은 x64 구성은 넣지 않는다.

### 자주 겪는 실행 오류

| 증상 | 원인 | 조치 |
| --- | --- | --- |
| F5 시 "`UbisamBase.Launcher.exe` 프로그램을 시작할 수 없습니다 / 지정된 파일을 찾을 수 없습니다" | `StartProgram`에 `$(TargetDir)`를 썼다 — PropertyGroup 시점엔 비어 있어 파일명만 남는다 | 위 템플릿처럼 `$(MSBuildProjectDirectory)\bin\$(Configuration)\$(TargetFramework)\` 절대경로로 |
| 빌드 출력이 `bin\x64\Debug\net48\`로 나온다 | csproj `<Platforms>`에 x64가 들어갔거나 솔루션이 x64로 매핑됨 | csproj에서 `<Platforms>` 삭제, `.slnx`의 x64 `Platform` 항목 삭제, 구성 관리자 활성 플랫폼 = Any CPU |
| VS 상단에 "알 수 없는 프로젝트 구성 매핑이 있습니다" 배너 | `.slnx`에 프로젝트에 없는 플랫폼(x64) 매핑이 남음 | `.slnx`에서 해당 `<Platform .../>` 삭제 후 VS 재시작 |
| 켜졌다가 꺼지고 새 창으로 다시 뜬다 / 중단점이 안 걸린다 | 배포본의 Launcher가 옛 버전(Shell을 별도 프로세스로 띄움) | `D:\UbisamPlatform\bin` 최신화 후 재빌드 |
| 실행 직후 창 없이 조용히 종료 (이벤트 뷰어에 `FileLoadException`) | `UbisamBase.Launcher.exe.config`에 바인딩 리디렉트가 없음(옛 배포본) | `D:\UbisamPlatform\bin` 최신화 — 플랫폼을 빌드하면 Shell 설정이 Launcher 설정으로 복사된다(다른 PC면 최신 배포 폴더를 통째로 다시 복사) |
| 컴파일 에러 "…형식이 없습니다", "…정의가 포함되어 있지 않습니다" | 코드가 기대하는 플랫폼 API가 배포본에 없다 | 배포본 최신화. 그래도 없으면 플랫폼에 추가할지, 코드에서 뺄지 결정 |

---

## 0-2. `IAppSetup` 구현

새 프로그램 = `IAppSetup` 구현 클래스 하나 + 필요한 View/ViewModel. 나머지(테마, 네비게이션,
설정 저장, 로그, 대화상자, 토스트, 잠금 등)는 전부 플랫폼이 제공합니다.

```csharp
public class AppSetup : IAppSetup
{
    public string GetAppName() => "내 프로그램 이름"; // 상단 브랜딩 바에 표시

    public void RegisterViews(ITabManager manager)
    {
        manager.AddMain<HomeView, HomeViewModel>(Icon.Home, ViewIds.Home, "홈");
        manager.AddSub<DetailView, DetailViewModel>(ViewIds.Home, "상세");
    }

    public void RegisterSettings(ITabManager manager)
    {
        manager.AddSettingsSub<MySettings>(TabManager.SettingsTabKey, "내 설정");
    }

    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<MyLegacyService>(); // 기존 코드를 여기서 DI에 등록
    }
}
```

`AddMain`/`AddSub`로 등록한 View는 DI가 자동으로 ViewModel을 만들어 `DataContext`에 연결합니다.
ViewModel 생성자에 필요한 서비스(플랫폼 제공이든 RegisterServices로 직접 등록한 것이든)를
선언하기만 하면 주입됩니다 — `ActivatorUtilities.CreateInstance` 기반이라 별도 배선이 필요 없습니다.

---

## 1. 설정 화면

### 1-1. 자동 생성 (View 없이)

```csharp
public class MySettings
{
    [Category("장비")]
    [DisplayName("측정값 개수")]
    public int MeasureCount { get; set; }

    [Category("장비")]
    public bool UseAutoSave { get; set; }

    [Category("데이터")]
    public ObservableCollection<Row> Rows { get; set; } = new();
}

manager.AddSettingsSub<MySettings>(TabManager.SettingsTabKey, "내 설정");
```

- 프로퍼티 타입 → 컨트롤 자동 매핑: `bool`→체크박스, `int/long/short`→정수 텍스트박스,
  `float/double/decimal`→소수 텍스트박스, `DateTime`→달력, `string`→텍스트박스,
  `ObservableCollection<T>`→DataGrid.
- `[Category("말머리")]` 같은 것끼리 화면에서 그룹으로 묶임 (없으면 말머리 없는 그룹).
- `[DisplayName("이름")]` 생략 시 프로퍼티 이름 그대로 표시.
- `[Browsable(false)]`면 화면에서 숨김.
- 저장 파일: `D:\UbisamConfig\Settings\{클래스이름}.json` — 어느 탭에 붙여도 클래스가 같으면
  같은 파일 공유.

### 1-2. 직접 만든 View + 수동 등록

View를 직접 그리고 싶으면 `ISettingsService`를 주입받아 등록합니다.

```csharp
public MyViewModel(ISettingsService settingsService)
{
    settings = settingsService.RegisterSettings("MyKey", new MySettingsClass());
}
// settingsService.Save("MyKey"); / settingsService.Reload("MyKey");
```

### 1-3. 저장/새로고침 버튼 (플랫폼 자동 지원)

모든 설정 화면 우상단에 파란색 저장/새로고침 버튼이 동일하게 표시됩니다. 직접 만든 화면에서도
`SettingsToolbar` 컴포넌트를 붙이면 됩니다 — `DataContext`를 상속받아 호스트 ViewModel의
`RefreshCommand`/`SaveCommand`(`[RelayCommand] private void Refresh()/Save()`)에 바인딩됩니다.

```xml
<local:SettingsToolbar DockPanel.Dock="Top" Margin="0,0,0,16"/>
```

저장/새로고침 성공·실패 시 Success/Error 토스트가 자동으로 뜨도록 구현부에서
`MessageUtil.ShowSavedToast()` / `MessageUtil.ShowErrorToast(...)`를 호출하는 패턴을 따르세요
(플랫폼이 등록한 기본 4개 설정 화면이 이미 이 패턴입니다).

---

## 2. 로깅 — `UbisamBase.Core.Logging`

```csharp
private readonly Logger log = new Logger("MyFeature"); // DI 없이 어디서든 생성 가능

log.V("verbose"); log.D("debug"); log.I("info");
log.W("warning"); log.E("error"); log.E("error", ex);
```

- 호출 즉시 `D:\UbisamConfig\Log\yyyyMMdd.log`에 기록 + 설정 > 로그 탭(`LogService.Current.RecentEntries`)에 실시간 반영.
- **버튼 클릭 자동 로깅**: `ButtonClickLogger.Install()`이 부팅 시 한 번 걸려 있어서
  (`EventManager.RegisterClassHandler(typeof(ButtonBase), ButtonBase.ClickEvent, ...)`),
  플랫폼 전체(Core + 모든 소비 프로그램)의 모든 `Button`/`ToggleButton` 클릭이
  코드 한 줄 없이 `"UI"` 태그로 자동 기록됩니다. 버튼마다 로그 코드를 넣을 필요가 없습니다.
- 로그 뷰어: DataGrid, 레벨 필터(ALL = 전부, 특정 레벨 = 정확히 일치), 검색(LIKE, 부분 포함),
  우클릭 복사.

---

## 3. 대화상자 / 토스트 — `UbisamBase.Core.Messaging`

```csharp
// 대화상자 — OS MessageBox 대신 테마 적용된 MessageDialog가 자동으로 뜬다
MessageUtil.ShowInfo("알림", "저장되었습니다.");
MessageUtil.ShowError("오류", "저장하지 못했습니다.");
bool yes = MessageUtil.ShowYesNo("확인", "계속하시겠습니까?");

// 토스트 — 3초 후 자동 소멸, 화면 끝에서 슬라이드 인/아웃
MessageUtil.ShowToast("정보 메시지");
MessageUtil.ShowSuccessToast("저장되었습니다.");
MessageUtil.ShowWarningToast("값이 허용 범위를 벗어났습니다.");
MessageUtil.ShowErrorToast("장비 통신이 끊어졌습니다.");
```

토스트 표시 위치(좌상단/좌하단/우상단/우하단)는 설정 > UI Setting에서 사용자가 바꿀 수 있습니다
(`UiSettingsService.ToastPosition`).

---

## 4. 차트 — `UbisamBase.Core.Charting.ChartTheme` (LiveCharts2)

```csharp
public ISeries[] TrendSeries { get; private set; }
public Axis[] TrendXAxes { get; private set; }
public Axis[] TrendYAxes { get; private set; }

public MyViewModel()
{
    Rebuild();
    ChartTheme.ThemeChanged += Rebuild; // 뷰가 사라질 때 -= 로 해제할 것
}

private void Rebuild()
{
    TrendSeries = new ISeries[] {
        ChartTheme.ReferenceLine(baseline, "기준"),
        ChartTheme.Line(measured, "측정")
    };
    TrendXAxes = new[] { ChartTheme.XAxis(v => $"{v}s") };
    TrendYAxes = new[] { ChartTheme.YAxis("0", min: 0, max: 100) };
    OnPropertyChanged(nameof(TrendSeries)); // 등등
}
```

```xml
<lvc:CartesianChart Series="{Binding TrendSeries}" XAxes="{Binding TrendXAxes}"
                    YAxes="{Binding TrendYAxes}" Background="Transparent" Height="200"/>
```

- **`Background="Transparent"`를 꼭 지정** — 기본값은 흰 배경이라 다크 테마에서 튐.
- `ChartTheme.Column(values, name)`, `ChartTheme.Gauge(percent)`(도넛 게이지, `ISeries[]` 반환)도 있음.
- 소비 프로젝트에서 차트를 직접 쓰려면 `LiveChartsCore`, `LiveChartsCore.SkiaSharpView`,
  `LiveChartsCore.SkiaSharpView.WPF`, `SkiaSharp` — 4개를 `$(UbisamPlatformDir)`에서
  HintPath로 참조 추가해야 함(TestApp.csproj 예시 참고).

---

## 5. 화면 잠금 — `UbisamBase.Core.Locking`

- 하단 네비 "화면 잠금" → `LockConfirmDialog`가 뜨고, **거기서 입력한 아이디/비밀번호가 그 순간
  해제 키가 됨**(고정 계정 저장 방식 아님). `LockService.Current.Validator`를 그 자리에서 교체.
- `LockOverlay`가 화면 전체를 불투명하게 덮음(Esc로 못 나감), 3회 실패 시 입력 잠김 →
  구석의 "강제 잠금 해제" 버튼만 예외(`LockService.Current.ForceUnlock()`, 인증 없음).
- 코드에서 직접 잠그려면: `LockService.Current.Lock()` (주의: 이 경우 `Validator`를 미리
  설정해두지 않으면 아무도 못 풂 — 보통은 `LockConfirmDialog.ShowAndLock()`을 통해서만 잠금).
- 잠글 때 입력값은 평문으로 `D:\UbisamConfig\ScreenSaver\last-lock.json`에 기록됨(요청 사양).

---

## 6. 스크린 세이버 — `UbisamBase.Core.ScreenSaving.ScreenSaverService`

화면 잠금과 달리 **인증 없이** 코드에서 자유롭게 켜고 끕니다 — 특정 작업 중 사용자 조작을
막을 때 사용.

```csharp
ScreenSaverService.Current.Show("작업을 진행 중입니다...");     // 즉시 켜기
ScreenSaverService.Current.Hide();                              // 즉시 끄기

// 시간을 넘기면, 그 시간이 지나도 서비스가 스스로 끄지 않고 이벤트만 올린다 —
// 실제로 끌지는 구독한 쪽이 결정한다(항상 자유롭게 켜고 끌 수 있다는 원칙 유지).
ScreenSaverService.Current.TimerElapsed += OnElapsed;
ScreenSaverService.Current.Show("잠시만요...", seconds: 5);

void OnElapsed()
{
    ScreenSaverService.Current.TimerElapsed -= OnElapsed;
    ScreenSaverService.Current.Hide();
}
```

반투명 스크림 + 화면 가운데 accent색 큰 글자로 `Show()`에 넘긴 문구가 표시됩니다.

---

## 7. 이벤트 버스 — `UbisamBase.Core.Messaging.IEventBus`

문자열 subject 기반 Pub/Sub. **`AddUbisamBaseCore`에서 이미 DI 싱글턴으로 등록되어 있음.**

```csharp
public MyViewModel(IEventBus bus)
{
    subscription = bus.Subscribe("Order.Completed", OnOrderCompleted);
}

private void OnOrderCompleted(EventMessage msg)
{
    // msg.Subject, msg.Param, msg.From
}

// 다른 곳에서:
bus.Publish(new EventMessage("Order.Completed", orderId, from: "OrderService"));

// 화면이 사라질 때
subscription.Dispose();
```

---

## 8. 타임아웃 관리자 — `UbisamBase.Core.Utils.TimeOutManager`

문자열 키 기반 타이머 모음. **DI 싱글턴으로 등록되어 있음.**

```csharp
public MyViewModel(TimeOutManager timers) { _timers = timers; }

_timers["Ch1"].Start(limitMs: 5000);
if (_timers["Ch1"].IsTimeout()) { /* 5초 초과 */ }
long ms = _timers["Ch1"].End();
_timers.Remove("Ch1");
```

키는 미리 정의할 필요 없음(인덱서 접근 시 없으면 자동 생성). Start/End 시 `"TimeUtil"` 태그로
자동 로그가 남습니다.

---

## 8-1. 발표자 모드 — `UbisamBase.Core.Shell.PlatformChrome`

플랫폼이 그리는 껍데기(상단 브랜딩 바 · 네비게이션 탭 바 · 서브탭 줄)를 한 번에 숨긴다.
발표 자료를 띄울 때처럼 모듈 화면만 꽉 차게 보여주고 싶을 때 쓴다.

```csharp
using UbisamBase.Core.Shell;

PlatformChrome.IsVisible = false;  // 플랫폼 껍데기 전부 숨김 (발표자 모드)
PlatformChrome.IsVisible = true;   // 원래대로
PlatformChrome.Toggle();           // 뒤집기 — 버튼/단축키에 걸어 쓰기 좋다
```

- 기본값은 `true`. **저장하지 않는다** — 앱을 다시 켜면 항상 보이는 상태로 시작한다.
- 숨기면 종료 · 화면 잠금 · 로그 · 캡처 버튼도 같이 사라진다. **다시 켤 방법(모듈 화면의 버튼이나
  키 입력)을 반드시 모듈 쪽에 만들어 둬야 한다.**
- 숨기면 **컨텐츠 여백(26,22)도 0이 된다** — 껍데기를 걷었는데 액자처럼 테두리가 남지 않는다.
- 토스트 메시지, 화면 잠금, 스크린 세이버 오버레이는 이 스위치와 무관하게 그대로 뜬다.
- 바뀔 때마다 `VisibleChanged` 이벤트가 온다. 설정의 "상단 바 접기"나 다른 모듈이 바꾼 경우에도
  오므로, 발표 모드를 나갈 때 타이머를 멈추거나 띄워둔 창을 닫는 정리는 여기서 한다.

```csharp
PlatformChrome.VisibleChanged += OnChromeChanged;
Unloaded += (_, _) => PlatformChrome.VisibleChanged -= OnChromeChanged;  // 정적 이벤트라 해제 필수

private void OnChromeChanged(object? sender, bool visible)
{
    if (visible) { /* 발표 모드 종료 정리 — 타이머 정지, 발표자 창 닫기 등 */ }
}
```

## 8-2. 보조 모니터 — `UbisamBase.Core.Shell.Displays`

발표자 보기처럼 "본 창이 없는 쪽 모니터"에 창을 띄울 때 쓴다. 모듈이 `System.Windows.Forms`의
`Screen`을 직접 읽으면 실제 픽셀이 나와서, 배율(DPI)이 걸린 환경에서 DIP 환산을 빠뜨리면 창이
엉뚱한 자리에 뜬다 — 그 환산까지 여기서 처리한다.

```csharp
var area = Displays.SecondaryWorkArea(Window.GetWindow(this));   // 모니터가 하나면 null
if (area is Rect r)
{
    var win = new PresentWindow { WindowStartupLocation = WindowStartupLocation.Manual };
    win.Left = r.Left; win.Top = r.Top; win.Width = r.Width; win.Height = r.Height;
    win.Show();
}
```

- `Displays.HasSecondary` — 모니터가 두 대 이상인지.
- `Displays.WorkArea(owner)` — owner 창이 있는 모니터의 작업 영역(DIP).
- 값은 전부 **작업표시줄을 뺀 작업 영역**이고 WPF 좌표(DIP)다. 모니터가 셋 이상이면 owner가 없는
  것 중 첫 번째(주 모니터 우선)를 돌려준다.

## 8-3. 닫기 확인 — `UbisamBase.Core.Modules.ICloseGuard`

저장하지 않은 변경이 있을 때 창이 그냥 닫히는 것을 막는 표준 통로. 모듈 View나 그 ViewModel이
구현하면 셸이 닫기 전에 물어본다. **셸 창의 `Closing`에 직접 붙지 말 것** — 모듈이 여럿이면
서로 Cancel/Close를 걸어 충돌한다.

```csharp
public partial class DeckViewModel : ObservableObject, ICloseGuard
{
    public async Task<bool> CanCloseAsync()
    {
        if (!IsDirty) return true;
        return await AskUserAsync("저장하지 않은 변경이 있습니다. 닫을까요?");
    }
}
```

- **한 번이라도 연 탭**만 물어본다(아직 안 연 탭은 화면을 만들지 않는다).
- 하나라도 false를 돌려주면 거기서 멈추고 창은 닫히지 않는다(뒤의 화면에는 묻지 않는다).
- 하단 "종료" 버튼도 이 경로를 탄다. 반대로 `Application.Current.Shutdown()`을 직접 부르면
  이 확인을 건너뛴다 — 모듈에서는 쓰지 말 것.
- UI 설정의 "상단 바 접기"는 사용자가 저장해 두는 별개 취향이다. 둘 중 하나라도 숨기라고 하면 상단 바는 숨는다.

## 9. 테마 · UI 컨벤션

- `Ubisam.Brush.*`, `Ubisam.Style.*`로 시작하는 리소스가 다크/라이트 테마 전체를 정의
  (`src/UbisamBase.Core/Themes/Colors.Dark.xaml`, `Colors.Light.xaml`, `Controls.xaml`).
- `TextBox`/`ComboBox`/`CheckBox`/`Button`/`ToggleButton`/`DataGrid`는 전부 **암시적 스타일**
  (`x:Key` 없음) — 그냥 `<Button>`, `<TextBox>`를 쓰면 자동으로 테마 적용됨. 예외 처리하려면
  `Style="{x:Null}"`.
- 새 창(Window)을 별도로 띄울 땐(대화상자 등) `Window.Resources`에 `ShellStyles.xaml`을 다시
  merge해야 하고, 루트 컨테이너에 `TextElement.Foreground="{DynamicResource Ubisam.Brush.ContentForeground}"`를
  꼭 걸어야 함 — 안 그러면 스타일 없는 텍스트가 검정으로 보임(이 세션에서 실제로 겪은 버그).
- 테마가 바뀌면 `Application.Current.Resources.MergedDictionaries`에서 `Colors.*.xaml`을
  갈아끼우는 방식(`UiSettingsService.ApplyTheme`) — DynamicResource로 참조해야 실시간 반영됨
  (StaticResource는 최초 1회만 고정).

### 9-1. 설정 > UI Setting 항목 (`UiSettingsService`)

| 화면 항목 | 속성 | 설명 |
| --- | --- | --- |
| 테마 | `ThemeMode` | 시스템 설정 따르기 / 라이트 / 다크 |
| 네비게이션 위치 | `NavPosition` | 왼쪽 / 오른쪽 / 하단 |
| 상단 바 접기 | `TopBarCollapsed` | 상단 브랜딩 바(로고·장비명·시계와 로그/캡처/모드 변경 버튼)를 숨긴다 |
| 토스트 메시지 위치 | `ToastPosition` | 좌상단 / 우상단 / 좌하단 / 우하단 |
| 화면 배율 | `ChromeScale` | 상단 바·탭 바 배율. `BaselineChromeScale`(1.2)이 화면상 "100%" |
| 언어 | `LanguageService.CurrentCulture` | 등록된 언어 리소스가 있을 때만 보인다 |

- 바꾸면 화면에 즉시 반영되고, **"저장"을 눌러야** `<설정 루트>\Settings\UiSettingsService.json`에 남아
  다음 실행에도 유지된다("새로고침"은 저장된 값으로 되돌린다).
- **상단 바 접기**는 사용자가 저장해 두는 취향이고, 8-1의 `PlatformChrome`(발표자 모드)은 코드에서 잠깐
  껐다 켜는 모드다. 둘 중 하나라도 숨기라고 하면 상단 바는 숨는다.

### 9-2. 창 배치 (키오스크 / 창모드)

- 시작하면 키오스크(작업표시줄까지 덮는 전체화면). 상단 바의 "모드 변경" 버튼이나 **Ctrl+Shift+F**로
  창모드와 오간다. Topmost는 걸지 않는다 — 현장의 다른 경보 창이 가려지면 안 되기 때문.
- 키오스크는 모니터 경계를 창 크기로 쓰되, **만들어진 창을 실측해 모니터보다 큰 만큼 되돌린다**
  (`ShellWindow.FitToMonitor`). 화면 배율이 100%가 아닌 환경(예: 2560×1600 / 150%)에서 창이 20~30px
  크게 만들어져 하단 네비게이션 탭 글자가 잘리던 문제 대응이다. 하단이 잘려 보이면 이 지점을 먼저 본다.

---

### 9-3. WebView2를 쓰는 모듈 (참고)

- **WebView2는 WPF의 앞뒤 순서를 따르지 않는다.** 자기 자식 윈도우에 직접 그리기 때문에 WebView2
  위에 WPF 버튼을 올려도 보이지 않는다. 겹쳐 쓰려면 아래쪽 WebView2를 `Visibility.Hidden`으로 내린다.
- **사용자 데이터 폴더를 모듈마다 나눈다.** 같은 폴더를 쓰면 쿠키·로컬 저장소가 섞인다
  (예: UbisamDeck은 `D:\UbisamConfig\WebView2`).
- WebView2 안쪽은 크로미움이라 **플랫폼 테마(WPF 스타일)가 닿지 않는다.** 스크롤바 같은 것은
  그 웹 페이지 CSS에서 직접 맞춰야 한다(`::-webkit-scrollbar`).

---

## 10. 기존 C# 코드를 이 플랫폼에 연동하는 방법

### 10-1. 어디에 둘지 먼저 정한다

| 상황 | 두는 곳 |
| --- | --- |
| 여러 프로그램이 공유해서 쓸 공통 기능 | `UbisamBase.Core`에 새 폴더/네임스페이스로 옮기고 DI 싱글턴 등록 |
| 이 프로그램(소비 프로젝트)만 쓰는 기능 | 소비 프로젝트 안에 그대로 두고 `RegisterServices`에서 등록 |

공통으로 옮길 경우, `TimeOutManager`를 옮긴 실제 예시:

1. `src/UbisamBase.Core/Utils/TimeOutManager.cs`로 파일 복사, 네임스페이스를
   `UbisamBase.Core.Utils`로 변경.
2. **외부 의존성 정리** — 원래 코드가 `Aurora.Api.Logger` 같은 이 플랫폼에 없는 타입을 쓰고
   있으면, 시그니처가 호환되는 플랫폼 타입으로 바꿔치기한다. (`Logger(string tag)`,
   `.I/.W/.E(string)` 시그니처가 동일해서 `using Aurora.Api;` → `using UbisamBase.Core.Logging;`
   한 줄 교체로 끝났다.) 호환 안 되면 그 부분만 새로 짠다.
3. `src/UbisamBase.Core/DependencyInjection/ServiceCollectionExtensions.cs`의
   `AddUbisamBaseCore`에 `services.AddSingleton<TimeOutManager>();` 추가.
4. 이제 어떤 ViewModel이든 생성자에 `TimeOutManager`를 선언하면 주입받는다.

### 10-2. 소비 프로젝트에서 플랫폼을 참조하는 방식 — 모두 D드라이브 배포 폴더 하나

플랫폼 원본 소스는 `D:\업무\03_개발\Source\용접기\08_UbisamPlatform`에 있고, 그걸 빌드한
결과물은 `D:\UbisamPlatform\bin`에 모인다. 플랫폼을 쓰는 모든 프로그램은 이 배포 폴더만 본다 —
플랫폼을 고치는 개발자의 테스트 프로그램(`TestApp`)도 예외 없이 같다.

소비 프로젝트(App, TestApp, MergeHub 등)는 `UbisamBase.Core`를 **ProjectReference가 아니라 파일
Reference**로, 항상 `D:\UbisamPlatform\bin\`을 가리켜 참조한다:

```xml
<UbisamPlatformDir>D:\UbisamPlatform\bin\</UbisamPlatformDir>

<Reference Include="UbisamBase.Core">
  <HintPath>$(UbisamPlatformDir)UbisamBase.Core.dll</HintPath>
  <Private>false</Private>
</Reference>
```

#### 전체 흐름

```
08_UbisamPlatform 빌드 (Debug/Release 아무거나)
   └─ Directory.Build.targets가 결과물을 D:\UbisamPlatform\bin 에 반영
        (.pdb 제외, Shell 설정 → Launcher 설정 복사, README 갱신)

소비 프로젝트 실행 (F5 또는 exe 더블클릭)
   └─ UbisamBase.Launcher.exe
        1) D:\UbisamPlatform\bin 전체를 자기 폴더로 복사
        2) 같은 폴더의 Shell을 같은 프로세스 안에서 로드
        3) Shell이 같은 폴더의 IAppSetup 구현 dll(소비 프로젝트)을 찾아 화면을 띄움
```

- 플랫폼을 고쳤으면 **플랫폼만 빌드하고 소비 프로젝트를 실행**하면 된다. 소비 프로젝트를 다시
  빌드할 필요가 없다(빌드 절차가 없는 사용자 PC에서도 배포 폴더만 바뀌면 다음 실행부터 반영).
- Launcher가 Shell을 같은 프로세스에서 띄우므로 VS 디버거가 그대로 붙어 소비 프로젝트 중단점이 걸린다.
- 소비 프로젝트를 빌드할 때도 `CopyPlatformRuntime` 타겟이 배포 폴더 전체를 출력 폴더로 복사한다.
  Launcher.exe 자체가 출력 폴더에 처음 자리 잡게 하는 용도다.
- Debug 빌드도 바로 반영되므로, 고치다 만 코드도 빌드하는 순간 이 PC의 모든 플랫폼 프로그램에
  적용된다.
- Core만 빌드하면 Core.dll만, Shell을 빌드하면 Shell 출력 전체가 반영된다. 헷갈리면 솔루션
  (`UbisamBase.Platform.slnx`) 단위로 빌드한다.

#### 깨끗한 배포본 — `scripts\publish-platform.ps1`

자동 반영은 덮어쓰기만 하므로, 의존 패키지를 빼는 등으로 쓰지 않게 된 옛 dll이 배포 폴더에
남는다. 다른 PC에 넘기기 전처럼 깨끗한 Release 배포본이 필요하면 이 스크립트를 실행한다:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\publish-platform.ps1
```

```
1) D:\UbisamPlatform\bin 을 비운다
2) UbisamBase.Platform.slnx 를 Release로 빌드 → Directory.Build.targets가 폴더를 다시 채움
3) 필수 파일(Core.dll, Shell.exe, Launcher.exe, Launcher 설정, README) 확인
4) Launcher 설정에 바인딩 리디렉트가 있는지 확인
   — Launcher 프로세스 안에서 Shell이 돌기 때문에 실제로 적용되는 건 Launcher 쪽 설정이다.
     이게 없으면 실행 직후 FileLoadException으로 조용히 죽는다.
5) .pdb/.cs가 남아 있으면 에러로 중단 (안전장치)
```

다른 PC에서 쓰려면 `D:\UbisamPlatform\bin` 폴더를 통째로 그 PC의 같은 위치에 복사한다.

외부 NuGet 패키지가 새로 필요한 경우(예: LiveChartsCore)도 Core에 `PackageReference`를
추가하면 빌드 시 그 dll들이 배포 폴더에 자동으로 포함된다. 소비 프로젝트가 컴파일 타임에 그
타입을 쓰려면 소비 프로젝트 쪽에도 `$(UbisamPlatformDir)해당dll.dll`을 가리키는 파일
Reference를 추가해야 한다.

### 10-3. 체크리스트

- [ ] 네임스페이스를 `UbisamBase.Core.*`(공통) 또는 소비 프로젝트 네임스페이스(전용)로 정리했는가
- [ ] 이 플랫폼에 없는 외부 타입(로거, 설정 저장 등)을 플랫폼 대응 타입으로 교체했는가
- [ ] 공통이면 `AddUbisamBaseCore`에 등록, 전용이면 `IAppSetup.RegisterServices`에 등록했는가
- [ ] 플랫폼(Core/Shell/Launcher)을 빌드했는가 (빌드하면 `D:\UbisamPlatform\bin`에 자동 반영)
- [ ] `UbisamPlatformDir`이 `D:\UbisamPlatform\bin\`이고, `StartProgram`이 `$(MSBuildProjectDirectory)` 기반 절대경로로 Launcher를 가리키는가
- [ ] 플랫폼이 Any CPU인가 (csproj `<Platforms>`, `.slnx`에 x64 없음)
- [ ] F5로 떴을 때 `UbisamBase.Launcher` 프로세스 하나만 있고, 중단점이 걸리는가
- [ ] (외부 NuGet이 새로 필요하면) 소비 프로젝트에도 컴파일 타임 참조를 추가했는가

---

## 11. 저장 위치 요약

| 경로 | 내용 |
| --- | --- |
| `D:\UbisamConfig\Settings\` | 설정 클래스별 JSON |
| `D:\UbisamConfig\Log\` | 날짜별 로그 (`yyyyMMdd.log`) |
| `D:\UbisamConfig\ScreenSaver\` | 화면 잠금 시 마지막 아이디/비밀번호 |

여러 프로그램이 이 플랫폼으로 만들어져도 전부 `D:\UbisamConfig` 한 곳에 모입니다.

---

## 12. 플랫폼 자동 업데이트 (깃)

프로그램을 켤 때 깃 저장소의 업데이트 파일과 지금 깔린 플랫폼(`D:\UbisamPlatform\bin`)을 비교해,
새 버전이면 받아서 켠다. 확인과 내려받기는 **Launcher**가 Shell을 띄우기 전에 한다.

### 12-1. 동작 순서

```
설정에서 켜져 있나?  →  아니오면 끝
인터넷이 연결됐나?   →  아니오면 끝(조용히)
저장소에 새 커밋이 있나?(git ls-remote)  →  없으면 끝
dist의 버전이 지금 것보다 새것인가?      →  아니면 커밋만 기록하고 끝
자동 업데이트인가?   →  예: 바로 받기 / 아니오: "업데이트할까요?" 물어보고 예일 때만 받기
```

- 어느 단계에서 막히든(설정 꺼짐 · 네트워크 없음 · git 미설치 · 저장소 접근 실패) **조용히 넘어가고
  기존 버전으로 실행한다.** 업데이트 때문에 프로그램이 안 켜지는 일은 없어야 하기 때문이다.
- 물어봤을 때 "아니오"를 고르면 아무것도 기록하지 않는다 — 다음에 켤 때 다시 물어본다.
- 받는 것은 `dist` 폴더뿐이다(`--depth 1 --filter=blob:none --sparse`). 소스 이력은 받지 않는다.
- git 자격 증명 창이 뜨지 않게 막아 두었다(`GIT_TERMINAL_PROMPT=0`). 사내 비공개 저장소라면 그 PC에서
  한 번은 `git` 자격 증명이 저장돼 있어야 한다.

### 12-2. 쓰는 쪽 설정 — 설정 > UI Setting > "플랫폼 업데이트"

| 항목 | 값 |
| --- | --- |
| 사용 / 사용 안 함 | 업데이트 확인 기능 자체를 켜고 끈다(기본: 사용 안 함) |
| 자동으로 업데이트 / 업데이트할지 물어보기 | 새 버전을 찾았을 때 묻지 않고 받을지 |
| 저장소 주소 | 플랫폼 저장소 URL. 비어 있으면 확인하지 않는다 |

값은 `D:\UbisamPlatform\update-settings.json`에 저장된다(배포 폴더 **바깥** — 업데이트가 `bin`을
통째로 덮어쓰기 때문). 스키마는 아래와 같고, Core(`PlatformUpdateSettings`)와
Launcher(`PlatformUpdater.Settings`)가 같은 이름으로 읽고 쓴다. **이름을 바꾸면 양쪽을 같이 고칠 것.**

```json
{
  "Enabled": true,
  "AutoUpdate": false,
  "RepositoryUrl": "https://.../UbisamPlatform.git",
  "Branch": "main",
  "DistPath": "dist",
  "InstalledVersion": "2026-09-21 21:53:23",
  "InstalledCommit": "a1b2c3..."
}
```

`InstalledVersion`/`InstalledCommit`은 Launcher가 기록한다(손대지 않는다).

### 12-3. 내는 쪽 — 새 버전 배포하기

1. `scripts\publish-platform.ps1` 실행 — `D:\UbisamPlatform\bin`을 비우고 Release로 다시 채운 뒤,
   저장소의 `dist\` 폴더를 그 내용과 똑같이 맞춘다(robocopy /MIR).
2. `git add dist && git commit && git push`.
3. 다른 PC는 다음 실행 때 새 커밋을 보고 업데이트한다.

- 버전 기준은 `platform-version.json`의 `version`(= 배포 시각)이다. 빌드할 때마다
  `Directory.Build.targets`가 `D:\UbisamPlatform\bin`에 찍고, 그 파일이 `dist`에 함께 올라간다.
- 소스만 바꾼 커밋(=`dist`가 그대로)에는 아무에게도 업데이트를 묻지 않는다.
- 업데이트는 **덮어쓰기**다. 쓰지 않게 된 옛 파일까지 정리하려면 `publish-platform.ps1`로 `bin`을
  비우고 다시 채운 뒤 `dist`를 올리면 된다.

---

## 13. 사내 API 호출 — `UbisamBase.Core.Net.ApiService`

토큰이 필요한 사내 API를 **앱이 대신 호출해** 응답 본문(JSON 문자열)만 돌려준다. 화면(WebView2 등)이
직접 부르면 토큰이 그 문서에 섞여 들어가므로, 앱이 부르고 값만 넘기는 구조를 공용으로 둔 것이다.
브라우저에서 막히는 CORS도 이 경로로는 문제가 되지 않는다.

```csharp
using UbisamBase.Core.Net;

var json = await ApiService.Current.FetchAsync(url);          // 실패하면 예외
var (ok, body, error) = await ApiService.Current.TryFetchAsync(url);   // 예외 대신 결과로 받기
```

- 인증 헤더는 서비스 안에서 붙는다 — **부르는 쪽은 토큰을 몰라도 된다.**
- 같은 주소를 `MinIntervalMs`(기본 1초) 안에 다시 부르면 서버를 때리지 않고 직전 응답을 돌려준다.
  화면 갱신 주기가 짧아도 호출 제한에 걸리지 않게 하기 위한 것이다.
- 로그에는 주소의 `?` 앞부분만 남긴다(쿼리에 토큰이 섞인 경우 대비). 응답 본문도 남기지 않는다.

### 13-1. 설정 — `D:\UbisamConfig\Settings\ApiSettings.json`

```json
{
  "AuthMode": "BearerHeader",
  "HeaderName": "X-Api-Key",
  "Token": "...",
  "MinIntervalMs": 1000,
  "TimeoutSeconds": 10
}
```

| AuthMode | 붙는 헤더 |
| --- | --- |
| `None` | 없음(사내망 IP 제한 등) |
| `BearerHeader` | `Authorization: Bearer {Token}` |
| `CustomHeader` | `{HeaderName}: {Token}` |
| `Cookie` | `Cookie: {Token}` |

**토큰은 이 파일에만 있다.** 내보낸 HTML·`.ub` 같은 산출물에는 들어가지 않는다.

### 13-2. 내보낸 파일에서는 인증 API를 못 부른다

앱 밖으로 내보낸 HTML은 혼자 도는 파일이라 이 서비스가 없다. 그래서:

- **앱 안에서 볼 때** → `ApiService`로 최신 값
- **내보낸 파일을 남에게 줄 때** → 인증이 필요한 API는 호출 불가, 내보낼 때 담긴 마지막 값이 보인다
  (인증이 없는 공개 API라면 내보낸 파일에서도 실시간으로 돈다)

이는 제약이라기보다 의도된 동작이다 — 토큰이 파일에 들어가지 않기 때문이다.


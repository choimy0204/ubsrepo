# 회사 로고 — 재구성본

원본(저해상도 PNG)의 구성을 그대로 따라 벡터로 다시 그렸습니다: 둥근 사각 타일 + 같은 중심의 아치 2개 + 점.
좌표계는 64×64, 타일 라운드 15, 아치 스트로크 9.75(끝은 평평하게), 마크 중심은 (14.19, 49.81)입니다.
마크는 타일의 70%를 차지하고, 좌하단으로 4.5% 광학 보정이 들어가 있습니다 — 아치 질량이 우상단에 몰려 있어 bbox를 그냥 중앙에 놓으면 우상단으로 쏠려 보입니다.

| 파일 | 용도 |
| --- | --- |
| `ubisam-logo.svg` | 기본 — 그라디언트 타일(#56ADF7 → #1D82E8) + 오프화이트 마크. 원본에 가장 가깝습니다 |
| `ubisam-logo-flat.svg` | 단색 타일(#2196F3) + 흰 마크. 작은 크기(16~24px)나 인쇄용 |
| `ubisam-logo-mono.svg` | 타일이 `currentColor` — 다크 배경에서 타일 색만 바꿔 쓸 때 |
| `ubisam-mark.svg` | 타일 없이 마크만, `currentColor`. 이미 파란 판 위에 얹을 때 |
| `../code/UbisamBase.Core/Themes/Logo.xaml` | WPF용 `DrawingImage` 리소스 (같은 도형) |

## 워드마크

첨부해 주신 원본에서 앞의 "Ubi"만 색을 바꿨습니다 — "Sam"과 마크는 손대지 않았습니다.

원본에는 글자 뒤에 **불투명한 흰 판**이 깔려 있습니다. 색만 바꾸면 어두운 배경에서 그 판이 흰 사각형으로 그대로 보이므로 걷어냈습니다.

흰 픽셀을 남기는 조건은 하나입니다 — **파란 타일 영역 안 + 파랑에 둘러싸여 있음**. 그게 마크의 아치입니다. 글자 속공간(`a`·`b`·`U`)의 흰색은 파랑에 갇혀 있어도 타일 밖이므로 투명해지고, 타일 라운드 코너 밖의 흰 판도 배경에서 도달하므로 투명해집니다. 글자 경계의 안티에일리어싱은 알파로 보존됩니다.

| 파일 | 용도 |
| --- | --- |
| `wordmark-ubi-black.png` | 밝은 배경 — Ubi #1D1F20 |
| `wordmark-ubi-white.png` | 어두운 배경 — Ubi #FFFFFF |
| `wordmark-ubi-gray.png` | 원본 그대로 (보관용 — 흰 판이 남아 있습니다) |

3533×829px입니다 — **화면에 쓸 때는 이 파일을 직접 줄이지 마세요.** 30배 축소는 획을 부수므로, `wordmark/` 폴더의 미리 축소한 파일을 쓰세요.

### 화면용 (미리 축소, 단계 리샘플)

| 파일 | 크기 |
| --- | --- |
| `wordmark/{white,black}-180.png` | 180×42 |
| `wordmark/{white,black}-240.png` | 240×56 |
| `wordmark/{white,black}-360.png` | 360×84 |
| `wordmark/{white,black}-480.png` | 480×112 |
| `wordmark/{white,black}-720.png` | 720×169 |

실제 표시 폭보다 한 단계 큰 파일을 쓰고, WPF에서는 `RenderOptions.BitmapScalingMode="HighQuality"`를 지정하세요. 자세한 내용은 `code/FIX-13.md`에 있습니다.

accent 채움 위에는 쓰지 마세요 — "Sam"이 배경에 묻힙니다. 그 경우 흰 단색 버전이 필요하니 말씀해 주세요.

## 실행 아이콘 (.ico)

| 파일 | 용도 |
| --- | --- |
| `UbisamBase.ico` | 실행 파일·작업 표시줄·바로가기 아이콘. 16 / 24 / 32 / 48 / 64 / 128 / 256px 7종을 담고 있습니다 |
| `app-icon-{크기}.png` | 개별 크기 PNG — 설치 관리자나 문서에 쓸 때 |

크기별로 형태를 다르게 최적화했습니다. 48px 이상은 원본대로 아치 2개, **24px 이하는 아치를 1개로 줄이고 두껍게** 했습니다 — 작은 크기에서 두 아치가 뭉개져 회색 덩어리로 보이는 것을 막기 위해서입니다. 16·24px은 타일이 화면을 꽉 채우고, 큰 크기는 여백을 둡니다.

### 프로젝트에 등록

1. `UbisamBase.ico`를 `src/UbisamBase.App/`에 복사
2. `.csproj`에 한 줄 추가

```xml
<PropertyGroup>
  <ApplicationIcon>UbisamBase.ico</ApplicationIcon>
</PropertyGroup>
```

창 제목줄 아이콘은 `WindowStyle="None"`이라 보이지 않지만, 작업 표시줄에는 나옵니다. 명시하려면 `ShellWindow.xaml`에 추가하세요.

```xml
Icon="pack://application:,,,/UbisamBase.App;component/UbisamBase.ico"
```

빌드 후 탐색기에서 아이콘이 바뀌지 않으면 아이콘 캐시 때문입니다 — `ie4uinit.exe -show`를 실행하거나 재로그인하세요.

## WPF에서 쓰기

`ShellStyles.xaml`의 머지 목록에 `Logo.xaml`을 추가하고, `ShellWindow.xaml`의 로고 자리(accent 사각형 + 첫 글자)를 이미지로 교체하세요.

```xml
<Image Source="{StaticResource Ubisam.Logo}" Width="30" Height="30" VerticalAlignment="Center"/>
```

## 주의

- 원본이 저해상도라 라운드 반경·스트로크 두께·마크 여백은 눈으로 맞춘 근사값입니다. 원본 벡터(AI/SVG) 파일이 있으면 그걸 쓰는 게 정확합니다.
- 작은 크기에서는 그라디언트가 뭉개져 보이므로 `flat` 버전을 쓰세요.
- 타일 없이 흰 배경에 마크만 놓으면 대비가 부족합니다 — 마크만 쓸 때는 accent 색을 지정하세요.

# 패치 08 — Slider · ScrollBar + 적용 확인

## 먼저 확인: Controls.xaml이 빌드에 들어갔는지

다크모드에서 콤보박스가 **둥근 모서리 + 밝은 회색**으로 보이면 WPF 기본 크롬입니다. 패치 04의 스타일이 적용되면 **각진 모서리 + #2F343A 어두운 배경**이 됩니다. 두 가지를 확인하세요.

1. `src/UbisamBase.Core/Themes/Controls.xaml` 파일이 존재하는지
2. `Themes/ShellStyles.xaml` 상단에 머지 항목이 있는지

```xml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="pack://application:,,,/UbisamBase.Core;component/Themes/Icons.xaml"/>
    <ResourceDictionary Source="pack://application:,,,/UbisamBase.Core;component/Themes/Controls.xaml"/>
</ResourceDictionary.MergedDictionaries>
```

머지가 있는데도 기본 크롬이 나오면, 그 `UserControl`이 `ShellWindow.Resources`를 상속받지 못하는 경우입니다 — 그때는 `App.xaml`의 `Application.Resources`에 같은 머지를 넣으세요. 암시적 스타일은 리소스 조회 범위 안에서만 걸립니다.

## Slider

UI 설정 화면의 "화면 배율" 슬라이더가 기본 모양이었습니다. 트랙은 얇은 홈, 지나온 구간만 accent, 썸은 각진 10×20 세로 막대(둥근 다이얼이 아니라 계기 슬라이드)로 맞췄습니다.

## ScrollBar

폭 10의 얇은 막대. 트랙은 비우고 썸만 회색(#5A616B), hover·드래그 시 accent. 위아래 화살표 버튼은 없앴습니다.

두 스타일 모두 암시적이라 기존 화면을 고칠 필요는 없습니다.

## 파일

`code/UbisamBase.Core/Themes/Controls.xaml` 하나만 덮어쓰면 됩니다 (기존 내용 + Slider·ScrollBar 추가).

## 확인

1. 콤보박스가 어두운 배경 + 각진 모서리인지 (아니면 위 머지 문제)
2. 화면 배율 슬라이더의 왼쪽 구간만 파랗게 차는지
3. 썸을 잡고 끌 때 색이 밝아지는지
4. 목록·표를 스크롤할 때 막대가 얇고 어두운지
5. 슬라이더를 비활성화하면 45%로 흐려지는지

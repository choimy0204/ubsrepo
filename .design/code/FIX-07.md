# 패치 07 — 다크모드 accent를 회사 하늘색으로

다크에서 시안(#22D3EE)이 회사 색과 동떨어져 보이고, 밝은 accent 위에 검은 글자를 얹는 조합이 어색했습니다. 회사 하늘색 계열로 바꾸고 **accent를 두 단계로 나눕니다.**

| 역할 | 다크 | 라이트 | 쓰는 곳 |
| --- | --- | --- | --- |
| `Ubisam.Brush.Accent` | **#4DA6F5** | #2196F3 | 선 · 글자 · 탭 밑줄 · 포커스 테두리 — 바탕 위에 얹히는 것 |
| `Ubisam.Brush.Accent.Fill` | **#1E7FD4** | #1D82E8 | 글자를 얹는 채움면 — 주 버튼, 로고 타일, 선택된 서브탭·항목, 체크박스 |
| `Ubisam.Brush.Accent.OnFill` | **#F2F7FC** | #FFFFFF | 그 채움면 위의 글자 · 체크 표시 |

밝은 단계(#4DA6F5)에 검은 글자를 얹지 않는 것이 규칙입니다 — 흰 글자와의 대비가 부족하므로, 글자가 올라가는 면은 깊은 단계(#1E7FD4, 흰 글자와 약 4.2:1)를 씁니다.

라이트에도 같은 두 키를 추가했습니다 — 스타일이 양쪽 테마에서 같은 이름으로 해석됩니다. 라이트의 기존 색은 그대로입니다.

## 파일 교체

`code/UbisamBase.Core/Themes/` 아래 세 파일을 덮어쓰세요.

| 파일 | 변경 |
| --- | --- |
| `Colors.Dark.xaml` | accent 계열 전체 교체 (틴트 · 글로우 · 해치 · 태그 포함) + 새 키 2개 |
| `Colors.Light.xaml` | 새 키 2개 추가 |
| `Controls.xaml` | 채움면을 쓰는 4곳이 `Accent.Fill` / `Accent.OnFill`을 참조 |
| `Shell/ShellWindow.xaml` | 로고 타일이 `Accent.Fill` / `Accent.OnFill`을 참조 |

## 로고 — ShellWindow.xaml도 함께 바꿔야 합니다

상단바 로고 타일이 `Accent`(밝은 단계)에 묶여 있어 다크에서 흰 글자와 대비가 부족했습니다. 두 속성을 채움면 키로 바꾸세요 (`code/UbisamBase.Core/Shell/ShellWindow.xaml`에 이미 반영돼 있습니다).

```xml
<Border Width="30" Height="30" Background="{DynamicResource Ubisam.Brush.Accent.Fill}">
    <TextBlock Text="{Binding Brand, Converter={StaticResource FirstCharConverter}}"
               Foreground="{DynamicResource Ubisam.Brush.Accent.OnFill}"
               ... />
</Border>
```

`ShellStyles.xaml`의 다른 두 곳(좌측 네비 선택 테두리, 하단 탭 인디케이터 바)은 글자를 얹지 않는 선이라 `Accent`가 맞습니다 — 그대로 두세요.

## 로고 이미지

다크 상단바의 로고 타일도 #1E7FD4 + 흰 마크가 됩니다 — 원본 로고에 더 가깝습니다. `logo/ubisam-logo-mono.svg`는 타일이 `currentColor`이므로 부모에 `color: #1E7FD4`만 주면 됩니다.

## 확인

1. 다크에서 주 버튼("저장")이 진한 파랑 + 흰 글자인지
2. 탭 밑줄 · 시계 · 포커스 테두리는 밝은 파랑(#4DA6F5)인지
3. 체크박스 체크 표시가 흰색인지 — 검정이면 `Controls.xaml`이 갱신되지 않은 것입니다
4. 선택된 서브탭 글자가 읽히는지
5. 라이트 테마가 이전과 동일한지

# 패치 11 — 토스트 메시지 (Success · Fail · Error)

기존 토스트는 판 전체를 상태색으로 채우고 흰 글자를 얹는 방식이라, 셸의 다른 표면과 재질이 달랐고 다크에서 특히 튀었습니다. **판은 셸과 같은 재질로 두고 색은 좌측 3px 바와 아이콘에만** 쓰도록 바꿉니다.

`ToastType` enum(`Info` · `Success` · `Warning` · `Error`)은 그대로입니다. 요청하신 3종은 이렇게 매핑됩니다.

| 요청 | enum | 의미 | 라이트 | 다크 | 아이콘 |
| --- | --- | --- | --- | --- | --- |
| **Success** | `ToastType.Success` | 완료 | `#2E8B4F` | `#5FC97E` | `circle-check` |
| **Fail** | `ToastType.Warning` | 동작이 되지 않음 (예외는 아님) | `#B8791A` | `#E0A63C` | `triangle-alert` |
| **Error** | `ToastType.Error` | 예외 · 시스템 오류 | `#C93B3B` | `#F07070` | `circle-x` |

`Info`는 accent 색 + `circle-info`로 남습니다 — 기존 `ShowToast` 호출이 그대로 동작합니다.

종류마다 **색과 도형을 둘 다** 다르게 했습니다. 색만으로 구분하면 색약 사용자가 성공과 실패를 구별할 수 없습니다.

## 1. 갱신 파일

| 파일 | 변경 |
| --- | --- |
| `src/UbisamBase.Core/Converters/ToastTypeToBrushConverter.cs` | 하드코딩된 색 → 테마 브러시 리소스 조회 |
| `src/UbisamBase.Core/Converters/ToastTypeToGeometryConverter.cs` | **신규** — 종류별 아이콘 |
| `src/UbisamBase.Core/Themes/ShellStyles.xaml` | 새 컨버터 등록 (`ToastTypeToGeometry`) |
| `src/UbisamBase.Core/Shell/ShellWindow.xaml` | 토스트 `ItemTemplate` 교체 |
| `src/UbisamBase.Core/Themes/Colors.Light.xaml` | `Success` `#2E8B4F` / `Warning` `#B8791A` 추가 |
| `src/UbisamBase.Core/Themes/Colors.Dark.xaml` | `Success` `#5FC97E` / `Warning` `#E0A63C` 추가 |

`code/` 아래 같은 경로의 파일을 그대로 덮어쓰세요. `ToastService.cs`와 `ToastMessage.cs`는 손대지 않습니다.

## 2. 쓰는 법 (변화 없음)

```csharp
MessageUtil.ShowSavedToast(logger);                          // Success
MessageUtil.ShowSuccessToast("검사가 완료되었습니다.", logger);   // Success
MessageUtil.ShowWarningToast("값이 허용 범위를 벗어났습니다.", logger);  // Fail
MessageUtil.ShowErrorToast("장비 통신이 끊어졌습니다.", logger);  // Error
MessageUtil.ShowToast("레시피를 불러왔습니다.", logger);          // Info
```

표시 시간은 `ToastService.Show(text, type, durationMs)`의 기본 3000ms입니다. 오류는 더 길게 두는 편이 낫습니다 — 필요하시면 종류별 기본값을 다르게 만들어 드립니다.

## 3. 모양

- 판 배경은 `Sidebar.Background`(라이트 #FFFFFF / 다크 #2A2E34), 1px 헤어라인, 각진 모서리
- 좌측 3px 상태색 바 — 색이 들어가는 유일한 면
- 아이콘 19px, 상태색 스트로크 1.5, 상단 정렬(메시지가 길어도 위에 고정)
- 글자는 본문색 13px/19 — 상태색으로 쓰지 않습니다(가독성)
- 최소 폭 280 / 최대 380, 그림자는 `Ubisam.Effect.Popup`
- 위치는 기존과 같이 우상단, 아래로 8px 간격 쌓임

## 4. 확인

1. 세 종류의 좌측 바 색과 아이콘 도형이 서로 다른지
2. 다크에서 판이 어두운 회색이고 글자가 흰색인지 (색으로 채워진 판이 뜨면 파일이 안 바뀐 것)
3. 여러 개가 동시에 뜰 때 아래로 쌓이는지
4. 3초 후 사라지는지
5. 긴 메시지가 380px에서 줄바꿈되고 아이콘은 위에 고정되는지
6. 테마를 바꾼 뒤 새로 뜬 토스트의 색이 그 테마를 따르는지

## 주의

- 컨버터가 색을 **호출 시점에** 읽으므로, 테마를 바꿔도 이미 떠 있는 토스트는 이전 색입니다. 3초 후 사라지니 실질적인 문제는 없습니다.
- 판 전체를 상태색으로 채우는 이전 방식을 원하시면 알려주세요 — 그 경우 흰 글자 대비를 위해 색을 한 단계 어둡게 조정해야 합니다.

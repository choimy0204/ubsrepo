# 패치 09 — MessageBox를 테마 적용 대화상자로

WPF의 `MessageBox`는 OS 크롬이라 스타일을 입힐 수 없습니다. 다크모드에서 혼자 밝은 창이 뜨는 이유입니다. 같은 모양의 커스텀 창으로 대체합니다.

**`MessageUtil`의 시그니처는 그대로입니다** — `ShowInfo`, `ShowError`, `ShowYesNo`, `ShowSaveSuccessDialog`, `ShowSaveFailedDialog`, Toast 5종 전부 동일합니다. 호출하는 쪽은 한 줄도 고칠 필요가 없습니다.

## 모양

- OS 타이틀바 없음(`WindowStyle="None"`) — 제목줄을 직접 그리고 드래그로 옮길 수 있음
- 폭 420 고정, 높이는 내용에 맞춰 자동
- 셸과 같은 3단 구성: 제목줄(판) / 본문(바탕) / 버튼줄(판), 사이는 1px 헤어라인
- 본문은 아이콘 24px + 메시지, 아이콘은 상단 정렬(메시지가 길어도 위에 고정)
- 버튼은 우측 하단. 확인/예가 주 동작(`Accent.Fill` 채움), 아니오가 보조(외곽선)
- 아이콘: 알림 `circle-info`, 오류 `circle-x`(붉은색), 확인 `circle-help`
- Enter = 확인, Esc = 취소

## 1. 새 파일 3개

| 이 프로젝트 | 리포지토리 |
| --- | --- |
| `code/UbisamBase.Core/Messaging/MessageDialog.xaml` | `src/UbisamBase.Core/Messaging/MessageDialog.xaml` (신규) |
| `code/UbisamBase.Core/Messaging/MessageDialog.xaml.cs` | `src/UbisamBase.Core/Messaging/MessageDialog.xaml.cs` (신규) |
| `code/UbisamBase.Core/Messaging/MessageUtil.cs` | `src/UbisamBase.Core/Messaging/MessageUtil.cs` (교체) |

## 2. 갱신 파일 3개

| 파일 | 변경 |
| --- | --- |
| `Themes/Icons.xaml` | 아이콘 3종 추가 — `Ubisam.Icon.Info` / `.Question` / `.Error` |
| `Themes/Colors.Light.xaml` | 오류색 토큰 2개 — `Danger` #C93B3B / `Danger.Fill` #B32F2F |
| `Themes/Colors.Dark.xaml` | 오류색 토큰 2개 — `Danger` #F07070 / `Danger.Fill` #B33A3A |

붉은색은 대화상자 아이콘과 실패 표시에만 씁니다 — 장식용 색을 늘리지 않는 것이 이 시스템의 규칙입니다.

## 3. 쓰는 법 (변화 없음)

```csharp
MessageUtil.ShowInfo("저장", "저장되었습니다.");
MessageUtil.ShowError("저장 실패", "저장하지 못했습니다.");

if (MessageUtil.ShowYesNo("확인", "저장하지 않은 변경 사항이 있습니다.\n그래도 나가시겠습니까?"))
{
    // 예를 눌렀을 때
}

MessageUtil.ShowSavedToast(logger);
```

여러 줄 메시지는 `\n`을 그대로 쓰면 됩니다 — `TextWrapping="Wrap"`이라 폭을 넘기면 자동으로도 접힙니다.

## 4. 확인

1. `저장` 버튼을 눌러 대화상자가 뜰 때 OS 타이틀바가 없고 셸과 같은 각진 프레임인지
2. 다크 테마에서 대화상자도 어두운지 (밝은 창이 뜨면 파일이 교체되지 않은 것)
3. 제목줄을 끌어 창이 움직이는지
4. Enter로 확인, Esc로 취소가 되는지
5. 오류 대화상자의 아이콘만 붉은색이고, 버튼은 accent인지
6. 확인(YesNo) 대화상자에서 "아니오"가 보이고 false를 돌려주는지
7. 부모 창 가운데에 뜨는지

## 주의

- `MessageDialog`는 `ShellStyles.xaml`을 자체 `Window.Resources`에 머지합니다 — 별도 창이라 셸의 리소스를 상속받지 못하기 때문입니다. 이 줄을 지우면 스타일이 빠집니다.
- 작업 표시줄에 안 뜨게 `ShowInTaskbar="False"`로 두었습니다.
- 부팅 초기(`MainWindow`가 아직 로드되지 않은 시점)에 호출하면 화면 가운데에 뜹니다.

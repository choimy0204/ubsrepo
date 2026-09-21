# 패치 12 — 화면 잠금

화면 전체를 불투명하게 가리고, 아이디 + 비밀번호로만 해제하는 오버레이입니다. 잠금 중에는 밑 화면이 보이지 않고 마우스·키보드도 통과하지 않습니다.

## 1. 새 파일 3개

| 이 프로젝트 | 리포지토리 |
| --- | --- |
| `code/UbisamBase.Core/Locking/LockService.cs` | `src/UbisamBase.Core/Locking/LockService.cs` |
| `code/UbisamBase.Core/Locking/LockOverlay.xaml` | `src/UbisamBase.Core/Locking/LockOverlay.xaml` |
| `code/UbisamBase.Core/Locking/LockOverlay.xaml.cs` | `src/UbisamBase.Core/Locking/LockOverlay.xaml.cs` |
| `code/UbisamBase.Core/Locking/LockCredentialStore.cs` | `src/UbisamBase.Core/Locking/LockCredentialStore.cs` |
| `code/UbisamBase.Core/Locking/LockSettingsView.xaml` | `src/UbisamBase.Core/Locking/LockSettingsView.xaml` |
| `code/UbisamBase.Core/Locking/LockSettingsView.xaml.cs` | `src/UbisamBase.Core/Locking/LockSettingsView.xaml.cs` |

## 2. 색 토큰 1개 추가

`Colors.Light.xaml` / `Colors.Dark.xaml`에 실패 안내판 배경을 추가했습니다 (`code/`의 두 파일에 이미 반영).

| 키 | 라이트 | 다크 |
| --- | --- | --- |
| `Ubisam.Brush.Danger.Tint` | `#12C93B3B` | `#1AF07070` |

## 3. `ShellWindow.xaml` — 오버레이 얹기

`Grid`의 **마지막 자식**으로 넣으세요. 토스트보다 뒤에 와야 잠금 화면이 토스트를 덮습니다.

```xml
xmlns:locking="clr-namespace:UbisamBase.Core.Locking"
```

```xml
        <!-- 화면 잠금. 마지막 자식이라 다른 모든 것을 덮는다. -->
        <locking:LockOverlay
            Visibility="{Binding IsLocked, Source={x:Static locking:LockService.Current}, Converter={StaticResource BoolToVis}}"/>
    </Grid>
</Window>
```

`Visibility`가 `Collapsed`면 히트 테스트도 되지 않으므로, 잠금이 아닐 때는 아래 UI가 정상 동작합니다.

## 4. 하단 탭에 잠금 버튼

`NavActionItem`을 그대로 쓰면 됩니다 — "바탕화면"과 같은 방식입니다.

```csharp
new NavActionItem("화면 잠금", Icon.Lock, () => LockService.Current.Lock());
```

`Icon` enum에 `Lock`이 없으면 추가하세요. `Themes/Icons.xaml`에는 `Ubisam.Icon.Lock`이 이미 있습니다.

## 5. 잠금 설정 화면 — 설정 탭에 등록

아이디·비밀번호를 바꾸는 화면입니다. 설정 탭의 서브탭으로 넣으세요.

```csharp
manager.AddSub<LockSettingsView, object>(ViewIds.Settings, "화면 잠금");
```

ViewModel이 없는 화면입니다(코드비하인드가 `LockCredentialStore`를 직접 다룹니다). 등록 헬퍼가 ViewModel 타입을 요구하면 빈 클래스를 하나 넘기거나, ViewModel 없이 등록하는 오버로드를 쓰세요.

**저장 방식** — `LockCredentialStore`가 처리합니다.

- 비밀번호는 평문으로 두지 않습니다: **PBKDF2 (SHA-256, 10만 회)** 해시 + 16바이트 salt
- 파일 위치: `%ProgramData%\UbisamBase\lock.json` — 사용자 계정이 아니라 장비 단위 설정이므로
- 비교는 고정 시간 비교(`FixedTimeEquals`)로 — 응답 시간으로 비밀번호를 추측할 수 없게
- 파일이 없거나 깨졌으면 기본값(`operator` / `0000`)으로 동작하고, 설정 화면 상단에 **경고 배너**가 뜹니다
- 변경에는 **현재 비밀번호 확인**이 필요합니다

`LockService.Validator`의 기본값이 이 저장소를 가리키므로 별도 배선은 필요 없습니다. 나중에 실제 계정 서비스가 생기면 델리게이트만 바꾸세요.

```csharp
LockService.Current.Validator = (id, password) => accountService.Verify(id, password);
```

### 설정 화면 검증

| 조건 | 결과 |
| --- | --- |
| 새 비밀번호 4자 미만 | Fail 토스트 |
| 새 비밀번호 ≠ 확인 | 확인 칸 아래 붉은 안내 (입력 중 실시간) |
| 허용 횟수가 1 미만 또는 숫자 아님 | Fail 토스트 |
| 현재 비밀번호 불일치 | Error 토스트, 현재 비밀번호 칸 초기화 |
| 성공 | Success 토스트, 입력칸 초기화 |

## 6. 동작

- 잠금이 걸리면 아이디 칸에 자동으로 커서가 갑니다
- Enter = 잠금 해제 (`IsDefault`)
- **Esc로는 빠져나갈 수 없습니다** — 잠금이므로 의도한 동작입니다
- 틀리면 비밀번호만 지우고 남은 시도 횟수를 안내합니다
- 3회(`MaxAttempts`) 초과하면 입력과 버튼이 비활성화됩니다. `LockService.Current.ResetAttempts()`로 관리자가 풀어야 합니다

## 7. 확인

1. 하단 탭의 잠금 버튼을 누르면 화면 전체가 가려지는지 — 상단바·탭·컨텐츠가 전혀 보이지 않아야 합니다
2. 잠금 중에 탭을 클릭해도 반응이 없는지
3. 아이디 칸에 바로 타이핑이 되는지
4. 틀린 비밀번호로 남은 횟수 안내가 뜨는지, 3회 후 비활성화되는지
5. Esc를 눌러도 풀리지 않는지
6. 다크 테마에서도 판이 어둡고 자물쇠가 accent인지
7. 잠금 중에 토스트가 위로 뜨지 않는지 (오버레이가 덮어야 함)
8. 설정 → 화면 잠금에서 비밀번호를 바꾸고, 새 비밀번호로 해제되는지
9. 처음 실행 시 설정 화면에 "기본 비밀번호 사용 중" 경고가 뜨는지
10. `%ProgramData%\UbisamBase\lock.json`에 평문 비밀번호가 없는지 (salt·hash만 있어야 합니다)

## 주의

- `ShellWindow`가 `WindowState="Maximized"` + `WindowStyle="None"`이라 오버레이가 실제로 화면 전체를 덮습니다. 창 모드로 바꾸면 창 안쪽만 가려집니다 — OS 레벨 잠금이 필요하면 별도 작업입니다.
- 잠금 중에도 `ToastService`는 계속 메시지를 큐에 넣습니다(가려서 안 보일 뿐). 잠금 중 알림을 오버레이에 보여주려면 알려주세요.
- `PasswordBox`는 `Controls.xaml`의 암시적 TextBox 스타일을 받지 못합니다(다른 타입이라서). 두 화면에서 같은 모양을 각자 정의해 두었습니다 — 입력칸 모양을 바꾸실 때는 세 곳(`Controls.xaml`, `LockOverlay.xaml`, `LockSettingsView.xaml`)을 함께 고쳐야 합니다.
- `Alt+Tab`이나 `Ctrl+Alt+Del`은 이 오버레이로 막을 수 없습니다. 그 수준의 잠금이 필요하면 키보드 훅이나 셸 대체가 필요합니다 — 요구사항이면 말씀해 주세요.

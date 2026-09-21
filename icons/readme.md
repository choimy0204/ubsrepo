# Ubisam Base 아이콘 세트

**Lucide**(https://lucide.dev) 원본 경로를 그대로 씁니다 — 24×24 그리드, 스트로크 1.5, `currentColor`.
바인딩된 Industry 디자인 시스템이 지정한 아이콘 세트이므로, 새 아이콘도 Lucide에서 이름으로 가져오세요.

| 파일 | 용도 |
| --- | --- |
| `{이름}.svg` | 개별 아이콘 30개 |
| `_sprite.svg` | 전부 담은 스프라이트 — `<use href="_sprite.svg#icon-home">` |

WPF에서는 `code/UbisamBase.Core/Themes/Icons.xaml`의 `Geometry` 리소스를 쓰세요 (같은 도형).

## 목록

| 파일 | 뜻 | Lucide 원본 이름 | WPF 키 | AddMain icon |
| --- | --- | --- | --- | --- |
| `home.svg` | 홈 | `house` | `Ubisam.Icon.Home` | `"Home"` |
| `monitor.svg` | 모니터링·그래프 | `activity` | `Ubisam.Icon.Monitor` | `"Monitor"` |
| `chart.svg` | 통계·리포트 | `chart-column` | `Ubisam.Icon.Chart` | `"Chart"` |
| `maintenance.svg` | 유지보수 | `clipboard-check` | `Ubisam.Icon.Maintenance` | `"Maintenance"` |
| `settings.svg` | 설정 | `sliders-horizontal` | `Ubisam.Icon.Settings` | `"Settings"` |
| `alarm.svg` | 알람 | `bell` | `Ubisam.Icon.Alarm` | `"Alarm"` |
| `log.svg` | 로그 | `list` | `Ubisam.Icon.Log` | `"Log"` |
| `document.svg` | 문서·리포트 | `file-text` | `Ubisam.Icon.Document` | `"Document"` |
| `data.svg` | 데이터 | `database` | `Ubisam.Icon.Data` | `"Data"` |
| `power.svg` | 전원 | `power` | `Ubisam.Icon.Power` | `"Power"` |
| `temperature.svg` | 온도 | `thermometer` | `Ubisam.Icon.Temperature` | `"Temperature"` |
| `gauge.svg` | 압력·게이지 | `gauge` | `Ubisam.Icon.Gauge` | `"Gauge"` |
| `run.svg` | 운전 | `play` | `Ubisam.Icon.Run` | `"Run"` |
| `stop.svg` | 정지 | `square` | `Ubisam.Icon.Stop` | `"Stop"` |
| `reset.svg` | 리셋 | `rotate-cw` | `Ubisam.Icon.Reset` | `"Reset"` |
| `account.svg` | 계정 | `user` | `Ubisam.Icon.Account` | `"Account"` |
| `lock.svg` | 권한·보안 | `lock` | `Ubisam.Icon.Lock` | `"Lock"` |
| `export.svg` | 내보내기 | `download` | `Ubisam.Icon.Export` | `"Export"` |
| `import.svg` | 불러오기 | `upload` | `Ubisam.Icon.Import` | `"Import"` |
| `search.svg` | 검색 | `search` | `Ubisam.Icon.Search` | `"Search"` |
| `calendar.svg` | 일정·이력 | `calendar` | `Ubisam.Icon.Calendar` | `"Calendar"` |
| `clock.svg` | 시간·주기 | `clock` | `Ubisam.Icon.Clock` | `"Clock"` |
| `link.svg` | 통신·연결 | `link` | `Ubisam.Icon.Link` | `"Link"` |
| `warning.svg` | 경고 | `triangle-alert` | `Ubisam.Icon.Warning` | `"Warning"` |
| `pass.svg` | 검사 결과 | `circle-check` | `Ubisam.Icon.Pass` | `"Pass"` |
| `folder.svg` | 파일·폴더 | `folder` | `Ubisam.Icon.Folder` | `"Folder"` |
| `vision.svg` | 비전·카메라 | `camera` | `Ubisam.Icon.Vision` | `"Vision"` |
| `control.svg` | 제어·I/O | `cpu` | `Ubisam.Icon.Control` | `"Control"` |
| `recipe.svg` | 레시피 | `layers` | `Ubisam.Icon.Recipe` | `"Recipe"` |
| `modules.svg` | 모듈 목록 | `layout-grid` | `Ubisam.Icon.Modules` | `"Modules"` |
| `tool.svg` | 도구 | `wrench` | `Ubisam.Icon.Tool` | `"Tool"` |
| `display.svg` | 모니터·화면 | `monitor` | `Ubisam.Icon.Display` | `"Display"` |
| `computer.svg` | 컴퓨터·PC | `hard-drive` | `Ubisam.Icon.Computer` | `"Computer"` |

## 색

- 라이트: 기본 `#5D5D60`, 선택 `#2196F3`
- 다크: 기본 `#C2C7CE`, 선택 `#22D3EE`

`currentColor`라서 부모에 색만 지정하면 됩니다 — 파일을 색별로 복제하지 마세요.

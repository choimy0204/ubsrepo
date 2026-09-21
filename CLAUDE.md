# CLAUDE.md

## 프로젝트 개요
UbisamBase 플랫폼 본체 저장소(원본 소스). 구성:

- `UbisamBase.Core` — 플랫폼 라이브러리: Shell UI, DI, 설정/로그인/백업/로그/다국어 등 공통 서비스
- `UbisamBase.Shell` — 고정 호스트. `IAppSetup` 구현 dll을 실행 시 리플렉션으로 로드
- `UbisamBase.Launcher` — 실행 진입점. 실행할 때마다 `D:\UbisamPlatform\bin`을 자기 폴더로 복사한
  뒤 Shell을 **같은 프로세스 안에서** 로드한다(그래야 VS 디버거가 붙어 소비 프로젝트 중단점이 걸린다)

**이 저장소는 "플랫폼" 저장소다.** 이 플랫폼을 사용하는 예제/소비 프로젝트(`UbisamBase.App`)는
별도 저장소 `D:\업무\03_개발\Source\용접기\08_UbisamBase`에 있다. 사용법 문서
(`doc\platform-guide.md`, `doc\UbisamBase_플랫폼_사용_매뉴얼.pptx`)는 이 저장소 `doc\`에 있다.

## 디자인 자산
`.design`(FIX 설계 문서, 화면 코드 스냅샷, 로고)과 `icons`(Core의 `Icons.xaml` Geometry
리소스가 참고하는 원본 SVG 세트)도 이 저장소에 있다 — 전부 Core/Shell 관련 자산이라
`08_UbisamBase`(베이스 예제 저장소)가 아니라 여기에 둔다.

## 빌드 = D드라이브 배포
플랫폼을 빌드하면(Debug/Release 구분 없이) 루트의 `Directory.Build.targets`가 결과물을 곧바로
`D:\UbisamPlatform\bin`에 반영한다(`.pdb` 제외, Shell의 바인딩 리디렉트 설정을 Launcher 설정으로 복사,
README 갱신). 이 폴더를 보는 모든 소비 프로젝트(`08_UbisamBase\src\UbisamBase.App`,
`09_Temp\TestApp`, MergeHub 등)는 Launcher로 실행되므로, **플랫폼만 빌드하고 소비 프로젝트를
실행하면 새 플랫폼으로 뜬다** — 소비 프로젝트를 다시 빌드할 필요 없다.

- 솔루션(`UbisamBase.Platform.slnx`) 단위로 빌드하는 게 가장 확실하다. Core만 빌드하면 Core.dll만,
  Shell을 빌드하면 Shell 출력 전체가 반영된다.
- 고치다 만 코드도 빌드하는 순간 이 PC의 모든 플랫폼 프로그램에 적용된다는 점에 주의.

## 깨끗한 배포본 만들기
자동 반영은 덮어쓰기만 해서, 쓰지 않게 된 옛 dll이 배포 폴더에 남을 수 있다. 다른 PC에 넘기기
전처럼 Release 결과물만으로 깨끗하게 채우려면 아래 스크립트를 실행한다(폴더를 비우고 Release로
다시 빌드).

```powershell
powershell -ExecutionPolicy Bypass -File scripts\publish-platform.ps1
```

## 하네스: 기능 개발 SDLC 파이프라인

**목표:** 기능 아이디어를 설계 문서 → 기획 검토 → 개발/테스트 계획 → QA → 코드 리뷰 → 배포까지 추적 가능한 흐름으로 진행한다.

**트리거:** 신규 기능 아이디어 논의, 기획 검토, 개발/테스트 계획 수립, QA, 코드 리뷰, 배포 준비 등 기능 개발 SDLC 관련 요청 시 `feature-pipeline` 스킬을 사용하라. 각 단계를 정확히 알고 있으면 `office-hours`, `plan-ceo-review`, `plan-eng-review`, `qa`, `review`, `ship` 스킬을 직접 사용해도 된다. 단순 질문은 직접 응답 가능.

**변경 이력:** `doc/history/changelog.md` 참조.

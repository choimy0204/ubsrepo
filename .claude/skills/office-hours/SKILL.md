---
name: office-hours
description: "새 기능 아이디어를 질문으로 좁혀 설계 문서로 정리하는 feature-pipeline 1단계. '이런 기능 만들고 싶은데', '새 기능 논의하고 싶어' 같은 모호한 아이디어 논의 요청, 또는 기존 설계 문서를 보완/수정하는 요청에 사용."
---

# Office Hours — 설계 문서 작성

`office-hours-facilitator` 에이전트를 호출해 사용자의 기능 아이디어를 설계 문서로 정리하는 feature-pipeline의 시작 단계. 이 스킬만 신규 feature-slug 생성을 담당한다(다른 5개 단계는 기존 slug를 전제).

## 실행 모드: 서브 에이전트

## 워크플로우

### 1. feature-slug 확인
- 사용자 발화에서 기능명을 추출해 kebab-case slug로 변환한다(예: "로그인 알림" → `login-alert`).
- `_workspace/` 하위에 기존 slug 폴더가 여러 개 있으면 사용자에게 신규인지 기존 보완인지 확인한다(추측 금지).
- **신규**: `_workspace/{slug}/`, `plan/{slug}/` 디렉토리를 생성하고 `pipeline_state.json`을 §5 스키마로 초기화한다.
- **기존 보완**: `_workspace/{slug}/01_design_doc.md`가 있으면 이를 읽어 에이전트에게 "기존 문서 + 사용자 피드백"으로 전달한다.

### 2. 에이전트 호출
`Agent` 도구로 `office-hours-facilitator`를 호출한다(`model: "opus"`). 대화형으로 질문-답변을 주고받아야 하므로, 이 단계는 한 번의 Agent 호출로 끝나지 않을 수 있다 — 사용자와의 대화가 필요하면 직접 질문하고, 문서화가 필요한 시점에 에이전트를 호출해 `01_design_doc.md`를 작성/갱신한다.

### 3. 상태 갱신
- `pipeline_state.json`의 `stages.office_hours.status`를 `completed`로, `completed_at`을 갱신한다.
- `plan/{slug}/progress.md`가 없으면 §5 형식으로 생성하고, 있으면 액션 로그에 행을 추가한다. 체크리스트 1번 항목을 체크한다.

### 4. 안내
설계 문서 경로를 안내하고, 다음 단계로 `plan-ceo-review` 스킬(또는 `/plan-ceo-review`)을 사용할 것을 제안한다.

## 에러 핸들링
- 아이디어가 여러 기능을 섞고 있으면 분리를 제안하고 feature-slug를 나눌 것을 권고한다.
- 사용자가 "성공 기준"을 명확히 못 정하면 강요하지 않고 "미정 — 확인 필요"로 남긴 채 다음 질문으로 넘어간다.

## 테스트 시나리오

### 정상 흐름
1. 사용자: "새 기능 아이디어 논의하고 싶어"
2. slug 생성, `_workspace/`, `plan/` 초기화
3. 1~3개씩 질문 반복 → `01_design_doc.md` 작성
4. `pipeline_state.json`, `progress.md` 갱신
5. 다음 단계(`plan-ceo-review`) 안내

### 에러 흐름
1. 사용자가 "성공 기준이 뭔지 잘 모르겠다"고 답함
2. 강제로 답을 유도하지 않고 "미정 — 확인 필요"로 문서에 남김
3. 다른 항목(범위, 리스크) 질문으로 진행 후 문서 완성

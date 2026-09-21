---
name: plan-ceo-review
description: "설계 문서를 경영진 관점('만들 가치가 있는가')에서 검토해 진행/조건부 진행/보류/반려로 결론짓는 feature-pipeline 2단계. '기획 검토해줘', '이거 진행할 가치 있는지 봐줘' 요청이나 이전 반려/보류 후 재검토 요청에 사용."
---

# Plan CEO Review — 기획 검토

`ceo-reviewer` 에이전트를 호출해 설계 문서의 진행 가치를 판단하는 feature-pipeline 2단계.

## 실행 모드: 서브 에이전트

## 워크플로우

### 1. feature-slug 확인 + 선행 조건 확인
- 대상 feature-slug를 확인한다(여러 개면 사용자에게 확인).
- `_workspace/{slug}/01_design_doc.md`가 없으면 진행하지 않고 `office-hours` 먼저 완료할 것을 안내한다.

### 2. 에이전트 호출
`Agent` 도구로 `ceo-reviewer`를 호출한다(`model: "opus"`). 입력: `01_design_doc.md` 경로. 출력: `_workspace/{slug}/02_ceo_review.md`.

### 3. 상태 갱신
- `pipeline_state.json`의 `stages.plan_ceo_review.status`를 `completed`로, `result`를 결론(진행/조건부 진행/보류/반려)으로, `completed_at`을 갱신한다.
- `plan/{slug}/progress.md` 액션 로그에 행 추가. **결론이 진행/조건부 진행일 때만** 체크리스트 2번 항목을 체크한다.

### 4. 안내
- 결론이 **진행/조건부 진행**이면 다음 단계 `plan-eng-review` 안내(조건부 진행이면 조건을 함께 안내).
- 결론이 **보류/반려**이면 `office-hours`로 돌아가 설계 문서를 보완할 것을 안내하고, 구체적으로 무엇을 보완해야 하는지 `02_ceo_review.md`의 피드백을 요약해 전달한다.

## 에러 핸들링
- `01_design_doc.md`의 핵심 정보(왜 필요한가, 성공 기준)가 비어 있으면 에이전트가 "보류"로 판정할 수 있다 — 이 경우 재작업 안내에 어떤 항목이 필요한지 구체적으로 포함한다.

## 테스트 시나리오

### 정상 흐름
1. 사용자: "설계 문서 기획 검토해줘"
2. `01_design_doc.md` 존재 확인
3. `ceo-reviewer` 호출 → 결론 "진행"
4. 상태/진행 로그 갱신, 체크박스 체크
5. `plan-eng-review` 안내

### 에러 흐름
1. `01_design_doc.md`가 존재하지 않음
2. `office-hours` 먼저 완료할 것을 안내하고 중단

---
name: plan-eng-review
description: "승인된 설계를 개발/테스트 계획(수용 기준 + 테스트 케이스)으로 변환하는 feature-pipeline 3단계. '개발 계획 짜줘', '테스트 계획 만들어줘' 요청이나 기존 테스트 계획 수정 요청에 사용."
---

# Plan Eng Review — 개발/테스트 계획

`eng-planner` 에이전트를 호출해 승인된 설계를 측정 가능한 테스트 계획으로 변환하는 feature-pipeline 3단계.

## 실행 모드: 서브 에이전트

## 워크플로우

### 1. feature-slug 확인 + 선행 조건 확인
- `_workspace/{slug}/02_ceo_review.md`가 없으면 진행하지 않고 `plan-ceo-review` 먼저 완료할 것을 안내한다.
- `02_ceo_review.md`의 결론이 **보류/반려**이면 중단하고, 재검토가 먼저 필요함을 안내한다.

### 2. 에이전트 호출
`Agent` 도구로 `eng-planner`를 호출한다(`model: "opus"`). 입력: `01_design_doc.md`, `02_ceo_review.md` 경로. 출력: `_workspace/{slug}/03_test_plan.md`.

### 3. 상태 갱신
- `pipeline_state.json`의 `stages.plan_eng_review.status`를 `completed`로, `completed_at`을 갱신한다.
- `plan/{slug}/progress.md` 액션 로그에 행 추가. 체크리스트 3번 항목을 체크한다.

### 4. 안내
계획 경로를 안내하고, "이제 구현을 진행하세요. 구현 완료 후 `/qa`로 검증을 요청하세요"라고 안내한다(구현 자체는 사용자가 직접 수행 — 에이전트 개입 없음).

## 에러 핸들링
- `02_ceo_review.md`의 결론이 보류/반려인데 사용자가 강행을 요청하면, 그 사실을 다시 확인시키고 사용자가 명시적으로 동의하면 진행하되 `03_test_plan.md`에 "게이트 우회 진행" 사실을 기록한다.
- 설계 범위가 테스트 케이스로 구체화하기에 너무 모호하면 `office-hours`로 되돌릴 것을 제안한다.

## 테스트 시나리오

### 정상 흐름
1. 사용자: "개발/테스트 계획 짜줘"
2. `02_ceo_review.md` 결론 "진행" 확인
3. `eng-planner` 호출 → `03_test_plan.md` 생성
4. 상태/진행 로그 갱신, 체크박스 체크
5. 사용자에게 구현 시작 및 이후 `/qa` 안내

### 에러 흐름
1. `02_ceo_review.md` 결론이 "반려"
2. 재검토 필요함을 안내하고 중단

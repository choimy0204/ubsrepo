---
name: feature-pipeline
description: "기능 아이디어를 설계 문서→기획 검토→개발/테스트 계획→QA→코드 리뷰→배포까지 추적 가능한 흐름으로 진행하는 오케스트레이터. '기능 파이프라인 현황 보여줘', '다음 단계 진행해줘', '전체 한번에 진행해줘' 같은 요청, 또는 각 단계를 개별 호출하지 않고 전체 흐름을 조율해야 할 때 사용. 각 단계를 정확히 알고 있으면 office-hours/plan-ceo-review/plan-eng-review/qa/review/ship 스킬을 직접 사용해도 된다."
---

# Feature Pipeline Orchestrator

기능 개발 SDLC 파이프라인(office-hours → plan-ceo-review → plan-eng-review → qa → review → ship) 6단계를 조율하는 오케스트레이터.

## 실행 모드: 서브 에이전트 (전문가 풀)

6단계는 실시간 협업 팀이 아니라, 사용자가 자기 일정에 맞춰 개별 호출하는 독립된 전문가들이다. 각 단계는 `Agent` 도구로 해당 에이전트를 1회 호출 → 결과를 파일로 저장 → 다음 단계 안내로 끝난다.

## 에이전트 구성

| # | 단계 스킬 | 에이전트 | 입력 | 출력 | 결론 값 |
|---|-----------|---------|------|------|---------|
| 1 | office-hours | office-hours-facilitator | 사용자 아이디어 | `01_design_doc.md` | - |
| 2 | plan-ceo-review | ceo-reviewer | `01_*` | `02_ceo_review.md` | 진행/조건부 진행/보류/반려 |
| 3 | plan-eng-review | eng-planner | `01_*`, `02_*` | `03_test_plan.md` | - |
| 4 | qa | qa-inspector (general-purpose) | `03_*` + 변경 코드 | `04_qa_report.md` | 통과/조건부 통과/실패 |
| 5 | review | code-reviewer | diff, `03_*`, `04_*` | `05_code_review.md` | 승인/변경요청/차단 |
| 6 | ship | release-manager | `05_*` | `06_ship_notes.md` | 가능/보류 |

## 워크플로우

### Phase 0: 컨텍스트 확인
1. `_workspace/` 디렉토리 존재 여부 확인.
2. **미존재** → 진행 중 기능 없음. `office-hours`(또는 `/office-hours`)로 시작할 것을 안내하고 종료.
3. **존재, feature-slug 폴더 1개** → 해당 slug로 Phase 1 진행.
4. **존재, feature-slug 폴더 여러 개** → 사용자에게 대상 slug를 확인한다(추측 금지).

### Phase 1: 현황 판단
1. `_workspace/{slug}/pipeline_state.json`을 읽는다. 손상됐거나 없으면 `_workspace/{slug}/` 산출물 파일 목록(`01_*`~`06_*`)으로 역추정한다.
2. `stages`를 순서대로 확인해 첫 `pending` 단계를 "현재 진행할 단계"로 판단한다.
3. 가장 최근 완료 단계의 `result`가 부정적(보류/반려/실패/차단)이면, "다음 단계"가 아니라 "그 단계의 재실행"이 현재 상태임을 인지한다.

### Phase 2: 분기
사용자 요청에 따라 아래 중 하나로 동작한다.

- **"현황만 보여줘"** → Phase 1의 판단 결과만 보고한다. 에이전트를 호출하지 않는다.
- **"다음 단계 진행해줘"** → 현재 단계에 해당하는 스킬(Skill 도구)을 호출해 해당 에이전트를 1회 실행한다.
- **"전체 한번에 진행해줘"** → 순서대로 각 단계 스킬을 호출한다. 단, **`qa` 직전에는 반드시 멈추고 사용자에게 구현 완료 여부를 확인**하고, **`ship` 직전에는 반드시 멈추고 배포 진행 동의를 확인**한다.

### Phase 3: 보고
- 각 단계 스킬이 `pipeline_state.json`과 `plan/{slug}/progress.md`를 직접 갱신하므로, 오케스트레이터는 그 결과를 사용자에게 보여주기만 한다.
- 산출물 경로 + 단계별 결론을 요약해 보고한다.

## 에러 핸들링

| 상황 | 전략 |
|------|------|
| `pipeline_state.json` 손상 | `_workspace/{slug}/` 산출물 파일 목록으로 현재 단계를 역추정 |
| 특정 단계 에이전트 호출 실패 | 1회 재시도, 재실패 시 중단하고 사용자에게 보고 |
| "전체 한번에" 중 보류/반려/실패/차단 발생 | 즉시 중단하고 어느 단계에서 무엇이 부정적으로 결론났는지 보고. 다음 단계로 임의 진행하지 않음 |
| feature-slug 여러 개 존재 | 추측하지 않고 목록을 보여주며 사용자에게 확인 |

## 게이트 규칙

| 단계 | 선행 조건 | 부정적 결론 시 |
|------|----------|---------------|
| plan-ceo-review | `01_design_doc.md` 존재 | - |
| plan-eng-review | `02_ceo_review.md` 존재 | 결론이 보류/반려면 중단, office-hours로 안내 |
| qa | `03_test_plan.md` 존재 | - |
| review | `04_qa_report.md` 존재 | 결론이 실패면 수정 확인 먼저 요구 |
| ship | `05_code_review.md` 존재 | 결론이 승인이 아니면 중단 |

## 데이터 흐름

```
[사용자 요청]
     ↓
Phase 0: _workspace/ 존재 확인 → feature-slug 확정
     ↓
Phase 1: pipeline_state.json → 현재 단계 판단
     ↓
Phase 2: 현황 보고 / 단일 단계 진행 / 전체 진행 중 분기
     ↓
office-hours → plan-ceo-review → plan-eng-review → (사용자 구현) → qa → review → ship
     ↓
Phase 3: 산출물 경로 + 결론 요약 보고
```

## 테스트 시나리오

### 정상 흐름
1. 사용자: "새 기능 파이프라인 전체 한번에 진행해줘"
2. Phase 0: `_workspace/` 없음 → `office-hours`부터 시작
3. office-hours → plan-ceo-review(진행) → plan-eng-review 순차 실행
4. 구현 완료 여부를 사용자에게 확인 후 `qa` 실행(통과)
5. `review` 실행(승인) → 배포 동의 확인 후 `ship` 실행
6. `pipeline_status: shipped`로 최종 보고

### 에러 흐름
1. 사용자: "다음 단계 진행해줘"
2. Phase 1: 직전 `plan_ceo_review`의 `result`가 "반려"임을 확인
3. "다음 단계"가 아니라 "설계 문서 보완이 먼저 필요"함을 안내하고 `office-hours` 재실행을 제안
4. 사용자 동의 없이 `plan-eng-review`로 임의 진행하지 않음

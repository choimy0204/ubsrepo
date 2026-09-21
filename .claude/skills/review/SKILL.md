---
name: review
description: "feature-pipeline QA 통과 후 QA 결함 수정 여부를 diff로 대조해 승인/변경요청/차단으로 결론짓는 5단계 코드 리뷰 게이트. '코드 리뷰 해줘', 'QA 결함 고쳤는지 확인해줘' 요청에 사용. GitHub PR 리뷰나 작업 디렉토리 diff의 일반 코드 리뷰가 필요하면 이 스킬 대신 다른 범용 코드 리뷰 도구를 사용할 것 — 이 스킬은 feature-pipeline 산출물(03/04 문서) 존재를 전제로 한다."
---

# Review — 코드 리뷰 게이트

`code-reviewer` 에이전트를 호출해 QA 결함이 실제로 수정됐는지 diff로 확인하는 feature-pipeline 5단계.

## 실행 모드: 서브 에이전트

## 워크플로우

### 1. feature-slug 확인 + 선행 조건 확인
- `_workspace/{slug}/04_qa_report.md`가 없으면 진행하지 않고 `qa` 먼저 완료할 것을 안내한다.
- `04_qa_report.md`의 결론이 **실패**이면, 수정이 완료됐는지 먼저 사용자에게 확인한다(수정 없이 리뷰를 요청하면 즉시 차단될 것임을 안내).

### 2. 에이전트 호출
`Agent` 도구로 `code-reviewer`를 호출한다(`model: "opus"`). 입력: 변경 diff, `03_test_plan.md`, `04_qa_report.md` 경로. 출력: `_workspace/{slug}/05_code_review.md`.

### 3. 상태 갱신
- `pipeline_state.json`의 `stages.review.status`를 `completed`로, `result`를 결론(승인/변경요청/차단)으로, `completed_at`을 갱신한다.
- `plan/{slug}/progress.md` 액션 로그에 행 추가. **결론이 승인일 때만** 체크리스트 5번 항목을 체크한다.

### 4. 안내
- 결론이 **승인**이면 다음 단계 `ship` 안내.
- 결론이 **변경요청/차단**이면 이슈 목록을 요약해 전달하고, 수정 후 다시 `/review`를 호출할 것을 안내한다.

## 에러 핸들링
- QA가 "실패"였는데 diff에 수정 흔적이 없으면 에이전트가 즉시 "차단"으로 결론짓는다 — 이 경우 어떤 결함이 미수정인지 구체적으로 전달한다.

## 테스트 시나리오

### 정상 흐름
1. 사용자: "코드 리뷰 해줘"
2. `04_qa_report.md` 결론 "통과" 확인
3. `code-reviewer` 호출 → 결론 "승인"
4. 상태/진행 로그 갱신, 체크박스 체크
5. `ship` 안내

### 에러 흐름
1. QA 결론이 "실패"였는데 diff에 해당 결함 수정 흔적 없음
2. `code-reviewer`가 "차단" 결론
3. 미수정 결함을 사용자에게 전달, 수정 후 재호출 안내

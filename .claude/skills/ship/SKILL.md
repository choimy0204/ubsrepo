---
name: ship
description: "코드 리뷰 승인 확인 후 배포 노트를 작성하는 feature-pipeline 마지막 단계. '배포 준비해줘', '배포 노트 써줘', '이제 배포해도 될지 봐줘' 요청에 사용. 실제 배포 명령 실행은 사용자의 명시적 동의 없이 진행하지 않는다."
---

# Ship — 최종 배포 검토

`release-manager` 에이전트를 호출해 코드 리뷰 승인 여부를 재확인하고 배포 노트를 작성하는 feature-pipeline 마지막 단계.

## 실행 모드: 서브 에이전트

## 워크플로우

### 1. feature-slug 확인 + 선행 조건 확인
- `_workspace/{slug}/05_code_review.md`가 없으면 진행하지 않고 `review` 먼저 완료할 것을 안내한다.
- `05_code_review.md`의 결론이 **승인**이 아니면 중단하고, 재검토가 먼저 필요함을 안내한다.

### 2. 에이전트 호출
`Agent` 도구로 `release-manager`를 호출한다(`model: "opus"`). 입력: `05_code_review.md` 경로. 출력: `_workspace/{slug}/06_ship_notes.md`.

### 3. 상태 갱신
- `pipeline_state.json`의 `stages.ship.status`를 `completed`로, `completed_at`을 갱신한다.
- 배포 가능으로 판단되면 `pipeline_state.json` 최상위 `pipeline_status`를 `shipped`로 갱신한다.
- `plan/{slug}/progress.md` 액션 로그에 행 추가. 배포 가능일 때만 체크리스트 6번 항목을 체크한다.

### 4. 안내
배포 노트를 요약해 전달한다. **실제 배포 명령(빌드 배포, 프로덕션 반영 등) 실행은 반드시 먼저 사용자에게 동의를 구한다.** 사용자가 동의하지 않으면 노트 작성까지만 완료된 상태로 남긴다.

## 에러 핸들링
- `05_code_review.md`의 결론이 승인이 아니면 시작하지 않는다.
- 사용자가 배포 실행에 동의하지 않으면 "보류"로 남기고 배포 노트만 완성한 채 종료한다.

## 테스트 시나리오

### 정상 흐름
1. 사용자: "배포 준비해줘"
2. `05_code_review.md` 결론 "승인" 확인
3. `release-manager` 호출 → 배포 가능 판단, `06_ship_notes.md` 생성
4. 사용자에게 실제 배포 실행 동의 여부 확인
5. 동의 시 `pipeline_status`를 `shipped`로 갱신, 진행 로그/체크박스 갱신

### 에러 흐름
1. `05_code_review.md` 결론이 "변경요청"
2. 재검토(`review`)가 먼저 필요함을 안내하고 중단

# Research: 신고 시나리오

## R-1. 신고 시도 입력 경로
- **Decision**: `SuspicionEvents.OnReportAttempt(npc, player)` 하나만 구독한다. 002가 3 도달 시, 004가 깨어남 시(`unconscious_wake` 또는
  `ForceReportAttempt`) 발행하고, 003 침묵자는 `SuppressReportAttempt`로 발행되지 않는다. 005는 유형을 보지 않는다.
- **Rationale**: FR-005·FR-010. 발행 책임은 002에 있고 005는 소비만.

## R-2. 전화기 / 관리소
- **Decision**: `ReportPoint { Kind: Phone|Office, IsUsable, CanBeCut = Kind==Phone }`. 끊기 상호작용은 전화기에만 부착. 관리소는 항상
  사용 가능. 지점이 하나도 없으면 흐름은 시작하지 않고 `OnReportAbandoned`(사유 "지점 없음") + 섬 변화 없음(§5.2 "미리 끊어뒀다").
- **Alternatives**: 관리소도 끊기 가능 — SDD "전화선"만 언급. 기각.

## R-3. 도착 시간 보장
- **Decision**: 순수 `ReportSpeedRule.SpeedFor(distance, normalSpeed, minWalkSeconds) = min(normalSpeed, distance / minWalkSeconds)`.
  흐름 시작·재타깃 시 `NpcMover.Speed`에 적용, 종료 시 복구. 목적지 표시는 NPC 머리 위 라벨 "→ 전화기 A" (프리미티브·텍스트만).
- **Rationale**: FR-006 "플레이어가 반응할 수 있을 만큼 길다".

## R-4. 끊기 상호작용
- **Decision**: `PhoneCutInteractable.Interact(player)`가 `PlayerCutter.Begin(point)`을 호출. `CutProgress`(순수)가 매 프레임 `Tick(dt, stillValid)`;
  유효 조건 = 거리 ≤ range && `MovementState == Idle` && !IsLocked && !HandsBusy. 무효가 되면 취소. 완료 시 `point.SetUsable(false)` +
  `OnPhoneCut`. `cutRequiresTool`이 참이면 `PlayerToolkit.HasCuttingTool`이 거짓일 때 시작 거부 + 안내.
- **Rationale**: FR-002~FR-004. 도구 정의는 확정대기라 불리언 입력만.

## R-5. 흐름 상태기계와 재타깃
- **Decision**: 순수 `ReportFlowState`: `Moving(target)` → 도착 `BeginReporting(now)` → `reportDuration` 경과 `Complete()`; 이동·신고 중
  목적지가 사용 불가가 되면 `Retarget(next)` 또는 `Abandon()`; 기절 시 `Interrupt()`. 실제 위치·거리는 `ReportFlow`가 넣는다.
- **Rationale**: FR-007·FR-008·FR-011 테스트 가능성.

## R-6. 중단·재시작
- **Decision**: `ReportFlow.Update`가 `UnconsciousState.IsUnconscious(npc)`면 `Interrupt` 후 컴포넌트 제거. 재시작은 004 깨어남이 발행하는
  `OnReportAttempt`가 다시 흐름을 만든다. 정면 밀치기 실패는 흐름에 영향 없음(`shove_failed`는 개인 값만).

## R-7. 다른 행동과의 우선순위
- **Decision**: 흐름 중 `NpcSuspicionProfile.SuppressStageBehaviour=true`(원값 저장·복구)로 002 따라다니기 억제(FR-013), 003
  `InformerBehaviour`·`SentinelBehaviour`는 `enabled=false`(복구). 신고 흐름이 이동 주도권을 갖는다.

## R-8. 완료 이력
- **Decision**: `ReportSystem`이 `(npcId, playerId)` 완료 집합을 갖고, 같은 쌍의 `OnReportAttempt`는 무시(FR-009). 다른 플레이어는 별개.
  포기·중단은 이력에 남지 않는다(다시 시도 가능).

## R-9. 신고 완료 대가
- **Decision**: 002 JSON에 `report_completed`(personal 0, island 25, scope global, 확정, "§5.2 내버려둔다") 추가. `Raise(At(id, player, pos))`.
  이후 개인 값은 3 유지, 같은 플레이어 재신고 없음.

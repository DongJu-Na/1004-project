# Data Model: 신고 시나리오

## 열거형
- `ReportPointKind { Phone, Office }` — §5.1 "전화기 / 관리소"
- `ReportPhase { Moving, Reporting, Completed, Abandoned, Interrupted }`

## ReportRules (JSON) — contracts/report-rules-schema.json
| 필드 | 규칙 | 상태 |
|---|---|---|
| minWalkSeconds > 0 | 도보 최소 시간 | 확정대기 |
| reportDurationSeconds ≥ 0 | 도착 후 신고 동작 시간 | 확정대기 |
| cutDurationSeconds > 0 | 끊기 소요 | 확정대기 |
| cutRequiresTool (bool) | 도구 요구 | 확정대기 (v0.6 §3.1) |
| cutRange > 0 | 끊기 상호작용 거리 | 확정대기 |
| arriveDistance > 0 | 지점 도착 판정 거리 | 확정대기 |
| completedEventId | 002 events 존재 (`report_completed`) | 확정 |

## 002 사건 추가
`report_completed`: personal 0, island 25, scope global, 확정, "§5.2 신고 완료 — 섬 +25".

## 순수 로직
- `ReportPointSelector.PickNearestUsable(IReadOnlyList<(string id, Vector3 pos, bool usable)>, Vector3 from) → string id or null`
- `ReportFlowState`: `Phase`, `TargetId`; `Start(targetId)`, `Arrive(now)`(Moving→Reporting), `Tick(now, reportDuration) → bool completed`, `Retarget(newId or null)`(null→Abandoned), `Interrupt()`; Completed/Abandoned/Interrupted는 종단.
- `CutProgress(duration)`: `Progress01`, `IsDone`, `Tick(dt, valid) → CutTickResult {Progressed, Cancelled, Completed}`, `Reset()`.
- `ReportSpeedRule.SpeedFor(distance, normalSpeed, minWalkSeconds)`.

## 런타임
- `ReportPoint`(씬 배치): `Id`, `Kind`, `DisplayName`, `IsUsable`, `CanBeCut`, `SetUsable(bool)`; static `All`.
- `PhoneCutInteractable`(전화기): IInteractable, `PromptText="전화선 끊기 ({n}초)"`; `CanInteract`: usable && 도구 조건 && 빈손/잠금 아님.
- `PlayerCutter`(플레이어): `Begin(point)`, 진행 중 HUD 보조 안내 "끊는 중 {p}%", 이동 시 취소.
- `PlayerToolkit`(플레이어): `HasCuttingTool`(직렬화, 외부 입력).
- `ReportFlow`(NPC, 동적): `State`, `TargetPlayer`, `TargetPoint`, 저장한 원값(mover speed, SuppressStageBehaviour, 비활성화한 Behaviour).
- `ReportSystem`: `IsReady`, `Rules`, `TryStart(npc, player)`, `IsCompleted(npc, player)`, `ActiveFlows`.

## 이벤트 (ReportEvents)
- `OnReportStarted(NpcIdentity, PlayerEntity, ReportPoint)`
- `OnReportRetargeted(NpcIdentity, PlayerEntity, ReportPoint newPoint)`
- `OnReportAbandoned(NpcIdentity, PlayerEntity, string reason)`
- `OnReportCompleted(NpcIdentity, PlayerEntity, ReportPoint)`
- `OnReportInterrupted(NpcIdentity, PlayerEntity)`
- `OnPhoneCut(ReportPoint, PlayerEntity)`, `OnCutCancelled(ReportPoint, PlayerEntity)`

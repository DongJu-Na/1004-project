# Data Model: 의심 시스템과 시간대 위협

**Date**: 2026-09-23 | **Plan**: [plan.md](plan.md) | 조항 번호는 SDD v0.8.

## 순수 모델 (Model/, EditMode 테스트 대상)

### PersonalSuspicion — §2.1, §2.2

| 필드 | 타입 | 규칙 |
|---|---|---|
| Value | int | **0 ≤ Value ≤ 3**. 초깃값 0. |
| Stage | enum SuspicionStage {Indifferent=0, Aware=1, Alert=2, Certain=3} | Value에서 파생 |
| LastRaisedAt | float | 마지막 상승 시각(감소 계산용) |

`Apply(int delta, float now) → StageChange? { Old, New }` — 클램프 후 단계가 바뀌면 반환. delta > 0이면 LastRaisedAt 갱신.

### IslandAlert — §2.4, §2.5, §8.1

| 필드 | 타입 | 규칙 |
|---|---|---|
| Value | int | **0 ≤ Value ≤ 100**. 씬에 하나. |
| Zone | enum AlertZone {Calm, Watch, Tension, Lockdown} | 0~25 / 26~50 / 51~75 / 76~100 |
| DepartureBlocked | bool | Zone == Lockdown |
| fractional | float | 감소용 소수 누적(내부) |

`Apply(int delta) → ZoneChange? { Old, New }`, `ApplyDecay(float amount) → ZoneChange?`(소수 누적, 정수 내림).
`static AlertZone ZoneOf(int value)`.

### SuspicionRules (JSON 루트) — [contracts/suspicion-rules-schema.json](contracts/suspicion-rules-schema.json)

| 필드 | 타입 | 규칙 |
|---|---|---|
| version | string | 필수 |
| events | SuspicionEventRule[] | 1개 이상, id 유일 |
| decay | DecayRule | 필수 |
| propagation | PropagationRule | 필수 |
| timeOfDay | TimeOfDayRule | 필수 |
| carryOver | CarryOverRule | 필수 |
| barks | string[] | 2단계 "말을 건다" 문구, 1개 이상 |

**SuspicionEventRule** — §2.3(`확정대기`), §4.4·§5.2(`확정`, 후속 추가)

| 필드 | 타입 | 규칙 |
|---|---|---|
| id | string | 필수, 유일, snake_case |
| personalDelta | int | ≥ 0 (음수는 Warning + 0 클램프) |
| islandDelta | int | ≥ 0 (음수는 Warning + 0 클램프) |
| scope | string | "witness" \| "radius" \| "target" |
| radius | float | scope=radius면 > 0 |
| requiresSight | bool | |
| nightOnly | bool | |
| status | string | "확정" \| "확정대기" 필수 |
| note | string | SDD 조항 |

**DecayRule** — §2.7: `personalIntervalSeconds`(> 0), `islandPerMinute`(≥ 0), `status`.
**PropagationRule** — §2.6: `distance`(> 0), `delaySeconds`(≥ 0), `status`. 전파 +1은 events의 `propagation` 항목이 유일한 출처.
**TimeOfDayRule** — §7: `dayMultiplier`(> 0), `nightMultiplier`(> 0), `nightWanderEventId`(events에 존재), `nightWanderIntervalSeconds`(> 0), `status`.
**CarryOverRule** — §2.7: `personalFactor`(0~1), `islandFactor`(0~1), `islandMin`(0~100), `status`.

검증(`SuspicionRulesValidator.Validate → ValidationResult`): 위 규칙 위반은 Error(로드 실패, 시스템 비활성 + HUD 경고).
`status == "확정대기"` 항목 수는 Info로 집계해 HUD에 "확정 대기 N건" 표시.

### PropagationScheduler — §2.6

| 구조 | 내용 |
|---|---|
| Meeting | (npcA, npcB) 정렬된 쌍, Active bool, 이미 예약한 (from, to, player) 집합 |
| Reservation | From, To, Player, ExecuteAt |

`UpdateMeetings(pairsInRange, now)`, `TryReserve(from, to, player, executeAt) → bool`(같은 만남 동안 1회),
`Drain(now) → IEnumerable<Reservation>`. 만남 종료 시 예약 집합 초기화(다음 만남에 재전파 가능).

### DecayCalculator — §2.7

`PersonalSteps(lastRaisedAt, lastDecayAt, now, interval) → int`(감소할 단계 수), `IslandAmount(deltaSeconds, perMinute) → float`.

### CarryOverCalculator — §2.7

`Compute(personalValues, islandValue, rule) → CarryOverSnapshot { Dictionary<(npcId, playerId), int> Personal; int Island }`.
Island = max(islandMin, round(value × islandFactor)), Personal = floor(value × personalFactor).

## 런타임 엔티티 (Runtime/)

### SuspicionSystem (MonoBehaviour, 씬 단일)

| 상태 | 타입 |
|---|---|
| Rules | SuspicionRules |
| Island | IslandAlert |
| personal | Dictionary<(NpcIdentity, PlayerEntity), PersonalSuspicion> |
| scheduler | PropagationScheduler |
| nightWanderCooldown | Dictionary<(NpcIdentity, PlayerEntity), float> |
| lastPersonalDecayAt | Dictionary<(NpcIdentity, PlayerEntity), float> |
| IsReady | bool (규칙 로드 성공) |

API: `Raise(SuspicionEvent)`, `GetPersonal(npc, player) → PersonalSuspicion`, `GetStage(npc, player)`, `EndRun() → CarryOverSnapshot`,
`DebugAdjustIsland(int)`, `AllPersonal` 열거.

Update 순서(FR 규칙): (1) 밤 이동 사건 생성 → (2) 큐에 쌓인 사건 적용(상승 먼저) → (3) 전파 만남 갱신·예약 실행 → (4) 감소.

### SuspicionEvent (struct)

`EventId: string`, `Actor: PlayerEntity`(필수), `Position: Vector3`, `TargetNpc: NpcIdentity`(scope=target일 때 필수).

### TimeOfDay (MonoBehaviour, 씬 단일) — §7

`Phase: enum DayPhase {Day, Night}`, `Set(DayPhase)`, `Toggle()`, `event OnChanged(DayPhase)`.

### PlayerDisguise (MonoBehaviour, PlayerEntity당 0..1) — §7

`IsDisguised: bool`(외부 입력), `IsEffective(TimeOfDay) → IsDisguised && Phase == Day`. 이 기능에서 효과량은 없다(확정 대기);
값 노출만.

### NpcSuspicionProfile (MonoBehaviour, NPC당 0..1) — §2.6 플래그

| 필드 | 기본 | 설정 주체 |
|---|---|---|
| PropagatesSuspicion | true | 003 침묵자 → false |
| ReportsToManagerImmediately | false | 003 밀고자 → true |
| IsManager | false | 003 관리자 역할 |
| SuppressStageBehaviour | false | 003 밀고자 → true |

없는 NPC는 전부 기본값("일반")으로 취급.

### NpcMover (MonoBehaviour)

`MoveTo(Vector3)`, `Follow(Transform, float keepDistance)`, `FaceTowards(Vector3)`, `Stop()`, `IsMoving`. 속도 `speed`(1.6), 회전 속도.

### NpcSuspicionBehaviour (MonoBehaviour, NPC당 1)

| 상태 | 설명 |
|---|---|
| focus | 현재 최고 단계 플레이어 |
| lastSeenPos | 1단계 돌아보기용 |
| reported | HashSet<PlayerEntity> — 3단계 사건 1회 보장 |

## 이벤트 (SuspicionEvents)

- `OnPersonalStageChanged(NpcIdentity, PlayerEntity, SuspicionStage old, SuspicionStage new)` — FR-008
- `OnReportAttempt(NpcIdentity, PlayerEntity)` — FR-007 (005 구독)
- `OnIslandChanged(int old, int new)`
- `OnIslandZoneChanged(AlertZone old, AlertZone new)` — FR-014
- `OnDepartureBlockedChanged(bool)` — FR-015
- `OnPropagated(NpcIdentity from, NpcIdentity to, PlayerEntity)` — FR-017
- `OnRunEnded(CarryOverSnapshot)` — FR-022
- `OnEventApplied(SuspicionEvent, int appliedNpcCount)` — 디버그

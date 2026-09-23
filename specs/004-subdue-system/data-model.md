# Data Model: 제압 시스템

## 열거형
- `SubdueAction { Backstab(뒤에서 제압), Shove(정면 밀치기), ObjectStrike(물건으로 기절) }` — §4.1
- `HandState { Empty, OneHand, TwoHands }` — §4.2
- `HeldKind { None, Object, UnconsciousNpc }`
- `NoiseLevel { Low, Medium, High }` — §4.1 소음

## SubdueRules (JSON) — contracts/subdue-rules-schema.json
| 블록 | 필드 | 규칙 | 상태 |
|---|---|---|---|
| actions.backstab | range>0, backConeDegrees(0<x≤180), noiseLevel, noiseEventId | 성공률 100%, 대상이 나를 보고 있지 않아야 함 | 확정 |
| actions.shove | range>0, failChance(0~1), noiseLevel, noiseEventId | 실패 시 대상 개인 의심 3 (가정, clarify) | failChance 확정대기 |
| actions.objectStrike | range>0, noiseLevel, noiseEventId | 물건 필요, 물건 소모 없음 | 확정 |
| unconscious | minSeconds(>0), maxSeconds(≥min) | 90~180 | 확정 |
| carry | speedMultiplier(0<x<1) | 달리기 불가 | 확정대기 |
| costs | witnessedEventId, foundIslandEventId, foundPersonalEventId, wakeEventId, shoveFailEventId | 002 events에 존재 | 확정 |

## 002 사건 추가 (suspicion_rules.json)
| id | personal | island | scope | 상태 | 근거 |
|---|---|---|---|---|---|
| subdue_witnessed | 0 | 40 | global | 확정 | §4.4 제압 장면 목격 |
| unconscious_found_island | 0 | 30 | global | 확정 | §4.4 기절자 발견 |
| unconscious_found | 3 | 0 | target | 확정대기 | §2.3 (기존) |
| unconscious_wake | 3 | 0 | target, 배율 무시 | 확정 | §4.3/§4.4 깨어남 |
| shove_failed | 3 | 0 | target, 배율 무시 | 확정대기 | 가정: 제압 시도당한 NPC는 확신 |
| noise_low / noise_medium / noise_high | 1 | 0 | radius 4 / 8 / 14 | 확정대기 | §2.3 소음, §4.1 등급 |

## 순수 로직
- `HandRules.CanPerform(SubdueAction, HandState, HeldKind) → (bool ok, string reason)`: Backstab·Shove는 Empty만; ObjectStrike는 Held==Object만; 기절자를 든 상태에서는 모두 불가.
- `HandRules.CanPickUpObject(HandState)`: Empty 또는 OneHand(두 번째 한 손 물건은 미지원 → Empty만, 단순화) / `CanPickUpBody(HandState)`: Empty만.
- `BackConeCheck.IsBehind(targetPos, targetForward, actorPos, coneDeg)`: 수평 각도(−forward 기준) ≤ cone/2.
- `ShoveRoll.Succeeds(failChance, roll01)`: roll ≥ failChance.
- `UnconsciousTimer(min,max, roll01)`: Duration, Remaining, `Tick(dt)`, `IsAwake`.
- `CostLedger`: `RecordWitnesses(bodyId, observerIds) → bool firstTime`, `TryRecordDiscovery(bodyId, observerId) → bool`(목격자면 false, 이미 발견이면 false).

## 런타임
- `PlayerHands`(PlayerEntity당 1): `State`, `Held`, `HeldKind`, `TryPickUp(CarriableObject)`, `TryPickUpBody(UnconsciousState)`, `Drop()`, `HandsBusy` 동기화, 모터 배율.
- `CarriableObject`(IInteractable): `IsTwoHanded`(bool), `PromptText="들기"`, 무기 속성 없음.
- `UnconsciousState`(NPC): `SubduedBy`, `Timer`, `CarriedBy`, `WitnessIds`, 비활성화한 컴포넌트 목록, `Wake()`.
- `UnconsciousCarryInteractable`(기절 시 동적 부착): `PromptText="들어 옮기기"`.
- `SubdueActor`(PlayerEntity당 1): `CurrentTarget`, `PlannedAction`, F 입력 → `SubdueSystem.TrySubdue`.
- `SubdueSystem`: `TrySubdue(actor, npc, action) → SubdueResult { Success, Action, Reason }`, 기절 틱, 발견 스캔, `Ledger`.

## 이벤트 (SubdueEvents)
- `OnSubdued(PlayerEntity actor, NpcIdentity npc, SubdueAction, int witnessCount)`
- `OnSubdueRejected(PlayerEntity, NpcIdentity, SubdueAction, string reason)`
- `OnShoveFailed(PlayerEntity, NpcIdentity)`
- `OnUnconsciousWake(NpcIdentity, PlayerEntity subduer)`
- `OnBodyDiscovered(NpcIdentity observer, NpcIdentity body, PlayerEntity subduer)`
- `OnPickedUp(PlayerEntity, HeldKind)`, `OnDropped(PlayerEntity, HeldKind)`
- `OnNoise(NoiseLevel, Vector3, PlayerEntity)`

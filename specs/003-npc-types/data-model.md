# Data Model: NPC 유형 5종

## 열거형 (Model/NpcTypeDefinitions.cs)
- `NpcType { Watcher(감시자), Informer(밀고자), Sympathizer(동조자), Silent(침묵자), Sentinel(경계자) }` — §3 표
- `Faction { Antagonist, Victim, Neutral }` — §3.2 (v0.4 §3.3 발췌)
- 역할 상수: `Roles.Manager="manager"`, `Roles.Broker="broker"`(중개인)

## NpcTypeRules (JSON 루트) — contracts/npc-types-schema.json
| 필드 | 타입 | 규칙 |
|---|---|---|
| version | string | 필수 |
| rules.watcher | { investigateEventId, status } | events에 존재해야 함(002 규칙 파일 기준, 로드 시 교차 검증) |
| rules.informer | { deliveryDelaySeconds>0, deliveryEventId, arriveDistance>0, status } | |
| rules.sympathizer | { turnZone ∈ {Watch,Tension,Lockdown}, status } | §3 "51+" → Tension |
| rules.sentinel | { detectRadius>0, soundRadius>0, cooldownSeconds≥0, noiseEventId, status } | |
| assignments[] | { npcId(필수, 유일), type(enum 문자열), faction(선택), roles[](선택) } | faction 누락 시 유형 기본값 |

기본 진영: Watcher→Antagonist, Informer/Sympathizer/Silent→Victim, Sentinel→Neutral. 상충 시 진영 우선 + Warning.

## NpcTypeProfile (MonoBehaviour, NPC당 1)
| 필드 | 타입 | 설명 |
|---|---|---|
| Type | NpcType | |
| Faction | Faction | 제압 판정의 유일 근거 |
| Roles | string[] | manager/broker 등 |
| IsTurned | bool | 동조자 전환 상태(다른 유형은 항상 false) |
| WatcherRuleApplies | bool(파생) | Type==Watcher \|\| (Type==Sympathizer && IsTurned) |
| IsManager | bool(파생) | Roles ∋ manager |

## 순수 로직
- `SubdueJudgement.Evaluate(Faction) → SubdueVerdict { bool CanSubdue; bool PhysicalReaction; string Reason }` — Antagonist: (true,true,"Antagonist"), 그 외: (false,false,"Victim/Neutral 물리 반응 없음")
- `SympathizerTurnRule.IsTurned(AlertZone zone, AlertZone turnZone) => zone >= turnZone`
- `InformerDeliveryState` — 상태 `Idle, Waiting, Moving, Held, Delivered`; `Trigger(playerId, now)`(Idle→Waiting), `Tick(now, delay, managerAvailable) → Transition?`(Waiting→Moving/Held, Held→Moving), `Arrive() → Delivered`, `Reset()`(값<2). 플레이어별 인스턴스.
- `SentinelAlarmGate(cooldown)`: `TryAlarm(bool playerInRadius, float now) → bool`

## 002 가산 확장
- `SuspicionEventRule.ignoresTimeMultiplier: bool`(기본 false) — `ScaledPersonalDelta` 우회
- `NpcSuspicionProfile.SuppressReportAttempt`, `.IgnoresSuspicionEvents`(기본 false)
- `suspicion_rules.json` events: `watcher_investigate`(personal 2, target, requiresSight true, ignoresTimeMultiplier true, 확정, "§3 감시자"), `informer_delivery`(personal 1, target, 확정, "§3.1/§2.6 밀고자 전달"); `investigate_in_sight.scope: target`

## 이벤트 (NpcTypeEvents)
- `OnInvestigate(PlayerEntity actor, int notifiedNpcCount)`
- `OnSympathizerTurned(NpcIdentity, bool turned)`
- `OnInformerDeliveryStarted(NpcIdentity informer, NpcIdentity manager, PlayerEntity)`
- `OnInformerDeliveryCompleted(NpcIdentity informer, NpcIdentity manager, PlayerEntity)`
- `OnInformerHeld(NpcIdentity informer, PlayerEntity)`
- `OnSentinelAlarm(NpcIdentity sentinel, Vector3 position, int turnedNpcCount)`

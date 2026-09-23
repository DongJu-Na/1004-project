# Data Model: 인카운터 프레임워크

## 열거형
- `EncounterType { A(기회), B(위협), C(도덕적 선택), D(세계 채색) }` — §6.2
- `InstancePhase { Spawning, Dialogue, Choice, Outcome, Ended, Interrupted }`
- `TriggerKind { Zone }`(이 스펙은 zone만; 시간·확률 트리거는 데이터 확장 여지)

## EncounterRules (JSON) — contracts/encounters-schema.json
| 블록 | 필드 | 규칙 |
|---|---|---|
| spawn | perRunMin ≥ 2(미만이면 경고 후 2), perRunMax ≥ perRunMin, minIntervalSeconds ≥ 0, footChance 0~1, vehicleChance 0~1(≥ footChance 권장 → 아니면 Warning), despawnDelaySeconds ≥ 0, choiceTimeoutSeconds > 0, typeWeights {A,B,C,D ≥ 0, 합 > 0}, status | §6.4·§6.3(확정대기) |
| pools[] | poolId 유일, islandId("common" 또는 섬 id), encounterIds[](존재) | §6.4 |
| encounters[] | id 유일 snake_case, type, sample(bool), trigger{kind, position?, radius}, npcs[1~2]{displayName, offset xyz, facing xyz}, lines[3~5](벗어나면 Warning, 0줄 Error), outcome(유형별 계약), conditions{timeOfDay any/day/night, minZone Calm..Lockdown, requiresFlag?, forbidsFlag?}, variantOf? , choiceAfterLine?(C, 0..lines-1) | §6.1·§6.2 |
| callbacks[] | flag, targetKind encounter_variant/document/npc_line, targetId(variant면 encounters에 존재), description, sample | §6.5 |

유형별 outcome 계약(검증기):
- A: `flags` ≥ 1, `eventId` 없음.
- B: `condition ∈ {in_sight, always}`, `eventId`(002 존재), `flags` 선택.
- C: `prompt`, `optionA{label, flags[], eventId?}`, `optionB{...}`, `defaultOption ∈ {A,B}`, `choiceAfterLine` 필수.
- D: `flags` 없음·`eventId` 없음.
- 풀 전체에 D가 하나도 없으면 Warning "D 유형 필수"(FR-009).

## 순수 로직
- `CandidateFilter.Filter(...) → List<EncounterDef>`
- `WeightedTypePicker.Pick(candidates, weights, rollType, rollIndex) → EncounterDef?`
- `SpawnGate.CanSpawn(SpawnContext{runCount, perRunMax, sinceLastEnd, minInterval, moving, busy, scouting, activeInstance}) → (bool ok, string reason)`; `Roll(inVehicle, footChance, vehicleChance, roll01) → bool`
- `ChoiceResolver(defaultOption, timeout)`: `Choose(option)`, `Tick(dt) → resolved?`, `Resolved`, `Option`
- `RunLedger`: `Flags`, `History`, `Record(id)`, `AddFlags(ids)`, `Import(IEnumerable<string> prior)`, `Export() → (flags[], history[])`, `CallbacksFor(flag, callbacks)`
- `EncounterRulesValidator.Validate(rules, eventExists) → result{IsError, Messages, PendingCount, SampleCount, CostSummary}`

## 런타임
- `EncounterSystem`: `IsReady`, `Rules`, `Ledger`, `RunCount`, `Active`(EncounterInstance), `CurrentIslandId`(직렬화, 기본 "island_test"), `TryTrigger(player, trigger)`, `ForceStart(id, player)`, `Simulate(runs) → SimReport`.
- `EncounterTrigger`: `Radius`, 플레이어별 진입 상태, 진입 시 `EncounterSystem.TryTrigger`.
- `EncounterInstance`: `Def`, `Player`, `SpawnedNpcs`, `Phase`, `Chosen`; `Interrupt()`.
- `PlayerActivity`: `IsScouting`(외부 입력).

## 이벤트 (EncounterEvents)
- `OnTriggerEvaluated(PlayerEntity, string triggerId, bool spawned, string reason)`
- `OnEncounterStarted(EncounterDef, PlayerEntity)`, `OnChoiceRequested(EncounterDef, PlayerEntity)`, `OnChoiceMade(EncounterDef, PlayerEntity, string option, bool byTimeout)`
- `OnEncounterEnded(EncounterDef, PlayerEntity, IReadOnlyList<string> flags)`, `OnEncounterInterrupted(EncounterDef, PlayerEntity)`
- `OnFlagRecorded(string flag)`, `OnRunSnapshot(string[] flags, string[] history)`

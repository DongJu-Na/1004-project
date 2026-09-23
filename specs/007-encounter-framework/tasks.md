---
description: "Task list for 007 인카운터 프레임워크 (Encounter Framework)"
---

# Tasks: 인카운터 프레임워크 (Encounter Framework)

**Input**: `/specs/007-encounter-framework/`. 001·002·006 필수, 004·003 선택(중단·유형). **Tests**: 포함. 네임스페이스 `Project1028.Encounter`, 어셈블리 `Encounter`. 수치는 `encounters.json`.
**Organization**: US1 최소 단위(P1) → US2 네 유형 계약(P1) → US3 스폰 규격(P2) → US4 회수(P2).

## Phase 1: Setup
- [X] T001 Create folders `Assets/Scripts/Encounter/{Model,Runtime,Debug}`, `Assets/Tests/EditMode/Encounter`, `Assets/StreamingAssets/Encounter`
- [X] T002 Create `Assets/Scripts/Encounter/Encounter.asmdef` — references `["PlayFoundation","Suspicion","NpcTypes","Subdue","Vehicle","Unity.InputSystem","UnityEngine.UI"]`
- [X] T003 [P] Create `Assets/Tests/EditMode/Encounter/Encounter.Tests.EditMode.asmdef` — references `["Encounter","Suspicion","PlayFoundation","UnityEngine.TestRunner","UnityEditor.TestRunner"]`, nunit, UNITY_INCLUDE_TESTS
- [X] T004 [P] Edit `Assets/Editor/PlayFoundation.Editor.asmdef` — `"Encounter"` 추가

## Phase 2: Foundational
- [X] T005 Extend 001 `Assets/Scripts/Dialogue/DialogueRunner.cs` — `public bool TryBeginInline(NpcIdentity speaker, string[] lines)`: 파일 로드 없이 `DialogueData{ npcId = speaker.NpcId, lines, destination = null }`로 시작(검증은 lines 0줄만 Error, 3~5줄 외 Warning); `End`에서 destination null이면 목적지 전달 생략; `public void Pause()`/`Resume()`(일시 정지 중 Interact 무시 — C 선택 대기용); 006 `Assets/Scripts/Vehicle/Runtime/VehicleDriverInput.cs`: 운전자/동승자가 `IsLocked`(대화 중)면 입력 0으로 두고 Interact·Jump를 무시(대사 진행 E가 하차로 처리되는 것 방지)
- [X] T006 [P] Edit 002 `Assets/StreamingAssets/Suspicion/suspicion_rules.json` — `encounter_stared_at`(personal 1, target, requiresSight true, 확정대기, "§6.2 B 주민이 쳐다본다 → 지나가면 +1"), `encounter_picked_up_runaway`(island 20, global, 확정대기, "§6.2 C 태운다 → 섬 의심도 급등") 추가
- [X] T007 [P] Write `Assets/Tests/EditMode/Encounter/EncounterRulesValidatorTests.cs` — 정상(샘플 8)→OK·SampleCount 8·CostSummary(npc 합·줄 합) 계산; id 중복→Error; type A에 eventId→Error; type D에 flags→Error; C에 choiceAfterLine 누락→Error; C optionA 누락→Error; lines 2줄→Warning; lines 0→Error; npcs 3→Error; perRunMin 1→Warning + 2로 보정; 풀에 D 없음→Warning; pool encounterId 미존재→Error; variantOf 미존재→Error; callback encounter_variant targetId 미존재→Error; typeWeights 합 0→Error; B eventId 없음(콜백 false)→Error
- [X] T008 [P] Implement `Assets/Scripts/Encounter/Model/EncounterDefinitions.cs` — `enum EncounterType {A,B,C,D}`, `InstancePhase {Spawning, Dialogue, Choice, Outcome, Ended, Interrupted}`, 한국어 이름
- [X] T009 [P] Implement `Assets/Scripts/Encounter/Model/EncounterRules.cs` — `[Serializable]` data-model 구조: `EncounterRules{version, SpawnRule spawn, PoolDef[] pools, EncounterDef[] encounters, CallbackEntry[] callbacks}`, `SpawnRule{perRunMin, perRunMax, minIntervalSeconds, footChance, vehicleChance, despawnDelaySeconds, choiceTimeoutSeconds, TypeWeights typeWeights, status}`, `TypeWeights{A,B,C,D}`, `PoolDef{poolId, islandId, encounterIds[]}`, `EncounterDef{id, type, sample, variantOf, choiceAfterLine=-1, TriggerDef trigger, NpcDef[] npcs, string[] lines, OutcomeDef outcome, ConditionDef conditions}`, `NpcDef{displayName, Vec3 offset, Vec3 facing}`, `Vec3{x,y,z}`, `OutcomeDef{flags[], condition, eventId, prompt, ChoiceOption optionA, optionB, defaultOption}`, `ChoiceOption{label, flags[], eventId}`, `ConditionDef{timeOfDay="any", minZone="Calm", requiresFlag, forbidsFlag}`, `CallbackEntry{flag, targetKind, targetId, description, sample}`; `EncounterDef Find(id)`, `EncounterType TypeOf(def)`
- [X] T010 Implement pure `Assets/Scripts/Encounter/Model/EncounterRulesValidator.cs` — data-model 규칙 전부; `Result{IsError, Messages, PendingCount, SampleCount, TotalNpcs, TotalLines}` (depends on T009)
- [X] T011 Implement `Assets/Scripts/Encounter/Model/EncounterRulesLoader.cs` — `TryLoad(SuspicionRules, out rules, out result)` 경로 `StreamingAssets/Encounter/encounters.json` (depends on T010)
- [X] T012 [P] Create `Assets/StreamingAssets/Encounter/encounters.json` — spawn{perRunMin 2, perRunMax 4, minIntervalSeconds 20, footChance 0.35, vehicleChance 0.7, despawnDelaySeconds 6, choiceTimeoutSeconds 12, typeWeights{30,25,20,25}, 확정대기}; pools: `island_test`(섬 전용: sample_a_truck, sample_b_checkpoint, sample_c_runaway, sample_d_kids), `common`(sample_a_worker, sample_b_stare, sample_c_asking, sample_d_funeral, sample_c_runaway_ledger); encounters 9개: A `sample_a_truck`(정비공 1, 4줄, flags["met_mechanic"]), A `sample_a_worker`(노동자 1, 3줄, flags["worker_testimony_hint"], conditions minZone Calm), B `sample_b_checkpoint`(경비 2, 4줄, condition in_sight, eventId encounter_stared_at, conditions timeOfDay any), B `sample_b_stare`(주민 1, 3줄, in_sight, encounter_stared_at, timeOfDay night), C `sample_c_runaway`(노동자 1, 4줄, choiceAfterLine 1, prompt "태울까?", optionA{"태운다", flags["picked_up_runaway"], eventId encounter_picked_up_runaway}, optionB{"안 태운다", flags["left_runaway"]}, defaultOption B), C `sample_c_asking`(주민 1, 3줄, choiceAfterLine 0, "저 사람 못 봤어?", A{"거짓말", flags["lied_to_resident"]}, B{"말해준다", flags["told_resident"]}, default B), D `sample_d_kids`(아이 2, 3줄), D `sample_d_funeral`(주민 2, 3줄), C-변주 `sample_c_runaway_ledger`(variantOf sample_c_runaway, 관리자 1, 3줄 "장부에 취소선…", conditions requiresFlag left_runaway, outcome flags["saw_ledger_strikethrough"]) — 전부 sample:true; callbacks: {left_runaway → encounter_variant sample_c_runaway_ledger, "안 태운 사람을 장부에서 취소선으로 발견 (§6.5)"}, {picked_up_runaway → npc_line "testimony_next_island", "다음 섬에서 증언"}, {met_mechanic → document "truck_repair_note", "정비공이 트럭을 고쳐준다"} 전부 sample:true
- [X] T013 [P] Implement `Assets/Scripts/Encounter/Runtime/EncounterEvents.cs` — data-model 이벤트 8개 + Raise*
- [X] T014 [P] Implement `Assets/Scripts/Encounter/Runtime/PlayerActivity.cs` — `RequireComponent(PlayerEntity)`, `bool IsScouting`
- [X] T015 Implement `Assets/Scripts/Encounter/Runtime/EncounterSystem.cs` 골격 — 싱글톤; Start(SuspicionSystem 준비 후 로드, HUD: 확정대기·샘플 수·단가 합계); `Ledger`, `RunCount`, `Active`, `CurrentIslandId="island_test"`, `LastEndedAt=-inf`; `ForceStart(id, player)`(US1); `TryTrigger(player, trigger)`(US3); `CallbacksFor(flag)`(US4); `SubdueEvents.OnSubdued` 구독 → 활성 인스턴스의 스폰 NPC면 `Interrupt` (depends on T011, T013)

## Phase 3: US1 이동 중 인카운터가 뜨고 대화하고 플래그가 남는다 (P1) 🎯
### Tests
- [X] T016 [P] [US1] Write `Assets/Tests/EditMode/Encounter/RunLedgerTests.cs` — Record 후 History 포함; AddFlags 중복 무시; Import(prior) 후 History 포함; Export 두 배열; CallbacksFor(flag)가 해당 항목만 반환
### Implementation
- [X] T017 [P] [US1] Implement pure `Assets/Scripts/Encounter/Model/RunLedger.cs` — `Flags`, `History`, `Record(id)`, `AddFlags(IEnumerable<string>)`, `Import(prior)`, `Export()`, `static List<CallbackEntry> CallbacksFor(flag, CallbackEntry[] table)`
- [X] T018 [US1] Implement `Assets/Scripts/Encounter/Runtime/EncounterNpcSpawner.cs` — `static List<NpcIdentity> Spawn(EncounterDef, Vector3 anchor, Quaternion facingBase)`: 각 NpcDef → Capsule(콜라이더 유지) + `NpcIdentity.Configure($"{def.id}_{i}", displayName, 0.6f)` + `NpcVision.Configure(100,10,0.15)` + `PrimitiveTint` + (NpcTypes 있으면) `NpcTypeProfile.Configure(Sympathizer, Victim, [])`·`NpcSuspicionProfile`; 등장 위치는 `Physics.CheckCapsule`로 막힘 검사 후 좌우 1m 대체, 모두 막히면 그 NPC 건너뛰기(Edge case); `static void Despawn(list, delaySeconds)`(Destroy(go, delay)) (depends on T009)
- [X] T019 [US1] Implement `Assets/Scripts/Encounter/Runtime/EncounterInstance.cs` — `Begin(def, player, anchor)`: 스폰 → Phase Dialogue → `player.GetComponent<DialogueRunner>().TryBeginInline(npcs[0], def.lines)`; `DialogueEvents.OnDialogueStarted`(choiceAfterLine == 0이면 즉시 Pause·선택 요청)와 `OnLineAdvanced` 구독: C이고 index == choiceAfterLine이면 `runner.Pause()` + Phase Choice + `OnChoiceRequested` + HUD 보조 안내 "{prompt}  1: {A.label} / 2: {B.label}"; `DialogueEvents.OnDialogueEnded`(같은 npc·player) → Phase Outcome → `ApplyOutcome()`(US2) → Phase Ended → `Ledger.Record(def.id)` + `OnEncounterEnded` + Despawn + system.NotifyEnded(this); `Interrupt()`: 대화 중이면 runner 강제 종료(001 `DialogueRunner.Abort()` 가산 — `Assets/Scripts/Dialogue/DialogueRunner.cs`에 추가: HideDialogue·Unlock·이벤트 없음), 플래그 미기록, `Ledger.Record` 이력만, `OnEncounterInterrupted`, Despawn (depends on T017, T018, T005, T015)
- [X] T020 [US1] Implement `ForceStart` in `Assets/Scripts/Encounter/Runtime/EncounterSystem.cs` — Active 있으면 false; def 없으면 HUD; anchor = player.Position + player.Forward*3 (또는 트리거 위치); `EncounterInstance` GameObject 생성·Begin; `RunCount++`; `Active` 설정; `OnEncounterStarted`; `NotifyEnded(instance)`: `LastEndedAt=Time.time`, Active=null (depends on T019)
- [X] T021 [US1] Implement `Assets/Scripts/Encounter/Debug/EncounterTestConsole.cs` — `X`: 샘플 정의 순환 `ForceStart(next, P1)`; `1`/`2`: Active가 Choice면 `Active.Choose("A"/"B")`(US2); `S`: P1 `PlayerActivity.IsScouting` 토글; `Z`: `EncounterSystem.Simulate(100)`(US3) (depends on T020)
- [X] T022 [US1] Implement `Assets/Editor/EncounterSceneBuilder.cs` — 001·002·003·004·006 시스템 오브젝트(HUD·TimeOfDay·Suspicion·NpcType·Subdue 시스템·패널·콘솔) + `EncounterSystem`·`EncounterDebugPanel`·`EncounterTestConsole`; 도로: Plane 띠(스케일 (1.5,1,6), 위치 (0,0.02,0), 어두운 틴트); 트리거 6개 `EncounterTrigger`(반경 3, 도로 z = -25,-15,-5,5,15,25, 시각용 얇은 Cylinder 콜라이더 없음); 차량 `VehicleSceneBuilder.CreateTruck((5,1,-28))`; P1(0,1,-30)+`PlayerActivity`·`PlayerHands`·`SubdueActor`, P2(12,1,12); 저장 `Assets/Scenes/Test_Encounter.unity`(EncounterTrigger는 US3 T028에서 구현되므로 그 후 컴파일) (depends on T021)

**Checkpoint**: X → NPC 등장·대사 → 종료 시 플래그·이력 기록.

## Phase 4: US2 네 유형은 결과 계약이 다르다 (P1)
### Tests
- [X] T023 [P] [US2] Write `Assets/Tests/EditMode/Encounter/ChoiceResolverTests.cs` — default B, timeout 12: Choose("A")→Resolved·Option A·byTimeout false; Tick 11.9→미해결; Tick 12→Resolved·Option B·byTimeout true; 해결 후 Choose 무시
### Implementation
- [X] T024 [P] [US2] Implement pure `Assets/Scripts/Encounter/Model/ChoiceResolver.cs` — `ChoiceResolver(defaultOption, timeout)`, `Choose(option)`, `bool Tick(dt)`(해결 순간 true), `Resolved`, `Option`, `ByTimeout`
- [X] T025 [US2] Implement `ApplyOutcome()`·`Choose()`·Choice tick in `Assets/Scripts/Encounter/Runtime/EncounterInstance.cs` — A: `Ledger.AddFlags(outcome.flags)`; B: condition in_sight → 스폰 NPC 중 `NpcVision.IsSeeing(player)`가 하나라도 참이면(always면 무조건) `SuspicionSystem.Raise(Target(eventId, player, npc))` + flags 선택; C: `ChoiceResolver` Update 틱(타임아웃 또는 플레이어 이탈 = 트리거 반경 밖·탑승 이동 → default), 선택 시 `runner.Resume()` + `OnChoiceMade`, Outcome에서 선택 옵션 flags/eventId(eventId는 `SuspicionEvent.At(id, player, pos)` global 또는 target 규칙에 따라 — `Rules`의 scope를 보고 At/Target 선택); D: 아무 것도 없음(검증기가 보장) (depends on T024, T019)

**Checkpoint**: 샘플 8개 계약 일치, C는 선택 강제, D는 무결과.

## Phase 5: US3 스폰 규격 (P2)
### Tests
- [X] T026 [P] [US3] Write `Assets/Tests/EditMode/Encounter/SpawnGateTests.cs` — 상한 도달→불가("상한"); 간격 미달→불가; 정지→불가("이동 중 아님"); 대화 중/잠금→불가("바쁨"); 정찰→불가; 활성 인스턴스→불가; 전부 통과→가능; `Roll(inVehicle true, 0.35, 0.7, roll 0.5)`→true, (false, …, 0.5)→false
- [X] T027 [P] [US3] Write `Assets/Tests/EditMode/Encounter/CandidateFilterTests.cs` + `WeightedTypePickerTests.cs` — 다른 섬 풀 제외; common 포함; night 전용은 낮에 제외; minZone Tension은 Calm에서 제외; requiresFlag 없으면 제외, forbidsFlag 있으면 제외; 이력에 있는 원본 제외; variantOf는 원본 이력 + 콜백 flag 보유 시에만 포함; 가중치 {A:1,B:0,C:0,D:0}→항상 A; 후보에 A 없으면 남은 유형으로 정규화; roll에 따른 결정성
### Implementation
- [X] T028 [P] [US3] Implement pure `Assets/Scripts/Encounter/Model/SpawnGate.cs` — `struct SpawnContext{int RunCount, PerRunMax; float SinceLastEnd, MinInterval; bool Moving, Busy, Scouting, ActiveInstance;}`; `static (bool ok, string reason) CanSpawn(ctx)`; `static bool Roll(bool inVehicle, float foot, float vehicle, float roll01)`
- [X] T029 [P] [US3] Implement pure `Assets/Scripts/Encounter/Model/CandidateFilter.cs` — `Filter(EncounterDef[] defs, PoolDef[] pools, string islandId, DayPhase phase, AlertZone zone, ISet<string> flags, ISet<string> history, CallbackEntry[] callbacks) → List<EncounterDef>` 규칙 R-4
- [X] T030 [P] [US3] Implement pure `Assets/Scripts/Encounter/Model/WeightedTypePicker.cs` — `Pick(List<EncounterDef>, TypeWeights, float rollType, float rollIndex) → EncounterDef`
- [X] T031 [US3] Implement `Assets/Scripts/Encounter/Runtime/EncounterTrigger.cs` — 직렬화 `radius`, `triggerId`; Update: 각 플레이어 거리 ≤ radius 진입(이전 프레임 밖 → 안) 시 `EncounterSystem.Instance.TryTrigger(player, this)`; 이탈 시 상태 리셋; Gizmo 원 (depends on T015)
- [X] T032 [US3] Implement `TryTrigger`·`Simulate` in `Assets/Scripts/Encounter/Runtime/EncounterSystem.cs` — moving = player.IsInVehicle ? `VehicleController.Instance.State.IsMoving` : `player.MovementState != Idle`; busy = IsLocked || DialogueRunner.IsActive || HandsBusy; scouting = PlayerActivity; `SpawnGate.CanSpawn` 실패 → `OnTriggerEvaluated(false, reason)`; `Roll` 실패 → (false, "확률"); 후보 = `CandidateFilter.Filter(...)` 비어 있으면 (false, "후보 없음"); `WeightedTypePicker.Pick` → 인스턴스 시작(anchor = trigger 위치); `Simulate(runs)`: 순수 로직만으로 런마다 트리거 12회(도보/차량 교대), 시간 30초 간격 가정, 이력은 런 간 누적하지 않음(원본 중복은 런 내) → `SimReport{ countsHistogram, intervalViolations, duplicateOriginals, conditionViolations, vehicleSpawns, footSpawns }` HUD·Debug.Log (depends on T028, T029, T030, T020)
- [X] T033 [US3] Add `EncounterTrigger` creation to `Assets/Editor/EncounterSceneBuilder.cs` (T022 자리) (depends on T031, T022)

**Checkpoint**: 트리거 진입 시 조건·확률에 따라 발생/스킵 로그, Z 리포트 정상.

## Phase 6: US4 회수 테이블 (P2)
- [X] T034 [US4] Implement `CallbacksFor(flag)` in `Assets/Scripts/Encounter/Runtime/EncounterSystem.cs` — `RunLedger.CallbacksFor(flag, Rules.callbacks)`; `EncounterEvents.OnFlagRecorded` 구독 시 HUD에 "회수: {description} → {targetKind}:{targetId}" 표시(반환만, 실행 없음); 변주 후보 포함은 T029 필터가 담당 (depends on T017, T029)
- [X] T035 [US4] Implement `Assets/Scripts/Encounter/Debug/EncounterDebugPanel.cs` — 좌하단 고정 텍스트: 확정대기·샘플 수·단가 합계(NPC {n}·대사 {m}줄), 발생 수/상한, 다음 가능(초), 현재 후보 수(P1 기준 필터 재계산 0.5초마다), 활성 인스턴스 phase, 플래그 목록, 이력 목록, 최근 회수 조회 3줄, 로그 6줄, 키 안내 (depends on T032, T034)
- [X] T036 [US4] Add `Snapshot()`/`OnRunSnapshot` in `Assets/Scripts/Encounter/Runtime/EncounterSystem.cs` — 002 `SuspicionEvents.OnRunEnded` 구독 시 `Ledger.Export()`를 `OnRunSnapshot`으로 발행 + HUD "인카운터 이월: 플래그 {n}, 이력 {m} (저장은 후속)" (depends on T017, T015)

## Phase 7: Polish
- [X] T037 [P] Update `Assets/Scripts/README.md` — `Encounter/` 행, 콘텐츠 추가 절차(JSON만), 회수 조회 API, 입력 키
- [X] T038 [P] Update `specs/002-suspicion-system/contracts/runtime-api.md` — 007 사건 2개 추기; `specs/001-play-foundation/contracts/runtime-api.md` §10에 `TryBeginInline/Pause/Resume/Abort` 추기
- [ ] T039 (에디터 수동) Run `specs/007-encounter-framework/quickstart.md` + 전체 회귀
- [X] T040 헌장 최종 점검 — Encounter 코드 확률·비율·상한 리터럴 0(JSON), 샘플 전부 sample:true, 에셋 0, 단가 집계 표시 → `specs/007-encounter-framework/checklists/requirements.md` Notes; `specs/README.md`를 001~007 implement 완료 상태로 갱신

## Dependencies
Phase 1 → 2 → US1 → US2 → US3 → US4 → Polish. `EncounterSystem.cs`(T015→T020→T032→T034→T036), `EncounterInstance.cs`(T019→T025), `EncounterSceneBuilder.cs`(T022→T033) 순차. 씬 빌더 컴파일은 T031 이후.

## Parallel
Phase 2: T006 ∥ T007 ∥ T008 ∥ T009 ∥ T012 ∥ T013 ∥ T014 · US3: T026 ∥ T027 → T028 ∥ T029 ∥ T030

## Implementation Strategy
MVP = US1 + US2(단위·계약). US3(스폰 규격)로 체감 품질, US4(회수)로 v0.9 준비.

## Notes
- 샘플 9개는 콘텐츠가 아니다. v0.9 목록은 JSON에 sample:false로 추가하며 코드 변경 없음.
- C 선택 키 1/2와 X/S/Z는 테스트 콘솔 매핑. 정식 UI·입력은 후속.

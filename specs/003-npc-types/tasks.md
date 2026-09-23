---
description: "Task list for 003 NPC 유형 5종 (NPC Types)"
---

# Tasks: NPC 유형 5종 (NPC Types)

**Input**: `/specs/003-npc-types/` (plan, spec, research, data-model, contracts, quickstart). 001·002 코드 필요.
**Tests**: 포함(원칙 III). 순수 로직 테스트를 구현 앞에 둔다.
**Organization**: US1 감시자(P1), US2 밀고자(P1), US6 진영·제압 판정(P2, 004 전제라 US3 앞에 배치), US3 동조자(P2), US4 침묵자(P2), US5 경계자(P3).

네임스페이스 `Project1028.NpcTypes`, 어셈블리 `NpcTypes`. 수치·배정은 `npc_types.json`. 002 수정은 가산적 3건만.

---

## Phase 1: Setup

- [X] T001 Create folders `Assets/Scripts/NpcTypes/{Model,Runtime,Debug}`, `Assets/Tests/EditMode/NpcTypes`, `Assets/StreamingAssets/NpcTypes`
- [X] T002 Create `Assets/Scripts/NpcTypes/NpcTypes.asmdef` — references `["PlayFoundation","Suspicion","Unity.InputSystem","UnityEngine.UI"]`
- [X] T003 [P] Create `Assets/Tests/EditMode/NpcTypes/NpcTypes.Tests.EditMode.asmdef` — references `["NpcTypes","Suspicion","PlayFoundation","UnityEngine.TestRunner","UnityEditor.TestRunner"]`, overrideReferences, nunit, UNITY_INCLUDE_TESTS, Editor
- [X] T004 [P] Edit `Assets/Editor/PlayFoundation.Editor.asmdef` — references에 `"NpcTypes"` 추가

---

## Phase 2: Foundational (002 가산 확장 + 데이터 + 시스템 골격)

- [X] T005 Extend 002 `Assets/Scripts/Suspicion/Model/SuspicionRules.cs` — `SuspicionEventRule`에 `public bool ignoresTimeMultiplier;`(기본 false) 추가
- [X] T006 Extend 002 `Assets/Scripts/Suspicion/Runtime/NpcSuspicionProfile.cs` — 직렬화 필드·프로퍼티 `SuppressReportAttempt`(false), `IgnoresSuspicionEvents`(false) 추가; `Snapshot`에 두 값 포함, `Default` 갱신
- [X] T007 Extend 002 `Assets/Scripts/Suspicion/Runtime/SuspicionSystem.cs` — `ApplyQueuedEvents`: 대상 NPC의 프로필 `IgnoresSuspicionEvents`면 스킵; `New==Certain`일 때 프로필 `SuppressReportAttempt`면 `RaiseReportAttempt` 생략(단계 전환 이벤트는 발행); `ScaledPersonalDelta(rule)`: `rule.ignoresTimeMultiplier`면 배율 없이 원값; **또한** 002 `Assets/Scripts/Suspicion/Runtime/NpcSuspicionBehaviour.cs`: `SuppressStageBehaviour`면 `mover.Stop()`을 매 프레임 호출하지 말고(003 `InformerBehaviour`의 이동을 취소하므로) 억제 상태 진입 시 1회만 Stop 후 return (depends on T005, T006)
- [X] T008 [P] Edit 002 data `Assets/StreamingAssets/Suspicion/suspicion_rules.json` — `investigate_in_sight.scope`→`"target"`; events 추가: `watcher_investigate`(personalDelta 2, islandDelta 0, scope target, requiresSight true, nightOnly false, ignoresTimeMultiplier true, status 확정, note "§3 감시자: 시야 안 조사 시 즉시 +2"), `informer_delivery`(personalDelta 1, target, requiresSight false, status 확정, note "§3.1/§2.6 밀고자 → 관리자 전달"); 기존 항목엔 `ignoresTimeMultiplier: false` 명시
- [X] T009 [P] Write `Assets/Tests/EditMode/NpcTypes/NpcTypeRulesValidatorTests.cs` — 정상→Error 없음; rules 블록 누락→Error; deliveryDelaySeconds 0→Error; turnZone "Calm"→Error(Watch/Tension/Lockdown만); assignments npcId 중복→Error; type 오타→Error; faction 누락→기본 진영 채움(Warning 아님); Watcher+Victim 상충→Warning + faction 유지; broker 역할이 Antagonist 아님→Warning; status 누락→Error
- [X] T010 [P] Implement `Assets/Scripts/NpcTypes/Model/NpcTypeDefinitions.cs` — `enum NpcType {Watcher, Informer, Sympathizer, Silent, Sentinel}`, `enum Faction {Antagonist, Victim, Neutral}`, `static class Roles { Manager="manager"; Broker="broker"; }`, `static Faction DefaultFactionOf(NpcType)`(Watcher→Antagonist, Informer/Sympathizer/Silent→Victim, Sentinel→Neutral), `static bool TryParseType/TryParseFaction/TryParseZone`
- [X] T011 [P] Implement `Assets/Scripts/NpcTypes/Model/NpcTypeRules.cs` — `[Serializable] NpcTypeRules { string version; TypeRuleSet rules; NpcAssignment[] assignments; }`, `TypeRuleSet { WatcherRule watcher; InformerRule informer; SympathizerRule sympathizer; SentinelRule sentinel; }`, `WatcherRule { string investigateEventId; string status; }`, `InformerRule { float deliveryDelaySeconds; string deliveryEventId; float arriveDistance; string status; }`, `SympathizerRule { string turnZone; string status; }`, `SentinelRule { float detectRadius; float soundRadius; float cooldownSeconds; string noiseEventId; string status; }`, `NpcAssignment { string npcId; string type; string faction; string[] roles; }`; `NpcAssignment Find(string npcId)`
- [X] T012 Implement pure `Assets/Scripts/NpcTypes/Model/NpcTypeRulesValidator.cs` — `Validate(NpcTypeRules, Func<string,bool> suspicionEventExists) → TypeRulesValidationResult { IsError, Messages, PendingCount }`; data-model 규칙; 사건 id 존재는 콜백으로 확인(테스트에서는 항상 true 람다); faction 누락 시 `DefaultFactionOf`로 채움; 상충·broker 경고 (depends on T010, T011)
- [X] T013 Implement `Assets/Scripts/NpcTypes/Model/NpcTypeRulesLoader.cs` — `TryLoad(SuspicionRules suspicionRules, out NpcTypeRules, out result)`; 경로 `StreamingAssets/NpcTypes/npc_types.json`; 사건 존재 콜백 = `suspicionRules.FindEvent(id) != null` (depends on T012)
- [X] T014 [P] Create `Assets/StreamingAssets/NpcTypes/npc_types.json` — version "0.8-draft"; rules: watcher {investigateEventId "watcher_investigate", 확정}, informer {deliveryDelaySeconds 8, deliveryEventId "informer_delivery", arriveDistance 1.5, 확정대기}, sympathizer {turnZone "Tension", 확정}, sentinel {detectRadius 5, soundRadius 10, cooldownSeconds 5, noiseEventId "noise", 확정대기}; assignments: `type_watcher`(Watcher, roles []), `type_informer`(Informer), `type_sympathizer`(Sympathizer), `type_silent`(Silent), `type_sentinel`(Sentinel), `type_manager`(Watcher, roles ["manager"])
- [X] T015 [P] Implement `Assets/Scripts/NpcTypes/Runtime/NpcTypeEvents.cs` — contracts §이벤트 6개 + internal Raise*
- [X] T016 [P] Implement `Assets/Scripts/NpcTypes/Runtime/NpcTypeProfile.cs` — 직렬화 `type, faction, roles[]`, 런타임 `IsTurned`(setter internal); 파생 `WatcherRuleApplies`, `IsManager`; `Configure(NpcType, Faction, string[])`; `static NpcTypeProfile Get(NpcIdentity)`
- [X] T017 Implement `Assets/Scripts/NpcTypes/Runtime/NpcTypeSystem.cs` 골격 — 싱글톤; Start(SuspicionSystem 준비 후)에서 `NpcTypeRulesLoader.TryLoad(SuspicionSystem.Instance.Rules …)`; 실패 시 IsReady=false + HUD 경고; 성공 시 `NpcIdentity.All` 순회: 배정 있으면 `NpcTypeProfile` 부여·Configure, 없으면 Sympathizer 기본 + Warning "미배정 NPC {id} → 동조자"; 유형별 002 플래그 설정 `ApplyProfileFlags(npc, type)`: Watcher(기본), Informer(`PropagatesSuspicion=false, ReportsToManagerImmediately=true, SuppressStageBehaviour=true`), Sympathizer(기본), Silent(`PropagatesSuspicion=false, SuppressReportAttempt=true`), Sentinel(`IgnoresSuspicionEvents=true, PropagatesSuspicion=false, SuppressReportAttempt=true`); 컴포넌트 부착: Informer→`InformerBehaviour`, Sentinel→`SentinelBehaviour`; `LabelsVisible=true`; HUD "유형 규칙 확정 대기 {n}건" (depends on T013, T015, T016; Informer/SentinelBehaviour는 US2/US5에서 구현되므로 그 전엔 부착 코드만)

---

## Phase 3: User Story 1 - 감시자 (Priority: P1) 🎯 MVP

- [X] T018 [US1] Implement `Investigate(PlayerEntity actor)` in `Assets/Scripts/NpcTypes/Runtime/NpcTypeSystem.cs` — `NpcIdentity.All` 중 `NpcVision.IsSeeing(actor)`인 NPC마다: 프로필 `WatcherRuleApplies`면 `Raise(SuspicionEvent.Target(rules.watcher.investigateEventId, actor, npc))`, 아니면 `Raise(SuspicionEvent.Target("investigate_in_sight", actor, npc))`(002 일반 규칙); 통지 수 반환; `NpcTypeEvents.RaiseInvestigate(actor, count)` (depends on T017, T007, T008)
- [X] T019 [P] [US1] Implement `Assets/Scripts/NpcTypes/Debug/NpcTypeTestConsole.cs` — `Keyboard.current`: `I`→`NpcTypeSystem.Instance.Investigate(P1)` + HUD 로그 "조사: {n} NPC 통지", `L`→`LabelsVisible` 토글
- [X] T020 [US1] Implement `Assets/Editor/NpcTypesSceneBuilder.cs` — 001 빌더 재사용(바닥·벽·빛·HUD·P1·P2); 002 `TimeOfDay`·`SuspicionSystem`·`SuspicionDebugPanel`·`SuspicionTestConsole`; 003 `NpcTypeSystem`·`NpcTypeTestConsole`; NPC 6개(대화 없음, 고유 npcId): `NPC_W type_watcher`(0,1,6) 정면 -Z, `NPC_I type_informer`(-5,1,4), `NPC_S type_sympathizer`(5,1,4), `NPC_Q type_silent`(-9,1,8), `NPC_D type_sentinel`(9,1,-2), `NPC_M type_manager`(0,1,14); 각 NPC에 `NpcMover`·`NpcSuspicionBehaviour`·`NpcSuspicionProfile`; 001 빌더에 `CreateNpc(..., bool talkable)` 오버로드 추가(`talkable=false`면 `NpcTalkInteractable` 미부착) — `Assets/Editor/PlayFoundationSceneBuilder.cs` 수정; 저장 `Assets/Scenes/Test_NpcTypes.unity`; 001 `Assets/Scripts/UI/RuntimeHud.cs` `Start` 사전 대화 검증을 `NpcTalkInteractable`이 있는 NPC로 한정(대화 없는 NPC 경고 방지) (depends on T017, T019)

**Checkpoint**: W 정면에서 `I` → +2 즉시(밤에도), 시야 밖 무반응.

---

## Phase 4: User Story 2 - 밀고자 (Priority: P1)

### Tests
- [X] T021 [P] [US2] Write `Assets/Tests/EditMode/NpcTypes/InformerDeliveryStateTests.cs` — Idle에서 Trigger(t=0)→Waiting; Tick(t=7, delay 8, managerAvailable true)→null; Tick(t=8)→Moving; Waiting에서 Tick(t=8, managerAvailable false)→Held; Held에서 Tick(managerAvailable true)→Moving; Moving 중 Trigger 재호출→상태 불변; Arrive()→Delivered; Delivered에서 Trigger→불변; Reset()→Idle

### Implementation
- [X] T022 [P] [US2] Implement pure `Assets/Scripts/NpcTypes/Model/InformerDeliveryState.cs` — `enum DeliveryPhase {Idle, Waiting, Moving, Held, Delivered}`; `Phase`, `TriggeredAt`; `bool Trigger(float now)`(Idle→Waiting만 true), `DeliveryPhase? Tick(float now, float delay, bool managerAvailable)`, `void Arrive()`, `void Reset()`
- [X] T023 [US2] Implement `Assets/Scripts/NpcTypes/Runtime/InformerBehaviour.cs` — `RequireComponent(NpcIdentity, NpcMover)`; 플레이어별 `InformerDeliveryState`; Update: 각 플레이어 p — `SuspicionSystem.GetPersonal(self,p).Value ≥ 2`면 `Trigger(now)`, `< 2`면 `Reset()`; 활성 상태 1개(가장 먼저 Waiting된 플레이어) 처리: `Tick(now, rules.informer.deliveryDelaySeconds, 관리자 존재)`; Moving 진입 시 가장 가까운 관리자(`NpcTypeProfile.IsManager`, 자기 자신 제외) 선택 → `mover.MoveTo(manager.Position)` + `OnInformerDeliveryStarted`; Held 진입 시 `OnInformerHeld`; Moving 중 `Vector3.Distance ≤ arriveDistance`면 `Arrive()` + `SuspicionSystem.Raise(SuspicionEvent.Target(rules.informer.deliveryEventId, p, manager))` + `OnInformerDeliveryCompleted` + `mover.Stop()`; 관리자가 이동 중 사라지면 Held (depends on T022, T017)
- [X] T024 [US2] Wire `InformerBehaviour` attach in `Assets/Scripts/NpcTypes/Runtime/NpcTypeSystem.cs` `ApplyProfileFlags` (T017 자리 표시 채움) (depends on T023)

**Checkpoint**: 밀고자는 2에서도 무반응, 8초 후 관리자로 걸어가 도착 시 관리자 +1.

---

## Phase 5: User Story 6 - 진영과 제압 판정 (Priority: P2, 004 전제)

### Tests
- [X] T025 [P] [US6] Write `Assets/Tests/EditMode/NpcTypes/SubdueJudgementTests.cs` — Antagonist→CanSubdue true·PhysicalReaction true; Victim→false·false; Neutral→false·false; 유형 기본 진영 표: Watcher 가능, 나머지 4유형 불가; Reason 비어 있지 않음

### Implementation
- [X] T026 [P] [US6] Implement pure `Assets/Scripts/NpcTypes/Model/SubdueJudgement.cs` — `struct SubdueVerdict { bool CanSubdue; bool PhysicalReaction; string Reason; }`; `Evaluate(Faction)`; `Evaluate(NpcIdentity)`(프로필 없으면 Victim 취급 → 불가)
- [X] T027 [US6] Implement `Assets/Scripts/NpcTypes/Debug/NpcTypeLabel.cs` — NPC당 라벨(001 `RuntimeHud.CreateWorldLabel`, VisionDebugLabel보다 0.5 위): "{Type}/{Faction} 제압:{가능|불가}{ 전환}{ 관리자}{ 전달:상태}"; `NpcTypeSystem.LabelsVisible`이 false면 숨김; 밀고자 숨김 모드 검증(FR-011) (depends on T026, T016)
- [X] T028 [US6] Attach `NpcTypeLabel` to all NPCs in `Assets/Scripts/NpcTypes/Runtime/NpcTypeSystem.cs` 배정 루프 (depends on T027)

**Checkpoint**: 라벨에 유형·진영·제압 판정이 표와 일치.

---

## Phase 6: User Story 3 - 동조자 (Priority: P2)

### Tests
- [X] T029 [P] [US3] Write `Assets/Tests/EditMode/NpcTypes/SympathizerTurnRuleTests.cs` — turnZone Tension: Calm/Watch→false, Tension/Lockdown→true; turnZone Watch: Watch→true; turnZone Lockdown: Tension→false

### Implementation
- [X] T030 [P] [US3] Implement pure `Assets/Scripts/NpcTypes/Model/SympathizerTurnRule.cs` — `static bool IsTurned(AlertZone zone, AlertZone turnZone) => zone >= turnZone`
- [X] T031 [US3] Implement turn tracking in `Assets/Scripts/NpcTypes/Runtime/NpcTypeSystem.cs` — Start 시 1회 + `SuspicionEvents.OnIslandZoneChanged` 구독: 모든 Sympathizer 프로필 `IsTurned = SympathizerTurnRule.IsTurned(zone, parsedTurnZone)`; 변경 시 `OnSympathizerTurned`; 진영·002 플래그는 바꾸지 않음(제압 불가 유지) (depends on T030, T017)

**Checkpoint**: 섬 51+에서 S 조사 → +2 즉시, 50 이하에서 일반 규칙.

---

## Phase 7: User Story 4 - 침묵자 (Priority: P2)

- [X] T032 [US4] Verify silent flags in `NpcTypeSystem.ApplyProfileFlags` (T017): Silent → `PropagatesSuspicion=false`, `SuppressReportAttempt=true`; 002 `SuspicionSystem`이 `SuppressReportAttempt`를 존중하는지(T007) 재확인; 라벨에 "[무신고][무전파]" 표기 추가 `Assets/Scripts/NpcTypes/Debug/NpcTypeLabel.cs` (depends on T007, T027)

**Checkpoint**: Q를 3까지 올려도 신고 시도 로그 없음, 전파 없음.

---

## Phase 8: User Story 5 - 경계자 (Priority: P3)

### Tests
- [X] T033 [P] [US5] Write `Assets/Tests/EditMode/NpcTypes/SentinelAlarmGateTests.cs` — 반경 안 첫 호출→true; 쿨다운 내 재호출→false; 쿨다운 후→true; 반경 밖→false(쿨다운 갱신 없음)

### Implementation
- [X] T034 [P] [US5] Implement pure `Assets/Scripts/NpcTypes/Model/SentinelAlarmGate.cs` — `SentinelAlarmGate(float cooldown)`, `bool TryAlarm(bool playerInRadius, float now)`
- [X] T035 [US5] Implement `Assets/Scripts/NpcTypes/Runtime/SentinelBehaviour.cs` — `RequireComponent(NpcIdentity)`; Update: 가장 가까운 플레이어 거리 ≤ `rules.sentinel.detectRadius`면 `gate.TryAlarm(true, now)`; 알람 시: `NpcIdentity.All` 중 자기 제외, 거리 ≤ soundRadius인 NPC에 `NpcMover.FaceTowards(playerPos)`(없으면 transform.LookAt 수평) → count; `SuspicionSystem.Raise(SuspicionEvent.At(rules.sentinel.noiseEventId, player, self.Position))`; `OnSentinelAlarm(self, pos, count)`; HUD 로그 "경계자 소리" (depends on T034, T017)
- [X] T036 [US5] Wire `SentinelBehaviour` attach + flags in `Assets/Scripts/NpcTypes/Runtime/NpcTypeSystem.cs` `ApplyProfileFlags` (depends on T035)

**Checkpoint**: D 반경 진입 → 주변 NPC 회전, noise 사건, D 자신 의심 0.

---

## Phase 9: Polish

- [X] T037 [P] Update `Assets/Scripts/README.md` — `NpcTypes/` 행, 002 가산 확장 3건 기록, 조사 입력 `NpcTypeSystem.Investigate` 안내
- [X] T038 [P] Update 002 contracts `specs/002-suspicion-system/contracts/runtime-api.md` — 가산 확장(ignoresTimeMultiplier, 두 플래그, 사건 2개, scope 변경) 추기
- [ ] T039 (에디터 수동) Run `specs/003-npc-types/quickstart.md` §1~§4, 002 EditMode 회귀 포함, 결과 기록
- [X] T040 헌장 최종 점검 — NpcTypes 코드 숫자 리터럴 검토(허용: enum 비교, 라벨 오프셋), 유형 파라미터 하드코딩 0건, 에셋 0개 → `specs/003-npc-types/checklists/requirements.md` Notes 기록

---

## Dependencies
- Phase 1 → Phase 2 → US1 → US2 → US6 → US3 → US4 → US5 → Polish (US3~US5는 서로 병렬 가능, `NpcTypeSystem.cs`·`NpcTypeLabel.cs` 편집은 순차)
- 002 가산 확장(T005~T008)은 US1의 `watcher_investigate` 경로에 필수.

## Parallel
- Phase 2: T008 ∥ T009 ∥ T010 ∥ T011 ∥ T014 ∥ T015 ∥ T016
- US2: T021 ∥ T022 · US6: T025 ∥ T026 · US3: T029 ∥ T030 · US5: T033 ∥ T034

## Notes

- 스펙 FR-010은 밀고자 전달을 002 FR-020 플래그로 구현한다고 썼으나, 이중 +1을 막기 위해 003 `InformerBehaviour`가 이동·전달을 전담하고 밀고자의 002 전파는 끈다(research R-2). 겉으로 보이는 동작(지연 후 관리자 이동, 도착 시 관리자 +1)은 스펙과 동일하다.

## Implementation Strategy
- **MVP = US1 + US2**(감시자·밀고자). SDD가 핵심으로 지목한 밀고자까지가 첫 검증 대상.
- 이어 US6(004 전제) → US3·US4·US5.
- 수동: Test Runner, 씬 메뉴, Play 시나리오.

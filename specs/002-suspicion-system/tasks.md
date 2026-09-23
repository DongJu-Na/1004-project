---
description: "Task list for 002 의심 시스템과 시간대 위협 (Suspicion System)"
---

# Tasks: 의심 시스템과 시간대 위협 (Suspicion System)

**Input**: Design documents from `/specs/002-suspicion-system/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/runtime-api.md, contracts/suspicion-rules-schema.json, quickstart.md.
001 플레이 기반 코드(`PlayFoundation` 어셈블리)가 존재해야 한다.

**Tests**: 포함. 헌장 원칙 III·Tasks 게이트에 따라 순수 로직 EditMode 테스트를 구현 앞에 둔다.

**Organization**: 스펙 User Story 1~5(P1, P1, P2, P2, P2). US1·US2가 함께 MVP.

## Format: `[ID] [P?] [Story] Description`

- 모든 코드 네임스페이스 `Project1028.Suspicion`, 어셈블리 `Suspicion`. 001 타입은 `Project1028.PlayFoundation` 사용.
- 수치는 코드에 쓰지 않는다. JSON 파일에만 둔다 (FR-009). 코드 상수는 범위 0~3, 0~100, 구간 경계 25/50/75(§2.1·§2.4 확정)만 허용.

## Path Conventions

`Assets/Scripts/Suspicion/{Model,Runtime,Debug}/`, `Assets/Editor/`, `Assets/Tests/EditMode/Suspicion/`, `Assets/StreamingAssets/Suspicion/`.

---

## Phase 1: Setup

- [X] T001 Create folders `Assets/Scripts/Suspicion/{Model,Runtime,Debug}`, `Assets/Tests/EditMode/Suspicion`, `Assets/StreamingAssets/Suspicion`
- [X] T002 Create `Assets/Scripts/Suspicion/Suspicion.asmdef` — name `Suspicion`, rootNamespace `Project1028.Suspicion`, references `["PlayFoundation","Unity.InputSystem","UnityEngine.UI"]`, autoReferenced true
- [X] T003 [P] Create `Assets/Tests/EditMode/Suspicion/Suspicion.Tests.EditMode.asmdef` — references `["Suspicion","PlayFoundation","UnityEngine.TestRunner","UnityEditor.TestRunner"]`, `overrideReferences: true`, `precompiledReferences: ["nunit.framework.dll"]`, `defineConstraints: ["UNITY_INCLUDE_TESTS"]`, includePlatforms `["Editor"]`
- [X] T004 [P] Edit `Assets/Editor/PlayFoundation.Editor.asmdef` — references에 `"Suspicion"` 추가 (씬 빌더가 002 타입 참조)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 규칙 데이터·로더·이벤트 허브·시스템 골격. 모든 스토리가 의존한다.

- [X] T005 [P] Write `Assets/Tests/EditMode/Suspicion/SuspicionRulesValidatorTests.cs` — 케이스: 정상 파일→Error 없음·확정대기 카운트 = 7; events 비어 있음→Error; id 중복→Error; id에 대문자/공백→Error; personalDelta 음수→Warning(값 0으로 클램프됨을 메시지에 포함), Error 아님; scope=radius이고 radius ≤ 0→Error; scope 값 오타→Error; status 누락→Error; timeOfDay.nightWanderEventId가 events에 없음→Error; carryOver.personalFactor 1.5→Error; barks 비어 있음→Error; decay.personalIntervalSeconds 0→Error
- [X] T006 [P] Implement `Assets/Scripts/Suspicion/Model/SuspicionRules.cs` — `[Serializable]` 클래스: `SuspicionRules { string version; SuspicionEventRule[] events; DecayRule decay; PropagationRule propagation; TimeOfDayRule timeOfDay; CarryOverRule carryOver; string[] barks; }`, `SuspicionEventRule { string id; int personalDelta; int islandDelta; string scope; float radius; bool requiresSight; bool nightOnly; string status; string note; }`, `DecayRule { float personalIntervalSeconds; float islandPerMinute; string status; }`, `PropagationRule { float distance; float delaySeconds; string status; }`, `TimeOfDayRule { float dayMultiplier; float nightMultiplier; string nightWanderEventId; float nightWanderIntervalSeconds; string status; }`, `CarryOverRule { float personalFactor; float islandFactor; int islandMin; string status; }`; 상수 `StatusConfirmed="확정"`, `StatusPending="확정대기"`; `SuspicionEventRule FindEvent(string id)`
- [X] T007 Implement pure `Assets/Scripts/Suspicion/Model/SuspicionRulesValidator.cs` — `Validate(SuspicionRules) → RulesValidationResult { bool IsError; List<string> Messages; int PendingCount; }`; data-model.md 규칙 그대로: 필수 블록 null→Error, events ≥1, id 유일·`^[a-z][a-z0-9_]*$`, personal/islandDelta 음수는 Warning + 0으로 클램프(스펙 Edge case: 게임을 멈추지 않음), scope ∈ {witness,radius,target}, radius>0 when radius scope, status ∈ {확정,확정대기} 필수(모든 블록), decay.personalIntervalSeconds>0, islandPerMinute≥0, propagation.distance>0, delaySeconds≥0, events에 `propagation` id 존재(전파 +1의 유일한 출처), timeOfDay 배율>0, nightWanderEventId 존재, nightWanderIntervalSeconds>0, carryOver factor 0~1, islandMin 0~100, barks ≥1; PendingCount = status==확정대기 개수(events + 4블록) (depends on T006)
- [X] T008 Implement `Assets/Scripts/Suspicion/Model/SuspicionRulesLoader.cs` — `TryLoad(out SuspicionRules, out RulesValidationResult)`; 경로 `Application.streamingAssetsPath/Suspicion/suspicion_rules.json`; 파일 없음·파싱 예외→Error(경로 포함) (depends on T007)
- [X] T009 [P] Create data `Assets/StreamingAssets/Suspicion/suspicion_rules.json` — version "0.8-draft"; events 7개(§2.3 제안, 모두 `확정대기`): `investigate_in_sight`(personal 2, witness, requiresSight, note "§2.3 시야 안에서 조사"), `enter_restricted_area`(personal 2, witness, requiresSight), `evidence_seen_one_hand`(personal 1, witness, requiresSight), `evidence_seen_two_hands`(personal 2, witness, requiresSight), `night_wander`(personal 1, target, nightOnly true, requiresSight true, note "§7 밤 이동"), `sprint_nearby`(personal 1, radius 6, requiresSight false), `noise`(personal 1, radius 8, requiresSight false); + `unconscious_found`(personal 3, target, 확정대기, note "§2.3/§4.4 — 004가 발생") ; + `propagation`(personal 1, target, `확정`, note "§2.6 전파 — 시스템 내부 발생"); decay {personalIntervalSeconds 60, islandPerMinute 0.5, 확정대기}; propagation {distance 3.0, delaySeconds 4.0, 확정}; timeOfDay {dayMultiplier 1.0, nightMultiplier 0.7, nightWanderEventId "night_wander", nightWanderIntervalSeconds 3.0, 확정대기}; carryOver {personalFactor 0.5, islandFactor 0.5, islandMin 5, 확정대기}; barks ["거기, 잠깐.", "어디서 왔어요?", "처음 보는 사람이네."] — contracts/suspicion-rules-schema.json 준수
- [X] T010 [P] Implement `Assets/Scripts/Suspicion/Runtime/SuspicionEvent.cs` (struct: `EventId`, `Actor`, `Position`, `TargetNpc`) and `Assets/Scripts/Suspicion/Runtime/SuspicionEvents.cs` (static events per contracts §3 + `internal Raise*` 헬퍼: OnPersonalStageChanged, OnReportAttempt, OnIslandChanged, OnIslandZoneChanged, OnDepartureBlockedChanged, OnPropagated, OnRunEnded, OnEventApplied)
- [X] T011 [P] Implement `Assets/Scripts/Suspicion/Runtime/TimeOfDay.cs` — 싱글톤 `Instance`, `enum DayPhase {Day, Night}`, `Phase`, `Set`, `Toggle`, `event OnChanged`; 인스턴스 없을 때 조회용 `static DayPhase CurrentOrDay`
- [X] T012 [P] Implement `Assets/Scripts/Suspicion/Runtime/NpcSuspicionProfile.cs` — 4개 bool 프로퍼티(직렬화 필드) `PropagatesSuspicion=true`, `ReportsToManagerImmediately=false`, `IsManager=false`, `SuppressStageBehaviour=false`; `static NpcSuspicionProfile GetOrDefault(NpcIdentity)` → 없으면 정적 기본 인스턴스 값 반환(컴포넌트 추가 없이 값만)
- [X] T013 Implement `Assets/Scripts/Suspicion/Runtime/SuspicionSystem.cs` 골격 — 싱글톤, Awake에서 `SuspicionRulesLoader.TryLoad`; 실패 시 `IsReady=false` + `RuntimeHud.Warn` 각 메시지; 성공 시 `RuntimeHud.Warn($"의심 규칙 확정 대기 {PendingCount}건")`; 필드 `Island`, `personal` Dictionary<(NpcIdentity, PlayerEntity), PersonalSuspicion>, 이벤트 큐 `Queue<SuspicionEvent>`; `Raise(e)`(IsReady 아니면 무시+경고 1회); `GetPersonal`(없으면 0 생성); `GetStage`; `AllPersonal`; `PlayerEntity.OnRegistered`로 모든 NPC 쌍 0 생성(Edge case); Update 뼈대: `TickNightWander()→ApplyQueuedEvents()→TickPropagation()→TickDecay()` 순서(순서 고정, 상승이 감소보다 먼저) — 각 Tick 본문은 스토리 태스크에서 채움 (depends on T008, T010, T011, T012; PersonalSuspicion·IslandAlert는 US1/US2에서 구현되므로 이 시점엔 스텁 참조 → US1·US2 완료 전까지 컴파일하려면 T015·T021을 먼저 수행)

**Checkpoint**: 규칙 로드·검증·이벤트 허브·시스템 골격 준비.

---

## Phase 3: User Story 1 - 개인 의심 4단계와 NPC 행동 (Priority: P1) 🎯 MVP

**Goal**: (NPC, 플레이어) 개인 의심 0~3, 표 기반 상승, 단계 전환 사건, NPC 표현(쳐다보기·따라가기·말 걸기), 3에서 신고 시도 사건.

**Independent Test**: EditMode `PersonalSuspicionTests` + quickstart US1 표.

### Tests for User Story 1 ⚠️

- [X] T014 [P] [US1] Write `Assets/Tests/EditMode/Suspicion/PersonalSuspicionTests.cs` — 초깃값 0·Indifferent; +1→Aware·StageChange(0→1); +5→3 클램프·Certain; 3에서 +1→변화 없음(null); -1→2·Alert; -10→0 클램프; delta>0면 LastRaisedAt 갱신, delta<0면 유지; 같은 프레임 +2 +1 순차 적용 결과 3

### Implementation for User Story 1

- [X] T015 [P] [US1] Implement pure `Assets/Scripts/Suspicion/Model/PersonalSuspicion.cs` — `enum SuspicionStage {Indifferent=0, Aware=1, Alert=2, Certain=3}`, `class PersonalSuspicion { int Value; SuspicionStage Stage => (SuspicionStage)Value; float LastRaisedAt; StageChange? Apply(int delta, float now); }`, `struct StageChange { SuspicionStage Old, New; }`; 상수 `Min=0, Max=3`(§2.1 확정)
- [X] T016 [US1] Implement event application in `Assets/Scripts/Suspicion/Runtime/SuspicionSystem.cs` `ApplyQueuedEvents()` — 각 사건: `Rules.FindEvent(id)` 없으면 경고 1회/id; `nightOnly`이고 `TimeOfDay.CurrentOrDay != Night`면 스킵; 대상 NPC 집합 = scope별(witness: `NpcVision.IsSeeing(actor)` 참인 `NpcIdentity.All`; radius: `Vector3.Distance(npc.Position, e.Position) ≤ radius`; target: `e.TargetNpc`) ∩ (requiresSight면 IsSeeing 참); personalDelta에 시간대 배율 곱해 `Mathf.RoundToInt`, 원래 0이 아니면 최소 1(R-4); `GetPersonal(npc, actor).Apply(delta, Time.time)` → StageChange면 `SuspicionEvents.RaisePersonalStageChanged`; New==Certain이면 `RaiseReportAttempt(npc, actor)`(3 도달 1회 — 이후 값이 내려가 다시 3이면 재발행); islandDelta>0이면 `Island.Apply`(US2 T022에서 구간 이벤트 연결); `RaiseEventApplied(e, count)` (depends on T013, T015)
- [X] T017 [P] [US1] Implement `Assets/Scripts/Suspicion/Runtime/NpcMover.cs` — `speed=1.6`, `turnSpeed=360`, `arriveDistance=0.3`; `MoveTo(pos)`, `Follow(Transform, keepDistance)`, `FaceTowards(pos)`, `Stop()`, `IsMoving`, `HasArrived`; Update에서 transform 직선 이동(y 고정)·회전; Follow는 거리 > keepDistance일 때만 이동, 항상 대상을 향해 회전. 장애물 회피 없음(R-9)
- [X] T018 [US1] Implement `Assets/Scripts/Suspicion/Runtime/NpcSuspicionBehaviour.cs` — `RequireComponent(NpcIdentity, NpcMover)`; Update: `SuppressStageBehaviour`면 return; 모든 PlayerEntity 중 `GetStage` 최고인 focus 선택; Stage Aware: `NpcVision.IsSeeing(focus)`면 `mover.FaceTowards(focus.Position)`, 아니면 `lastSeenPos`를 향해 회전(돌아보기); Stage Alert: `mover.Follow(focus.transform, followDistance=2.5)`, 접근(≤ followDistance+0.5) 시 `RuntimeHud.Warn` 대신 `VisionDebugLabel` 옆 bark: `RuntimeHud.Instance.CreateWorldLabel`로 NPC당 1회 생성해 재사용하는 라벨에 `Rules.barks[random]` 표시(머리 위, VisionDebugLabel보다 0.5 위) 후 `barkCooldown=6s`, 쿨다운 후 문구 비움; Stage Certain: `mover.Stop()`, 표현 중단(신고 흐름은 005); Indifferent: `mover.Stop()`. 원위치 복귀는 하지 않음(단순화, Assumptions에 기록) (depends on T016, T017)
- [X] T019 [US1] Extend `Assets/Editor/PlayFoundationSceneBuilder.cs` — `CreatePlayer`·`CreateNpc`·`CreateGround/Walls/Light/OcclusionWall`을 `public static`으로 변경하고, `CreateNpc`가 GameObject를 반환하도록 수정(002 빌더 재사용). 001 동작 변화 없음
- [X] T020 [US1] Implement `Assets/Editor/SuspicionSceneBuilder.cs` — `[MenuItem("Tools/PROJECT 1028/Build Suspicion Test Scene")]`; 001 빌더 정적 메서드로 바닥·벽·빛·HUD·P1(입력)·P2(입력 없음, (10,1,10)) 생성; NPC 3: `NPC_A`(2,1,4) 정면 -Z 일반, `NPC_B`(-4,1,4) `NpcSuspicionProfile.PropagatesSuspicion=false`, `NPC_M`(6,1,12) `IsManager=true`; 각 NPC에 `NpcMover`, `NpcSuspicionBehaviour`, `NpcSuspicionProfile` 추가; `TimeOfDay`, `SuspicionSystem`, `SuspicionDebugPanel`, `SuspicionTestConsole` GameObject 추가; 저장 `Assets/Scenes/Test_Suspicion.unity` (depends on T019, T013, T018)

**Checkpoint**: F1로 NPC_A/P1이 0→2→3으로 오르며 따라오기·말 걸기·신고 시도 로그가 나온다(디버그 패널은 US2 뒤 T026에서 완성되므로 이 시점에는 콘솔 로그로 확인).

---

## Phase 4: User Story 2 - 섬 의심도 공유와 네 구간 (Priority: P1)

**Goal**: 섬 의심도 0~100, 구간 전환 사건(양방향), 봉쇄 시 출항 불가 상태, 공유 값.

**Independent Test**: EditMode `IslandAlertTests` + quickstart US2 표.

### Tests for User Story 2 ⚠️

- [X] T021 [P] [US2] Write `Assets/Tests/EditMode/Suspicion/IslandAlertTests.cs` — ZoneOf(0)=Calm, (25)=Calm, (26)=Watch, (50)=Watch, (51)=Tension, (75)=Tension, (76)=Lockdown, (100)=Lockdown; 25에서 +1→ZoneChange(Calm→Watch); 75에서 +1→Lockdown·DepartureBlocked true; 100에서 +30→100 유지·null; 30에서 -5→25→Watch→Calm 전환; ApplyDecay 0.4 두 번→0.8 누적 후 값 불변, 세 번째(1.2)→1 감소; 76에서 ApplyDecay로 75→봉쇄 해제 전환

### Implementation for User Story 2

- [X] T022 [P] [US2] Implement pure `Assets/Scripts/Suspicion/Model/IslandAlert.cs` — `enum AlertZone {Calm, Watch, Tension, Lockdown}`; 상수 `Min=0, Max=100, WatchFrom=26, TensionFrom=51, LockdownFrom=76`(§2.4 확정); `int Value`, `AlertZone Zone => ZoneOf(Value)`, `bool DepartureBlocked => Zone==Lockdown`; `ZoneChange? Apply(int delta)`; `ZoneChange? ApplyDecay(float amount)`(소수 누적 후 내림); `static AlertZone ZoneOf(int)`
- [X] T023 [US2] Wire island events in `Assets/Scripts/Suspicion/Runtime/SuspicionSystem.cs` — `ApplyIsland(int delta)` 공통 메서드: 이전 값·구간·DepartureBlocked 기억 → `Island.Apply` → `RaiseIslandChanged(old,new)`; 구간 바뀌면 `RaiseIslandZoneChanged`; DepartureBlocked 바뀌면 `RaiseDepartureBlockedChanged`; T016의 islandDelta 경로와 `DebugAdjustIsland`가 이 메서드를 사용 (depends on T016, T022)
- [X] T024 [P] [US2] Implement `Assets/Scripts/Suspicion/Debug/SuspicionTestConsole.cs` — `Keyboard.current` 폴링: F1~F7→`Rules.events[0..6]` 사건 Raise(witness/target: `InteractableSelector`와 같은 방식으로 P1 카메라 전방 최근접 NPC를 TargetNpc로, Position=P1.Position; radius: Position=P1.Position), nightOnly 사건이 낮에 눌리면 `RuntimeHud.Warn("밤 전용 사건, 현재 낮")`; N→`TimeOfDay.Toggle()`, R→`EndRun()`(US4에서 구현, 그 전엔 경고), T→`Time.timeScale` 1↔10, `[`/`]`→`DebugAdjustIsland(∓10)`; 각 키마다 `RuntimeHud.Warn`으로 무엇을 했는지 로그
- [X] T025 [P] [US2] Implement `Assets/Scripts/Suspicion/Debug/SuspicionDebugPanel.cs` — `RuntimeHud.Instance.CreateWorldLabel` 대신 캔버스에 좌상단 아래(경고 로그 밑) 고정 Text 1개 생성(`RuntimeHud`에 `CreateFixedText(anchor, size)` 공개 메서드 추가 필요 → 001 `RuntimeHud.cs`에 추가); 매 0.2초 갱신: "섬 의심도 {v} [{zone}] 출항 불가: {y/n} | 시간대 {day/night} | 확정 대기 {n}건" + 각 (NPC, P) "NPC_A P1=2(경계) P2=0" + 예약 전파 목록(US3) + 최근 이벤트 로그 5줄(SuspicionEvents 구독) (depends on T013)
- [X] T026 [US2] Register console/panel in `Assets/Editor/SuspicionSceneBuilder.cs` (T020에서 GameObject만 두었다면 컴포넌트 연결 확인) 및 `Assets/Scripts/PlayFoundation.asmdef`가 `UnityEngine.UI`를 참조함을 확인 (depends on T020, T024, T025)

**Checkpoint**: `]`로 구간이 바뀌고 봉쇄에서 출항 불가가 참이 된다. P2 슬롯도 같은 값. F키로 개인 의심 상승과 함께 표시.

---

## Phase 5: User Story 3 - 전파 (Priority: P2)

**Goal**: 2 이상 NPC가 다른 NPC와 만나면 지연 후 +1. 만남당 1회. 플래그(전파 안 함·밀고자 즉시 관리자) 반영.

**Independent Test**: EditMode `PropagationSchedulerTests` + quickstart US3 표.

### Tests for User Story 3 ⚠️

- [X] T027 [P] [US3] Write `Assets/Tests/EditMode/Suspicion/PropagationSchedulerTests.cs` — 순수 키(string npcId, string playerId) 사용 버전으로: 만남 시작 후 `TryReserve(A,B,P1,t+4)` true, 같은 만남 재시도 false; `Drain(t+3)` 비어 있음, `Drain(t+4)` 1건; 만남 종료 후 재만남 → 다시 true; A→B와 B→A는 별개; 다른 플레이어 P2는 별개 예약; 만남 종료 후에도 이미 예약된 건은 Drain 시 실행됨(Edge case "예약은 실행")

### Implementation for User Story 3

- [X] T028 [P] [US3] Implement pure `Assets/Scripts/Suspicion/Model/PropagationScheduler.cs` — 제네릭 키가 아닌 `string` 키(npcId, playerId)로 순수 구현: `SetMeeting(string a, string b, bool inRange, float now)`, `bool TryReserve(string from, string to, string playerId, float executeAt)`, `IEnumerable<Reservation> Drain(float now)`, `IReadOnlyList<Reservation> Pending`; `struct Reservation { string From, To, PlayerId; float ExecuteAt; }`; 만남 키는 (min(a,b), max(a,b))
- [X] T029 [US3] Implement `TickPropagation()` in `Assets/Scripts/Suspicion/Runtime/SuspicionSystem.cs` — 모든 NPC 쌍에 대해 `Vector3.Distance ≤ Rules.propagation.distance`로 `SetMeeting`; 만남 중인 쌍 (A,B) 각 방향에 대해: `profileA.PropagatesSuspicion`이 false면 스킵; 각 플레이어 p에 대해 `GetPersonal(A,p).Value ≥ 2`면 — `profileA.ReportsToManagerImmediately`가 true면 `profileB.IsManager`일 때만 `TryReserve(A,B,p, now+0)`, 아니면 `TryReserve(A,B,p, now+delaySeconds)`; `Drain(now)`된 예약마다 `Raise(new SuspicionEvent{ EventId="propagation", Actor=p, TargetNpc=B })` + `RaisePropagated(A,B,p)`; NPC/플레이어 id→객체 조회 딕셔너리 유지 (depends on T028, T016)
- [X] T030 [US3] Show pending reservations in `Assets/Scripts/Suspicion/Debug/SuspicionDebugPanel.cs` — `SuspicionSystem.PendingPropagations` 노출 후 "전파 예약: A→M (P1) 2.3s" 형식 (depends on T029, T025)

**Checkpoint**: NPC_A(2)가 NPC_M 근처로 가면 4초 뒤 NPC_M/P1=1. NPC_B는 전파하지 않음.

---

## Phase 6: User Story 4 - 감소와 런 종료 이월 (Priority: P2)

**Goal**: 매우 느린 감소(개인 주기 -1, 섬 분당 소수), 감소로 구간 하강 시 전환 사건, `EndRun()` 이월 값.

**Independent Test**: EditMode `DecayCalculatorTests`, `CarryOverCalculatorTests` + quickstart US4 표.

### Tests for User Story 4 ⚠️

- [X] T031 [P] [US4] Write `Assets/Tests/EditMode/Suspicion/DecayCalculatorTests.cs` — `PersonalSteps(lastRaised=0, lastDecay=0, now=59, interval=60)`=0; now=60→1; now=125→2; lastDecay=60,now=125→1; `IslandAmount(60s, 0.5/min)`=0.5; `IslandAmount(0,…)`=0
- [X] T032 [P] [US4] Write `Assets/Tests/EditMode/Suspicion/CarryOverCalculatorTests.cs` — island 80, factor 0.5, min 5→40; island 4, factor 0.5, min 5→5; island 0→5(islandMin 보장, 0 아님); personal 3, factor 0.5→1; personal 1, factor 0.5→0; personal 2, factor 0.5→1

### Implementation for User Story 4

- [X] T033 [P] [US4] Implement pure `Assets/Scripts/Suspicion/Model/DecayCalculator.cs` — `static int PersonalSteps(float lastRaisedAt, float lastDecayAt, float now, float interval)`: 기준 = max(lastRaisedAt, lastDecayAt), steps = floor((now-기준)/interval); `static float IslandAmount(float deltaSeconds, float perMinute)`
- [X] T034 [P] [US4] Implement pure `Assets/Scripts/Suspicion/Model/CarryOverCalculator.cs` — `class CarryOverSnapshot { Dictionary<(string npcId, string playerId), int> Personal; int Island; }`; `static CarryOverSnapshot Compute(IEnumerable<(string,string,int)> personal, int island, CarryOverRule rule)`: Personal = floor(v×personalFactor), Island = max(islandMin, round(v×islandFactor))
- [X] T035 [US4] Implement `TickDecay()` and `EndRun()` in `Assets/Scripts/Suspicion/Runtime/SuspicionSystem.cs` — 개인: 각 쌍 `DecayCalculator.PersonalSteps` > 0이면 `Apply(-steps, now)`(StageChange면 이벤트, lastDecayAt = now); 섬: `ApplyDecay(DecayCalculator.IslandAmount(Time.deltaTime, islandPerMinute))`를 T023 `ApplyIsland` 경로와 같은 이벤트 처리로; `EndRun()`: `CarryOverCalculator.Compute` → `RaiseRunEnded(snapshot)` → `RuntimeHud.Warn` 요약("이월: 섬 {n}, 개인 {k}쌍") → 스냅샷 반환(값은 리셋하지 않음: 저장 기능이 다음 방문에 적용) (depends on T033, T034, T023)

**Checkpoint**: 배속 10에서 개인 의심이 주기마다 내려가고, R로 이월 값이 표시된다.

---

## Phase 7: User Story 5 - 시간대 위협 (Priority: P2)

**Goal**: 밤에 보이는 곳에서 이동하면 +1(쿨다운), 위장 밤 무효, 누적 배율.

**Independent Test**: quickstart US5 표. (배율 적용은 T016에 포함되어 있음, 밤 이동 사건 생성만 추가)

### Implementation for User Story 5

- [X] T036 [P] [US5] Implement `Assets/Scripts/Suspicion/Runtime/PlayerDisguise.cs` — `RequireComponent(PlayerEntity)`, `bool IsDisguised`(직렬화, 인스펙터 토글), `bool IsEffective => IsDisguised && TimeOfDay.CurrentOrDay == Day`(§7 밤 무효). 효과량은 이 기능에서 없음
- [X] T037 [US5] Implement `TickNightWander()` in `Assets/Scripts/Suspicion/Runtime/SuspicionSystem.cs` — `TimeOfDay.CurrentOrDay == Night`일 때만; 각 NPC·플레이어 쌍: `NpcVision.IsSeeing(p)` && `p.MovementState != Idle` && 쿨다운 만료(`nightWanderCooldown[(npc,p)] ≤ now`)면 `Raise(new SuspicionEvent{ EventId=Rules.timeOfDay.nightWanderEventId, Actor=p, TargetNpc=npc, Position=p.Position })` 후 쿨다운 = now + nightWanderIntervalSeconds (depends on T016)
- [X] T038 [US5] Add disguise readout to `Assets/Scripts/Suspicion/Debug/SuspicionDebugPanel.cs` — 각 플레이어 "위장: 착용/미착용, 유효: 예/아니오" (depends on T036, T025)
- [X] T039 [US5] Add `PlayerDisguise` to players in `Assets/Editor/SuspicionSceneBuilder.cs` (depends on T036, T020)

**Checkpoint**: N으로 밤 전환 후 NPC 앞에서 걷으면 3초마다 +1, 서 있으면 없음, 낮에는 없음.

---

## Phase 8: Polish & Cross-Cutting

- [X] T040 [P] Update `Assets/Scripts/README.md` — `Suspicion/` 폴더 행 추가, 후속 기능이 구독할 `SuspicionEvents`·`Raise` 사용법·JSON 사건 추가 절차 링크
- [X] T041 [P] Verify 001 regression in `Assets/Scenes/Test_PlayFoundation.unity` — 씬은 002 컴포넌트 없이 그대로 동작해야 함(`PlayFoundation` asmdef가 `Suspicion`을 참조하지 않음을 확인)
- [ ] T042 (에디터 수동) Run `specs/002-suspicion-system/quickstart.md` §1~§4 (에디터 수동) 후 §4 체크박스에 결과 기록
- [X] T043 헌장 최종 점검 — `grep -rnE '[0-9]+' Assets/Scripts/Suspicion --include='*.cs'`로 숫자 리터럴 검토: 허용 목록(0,1,2,3,25/26,50/51,75/76,100, 배열 인덱스, 시간 비교 0, 라벨 레이아웃, 기본 이동 속도/거리 등 표현 파라미터)을 벗어난 의심 수치·감소·배율 하드코딩 0건; 신규 에셋 0개; 결과를 `specs/002-suspicion-system/checklists/requirements.md` Notes에 기록

---

## Dependencies & Execution Order

- Phase 1 → Phase 2 → US1 → US2 → (US3 ∥ US4 ∥ US5) → Polish
- T013(시스템 골격)은 `PersonalSuspicion`(T015)·`IslandAlert`(T022)를 참조하므로 실제 컴파일 순서는 T015·T022를 T013과 함께 먼저 작성한다. 테스트 T014·T021은 그 전에 작성.
- 같은 파일 순차: `SuspicionSystem.cs`(T013→T016→T023→T029→T035→T037), `SuspicionDebugPanel.cs`(T025→T030→T038), `SuspicionSceneBuilder.cs`(T020→T026→T039), 001 `PlayFoundationSceneBuilder.cs`(T019), 001 `RuntimeHud.cs`(T025의 CreateFixedText 추가).

### Parallel Opportunities

- Phase 2: T005 ∥ T006 ∥ T009 ∥ T010 ∥ T011 ∥ T012
- US1: T014 ∥ T015 ∥ T017
- US2: T021 ∥ T022 ∥ T024 ∥ T025
- US3~US5 서로 병렬(파일 겹침 없음, 단 `SuspicionSystem.cs` 편집은 순차)

## Implementation Strategy

- **MVP = US1 + US2**: 개인 의심·NPC 행동·섬 의심도 구간. 여기까지가 "누가 나를 어떻게 보는가 + 섬이 어떤 상태인가".
- 이후 전파(US3)로 "마을이 상대다", 감소·이월(US4)로 재방문 제동, 시간대(US5)로 낮·밤 차이.
- 에디터 수동: Test Runner, 씬 생성 메뉴, Play 시나리오(quickstart).

## Notes

- 이 기능 코드에서 SDD 수치는 JSON에만. 코드 리뷰 시 T043 grep을 기준으로 판정.
- 001 코드 수정은 두 곳만: `PlayFoundationSceneBuilder.cs` 메서드 공개(T019), `RuntimeHud.cs` 고정 텍스트 생성 API(T025). 001 동작 변화 없음.
- 저장소는 git이 아니므로 완료 시 체크박스 갱신.

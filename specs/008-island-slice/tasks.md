---
description: "Task list for 008 염전섬 그레이박스 수직 슬라이스 (Island Slice)"
---

# Tasks: 염전섬 그레이박스 수직 슬라이스

**Input**: `/specs/008-island-slice/`. 001~007 전부 필요. **Tests**: 포함. 네임스페이스 `Project1028.Slice`, 어셈블리 `Slice`. 수치·문구·좌표는 JSON.
**Organization**: US1 목표·안내(P1) → US2 시계·출항(P1) → US3 제한구역·증거(P1) → US4 즉각 반응(P1) → US5 섬 배치(P2) → US6 결과·재시작(P2). 씬 빌더(US5)는 US1~US3 컴포넌트가 있어야 컴파일되므로 실제 작성 순서는 모델 → 런타임 전체 → 빌더.

## Phase 1: Setup
- [X] T001 Create folders `Assets/Scripts/Slice/{Model,Runtime,Debug}`, `Assets/Tests/EditMode/Slice`, `Assets/StreamingAssets/Slice`
- [X] T002 Create `Assets/Scripts/Slice/Slice.asmdef` — references `["PlayFoundation","Suspicion","NpcTypes","Subdue","Report","Vehicle","Encounter","Unity.InputSystem","UnityEngine.UI"]`
- [X] T003 [P] Create `Assets/Tests/EditMode/Slice/Slice.Tests.EditMode.asmdef` — references `["Slice","Suspicion","PlayFoundation","UnityEngine.TestRunner","UnityEditor.TestRunner"]`, nunit, UNITY_INCLUDE_TESTS
- [X] T004 [P] Edit `Assets/Editor/PlayFoundation.Editor.asmdef` — `"Slice"` 추가

## Phase 2: Foundational (가산 확장 + 데이터 + 모델)
- [X] T005 Extend 001 `Assets/Scripts/Player/OrbitCamera.cs` — `public void Shake(float amplitude, float seconds)`; LateUpdate에서 남은 시간 동안 `Random.insideUnitSphere * amplitude * (remaining/seconds)` 오프셋을 위치에 더함
- [X] T006 [P] Extend 004 `Assets/Scripts/Subdue/Runtime/CarriableObject.cs` — `public bool Locked {get;set;}`(직렬화), `public bool IsEvidence {get;set;}`(직렬화); `CanInteract`에 `&& !Locked`; `Configure(name, twoHanded, isEvidence=false, locked=false)` 오버로드
- [X] T007 [P] Extend 006 `Assets/Scripts/Vehicle/Runtime/VehicleSeats.cs` — `public static bool AllowBusyHands`(기본 false); `TryEnter`의 `player.HandsBusy` 거부를 `!AllowBusyHands && player.HandsBusy`로; `VehicleEnterInteractable.CanInteract`도 동일
- [X] T008 [P] Edit 003 `Assets/StreamingAssets/NpcTypes/npc_types.json` — assignments 추가: `slice_watcher_1`(Watcher), `slice_watcher_2`(Watcher), `slice_informer_1`(Informer), `slice_informer_2`(Informer), `slice_symp_1`(Sympathizer), `slice_symp_2`(Sympathizer), `slice_silent_1`(Silent), `slice_silent_2`(Silent), `slice_dog`(Sentinel), `slice_manager`(Watcher, roles ["manager"])
- [X] T009 [P] Write `Assets/Tests/EditMode/Slice/SliceRulesValidatorTests.cs` — 정상→OK·PendingCount 3(clock·evidence·restricted); departureAt ≤ nightAt→Error; objectives 비어 있음→Error; condition 오타→Error; heartbeat intervalsByStage 길이 3→Error; volume 1.5→Error; 사건 id 미존재(콜백 false)→Error; hints 비어 있음→Error
- [X] T010 [P] Implement `Assets/Scripts/Slice/Model/SliceRules.cs` — data-model 블록 `[Serializable]`; `ObjectiveStep{id, text, markerFacilityId, condition}`; `FeedbackRule{HeartbeatRule heartbeat(intervalsByStage[], zoneMultiplier[], volume), PulseRule(seconds, alpha), TintRule(alphaByZone[], nightAlpha), HitstopRule(seconds), ShakeRule(amplitude, seconds), BigTextRule(seconds)}`
- [X] T011 [P] Implement `Assets/Scripts/Slice/Model/IslandLayout.cs` — `[Serializable] IslandLayout{version, Facility[] facilities, NpcPlacement[] npcs, ZoneDef[] restrictedZones, dockFacilityId, warehouseFacilityId, evidenceFacilityId}`, `Facility{id, kind, Vec3 position, Vec3 size, float rotationY, label}`, `NpcPlacement{npcId, displayName, Vec3 position, Vec3 facing}`, `ZoneDef{id, Vec3 center, Vec3 size}`, `Vec3{x,y,z}`; `Facility Find(id)`; `IslandLayoutLoader.TryLoad(out layout, out error)` 경로 `StreamingAssets/Slice/island_layout.json`(에디터·런타임 공용, `System.IO`)
- [X] T012 Implement pure `Assets/Scripts/Slice/Model/SliceRulesValidator.cs` + `Assets/Scripts/Slice/Model/SliceRulesLoader.cs` — data-model 규칙, 사건 id는 콜백; 경로 `StreamingAssets/Slice/slice_rules.json` (depends on T010)
- [X] T013 [P] Create `Assets/StreamingAssets/Slice/slice_rules.json` — clock{300, 600, 180, 확정대기}; evidence{investigateSeconds 2.5, twoHanded true, seenCooldownSeconds 3, oneHandEventId "evidence_seen_one_hand", twoHandEventId "evidence_seen_two_hands", 확정대기}; restricted{cooldownSeconds 5, eventId "enter_restricted_area", 확정대기}; dock{boardRadius 6, requireEvidenceForSuccess true, 확정}; vehicle{allowEvidenceAboard true}; objectives[{obj_evidence, "창고에서 장부를 조사해 가져와라", warehouse, evidence_picked}, {obj_depart, "부두로 돌아가 배를 타라", dock, at_dock_with_evidence}]; hints["WASD 이동 · Shift 달리기 · 마우스 시점", "E 조사·들기·탑승·배 타기 (길게 누르면 조사)", "F 제압 (빈손) · G 내려놓기", "누가 보고 있으면 화면 위 눈이 켜진다"]; hintSeconds 30; feedback{heartbeat{[2.0,1.2,0.7,0.4],[1.0,0.9,0.75,0.55],0.5}, pulse{0.6,0.45}, tint{[0,0.08,0.18,0.3],0.35}, hitstop{0.1}, shake{0.25,0.25}, bigText{1.2}}
- [X] T014 [P] Create `Assets/StreamingAssets/Slice/island_layout.json` — 시설: `dock`(dock, (0,0,-34), size (14,0.3,8)), `boat`(boat, (0,0.6,-40), (4,1.2,8), label "배"), `road_main`(road, (0,0.02,-2), (6,1,64)), `road_east`(road, (16,0.02,10), (32,1,5), rotationY 0), `warehouse`(warehouse, (-14,0,22), (10,4,8), label "창고"), `office`(office, (18,0,26), (5,3,5), label "관리소"), `phone_a`(phone, (-6,0,12)), `phone_b`(phone, (22,0,0)), `house_1..4`(house, (-12,0,-6),(-12,0,4),(12,0,-8),(12,0,14), (5,3,5)), `saltfield`(saltfield, (24,0,-20), (16,0.1,14)), `evidence`(evidence, (-14,0.5,22)), `vehicle`(vehicle, (6,1,-28)), `trig_1..3`(trigger, (0,0,-16),(0,0,4),(16,0,10)), `spawn`(spawn, (0,1,-36)); npcs: watcher_1 창고 앞 (-9,1,20) facing -Z, watcher_2 관리소 앞 (18,1,22), informer_1 길가 (-5,1,-10), informer_2 (5,1,8), symp_1 집 앞 (-9,1,-6), symp_2 (9,1,14), silent_1 염전 (24,1,-16), silent_2 (28,1,-22), dog 집 앞 (10,1,-4), manager 관리소 안 (18,1,26); restrictedZones[{warehouse_zone, center (-14,1.5,22), size (12,3,10)}]; dockFacilityId dock, warehouseFacilityId warehouse, evidenceFacilityId evidence

## Phase 3: US1 목표·안내 (P1) 🎯
### Tests
- [X] T015 [P] [US1] Write `Assets/Tests/EditMode/Slice/ObjectiveTrackerTests.cs` — 2단계: Current=1단계; Satisfy("at_dock_with_evidence")→false(순서 아님); Satisfy("evidence_picked")→true·Index 1; 다시 Satisfy("evidence_picked")→false; Satisfy("at_dock_with_evidence")→true·IsComplete; 완료 후 Current null
### Implementation
- [X] T016 [P] [US1] Implement pure `Assets/Scripts/Slice/Model/ObjectiveTracker.cs` — `ObjectiveTracker(IReadOnlyList<ObjectiveStep>)`, `Current`, `Index`, `IsComplete`, `bool Satisfy(string condition)`(현재 단계 조건과 일치할 때만 전진)
- [X] T017 [P] [US1] Implement `Assets/Scripts/Slice/Runtime/SliceEvents.cs` — data-model 이벤트 6개 + Raise*
- [X] T018 [US1] Implement `Assets/Scripts/Slice/Runtime/ObjectiveHud.cs` — 001 `RuntimeHud.CreateFixedText`(상단 중앙 anchor (0.5,1)) 큰 글씨: "{목표 문구} · {마커 시설}까지 {m}m · 출항까지 mm:ss"(출항 후: "배가 떠난다 — 부두로", 봉쇄: "봉쇄 — 다음 배 mm:ss"); 마커 = 001 `DestinationMarker.Spawn`으로 `markerFacilityId` 위치에 1개 유지(단계 바뀌면 이동); 시작 후 `hintSeconds` 동안 두 번째 줄에 `hints` 순환(5초마다); `SliceEvents.OnObjectiveAdvanced`에서 문구 강조(0.8초 노란색) (depends on T016, T017)

## Phase 4: US2 시계·출항·봉쇄 (P1)
### Tests
- [X] T019 [P] [US2] Write `Assets/Tests/EditMode/Slice/RunClockTests.cs` — night 300, departure 600, nextBoat +180: Tick(299)→Day; Tick(1)→Night 전이 반환; Tick(300)→DepartureOpen; blocked true로 Tick→Lockdown 전이; blocked false로 Tick→DepartureOpen 복귀; NextBoatAt=780; RemainingToDeparture 계산; End()→Ended·이후 Tick 무시
- [X] T020 [P] [US2] Write `Assets/Tests/EditMode/Slice/DepartureRuleTests.cs` — 출항 전→CanBoard false "아직"; 출항 후·봉쇄→false "봉쇄"; 출항 후·증거·전원→true Success; 출항 후·증거 없음·requireEvidence→true LeftWithoutEvidence; 전원 아님→false "전원"; Forced(elapsed 780, nextBoat 780, blocked true)→ForcedDeparture; blocked false→null
### Implementation
- [X] T021 [P] [US2] Implement pure `Assets/Scripts/Slice/Model/RunClock.cs` — data-model API; `Tick(dt, blocked)`가 전이 발생 시 새 Phase 반환
- [X] T022 [P] [US2] Implement pure `Assets/Scripts/Slice/Model/DepartureRule.cs` — `enum RunOutcome`, `struct DepartureVerdict{CanBoard, Reason, Outcome}`, `Evaluate(...)`, `Forced(...)`
- [X] T023 [US2] Implement `Assets/Scripts/Slice/Runtime/RunDirector.cs` — 싱글톤; Start: `SliceRulesLoader`(SuspicionSystem 준비 후, 사건 id 존재 확인)·`IslandLayoutLoader`; `VehicleSeats.AllowBusyHands = rules.vehicle.allowEvidenceAboard`; `Clock`, `Objectives`, `Stats`(구독: 005 `OnReportCompleted`→신고++, 004 `OnSubdued`→제압++, 007 `OnEncounterEnded`→인카운터++·선택 기록, 007 `OnFlagRecorded`); Update: `Clock.Tick(dt, SuspicionSystem.Island.DepartureBlocked)` 전이 시 `OnRunPhaseChanged` + Night면 `TimeOfDay.Set(Night)`; `DepartureRule.Forced` → `EndRun(ForcedDeparture)`; `EndRun(outcome)`: Clock.End, `OnRunEnded(outcome, stats)`, 002 `EndRun()` 호출(이월 값 표시용); `Restart()`: `SceneManager.LoadScene(active.buildIndex 또는 name)`; 실패 시 IsReady=false + HUD (depends on T021, T022, T016, T012, T011)
- [X] T024 [US2] Implement `Assets/Scripts/Slice/Runtime/DockBoat.cs` — IInteractable "배 타기"(range boardRadius); `CanInteract`: !IsLocked; `Interact`: `DepartureRule.Evaluate(Clock.Elapsed, …, blocked, hasEvidence(플레이어 중 누구든 IsEvidence 물건 소지), allAtDock(모든 PlayerEntity가 부두 반경), requireEvidence)` → CanBoard false면 `OnDepartureDenied(reason)` + HUD; LeftWithoutEvidence면 첫 E에 "빈손으로 떠날까? 다시 E" 확인(5초), 두 번째 E에 `EndRun`; Success면 `EndRun(Success)` (depends on T023)

## Phase 5: US3 제한 구역·증거 (P1)
- [X] T025 [P] [US3] Implement `Assets/Scripts/Slice/Runtime/RestrictedZone.cs` — 중심·크기(박스), 플레이어별 안/밖 상태·쿨다운; 진입 시 `SuspicionSystem.Raise(SuspicionEvent.Witness(rules.restricted.eventId, player))`(002 표: witness·requiresSight → 보는 NPC만) + `OnRestrictedEntered`; Gizmo 박스 (depends on T023)
- [X] T026 [US3] Implement `Assets/Scripts/Slice/Runtime/EvidenceItem.cs` — `RequireComponent(CarriableObject)`; IInteractable "조사 (E 길게 {n}초)"; `CanInteract`: carriable.Locked && !IsLocked && !HandsBusy; `Interact`: 005 `CutProgress(investigateSeconds)` 시작 + `NpcTypeSystem.Instance?.Investigate(player)` 1회; Update: 거리 ≤ 2 && MovementState==Idle 유지 → 진행률 보조 안내, 취소 시 안내; 완료 → `carriable.Locked=false`, 자신 `enabled=false`, HUD "장부를 확인했다 — E로 든다", `OnEvidencePicked`는 실제 들었을 때(004 `SubdueEvents.OnPickedUp` 구독 후 IsEvidence면) 발생 → `RunDirector.Objectives.Satisfy("evidence_picked")` (depends on T006, T023)
- [X] T027 [US3] Implement `Assets/Scripts/Slice/Runtime/EvidenceWatcher.cs` — 플레이어 컴포넌트; `PlayerHands.HeldKind==Object && HeldObject.IsEvidence`이고 어느 `NpcVision.IsSeeing(owner)`면 쿨다운마다 `Raise(Witness(twoHanded ? twoHandEventId : oneHandEventId, owner))`; 부두 반경 안 + 증거 소지 → `Objectives.Satisfy("at_dock_with_evidence")` (depends on T023)

## Phase 6: US4 즉각 반응 (P1)
### Tests
- [X] T028 [P] [US4] Write `Assets/Tests/EditMode/Slice/HeartbeatRuleTests.cs` — intervals [2,1.2,0.7,0.4], mult [1,0.9,0.75,0.55]: (0,0)→2.0; (3,0)→0.4; (0,3)→1.1; (3,3)→0.22; 범위 밖 인덱스는 클램프
### Implementation
- [X] T029 [P] [US4] Implement pure `Assets/Scripts/Slice/Model/HeartbeatRule.cs` — `IntervalFor(maxStage, zoneIndex, intervalsByStage, zoneMultiplier)`
- [X] T030 [P] [US4] Implement `Assets/Scripts/Slice/Runtime/ProceduralAudio.cs` — `AudioSource` 2개(원샷·루프); `AudioClip.Create`로 `Tone(freq, seconds, WaveKind{Sine,Square,Triangle}, volume, decay)`·`Noise(seconds, volume)` 캐시; `SetHeartbeat(interval)`(0이면 정지; 60Hz 사인 2박 "둥-둥"); `SetDrone(bool)`(55Hz 사인 루프, 볼륨 0.15); `Alarm()`(880/660Hz 교대 0.4초); 오디오 장치 없으면 무시
- [X] T031 [P] [US4] Implement `Assets/Scripts/Slice/Runtime/ScreenFx.cs` — 001 `RuntimeHud` 캔버스에 전체 화면 `Image`(색조), 가장자리 `Image` 4장(맥동), 중앙 큰 `Text`; `Pulse(color, seconds, alpha)`, `SetTint(Color, alpha)`, `BigText(text, seconds)`, `HitStop(seconds)`(코루틴: `Time.timeScale` 저장→0→WaitForSecondsRealtime→복구)
- [X] T032 [P] [US4] Implement `Assets/Scripts/Slice/Runtime/EyeIndicator.cs` — P1을 보는 `NpcVision` 수를 0.1초마다 집계 → 상단 우측 "◉ 보고 있음 ×N"(0이면 숨김), 켜질 때 `ProceduralAudio.Tone(1200, 0.08)`
- [X] T033 [US4] Implement `Assets/Scripts/Slice/Runtime/FeedbackDirector.cs` — 구독(OnEnable/OnDisable 해제): 001 `NpcVision.OnSeeingChanged`(씬 내 모든 NpcVision에 구독, 새로 스폰된 NPC는 0.5초마다 재검색), 002 `OnPersonalStageChanged`(단계별 톤 1:1200 톡/2:300 둥/3:Alarm + 맥동 + 해당 NPC 위 "!" 라벨 1.5초), `OnIslandZoneChanged`(`SetTint(alphaByZone)` + 심박 갱신), `OnDepartureBlockedChanged`(BigText "배가 뜨지 않는다 — 봉쇄" + Alarm + SetDrone), `OnReportAttempt`(경보), 005 `OnReportStarted`(BigText "{npc}가 신고하러 간다" + 맥동 지속), `OnReportCompleted`(BigText "신고됐다" + Alarm), `OnReportInterrupted`, 004 `OnSubdued`(HitStop + Shake + Noise 둔탁), `OnShoveFailed`(Shake 절반 + BigText "실패"), `OnBodyDiscovered`(맥동 + 톤), 002 `TimeOfDay.OnChanged`(Night: SetTint nightAlpha + SetDrone + BigText "밤이다 — 작업복이 통하지 않는다"), 007 `OnEncounterStarted/OnChoiceRequested/OnEncounterEnded`(톤·BigText), `SliceEvents.OnObjectiveAdvanced`(상승 톤 3연타 + BigText), `OnDepartureDenied`(저음 + BigText), `OnRunEnded`(성공: 상승 아르페지오 / 실패: 하강); 심박: 매 0.5초 `HeartbeatRule.IntervalFor(P1에 대한 최고 개인 단계, 구간)` → `SetHeartbeat` (depends on T029, T030, T031, T017, T023)

## Phase 7: US5 섬 배치 (P2)
- [X] T034 [US5] Implement `Assets/Editor/IslandSliceBuilder.cs` — `[MenuItem("Tools/PROJECT 1028/Build Island Slice")]`; `IslandLayoutLoader.TryLoad`; 바닥(Plane 스케일 8 = 80×80)·벽·빛; 시스템 오브젝트 전부(001 HUD, 002 TimeOfDay·SuspicionSystem·패널·콘솔, 003 NpcTypeSystem·콘솔, 004 SubdueSystem·패널, 005 ReportSystem·패널·콘솔, 006 없음(트럭만), 007 EncounterSystem·패널·콘솔, 008 RunDirector·ObjectiveHud·ScreenFx·ProceduralAudio·EyeIndicator·FeedbackDirector·ResultScreen·SliceTestConsole); 시설 kind별: dock=넓은 납작 Cube(회색), boat=Cube(파랑)+`DockBoat`, road=Plane 띠(어둡게, 콜라이더 제거), warehouse=Cube 벽 4면+지붕 없음(문 쪽 한 면 비움)+`RestrictedZone`(layout.restrictedZones), office=`ReportSceneBuilder`와 같은 Cube+`ReportPoint(Office)`, house=Cube, saltfield=납작 Plane(흰색), phone=005 전화기 생성(`ReportPoint(Phone)`+`PhoneCutInteractable`), evidence=Cube 0.5(노랑)+`CarriableObject.Configure("장부", twoHanded, isEvidence true, locked true)`+`EvidenceItem`, vehicle=`VehicleSceneBuilder.CreateTruck`, trigger=`EncounterTrigger`, spawn=P1 위치; P2는 dock 위 (3,1,-34) 입력 없음; NPC는 `PlayFoundationSceneBuilder.CreateNpc(talkable:false)` + `NpcSuspicionProfile`·`NpcMover`·`NpcSuspicionBehaviour`; 플레이어에 `PlayerDisguise`·`PlayerHands`·`SubdueActor`·`PlayerToolkit`·`PlayerCutter`·`PlayerActivity`·`EvidenceWatcher`; 저장 `Assets/Scenes/Island_Slice.unity` (depends on T024, T025, T026, T027, T033)
- [X] T035 [US5] Verify walk time in `Assets/StreamingAssets/Slice/island_layout.json` — 바닥 120×120(벽 ±60), 부두 남쪽·창고 북서·관리소 북동, 도로는 남→중앙→서/동 갈래로 돌아가게 배치해 부두→창고 도보 경로 ≥ 90m(걷기 약 25초, 달리기 15초). 스펙 SC-006의 60~120초는 SDD "섬 횡단 2~3분" 실제 규모 기준이라 그레이박스에서는 미달 — 체크리스트에 기록하고 규모 결정은 플레이 후 (depends on T014, T034)

## Phase 8: US6 결과·재시작 (P2)
- [X] T036 [US6] Implement `Assets/Scripts/Slice/Runtime/ResultScreen.cs` — `SliceEvents.OnRunEnded` 구독 → 중앙 패널(Image 반투명 + Text): 결과 종류(성공/증거 없이 출도/강제 출도)·걸린 시간·최종 섬 의심도·구간·신고 완료 수·제압 수·인카운터 수·선택·플래그·002 이월 값(있으면); "R 다시 시작"; `Keyboard.current.rKey` → `RunDirector.Restart()`; 표시 중 `Time.timeScale=0`(재시작 전 복구) (depends on T023)
- [X] T037 [US6] Implement `Assets/Scripts/Slice/Debug/SliceTestConsole.cs` — F9 `Clock.Skip(60)`(RunClock에 `Skip(seconds)` 추가), F10 출항 시각 직전으로 Skip, F11 `SuspicionSystem.DebugAdjustIsland(±80)` 봉쇄 토글, F12 반응 20종 순환 테스트(`FeedbackDirector.TestNext()` 공개 메서드: 각 반응을 이벤트 없이 직접 호출) (depends on T033, T036)

## Phase 9: Polish
- [X] T038 [P] Update `Assets/Scripts/README.md` — `Slice/` 행, 가산 확장 3건, 입력 키, "게임처럼 보이는 첫 씬"
- [X] T039 [P] Update `specs/README.md`·`specs/TODO.md` — 008 추가, 상태 표 갱신, TODO A11(008 Play 검증)·B11(출항 시각·강제 출도 해석)
- [ ] T040 (에디터 수동) Run `specs/008-island-slice/quickstart.md` 한 런 + 반응 점검
- [X] T041 헌장 최종 점검 — Slice 코드 수치 리터럴은 UI 레이아웃·파형 주파수만(규칙 값은 JSON), 오디오 파일 0, 에셋 0 → `specs/008-island-slice/checklists/requirements.md` Notes

## Dependencies
Phase 1 → 2 → US1·US2·US3·US4(모델은 병렬, 런타임은 RunDirector 이후) → US5(빌더, 전 컴포넌트 필요) → US6 → Polish.

## Parallel
Phase 2: T006 ∥ T007 ∥ T008 ∥ T009 ∥ T010 ∥ T011 ∥ T013 ∥ T014 · 모델: T015 ∥ T016 ∥ T019 ∥ T020 ∥ T021 ∥ T022 ∥ T028 ∥ T029 · 런타임: T025 ∥ T030 ∥ T031 ∥ T032

## Implementation Strategy
전부가 MVP다. 이 기능의 목적은 "한 런을 끝까지 플레이할 수 있는 씬 하나"이므로 부분 완료는 의미가 없다.

## Notes
- 새 규칙 4개(시계·출항·제한 구역·증거 조사) 외에는 전부 기존 이벤트의 표현이다.
- 강제 출도·출항 시각 값은 clarify. 플레이 후 "긴장되는 순간" 유무(SC-005)가 이 기능의 진짜 판정 기준이다.

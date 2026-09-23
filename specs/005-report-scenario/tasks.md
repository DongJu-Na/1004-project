---
description: "Task list for 005 신고 시나리오 (Report Scenario)"
---

# Tasks: 신고 시나리오 (Report Scenario)

**Input**: `/specs/005-report-scenario/`. 001~004 코드 필요. **Tests**: 포함(원칙 III). 네임스페이스 `Project1028.Report`, 어셈블리 `Report`. 수치는 `report_rules.json`.
**Organization**: US1 이동 시작(P1) → US2 내버려두기·완료(P1) → US3 제압 중단(P2) → US4 끊기·재타깃(P2) → US5 사전 차단(P2).

## Phase 1: Setup
- [X] T001 Create folders `Assets/Scripts/Report/{Model,Runtime,Debug}`, `Assets/Tests/EditMode/Report`, `Assets/StreamingAssets/Report`
- [X] T002 Create `Assets/Scripts/Report/Report.asmdef` — references `["PlayFoundation","Suspicion","NpcTypes","Subdue","Unity.InputSystem","UnityEngine.UI"]`
- [X] T003 [P] Create `Assets/Tests/EditMode/Report/Report.Tests.EditMode.asmdef` — references `["Report","Subdue","NpcTypes","Suspicion","PlayFoundation","UnityEngine.TestRunner","UnityEditor.TestRunner"]`, nunit, UNITY_INCLUDE_TESTS
- [X] T004 [P] Edit `Assets/Editor/PlayFoundation.Editor.asmdef` — `"Report"` 추가

## Phase 2: Foundational
- [X] T005 [P] Edit 002 `Assets/StreamingAssets/Suspicion/suspicion_rules.json` — `report_completed`(personal 0, island 25, scope global, ignoresTimeMultiplier false, 확정, "§5.2 내버려둔다 — 신고 완료 섬 +25") 추가; 003 `Assets/StreamingAssets/NpcTypes/npc_types.json` assignments에 `rep_w1`(Watcher), `rep_w2`(Watcher), `rep_q`(Silent) 추가
- [X] T006 [P] Write `Assets/Tests/EditMode/Report/ReportRulesValidatorTests.cs` — 정상→OK·PendingCount 1(status 확정대기 1블록); minWalkSeconds 0→Error; cutDurationSeconds 0→Error; reportDurationSeconds -1→Error; cutRange 0→Error; completedEventId 없음(콜백 false)→Error; status 누락→Error
- [X] T007 [P] Implement `Assets/Scripts/Report/Model/ReportDefinitions.cs` — `enum ReportPointKind {Phone, Office}`, `enum ReportPhase {Moving, Reporting, Completed, Abandoned, Interrupted}`, 한국어 이름 헬퍼
- [X] T008 [P] Implement `Assets/Scripts/Report/Model/ReportRules.cs` — `[Serializable] ReportRules { string version; float minWalkSeconds; float reportDurationSeconds; float cutDurationSeconds; bool cutRequiresTool; float cutRange; float arriveDistance; string completedEventId; string status; }`
- [X] T009 Implement pure `Assets/Scripts/Report/Model/ReportRulesValidator.cs` — `Validate(rules, Func<string,bool> eventExists) → ReportValidationResult{IsError, Messages, PendingCount}` data-model 규칙 (depends on T008)
- [X] T010 Implement `Assets/Scripts/Report/Model/ReportRulesLoader.cs` — `TryLoad(SuspicionRules, out rules, out result)` 경로 `StreamingAssets/Report/report_rules.json` (depends on T009)
- [X] T011 [P] Create `Assets/StreamingAssets/Report/report_rules.json` — version "0.8-draft", minWalkSeconds 6, reportDurationSeconds 3, cutDurationSeconds 4, cutRequiresTool false, cutRange 2.0, arriveDistance 1.2, completedEventId "report_completed", status "확정대기"
- [X] T012 [P] Implement `Assets/Scripts/Report/Runtime/ReportEvents.cs` — data-model 이벤트 7개 + internal Raise*
- [X] T013 [P] Implement `Assets/Scripts/Report/Runtime/ReportPoint.cs` — 직렬화 `id, kind, displayName, isUsable=true`; `CanBeCut => kind==Phone`; `SetUsable(bool)`(변경 시 static event `OnUsabilityChanged(point)`); static `All`(OnEnable/OnDisable); `Configure(id, kind, displayName)`
- [X] T014 Implement `Assets/Scripts/Report/Runtime/ReportSystem.cs` 골격 — 싱글톤; Start(SuspicionSystem 준비 후 로드, HUD 경고·확정대기 건수); `SuspicionEvents.OnReportAttempt += HandleAttempt`(OnEnable/OnDisable); 완료 이력 `HashSet<(string,string)>`; `IsCompleted`; `ActiveFlows` 목록; `HandleAttempt`: IsReady && !IsCompleted && 이미 흐름 없음 && !UnconsciousState.IsUnconscious(npc) → `TryStart(npc, player)`(US1에서 채움) (depends on T010, T012, T013)

## Phase 3: US1 확신한 NPC가 신고하러 걸어간다 (P1) 🎯
### Tests
- [X] T015 [P] [US1] Write `Assets/Tests/EditMode/Report/ReportPointSelectorTests.cs` — 후보 없음→null; 사용 불가만→null; 가까운 사용 불가 + 먼 사용 가능→먼 것; 두 사용 가능→가까운 것; 동거리→첫 항목
- [X] T016 [P] [US1] Write `Assets/Tests/EditMode/Report/ReportSpeedRuleTests.cs` — distance 16, normal 1.6, minWalk 6 → 1.6(16/6=2.67 > 1.6 → normal); distance 6, minWalk 6 → 1.0; distance 0 → 0 아닌 최소 0.1 클램프; minWalk 0 → normal
- [X] T017 [P] [US1] Write `Assets/Tests/EditMode/Report/ReportFlowStateTests.cs` — Start("A")→Moving·TargetId A; Arrive(t=10)→Reporting; Tick(t=12, dur 3)→false; Tick(t=13)→true·Completed; Moving 중 Retarget("B")→Moving·B; Retarget(null)→Abandoned; Interrupt()→Interrupted; 종단 상태에서 Arrive/Retarget 무시
### Implementation
- [X] T018 [P] [US1] Implement pure `Assets/Scripts/Report/Model/ReportPointSelector.cs` — `struct PointInfo { string Id; Vector3 Position; bool Usable; }`; `static string PickNearestUsable(IReadOnlyList<PointInfo>, Vector3 from)`
- [X] T019 [P] [US1] Implement pure `Assets/Scripts/Report/Model/ReportSpeedRule.cs` — `static float SpeedFor(float distance, float normalSpeed, float minWalkSeconds)`: minWalk ≤ 0이면 normal; 아니면 `Mathf.Max(0.1f, Mathf.Min(normal, distance / minWalk))`
- [X] T020 [P] [US1] Implement pure `Assets/Scripts/Report/Model/ReportFlowState.cs` — `Phase`, `TargetId`, `ReportingSince`; `Start(id)`, `Arrive(now)`, `bool Tick(now, duration)`, `Retarget(idOrNull)`, `Interrupt()`; 종단 보호
- [X] T021 [US1] Implement `Assets/Scripts/Report/Runtime/ReportFlow.cs` — NPC 동적 컴포넌트; `Begin(player, point, rules)`: 원값 저장(NpcMover.Speed, NpcSuspicionProfile.SuppressStageBehaviour, InformerBehaviour/SentinelBehaviour enabled) → Suppress=true, 두 Behaviour disabled, `mover.Speed = ReportSpeedRule.SpeedFor(dist, original, minWalk)`, `mover.MoveTo(point.Position)`, 머리 위 라벨(`RuntimeHud.CreateWorldLabel`) "→ {point.DisplayName}"; Update: **Moving 동안 매 프레임 `mover.MoveTo(target)` 재발행**(002 행동 컴포넌트의 억제 진입 Stop이 같은 프레임 이동을 취소하는 것 방지); `UnconsciousState.IsUnconscious(npc)`면 `state.Interrupt()` → `Finish()`; 목적지 `!IsUsable`이면(**Moving·Reporting 모두** — 끊긴 전화로는 신고를 마칠 수 없다) `ReportSystem.PickPoint(npc)`로 `Retarget`(없으면 Abandon) + `OnReportRetargeted/OnReportAbandoned`; Moving이고 거리 ≤ arriveDistance면 `Arrive(now)` + `mover.Stop()`; Reporting이고 `Tick`이 true면 `ReportSystem.Complete(this)`; `Finish()`: 원값 복구·라벨 제거·`Destroy(this)` (depends on T018, T019, T020, T014)
- [X] T022 [US1] Implement `TryStart`·`PickPoint`·`Complete` in `Assets/Scripts/Report/Runtime/ReportSystem.cs` — `PickPoint(npc)`: `ReportPoint.All` → PointInfo 목록 → `PickNearestUsable`; 없으면 `OnReportAbandoned(npc, player, "지점 없음")` + HUD "신고 지점 없음 → 포기(§5.2 미리 끊어뒀다)" 후 false; 있으면 `ReportFlow` 부착·`Begin` + `OnReportStarted` + HUD "★ {npc}가 {point}로 신고하러 간다 ({player})"; `Complete(flow)`: `Raise(SuspicionEvent.At(rules.completedEventId, player, point.Position))`(섬 +25) + 이력 추가 + `OnReportCompleted` + HUD + `flow.Finish()` (depends on T021)
- [X] T023 [US1] Implement `Assets/Editor/ReportSceneBuilder.cs` — 001~004 시스템 오브젝트 생성(HUD·TimeOfDay·Suspicion·NpcType·Subdue 시스템·패널·콘솔) + `ReportSystem`·`ReportDebugPanel`·`ReportTestConsole`; 지점: `Phone_A`(-6,0,10) Phone, `Phone_B`(14,0,-10) Phone, `Office`(0,0,17) Office — 전화기 = Cylinder(0.3,1.2,0.3)+Cube(0.4) 상단, 관리소 = Cube(3,2.5,3), 각 `ReportPoint.Configure` + `PrimitiveTint`, 전화기에 `PhoneCutInteractable`(US4, 그 전엔 컴포넌트 부착만 컴파일 안 되므로 US4 후 추가); NPC(talkable false): `NPC_W1 rep_w1`(0,1,6) -Z, `NPC_W2 rep_w2`(6,1,2) -Z, `NPC_Q rep_q`(-6,1,2) -Z + `NpcSuspicionProfile`·`NpcMover`·`NpcSuspicionBehaviour`; P1(0,1,-6)+`PlayerDisguise`·`PlayerHands`·`SubdueActor`·`PlayerToolkit`·`PlayerCutter`(US4 후), P2(12,1,12); 저장 `Assets/Scenes/Test_Report.unity` (depends on T022)

**Checkpoint**: W1이 3이 되면 Phone_A로 걸어가고 라벨에 목적지가 보인다. Q는 이동 없음(002 억제).

## Phase 4: US2 내버려두면 신고 완료 (P1)
- [X] T024 [US2] Verify `Complete` path in `Assets/Scripts/Report/Runtime/ReportSystem.cs` — 도착 후 `reportDurationSeconds` 대기, +25 1회, 이력 (npc, player) 추가, 같은 쌍 `OnReportAttempt` 무시(FR-009), 다른 플레이어 쌍은 새 흐름(FR-009) — T022 구현 확인 및 HUD 로그 문구 확정 (depends on T022)
- [X] T025 [US2] Implement `Assets/Scripts/Report/Debug/ReportDebugPanel.cs` — 우하단 고정 텍스트(001 `CreateFixedText` anchor (1,0)): 지점별 "{name} [{kind}] {사용 가능|끊김}"; 흐름별 "{npc} → {point} {phase} ({player})"; 완료 이력 수; 로그 6줄(ReportEvents 구독); 키 안내 (depends on T014)

**Checkpoint**: 개입 없이 두면 섬 +25, 로그 "신고 완료", 재신고 없음.

## Phase 5: US3 제압으로 막는다 (P2)
- [X] T026 [US3] Verify interrupt path in `Assets/Scripts/Report/Runtime/ReportFlow.cs` — 기절 감지 → `Interrupt` → `OnReportInterrupted` + HUD; 이력에 남기지 않음; 004 깨어남이 발행하는 `OnReportAttempt`로 `ReportSystem.HandleAttempt`가 새 흐름 생성(중복 흐름 방지: 기존 흐름 있으면 무시); 정면 밀치기 실패는 `shove_failed`(개인만)이므로 영향 없음 확인 (depends on T021, T014)

**Checkpoint**: 이동 중 제압 → 중단, 배속 후 깨어남 → 재시작.

## Phase 6: US4 앞질러 전화선을 끊는다 (P2)
### Tests
- [X] T027 [P] [US4] Write `Assets/Tests/EditMode/Report/CutProgressTests.cs` — duration 4: Tick(1, valid)→Progressed·Progress 0.25; Tick(1, invalid)→Cancelled·Progress 0; 4초 누적→Completed·IsDone; IsDone 이후 Tick 무시; Reset→0
### Implementation
- [X] T028 [P] [US4] Implement pure `Assets/Scripts/Report/Model/CutProgress.cs` — `enum CutTickResult {Idle, Progressed, Cancelled, Completed}`; `CutProgress(duration)`, `Progress01`, `IsDone`, `Tick(dt, valid)`, `Reset()`
- [X] T029 [P] [US4] Implement `Assets/Scripts/Report/Runtime/PlayerToolkit.cs` — `RequireComponent(PlayerEntity)`, `bool HasCuttingTool`(직렬화)
- [X] T030 [US4] Implement `Assets/Scripts/Report/Runtime/PlayerCutter.cs` — `RequireComponent(PlayerEntity)`; `Begin(ReportPoint)`: 규칙 `cutRequiresTool` && !toolkit.HasCuttingTool → HUD "도구 필요(확정 대기)" 거부; `CutProgress(rules.cutDurationSeconds)` 시작(다른 지점으로 `Begin`이 다시 호출되면 진행 리셋 = 다른 상호작용으로 취소) — **잠금 대신** 이동 감지로 취소(스펙 "중간에 이동하면 취소"): Update에서 valid = 거리 ≤ cutRange && `owner.MovementState == Idle` && !owner.IsLocked && !owner.HandsBusy && point.IsUsable; Progressed → HUD 보조 안내 "전화선 끊는 중 {p:0}%"; Cancelled → `OnCutCancelled` + 안내; Completed → `point.SetUsable(false)` + `OnPhoneCut` + HUD "★ {point} 끊음 — 이 런 동안 사용 불가" (depends on T028, T029, T014)
- [X] T031 [US4] Implement `Assets/Scripts/Report/Runtime/PhoneCutInteractable.cs` — `RequireComponent(ReportPoint)`; IInteractable: `PromptText = "전화선 끊기 ({cutDurationSeconds}초)"`, `InteractionRange = rules.cutRange`; `CanInteract`: point.CanBeCut && point.IsUsable && !player.IsLocked && !player.HandsBusy && (도구 조건); `Interact`: `player.GetComponent<PlayerCutter>().Begin(point)` (depends on T030, T013)
- [X] T032 [US4] Wire retarget on cut in `Assets/Scripts/Report/Runtime/ReportFlow.cs` — `ReportPoint.OnUsabilityChanged` 구독 또는 Update 폴링(T021에 이미 폴링) 확인; 끊긴 전화기로 향하던 흐름이 다음 지점으로 바뀌고 없으면 포기 (depends on T031, T021)
- [X] T033 [US4] Add `PhoneCutInteractable` to phones and `PlayerToolkit`·`PlayerCutter` to players in `Assets/Editor/ReportSceneBuilder.cs`; `Assets/Scripts/Report/Debug/ReportTestConsole.cs` 구현 — `K`: P1 `PlayerToolkit.HasCuttingTool` 토글 + HUD, `O`: 가장 가까운 사용 가능 전화기 즉시 `SetUsable(false)`(디버그) (depends on T031, T023)

**Checkpoint**: E 유지로 끊기, 이동 시 취소, 끊기면 재타깃, 관리소는 안내 없음.

## Phase 7: US5 미리 끊어뒀다 (P2)
- [X] T034 [US5] Verify pre-cut path — `Assets/Scripts/Report/Runtime/ReportSystem.cs` `TryStart`에서 사용 가능한 지점 0 → 포기·섬 변화 없음(FR-007·시나리오 1); 지점 존재(Office) → 이동(시나리오 2); 흐름 없이 `OnReportAttempt`만 소비되고 개인 의심 3 유지 확인 (depends on T022)

## Phase 8: Polish
- [X] T035 [P] Update `Assets/Scripts/README.md` — `Report/` 행, `ReportPoint` 레벨 배치 안내, 입력 키
- [X] T036 [P] Update `specs/002-suspicion-system/contracts/runtime-api.md` §11 아래에 `report_completed` 추기
- [ ] T037 (에디터 수동) Run `specs/005-report-scenario/quickstart.md` + 002~004 회귀
- [X] T038 헌장 최종 점검 — Report 코드 수치 리터럴(허용: 라벨 오프셋·프리미티브 크기), 무기 없음, 에셋 0 → `specs/005-report-scenario/checklists/requirements.md` Notes

## Dependencies
Phase 1 → 2 → US1 → US2 → US3 → US4 → US5 → Polish. `ReportSystem.cs`(T014→T022→T024→T034), `ReportFlow.cs`(T021→T026→T032), `ReportSceneBuilder.cs`(T023→T033) 순차.

## Parallel
Phase 2: T005 ∥ T006 ∥ T007 ∥ T008 ∥ T011 ∥ T012 ∥ T013 · US1: T015 ∥ T016 ∥ T017 → T018 ∥ T019 ∥ T020 · US4: T027 ∥ T028 ∥ T029

## Implementation Strategy
MVP = US1 + US2(걸어가서 신고 완료). US3(004 통합) → US4(끊기) → US5(사전 차단). 수동: Test Runner·씬 메뉴·Play.

## Notes
- 005는 유형을 모른다. 침묵자 억제는 002 플래그(003이 설정)로 이미 처리된다.
- 스펙 Assumptions(관리소 끊기 불가, 지점 없으면 포기)는 clarify 항목.

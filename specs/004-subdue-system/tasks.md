---
description: "Task list for 004 제압 시스템 (Subdue System)"
---

# Tasks: 제압 시스템 (Subdue System)

**Input**: `/specs/004-subdue-system/`. 001·002·003 코드 필요.
**Tests**: 포함(원칙 III). 순수 로직 테스트를 구현 앞에.
**Organization**: US1 뒤에서 제압(P1) → US4 기절·운반(P1) → US5 대가(P1) → US2 밀치기(P2) → US3 물건(P2). 네임스페이스 `Project1028.Subdue`, 어셈블리 `Subdue`. 수치는 `subdue_rules.json`·002 사건 표.

---

## Phase 1: Setup
- [X] T001 Create folders `Assets/Scripts/Subdue/{Model,Runtime,Debug}`, `Assets/Tests/EditMode/Subdue`, `Assets/StreamingAssets/Subdue`
- [X] T002 Create `Assets/Scripts/Subdue/Subdue.asmdef` — references `["PlayFoundation","Suspicion","NpcTypes","Unity.InputSystem","UnityEngine.UI"]`
- [X] T003 [P] Create `Assets/Tests/EditMode/Subdue/Subdue.Tests.EditMode.asmdef` — references `["Subdue","NpcTypes","Suspicion","PlayFoundation","UnityEngine.TestRunner","UnityEditor.TestRunner"]`, nunit, UNITY_INCLUDE_TESTS
- [X] T004 [P] Edit `Assets/Editor/PlayFoundation.Editor.asmdef` — `"Subdue"` 참조 추가

## Phase 2: Foundational (가산 확장 + 데이터 + 골격)
- [X] T005 Extend 001 `Assets/Scripts/Player/ThirdPersonMotor.cs` — `public float SpeedMultiplier {get;set;}=1f`, `public bool SprintAllowed {get;set;}=true`; targetSpeed 계산에 배율 곱, `SprintAllowed`가 false면 sprint 무시(IsSprinting도 false)
- [X] T006 [P] Extend 001 `Assets/Scripts/Player/PlayerEntity.cs` — `public bool HandsBusy {get;set;}`; `Assets/Scripts/NPC/NpcTalkInteractable.cs` `CanInteract`에 `&& !player.HandsBusy`; `Assets/Scripts/Interaction/InteractionDetector.cs` `FindBest`를 `GetComponentsInParent<IInteractable>()`로 바꾸고 `Behaviour`인 경우 `isActiveAndEnabled`가 false면 건너뛴다(기절 시 비활성화된 말하기가 선택되는 것 방지)
- [X] T007 [P] Extend 002 `Assets/Scripts/Suspicion/Model/SuspicionRules.cs` — `SuspicionEventRule.ScopeGlobal="global"`; `Assets/Scripts/Suspicion/Model/SuspicionRulesValidator.cs` scope 허용 목록에 global 추가, global이고 personalDelta>0이면 Warning; `Assets/Scripts/Suspicion/Runtime/SuspicionSystem.cs` `CollectTargets`에 `case ScopeGlobal: break;`(대상 없음, islandDelta만 적용) + `public void ForceReportAttempt(NpcIdentity npc, PlayerEntity player)`(프로필 `SuppressReportAttempt`면 무시, 아니면 `RaiseReportAttempt`)
- [X] T008 [P] Edit 002 `Assets/StreamingAssets/Suspicion/suspicion_rules.json` — events 추가: `subdue_witnessed`(0/40, global, 확정, "§4.4 제압 장면 목격"), `unconscious_found_island`(0/30, global, 확정, "§4.4 기절자 발견"), `unconscious_wake`(3/0, target, ignoresTimeMultiplier, 확정, "§4.3 깨어남"), `shove_failed`(3/0, target, ignoresTimeMultiplier, 확정대기, "가정: 밀치기 실패 시 확신"), `noise_low`(1/0, radius 4, 확정대기), `noise_medium`(1/0, radius 8, 확정대기), `noise_high`(1/0, radius 14, 확정대기); 기존 `unconscious_found` note에 "004 발견 시 발생" 명시
- [X] T009 [P] Write `Assets/Tests/EditMode/Subdue/SubdueRulesValidatorTests.cs` — 정상→OK·PendingCount=2(shove·carry); actions 누락→Error; range 0→Error; backConeDegrees 200→Error; failChance 1.5→Error; maxSeconds<minSeconds→Error; speedMultiplier 1.0→Error; costs id가 002에 없음(콜백 false)→Error; status 누락→Error; noiseLevel 오타→Error
- [X] T010 [P] Implement `Assets/Scripts/Subdue/Model/SubdueDefinitions.cs` — enum `SubdueAction {Backstab, Shove, ObjectStrike}`, `HandState {Empty, OneHand, TwoHands}`, `HeldKind {None, Object, UnconsciousNpc}`, `NoiseLevel {Low, Medium, High}`, 한국어 이름 헬퍼
- [X] T011 [P] Implement `Assets/Scripts/Subdue/Model/SubdueRules.cs` — data-model 블록 그대로 `[Serializable]`; `ActionRule ForAction(SubdueAction)`
- [X] T012 Implement pure `Assets/Scripts/Subdue/Model/SubdueRulesValidator.cs` — `Validate(rules, Func<string,bool> eventExists) → SubdueValidationResult{IsError, Messages, PendingCount}` (depends on T010, T011)
- [X] T013 Implement `Assets/Scripts/Subdue/Model/SubdueRulesLoader.cs` — `TryLoad(SuspicionRules, out rules, out result)` 경로 `StreamingAssets/Subdue/subdue_rules.json` (depends on T012)
- [X] T014 [P] Create `Assets/StreamingAssets/Subdue/subdue_rules.json` — backstab {range 1.8, backConeDegrees 120, Low, noise_low, 확정}, shove {range 1.8, failChance 0.35, Medium, noise_medium, 확정대기}, objectStrike {range 2.0, High, noise_high, 확정}, unconscious {90, 180, 확정}, carry {speedMultiplier 0.45, 확정대기}, costs {subdue_witnessed, unconscious_found_island, unconscious_found, unconscious_wake, shove_failed, 확정}
- [X] T015 [P] Implement `Assets/Scripts/Subdue/Runtime/SubdueEvents.cs` — data-model 이벤트 8개 + internal Raise*
- [X] T016 Implement `Assets/Scripts/Subdue/Runtime/SubdueSystem.cs` 골격 — 싱글톤, Start에서 로드(SuspicionSystem 준비 필요), IsReady, Rules, HUD 경고·확정대기 건수; `Unconscious` 목록; Update 뼈대 `TickUnconscious()→ScanDiscovery()` (본문은 US4/US5) (depends on T013, T015)

## Phase 3: US1 뒤에서 제압 (P1) 🎯 MVP
### Tests
- [X] T017 [P] [US1] Write `Assets/Tests/EditMode/Subdue/HandRulesTests.cs` — 9조합: Backstab/Shove ×(Empty ok, OneHand 거부, TwoHands 거부), ObjectStrike ×(Empty 거부, Object 든 OneHand ok, Object 든 TwoHands ok); 기절자를 든 TwoHands → 세 동작 모두 거부; `CanPickUpObject(Empty)=true, (OneHand)=false`; `CanPickUpBody(Empty)=true, (OneHand)=false`; 거부 이유 문자열 비어 있지 않음
- [X] T018 [P] [US1] Write `Assets/Tests/EditMode/Subdue/BackConeCheckTests.cs` — cone 120: 정후방→true; 후방 59°→true; 후방 61°→false; 정면→false; 높이 차 무시
### Implementation
- [X] T019 [P] [US1] Implement pure `Assets/Scripts/Subdue/Model/HandRules.cs` — `static (bool ok, string reason) CanPerform(SubdueAction, HandState, HeldKind)`, `CanPickUpObject(HandState)`, `CanPickUpBody(HandState)`
- [X] T020 [P] [US1] Implement pure `Assets/Scripts/Subdue/Model/BackConeCheck.cs` — `static bool IsBehind(Vector3 targetPos, Vector3 targetForward, Vector3 actorPos, float coneDeg)` 수평 각도, -forward 기준
- [X] T021 [P] [US1] Implement `Assets/Scripts/Subdue/Runtime/PlayerHands.cs` — `RequireComponent(PlayerEntity)`; `State`, `HeldKind`, `HeldObject`, `HeldBody`; `TryPickUp(CarriableObject)`(HandRules, 물건을 손 위치 자식으로, 콜라이더 off, State=One/Two by IsTwoHanded), `TryPickUpBody(UnconsciousState)`(Empty만, 두 손, 어깨 위치, 모터 SpeedMultiplier=carry 값·SprintAllowed=false, body.CarriedBy=owner), `Drop()`(앞 바닥 1m, 복구), `HandsBusy = State==TwoHands` 동기화; `OnPickedUp/OnDropped`
- [X] T022 [P] [US1] Implement `Assets/Scripts/Subdue/Runtime/CarriableObject.cs` — `IInteractable`: `PromptText="들기"`, `InteractionRange=2.0`, `IsTwoHanded`(직렬화), `CanInteract`: 플레이어 `PlayerHands.State==Empty` && !IsLocked, `Interact`: `hands.TryPickUp(this)`. 무기 속성·내구도·살상 판정 없음(주석 명시)
- [X] T023 [US1] Implement `TrySubdue` in `Assets/Scripts/Subdue/Runtime/SubdueSystem.cs` — 순서: IsReady → `SubdueJudgement.Evaluate(target).CanSubdue` 아니면 Rejected("제압 불가: {Reason}", 물리 반응 없음) → 이미 기절이면 Rejected → 거리 ≤ rule.range → `HandRules.CanPerform` → 동작별: Backstab: `!vision.IsSeeing(actor)` && `BackConeCheck.IsBehind` 아니면 Rejected("인지 상태/후면 아님"); Shove: `ShoveRoll`(US2에서 확정, 여기선 항상 성공 스텁 금지 → US2 T029까지 Shove는 Rejected("미구현") 반환); ObjectStrike: 물건 필요(US3) → 성공 시 `ApplySubdue(actor, target, action)`: 목격자 집합(대상 제외·기절 제외·`IsSeeing(actor)`) → `CostLedger.RecordWitnesses`; 목격자 ≥1이면 `Raise(Witness(costs.witnessedEventId, actor))`… **global scope는 Actor만 필요** → `SuspicionEvent.At(id, actor, target.Position)`; 소음 `Raise(At(rule.noiseEventId, actor, target.Position))` + `OnNoise`; `UnconsciousState.Attach(target, actor, timer)`; `OnSubdued` (depends on T016, T019, T020, T021)
- [X] T024 [US1] Implement `Assets/Scripts/Subdue/Runtime/SubdueActor.cs` — `RequireComponent(PlayerEntity, PlayerHands)`; Update: 대상 = 카메라 전방 최근접 `NpcIdentity`(사거리 = max action range, `InteractableSelector`); `PlannedAction = SubdueSystem.PlanAction(owner, target)`(R-5: 물건 들면 ObjectStrike, 빈손이고 !IsSeeing && IsBehind면 Backstab, 아니면 Shove); HUD 두 번째 안내 줄(001 `RuntimeHud.ShowPrompt`는 한 줄이므로 `RuntimeHud`에 `ShowSecondaryPrompt(player, text)` 가산 추가 — `Assets/Scripts/UI/RuntimeHud.cs`): "{동작명} [F]" 또는 거부 이유; `Keyboard.current.fKey.wasPressedThisFrame`이고 !IsLocked면 `TrySubdue`; Rejected면 HUD Warn (depends on T023)
- [X] T025 [US1] Implement `Assets/Editor/SubdueSceneBuilder.cs` — 001·002·003 빌더 재사용(바닥·벽·빛·HUD·TimeOfDay·SuspicionSystem·패널·콘솔·NpcTypeSystem); NPC(talkable false, npcId 고유, 003 배정 필요 → `npc_types.json` assignments에 `sub_w1`(Watcher), `sub_w2`(Watcher), `sub_q`(Silent), `sub_v`(Watcher) 추가 — `Assets/StreamingAssets/NpcTypes/npc_types.json` 수정): `NPC_W1`(0,1,6) 정면 +Z(등 돌림), `NPC_W2`(6,1,4) 정면 -Z, `NPC_Q`(-6,1,4) -Z, `NPC_V`(0,1,14) -Z 목격자; 각 NPC `NpcSuspicionProfile`·`NpcMover`·`NpcSuspicionBehaviour`; 물건 `Crate_A`(Cube 0.5, (-2,0.25,0), 한 손), `Crate_B`(Cube 0.8, (2,0.4,0), 두 손) with `CarriableObject`+`PrimitiveTint`; P1(0,1,-6) + `PlayerHands`·`SubdueActor`, P2(12,1,12) 같은 컴포넌트; `SubdueSystem`, `SubdueDebugPanel`(US4); 저장 `Assets/Scenes/Test_Subdue.unity` (depends on T024)

**Checkpoint**: W1 뒤에서 F → 기절(누움)·소음 낮음. 물건 들고 F → 거부. Q → 불가.

## Phase 4: US4 기절·깨어남·운반 (P1)
### Tests
- [X] T026 [P] [US4] Write `Assets/Tests/EditMode/Subdue/UnconsciousTimerTests.cs` — roll 0→Duration=min; roll 1→max; roll 0.5→중간; Tick(dt) 누적 후 Remaining 감소; Remaining≤0→IsAwake; 음수 dt 무시; carried 플래그와 무관하게 Tick 진행
### Implementation
- [X] T027 [P] [US4] Implement pure `Assets/Scripts/Subdue/Model/UnconsciousTimer.cs` — `UnconsciousTimer(min, max, roll01)`, `Duration`, `Remaining`, `Tick(dt)`, `IsAwake`
- [X] T028 [US4] Implement `Assets/Scripts/Subdue/Runtime/UnconsciousState.cs` — `static UnconsciousState Attach(NpcIdentity, PlayerEntity subduer, UnconsciousTimer)`: 컴포넌트 추가, 비활성화 목록(`NpcVision`, `NpcMover`, `NpcSuspicionBehaviour`, `InformerBehaviour`, `SentinelBehaviour`, `NpcTalkInteractable`) enabled=false 저장, `NpcSuspicionProfile.PropagatesSuspicion` 원값 저장 후 false, 캡슐 회전 90°(눕힘), `UnconsciousCarryInteractable` 부착; `Timer`, `SubduedBy`, `CarriedBy`, `IsCarried`, `WitnessIds`(HashSet<string>); `Wake()`: 복구·세우기·Carry 해제(운반 중이면 `PlayerHands.Drop`)·`OnUnconsciousWake`·Destroy(this)·carry interactable 제거; `static bool IsUnconscious(NpcIdentity)`
- [X] T029 [P] [US4] Implement `Assets/Scripts/Subdue/Runtime/UnconsciousCarryInteractable.cs` — `IInteractable`: `PromptText="들어 옮기기"`, range 2.0, `CanInteract`: 플레이어 빈손 && !IsLocked && !IsCarried; `Interact`: `hands.TryPickUpBody(state)`
- [X] T030 [US4] Implement `TickUnconscious()` in `Assets/Scripts/Subdue/Runtime/SubdueSystem.cs` — 각 기절자 `Timer.Tick(Time.deltaTime)`(운반 중 포함, FR-014); `IsAwake`면: `Raise(SuspicionEvent.Target(costs.wakeEventId, subduer, npc))`(개인 3), 이미 3이었으면 `SuspicionSystem.ForceReportAttempt(npc, subduer)`; `state.Wake()`; 목록 제거 (depends on T028, T007)
- [X] T031 [US4] Implement `Assets/Scripts/Subdue/Debug/SubdueDebugPanel.cs` — 우측 고정 텍스트(001 `CreateFixedText` anchor (1,1)): 플레이어별 손 상태·든 것; 기절자별 "NPC / 제압자 / 남은 s / 운반자 / 목격자 n / 발견자 n"; 최근 이벤트 로그 6줄(SubdueEvents 구독); 키 안내 "E 들기/옮기기 · F 제압 · G 내려놓기 · T 배속" (depends on T016)
- [X] T032 [US4] Implement `G` drop in `Assets/Scripts/Subdue/Runtime/PlayerHands.cs` Update — `Keyboard.current.gKey.wasPressedThisFrame` && PlayerInput 있음 → `Drop()`; 001 `InteractionDetector`가 두 손 점유 시 대화 안내를 내지 않도록 `NpcTalkInteractable.CanInteract`의 HandsBusy 검사(T006) 확인 (depends on T021)

**Checkpoint**: 배속 후 90~180초에 깨어남 → 3·신고 시도. E로 들면 느림·달리기 불가·대화 불가, G로 내려놓음.

## Phase 5: US5 대가 (P1)
### Tests
- [X] T033 [P] [US5] Write `Assets/Tests/EditMode/Subdue/CostLedgerTests.cs` — `RecordWitnesses(body, {A,B})`→true(첫 기록), 재호출→false; `TryRecordDiscovery(body, A)`(목격자)→false; `(body, C)`→true; `(body, C)` 재→false; `(body2, C)`→true; 목격자 없이 발견만→true
### Implementation
- [X] T034 [P] [US5] Implement pure `Assets/Scripts/Subdue/Model/CostLedger.cs` — string 키(bodyId, observerId); `RecordWitnesses`, `TryRecordDiscovery`, `IsWitness`, `Clear(bodyId)`(깨어나면 정리)
- [X] T035 [US5] Implement `ScanDiscovery()` in `Assets/Scripts/Subdue/Runtime/SubdueSystem.cs` — 각 기절자 body(운반 중이면 운반자 위치 기준)에 대해 `NpcIdentity.All` 중 (body 제외, 기절 제외, `NpcVision` 있고 enabled) 관찰자: `VisionEvaluator.Evaluate(observer.EyePosition, observer.Forward, bodyCenter, vision.FovDegrees, vision.Range).InCone` && `!Physics.Linecast(eye, bodyCenter, mask)`(body·observer 자신 무시) → `Ledger.TryRecordDiscovery`가 true면 `Raise(At(costs.foundIslandEventId, subduer, body.Position))`(섬 +30) + `Raise(Target(costs.foundPersonalEventId, subduer, observer))`(개인 +3 확정대기) + `OnBodyDiscovered`; HUD 로그 (depends on T034, T028)
- [X] T036 [US5] Wire witness recording in `Assets/Scripts/Subdue/Runtime/SubdueSystem.cs` `ApplySubdue` (T023) — 목격자 id 집합을 `Ledger.RecordWitnesses(target.NpcId, ids)`에 기록하고 `UnconsciousState.WitnessIds`에 복사; 깨어날 때 `Ledger.Clear` (depends on T034, T030)

**Checkpoint**: V가 볼 때 제압 → +40 1회; 나중에 V가 기절자 발견 → +30 1회(목격했다면 0).

## Phase 6: US2 정면 밀치기 (P2)
### Tests
- [X] T037 [P] [US2] Write `Assets/Tests/EditMode/Subdue/ShoveRollTests.cs` — failChance 0 → roll 0.0도 성공; failChance 1 → roll 0.99도 실패; 0.35: roll 0.34 실패, 0.35 성공
### Implementation
- [X] T038 [P] [US2] Implement pure `Assets/Scripts/Subdue/Model/ShoveRoll.cs` — `static bool Succeeds(float failChance, float roll01) => roll01 >= failChance`
- [X] T039 [US2] Implement Shove branch in `Assets/Scripts/Subdue/Runtime/SubdueSystem.cs` `TrySubdue` — 인지 여부 무관, 빈손, 거리; `ShoveRoll.Succeeds(rule.failChance, UnityEngine.Random.value)`; 성공→`ApplySubdue`; 실패→소음(Medium) + `Raise(Target(costs.shoveFailEventId, actor, target))`(개인 3 확정대기) + `OnShoveFailed` + Result(Success=false, Reason="밀치기 실패") (depends on T038, T023)

**Checkpoint**: W2 정면 F 반복 → 성공·실패 섞임, 실패 시 3.

## Phase 7: US3 물건으로 기절 (P2)
- [X] T040 [US3] Implement ObjectStrike branch in `Assets/Scripts/Subdue/Runtime/SubdueSystem.cs` `TrySubdue` — `hands.HeldKind==Object` 필요(HandRules), 인지 무관, 성공 100%, 소음 High, 물건 유지(소모 없음) → `ApplySubdue` (depends on T023, T021)
- [X] T041 [US3] Verify `PlanAction` in `Assets/Scripts/Subdue/Runtime/SubdueSystem.cs` — 물건 들면 ObjectStrike, 기절자 들면 None(거부 이유 "두 손 점유"), 빈손: !IsSeeing && IsBehind → Backstab, 아니면 Shove (depends on T024, T040)

**Checkpoint**: Crate 들고 F → 기절·소음 높음, 물건 유지.

## Phase 8: Polish
- [X] T042 [P] Update `Assets/Scripts/README.md` — `Subdue/` 행, 가산 확장(001 모터·HandsBusy, 002 global scope·ForceReportAttempt·사건 7개), 입력 키
- [X] T043 [P] Update `specs/002-suspicion-system/contracts/runtime-api.md` §10에 004 가산 확장 추기
- [ ] T044 (에디터 수동) Run `specs/004-subdue-system/quickstart.md` §1~§4 + 002·003 회귀
- [X] T045 헌장 최종 점검 — `grep -rniE 'weapon|health|damage|kill|무기|체력|살상' Assets/Scripts/Subdue`에서 금지 선언 주석 외 0건; 수치 리터럴은 JSON 외 표현 파라미터만; 에셋 0 → `specs/004-subdue-system/checklists/requirements.md` Notes

## Dependencies
Phase 1 → 2 → US1 → US4 → US5 → US2 → US3 → Polish. `SubdueSystem.cs`(T016→T023→T030→T035→T036→T039→T040→T041) 순차.

## Parallel
Phase 2: T006 ∥ T007 ∥ T008 ∥ T009 ∥ T010 ∥ T011 ∥ T014 ∥ T015 · US1: T017 ∥ T018 → T019 ∥ T020 ∥ T021 ∥ T022 · US4: T026 ∥ T027 ∥ T029 · US5: T033 ∥ T034 · US2: T037 ∥ T038

## Implementation Strategy
MVP = US1 + US4 + US5(뒤에서 제압 → 깨어남·운반 → 대가). "제압은 미룬다, 그리고 비싸다"가 여기서 완성. 이후 US2·US3.

## Notes
- 원칙 V: `Weapon`·`Health`·`Damage` 타입·필드 금지. `CarriableObject`는 무게 등급만.
- 스펙 Assumptions의 해석(밀치기 실패 시 3, 정면 밀치기 목격 대가 없음)은 clarify 대상. 데이터 `shove_failed`는 확정대기.

---
description: "Task list for 001 플레이 기반 (Play Foundation)"
---

# Tasks: 플레이 기반 (Play Foundation)

**Input**: Design documents from `/specs/001-play-foundation/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/runtime-api.md, contracts/dialogue-schema.json, quickstart.md

**Tests**: 포함한다. 헌장 원칙 III과 Tasks 게이트("독립 테스트 태스크가 구현 태스크보다 앞서 배치")에 따라 각 스토리의
EditMode 테스트를 구현 앞에 둔다. 테스트는 먼저 작성해 컴파일 실패/빨간 상태를 확인한 뒤 구현한다.

**Organization**: 스펙의 User Story 1~4 순서(P1→P4). 각 스토리는 독립 검증 가능한 증분이다.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 병렬 가능(다른 파일, 미완료 태스크 의존 없음)
- **[Story]**: US1~US4
- 모든 경로는 저장소 루트 기준. 네임스페이스는 전부 `Project1028.PlayFoundation`.

## Path Conventions

Unity 단일 프로젝트. 런타임 `Assets/Scripts/`, 에디터 `Assets/Editor/`, 테스트 `Assets/Tests/EditMode/`, 데이터
`Assets/StreamingAssets/Dialogue/`, 씬 `Assets/Scenes/`. 자세한 트리는 plan.md §Project Structure.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: 폴더·어셈블리 정의·데이터 폴더. 신규 패키지 설치 없음(헌장 II).

- [X] T001 Create folders `Assets/Scripts/{Player,Npc,Dialogue,World,UI,Interaction}`, `Assets/Editor`, `Assets/Tests/EditMode`, `Assets/StreamingAssets/Dialogue` (빈 폴더는 `.gitkeep` 없이 첫 파일로 생성)
- [X] T002 Create runtime assembly definition `Assets/Scripts/PlayFoundation.asmdef` — name `PlayFoundation`, rootNamespace `Project1028.PlayFoundation`, references `["Unity.InputSystem"]`, autoReferenced true, noEngineReferences false. 기존 `Assets/Scripts/Player/ThirdPersonMotor.cs`가 이 어셈블리에 포함됨을 확인
- [X] T003 [P] Create editor assembly definition `Assets/Editor/PlayFoundation.Editor.asmdef` — name `PlayFoundation.Editor`, references `["PlayFoundation","Unity.InputSystem"]`, includePlatforms `["Editor"]`
- [X] T004 [P] Create test assembly definition `Assets/Tests/EditMode/PlayFoundation.Tests.EditMode.asmdef` — `references: ["PlayFoundation","UnityEngine.TestRunner","UnityEditor.TestRunner"]`, `precompiledReferences: ["nunit.framework.dll"]`, `overrideReferences: true`, `defineConstraints: ["UNITY_INCLUDE_TESTS"]`, `includePlatforms: ["Editor"]`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 모든 스토리가 의존하는 개체·이벤트·HUD·씬 빌더 골격.

**⚠️ CRITICAL**: 이 단계 완료 전에는 스토리 작업을 시작하지 않는다.

- [X] T005 Implement `Assets/Scripts/Player/PlayerEntity.cs` per contracts/runtime-api.md §1 — `Id`(string, 비어 있지 않음, 씬 내 유일: 중복 시 `Debug.LogError`), `MovementState` enum {Idle, Walking, Sprinting}, `IsLocked`, `Lock(object owner)`/`Unlock(object owner)`(소유자 HashSet, 비었을 때만 해제), `Position`, `Forward`, `Camera`(nullable), `ReceivedDestinations`(내부 List, 읽기 전용 노출) + `internal void AddDestination(Destination)` + `bool HasDestinationFrom(string npcId)`, `SpawnPosition`(Awake 시 기록), static `All`/`OnRegistered`/`OnUnregistered`(OnEnable/OnDisable 등록). Lock 변경 시 같은 GameObject의 `ThirdPersonMotor.SetMovementEnabled(!IsLocked)` 호출
- [X] T006 [P] Implement `Assets/Scripts/NPC/NpcIdentity.cs` per contracts §2 — `NpcId`(string, 비어 있지 않음), `DisplayName`, `EyeHeight`(float > 0, 기본 1.6), `Position`, `Forward`, `EyePosition = Position + up*EyeHeight`, static `All` 등록. 이동 로직 없음(FR-015, PrototypePlan 범위)
- [X] T007 [P] Implement `Assets/Scripts/UI/RuntimeHud.cs` per contracts §6 — 싱글톤 `Instance`; Awake에서 코드로 Canvas(ScreenSpaceOverlay, CanvasScaler 1920x1080) 생성; 내장 폰트 `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")`; 플레이어별 안내 Text(하단 중앙)·대화 패널(하단, 화자·줄·"n/total")·경고 로그(좌상단, 최근 5줄, 8초 후 소거) 생성; API `ShowPrompt`, `ShowDialogueLine`, `HideDialogue`, `Warn`(화면 + `Debug.LogWarning` + static `OnWarning` 이벤트). 신규 에셋 사용 금지
- [X] T008 Implement scene builder skeleton `Assets/Editor/PlayFoundationSceneBuilder.cs` — `[MenuItem("Tools/PROJECT 1028/Build Play Foundation Test Scene")]`와 `("... (2 Players)")` 두 진입점이 `Build(bool twoPlayers)` 호출; `EditorSceneManager.NewScene(EmptyScene, Single)`; Directional Light; Ground(Plane 스케일 4, 위치 0,0,0) ; Walls(Cube 4개: 두께 0.5, 높이 3, 바닥 가장자리 안쪽) + 시야 가림용 벽(Cube 4x3x0.5, 위치 (6,1.5,4)); RuntimeHud 빈 GameObject; `EditorSceneManager.SaveScene(scene, "Assets/Scenes/Test_PlayFoundation.unity")` 후 열기. 플레이어·NPC 배치는 이후 태스크가 이 파일에 추가

**Checkpoint**: 컴파일 통과, 메뉴로 빈 테스트 씬(바닥·벽·HUD)이 생성·저장된다.

---

## Phase 3: User Story 1 - 도보 이동과 카메라 (Priority: P1) 🎯 MVP

**Goal**: WASD 카메라 기준 이동·달리기·회전, 마우스 궤도 카메라(피치 제한·장애물 당김), 정지 감속, 낙하 복귀.

**Independent Test**: `Test_PlayFoundation` 씬에서 quickstart §3 US1 표 6단계. EditMode 테스트는 이 스토리에 순수
로직이 없어 없음(카메라·모터는 PlayMode 수동 검증).

### Implementation for User Story 1

- [X] T009 [US1] Adjust existing `Assets/Scripts/Player/ThirdPersonMotor.cs` — 네임스페이스 `Project1028.PlayFoundation` 추가; `MovementState CurrentState` 프로퍼티 노출(Idle: Speed<0.05, Sprinting: IsSprinting, 그 외 Walking); `Attack` 액션은 읽지 않음(헌장 V, 주석 명시); 기존 이동·회전·점프 동작 유지(FR-007). `GetCameraRelativeDirection`이 `PlayerEntity.Camera`가 있으면 그 transform을 우선 사용
- [X] T010 [P] [US1] Implement `Assets/Scripts/Player/OrbitCamera.cs` — 필드: `target`(Transform), `PlayerEntity owner`, `distance`(기본 4.5), `pivotHeight`(1.5), `yawSpeed`/`pitchSpeed`(도/픽셀), `pitchMin=-30`, `pitchMax=70`, `collisionMask`, `collisionRadius=0.25`; `PlayerInput`에서 `Look` 액션 델타 읽기(owner.IsLocked면 무시, FR-012); yaw 무제한·pitch 클램프(FR-004); pivot→희망 위치 `Physics.SphereCast`로 장애물에 맞으면 hit 거리-radius로 당김(FR-005); LateUpdate에서 위치·회전 적용
- [X] T011 [P] [US1] Implement `Assets/Scripts/Player/FallRespawn.cs` — `fallY=-5`; Update에서 `transform.position.y < fallY`이면 `CharacterController.enabled=false` → 위치를 `PlayerEntity.SpawnPosition`으로 → 다시 enable; `RuntimeHud.Warn("낙하 복귀")` (Edge case)
- [X] T012 [US1] Extend `Assets/Editor/PlayFoundationSceneBuilder.cs` — `CreatePlayer(string id, Vector3 pos, bool withInput)`: Capsule(콜라이더 제거) + `CharacterController`(height 2, radius 0.4, center y1) + `PlayerEntity`(Id) + `ThirdPersonMotor` + `FallRespawn`; `withInput`이면 `PlayerInput`(actions = `AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions")`, defaultActionMap "Player", notificationBehavior InvokeUnityEvents 대신 폴링이므로 기본값) + 새 Camera GameObject(`Camera`, `AudioListener`, `UniversalAdditionalCameraData` 자동, tag MainCamera) + `OrbitCamera`(target/owner 연결) 생성 후 `PlayerEntity.Camera` 세팅. P1은 (0,1,-8)에 입력 포함으로 배치. 기존 씬 MainCamera 미생성(EmptyScene)

**Checkpoint**: US1 quickstart 표 1~6 전부 통과. 여기서 멈춰 "자연스럽다" 관찰(SC-002) 가능.

---

## Phase 4: User Story 2 - NPC에게 말을 걸어 목적지를 받는다 (Priority: P2)

**Goal**: 근접 "말하기" 안내 → E → JSON 3~5줄 순차 표시(이동 잠금) → 종료 시 목적지 마커. 규격 위반 경고, 중복 마커 방지.

**Independent Test**: quickstart §3 US2 표 7단계 + EditMode `DialogueValidatorTests`, `InteractableSelectorTests`.

### Tests for User Story 2 (구현 전 작성, 컴파일 실패 확인) ⚠️

- [X] T013 [P] [US2] Write `Assets/Tests/EditMode/DialogueValidatorTests.cs` — 케이스: lines 0줄→`IsError=true`; 2줄→Error 아님·Messages에 "3~5줄" 포함(Warning); 3·4·5줄→Messages 비어 있음; 6줄→Warning; npcId 빈 문자열→Error; destination null→Error; destination.name 빈 문자열→Error; x가 NaN→Error
- [X] T014 [P] [US2] Write `Assets/Tests/EditMode/InteractableSelectorTests.cs` — 케이스: 후보 없음→null; 하나가 Range 밖→제외; 두 후보 중 카메라 전방 각도 작은 쪽 선택; 각도 동률(±1° 이내)이면 거리 가까운 쪽; 모두 Range 밖→null; 수평 각도만 사용(높이 차 무시)

### Implementation for User Story 2

- [X] T015 [P] [US2] Implement `Assets/Scripts/Interaction/IInteractable.cs` per contracts §3 — `PromptText`, `InteractionRange`, `WorldPosition`, `CanInteract(PlayerEntity)`, `Interact(PlayerEntity)`
- [X] T016 [P] [US2] Implement pure `Assets/Scripts/Interaction/InteractableSelector.cs` — `struct Candidate { Vector3 Position; float Range; object Payload; }`; `static Candidate? Pick(IReadOnlyList<Candidate>, Vector3 playerPos, Vector3 cameraForward)`; 규칙: 수평 거리 ≤ Range만, 카메라 전방(수평 투영)과 후보 방향의 각도 최소, 동률 1° 이내면 거리 최소. UnityEngine 의존은 Vector3/Mathf만
- [X] T017 [P] [US2] Implement `Assets/Scripts/Dialogue/DialogueData.cs` — `[Serializable] class DialogueData { string npcId; string[] lines; DestinationData destination; }`, `[Serializable] class DestinationData { string name; float x, y, z; }` (contracts/dialogue-schema.json과 필드명 일치, JsonUtility 호환)
- [X] T018 [P] [US2] Implement pure `Assets/Scripts/Dialogue/DialogueValidator.cs` — `class ValidationResult { bool IsError; List<string> Messages; }`; `static ValidationResult Validate(DialogueData)`; 규칙(data-model.md): npcId 필수(Error), lines null/0줄(Error), 3≤len≤5 아니면 Warning "§6.1 3~5줄 규격 위반: {n}줄", destination null/name 빈 값/좌표 비유한값(Error)
- [X] T019 [US2] Implement `Assets/Scripts/Dialogue/DialogueLoader.cs` — `static bool TryLoad(string npcId, out DialogueData data, out ValidationResult result)`; 경로 `Path.Combine(Application.streamingAssetsPath, "Dialogue", npcId + ".json")`; 파일 없음/파싱 예외→Error 메시지에 경로 포함; 성공 시 `DialogueValidator.Validate` 결과 반환 (depends on T017, T018)
- [X] T020 [P] [US2] Implement `Assets/Scripts/Dialogue/DialogueEvents.cs` per contracts §4 — static events `OnDialogueStarted`, `OnLineAdvanced`, `OnDialogueEnded`, `OnDestinationReceived` + 내부 `Raise*` 헬퍼
- [X] T021 [P] [US2] Implement `Assets/Scripts/World/Destination.cs` (record: Name, Position, SourceNpcId, Receiver) and `Assets/Scripts/World/DestinationMarker.cs` — `static DestinationMarker Spawn(Destination d)`: Cylinder(스케일 0.6,1.5,0.6) + 위에 작은 Sphere, 이름 `Marker_{Name}_{Receiver.Id}`; `Destination`을 보관. 마커는 프리미티브만(헌장 II)
- [X] T022 [US2] Implement `Assets/Scripts/Dialogue/DialogueRunner.cs` per contracts §4 — `PlayerEntity`당 1개; `TryBegin(NpcIdentity)`: `DialogueLoader.TryLoad` → Error면 `RuntimeHud.Warn` 후 false; Warning이면 Warn 후 진행; `owner.Lock(this)`, `StartedFrame=Time.frameCount`, LineIndex 0 표시(`RuntimeHud.ShowDialogueLine`), `OnDialogueStarted`; Update에서 `Interact.WasPressedThisFrame()`(Hold 인터랙션이 붙어 있으므로 WasPerformedThisFrame 금지)이고 `Time.frameCount != StartedFrame`이면 `Advance()`; 마지막 줄에서 Advance→`HideDialogue`, `owner.Unlock(this)`, `LastEndedFrame = Time.frameCount` 기록(public 읽기 전용), 목적지 전달: `owner.HasDestinationFrom(npc.NpcId)`이면 마커 미생성, 아니면 `DestinationMarker.Spawn` + `owner.AddDestination` + `OnDestinationReceived`; `OnDialogueEnded` (depends on T019, T020, T021, T005, T007)
- [X] T023 [US2] Implement `Assets/Scripts/Interaction/InteractionDetector.cs` per contracts §3 — `PlayerEntity`당 1개; `searchRadius=4`; Update: owner.IsLocked면 Current=null·프롬프트 숨김; 아니면 `Physics.OverlapSphere`로 `IInteractable` 수집 → `InteractableSelector.Pick`(카메라 forward는 `owner.Camera`가 있으면 그 transform.forward, 없으면 owner.Forward) → `CanInteract` 통과한 것만; `Current` 변경 시 `OnTargetChanged` + `RuntimeHud.ShowPrompt(owner, "{PromptText} [E]")`; `Interact.WasPressedThisFrame()`(WasPerformedThisFrame 금지)이고 Current≠null이며 `Time.frameCount != GetComponent<DialogueRunner>().LastEndedFrame`이면 `Current.Interact(owner)` — 대화 종료 프레임의 같은 입력으로 즉시 재시작되는 것을 막는다 (depends on T015, T016, T007, T022)
- [X] T024 [US2] Implement `Assets/Scripts/NPC/NpcTalkInteractable.cs` — `IInteractable` 구현; `PromptText="말하기"`, `InteractionRange=2.5`, `WorldPosition=transform.position`; `CanInteract`: player.IsLocked==false; `Interact`: `player.GetComponent<DialogueRunner>().TryBegin(GetComponent<NpcIdentity>())`. NPC 유형·의심 필드 금지(헌장 I) (depends on T006, T022)
- [X] T025 [P] [US2] Create sample data `Assets/StreamingAssets/Dialogue/npc_dock_worker.json` — contracts/dialogue-schema.json의 examples[0] 그대로(4줄, 목적지 "관리소" (18,0,24))
- [X] T026 [US2] Extend `Assets/Editor/PlayFoundationSceneBuilder.cs` — `CreateNpc(string npcId, string displayName, Vector3 pos, Vector3 facing)`: Capsule(콜라이더 유지, layer Default) + `NpcIdentity`(NpcId, DisplayName, EyeHeight 1.6) + `NpcTalkInteractable`; NPC_dock_worker를 (4,1,6), 정면 -Z(플레이어 스폰 방향)로 배치; 선택 규칙 Play 검증용 두 번째 NPC `npc_dock_worker_2`(같은 대화 파일 재사용은 불가하므로 NpcId는 `npc_dock_worker`로 두고 GameObject 이름만 다르게, (1.5,1,6) 배치); `CreatePlayer`에 `InteractionDetector`·`DialogueRunner` 추가 (depends on T012, T023, T024)

**Checkpoint**: EditMode 테스트 T013·T014 녹색, quickstart US2 표 1~7 통과.

---

## Phase 5: User Story 3 - NPC 뼈대: 위치·시야 방향·"보고 있는가" (Priority: P3)

**Goal**: 플레이어별 시야 판정(부채꼴·거리·가림) + 히스테리시스 + Gizmo·라벨. NPC 행동 변화 없음.

**Independent Test**: quickstart §3 US3 표 + EditMode `VisionEvaluatorTests`.

### Tests for User Story 3 (구현 전 작성) ⚠️

- [X] T027 [P] [US3] Write `Assets/Tests/EditMode/VisionEvaluatorTests.cs` — fov 90, range 10 기준: 정면 5m→InFov·InRange 모두 true; 44° 5m→InFov true; 46° 5m→InFov false; 180°(후면) 3m→InFov false; 정면 11m→InRange false; 정면 5m이되 높이 +3m→수평 판정으로 InFov true; HorizontalAngleDeg·Distance 값 검증(±0.01)

### Implementation for User Story 3

- [X] T028 [P] [US3] Implement pure `Assets/Scripts/NPC/VisionEvaluator.cs` — `struct VisionGeometryResult { bool InRange; bool InFov; float HorizontalAngleDeg; float Distance; }`; `static VisionGeometryResult Evaluate(Vector3 eyePos, Vector3 forward, Vector3 targetPos, float fovDeg, float range)`; 수평 투영 후 `Vector3.Angle`, `InFov = angle ≤ fovDeg/2`, `Distance`는 3D 거리
- [X] T029 [US3] Implement `Assets/Scripts/NPC/NpcVision.cs` per contracts §2 — 필드 `FovDegrees`(0<x≤180, 기본 100), `Range`(>0, 기본 10), `OcclusionMask`(기본 Default), `StableSeconds`(≥0, 기본 0.15); `Dictionary<PlayerEntity, VisionState>`(PlayerEntity.OnRegistered/OnUnregistered로 유지); Update: 각 플레이어에 대해 `VisionEvaluator.Evaluate(EyePosition, Forward, player.Position+up*1.0, ...)` → InFov&&InRange이면 `Physics.Linecast(EyePosition, targetCenter, OcclusionMask)`로 플레이어 자신 외 충돌이 없을 때 Raw=true(플레이어 콜라이더는 CharacterController이므로 hit.transform이 플레이어면 무시); Raw 변경 시 RawSince 갱신; `Raw≠Public && now-RawSince≥StableSeconds`면 Public 갱신 + `OnSeeingChanged`; `IsSeeing`, `IsSeeingAny`, `SeenPlayers`. NPC 행동·transform 변경 코드 금지(FR-018) (depends on T028, T006, T005)
- [X] T030 [P] [US3] Implement Gizmo in `Assets/Scripts/NPC/NpcVision.cs` `OnDrawGizmosSelected` — 눈 위치에서 fov 좌우 경계선(range 길이)과 호(선분 24개), 현재 SeenPlayers 있으면 색 빨강 아니면 노랑 (FR-019). 같은 파일 수정이므로 T029 완료 후 진행
- [X] T031 [P] [US3] Implement `Assets/Scripts/UI/VisionDebugLabel.cs` — NPC 머리 위(EyePosition+0.6)에 월드 좌표를 `Camera.main.WorldToScreenPoint`로 변환해 `RuntimeHud`가 관리하는 Text로 "{DisplayName}\nP1: 보고 있음 / P2: 안 보임" 형식 표시; `NpcVision.OnSeeingChanged`와 매 프레임 폴링 병행 (depends on T029, T007)
- [X] T032 [US3] Extend `Assets/Editor/PlayFoundationSceneBuilder.cs` `CreateNpc` — `NpcVision`(Fov 100, Range 10, StableSeconds 0.15) + `VisionDebugLabel` 추가; 가림 검증용 벽을 절대좌표 (4, 1.5, 2.5)(NPC 정면 3.5m 앞, T008의 (6,1.5,4)를 대체)로 재배치 (depends on T029, T031)

**Checkpoint**: T027 녹색, quickstart US3 표 6배치 통과, 5초 대기 시 NPC 무변화.

---

## Phase 6: User Story 4 - 코옵 동등성: 플레이어 개체 2개 (Priority: P4)

**Goal**: 두 번째 `PlayerEntity`(입력 없음)를 배치해 시야·잠금·상호작용이 개체별로 독립임을 검증.

**Independent Test**: quickstart §3 US4 표 3단계 (2 Players 씬).

### Implementation for User Story 4

- [X] T033 [US4] Extend `Assets/Editor/PlayFoundationSceneBuilder.cs` `Build(bool twoPlayers)` — `twoPlayers`면 `CreatePlayer("P2", NPC 후면 위치 (4,1,9), withInput:false)`; P2에는 `PlayerInput`·카메라 없음, `ThirdPersonMotor`·`InteractionDetector`·`DialogueRunner`·`FallRespawn`은 포함(입력 액션이 null이면 각 컴포넌트가 무시해야 함) (depends on T012, T026, T032)
- [X] T034 [US4] Harden input-null paths — `Assets/Scripts/Player/ThirdPersonMotor.cs`(이미 null 안전), `Assets/Scripts/Player/OrbitCamera.cs`, `Assets/Scripts/Interaction/InteractionDetector.cs`, `Assets/Scripts/Dialogue/DialogueRunner.cs`에서 `PlayerInput`이 없거나 액션이 null이면 입력 처리만 건너뛰고 상태 계산(프롬프트·잠금·시야 대상 등록)은 계속되도록 확인·수정 (FR-021, FR-022)
- [X] T035 [US4] Verify per-player independence in `Assets/Scripts/UI/RuntimeHud.cs` — 프롬프트·대화 패널이 `PlayerEntity`별 슬롯이며 P1 대화 중 P2 슬롯이 영향받지 않음; P2 슬롯은 화면 우측에 축소 표시(입력 없는 개체도 상태가 보이도록)

**Checkpoint**: 2 Players 씬에서 NPC 라벨이 "P1: 보고 있음 / P2: 안 보임"으로 갈리고, P1 대화 중 P2.IsLocked=false.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [X] T036 [P] Add `Assets/Scripts/Dialogue/DialogueLoader.cs` warning on files with `lines` outside 3~5 at scene start — `RuntimeHud.Start()`(Awake/OnEnable 등록 순서에 의존하지 않도록 Start 사용)에서 `NpcIdentity.All`을 순회하며 사전 로드·검증해 경고를 미리 표시(FR-014 조기 발견)
- [X] T037 [P] Write `Assets/Scripts/README.md` — 폴더 구조, 네임스페이스, contracts/runtime-api.md 링크, 테스트 씬 생성 메뉴, 후속 기능이 구독할 이벤트 목록
- [ ] T038 (에디터 수동) Run quickstart.md §1~§4 전체 검증 후 `specs/001-play-foundation/quickstart.md` §4 체크박스에 결과 기록(통과/실패 현상)
- [X] T039 Confirm 헌장 준수 최종 점검 — `grep -ri "suspicion\|faction\|의심\|진영\|weapon\|attack" Assets/Scripts`에서 Attack 미사용 주석 외 결과 0건; `Assets/` 하위에 .fbx/.png/.wav/.anim 신규 파일 0개; 결과를 `specs/001-play-foundation/checklists/requirements.md` Notes에 추가

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 Setup** → **Phase 2 Foundational** → 스토리 단계(3~6) → **Phase 7 Polish**
- Phase 2는 모든 스토리를 막는다(PlayerEntity·NpcIdentity·RuntimeHud·씬 빌더 골격).

### User Story Dependencies

- **US1 (P1)**: Phase 2 이후 즉시. 다른 스토리 의존 없음. MVP.
- **US2 (P2)**: Phase 2 이후. 씬 빌더 확장(T026)이 T012의 `CreatePlayer`를 확장하므로 US1의 T012 이후.
- **US3 (P3)**: Phase 2 이후. `NpcIdentity`만 필요. US2와 병렬 가능(T032는 T026의 `CreateNpc` 확장이므로 T026 이후).
- **US4 (P4)**: US1~US3 완료 후(씬 빌더 최종 형태 필요).

### Within Each Story

- 테스트(T013, T014, T027) → 순수 로직(T016, T018, T028) → MonoBehaviour → 씬 빌더 확장.
- 같은 파일(`PlayFoundationSceneBuilder.cs`, `NpcVision.cs`)을 건드리는 태스크는 순차.

### Parallel Opportunities

- Phase 1: T003 ∥ T004 (T002 이후)
- Phase 2: T006 ∥ T007 (T005와도 병렬 가능, 파일 다름)
- US1: T010 ∥ T011
- US2: T013 ∥ T014 → T015 ∥ T016 ∥ T017 ∥ T018 ∥ T020 ∥ T021 ∥ T025
- US3: T027 → T028 ∥ (T029 이후) T030 ∥ T031
- Polish: T036 ∥ T037

---

## Parallel Example: User Story 2

```bash
# 테스트 먼저 (병렬)
Task: "Write Assets/Tests/EditMode/DialogueValidatorTests.cs"
Task: "Write Assets/Tests/EditMode/InteractableSelectorTests.cs"

# 순수 로직·모델·이벤트 (병렬)
Task: "Implement Assets/Scripts/Interaction/IInteractable.cs"
Task: "Implement Assets/Scripts/Interaction/InteractableSelector.cs"
Task: "Implement Assets/Scripts/Dialogue/DialogueData.cs"
Task: "Implement Assets/Scripts/Dialogue/DialogueValidator.cs"
Task: "Implement Assets/Scripts/Dialogue/DialogueEvents.cs"
Task: "Implement Assets/Scripts/World/Destination.cs + DestinationMarker.cs"
Task: "Create Assets/StreamingAssets/Dialogue/npc_dock_worker.json"

# 이후 순차: T019 → T022 → T023 → T024 → T026
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Phase 1 → Phase 2 → Phase 3(US1)
2. **STOP and VALIDATE**: quickstart US1 표 6단계. "자연스럽다" 관찰 3명(SC-002).
3. 여기까지가 PrototypePlan 완료 기준 1이다.

### Incremental Delivery

1. + US2 → 완료 기준 3 "NPC와 대화해 목적지를 받는다" 충족 → 데모 가능
2. + US3 → 002 의심 시스템이 소비할 시야 입력 확보
3. + US4 → 헌장 원칙 IV 검증 완료
4. Polish → 헌장 최종 점검(T039) 후 `feature.json`을 002로 전환

### 에디터 수동 작업 (LLM이 직접 못 하는 것)

- Unity 에디터 열기·컴파일 대기, Test Runner 실행(quickstart §1), 씬 생성 메뉴 클릭(quickstart §2), Play 모드 수동
  시나리오(quickstart §3). 코드·JSON·asmdef·씬 빌더는 모두 `/speckit-implement`가 작성한다.

---

## Notes

- [P] = 다른 파일, 의존 없음. 같은 파일 수정 태스크는 [P] 없이 순차.
- 모든 스크립트 네임스페이스 `Project1028.PlayFoundation`, 어셈블리 `PlayFoundation`.
- 헌장 I: 의심·진영·위협 관련 타입·필드·문자열을 이 기능 코드에 넣지 않는다.
- 헌장 II: `Assets/`에 프리미티브·코드·JSON·asmdef·씬 외 파일 추가 금지.
- 헌장 V: `Attack` 액션 미사용. 무기·체력 타입 금지.
- 이 저장소는 git이 아니므로 커밋 대신 태스크 완료 시 체크박스를 갱신한다.

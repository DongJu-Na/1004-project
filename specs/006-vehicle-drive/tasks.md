---
description: "Task list for 006 차량 이동 (Vehicle Drive)"
---

# Tasks: 차량 이동 (Vehicle Drive)

**Input**: `/specs/006-vehicle-drive/`. 001 코드 필요(002 이후 불필요). **Tests**: 포함. 네임스페이스 `Project1028.Vehicle`, 어셈블리 `Vehicle`. 게임 규칙 코드 금지(기반 기능).
**Organization**: US1 탑승·운전·하차(P1) → US2 충돌 복구(P1) → US3 NPC 무해(P2) → US4 동승자(P3).

## Phase 1: Setup
- [X] T001 Create folders `Assets/Scripts/Vehicle/{Model,Runtime,Debug}`, `Assets/Tests/EditMode/Vehicle`, `Assets/StreamingAssets/Vehicle` (`Assets/Scripts/Vehicle/`는 기존 빈 폴더)
- [X] T002 Create `Assets/Scripts/Vehicle/Vehicle.asmdef` — references `["PlayFoundation","Unity.InputSystem","UnityEngine.UI"]` (Suspicion 참조 금지)
- [X] T003 [P] Create `Assets/Tests/EditMode/Vehicle/Vehicle.Tests.EditMode.asmdef` — references `["Vehicle","PlayFoundation","UnityEngine.TestRunner","UnityEditor.TestRunner"]`, nunit, UNITY_INCLUDE_TESTS
- [X] T004 [P] Edit `Assets/Editor/PlayFoundation.Editor.asmdef` — `"Vehicle"` 추가

## Phase 2: Foundational
- [X] T005 Extend 001 `Assets/Scripts/Player/PlayerEntity.cs` — `public bool IsInVehicle {get;set;}`; `Assets/Scripts/Interaction/InteractionDetector.cs` Update에서 `owner.IsLocked || owner.IsInVehicle`이면 Current=null; `Assets/Scripts/Player/OrbitCamera.cs` `public void SetTarget(Transform newTarget, float newDistance)`(target·distance 교체, currentDistance 즉시 갱신); `Assets/Scripts/Player/FallRespawn.cs` Update에서 `owner.IsInVehicle`이면 return(차량 리셋과 충돌 방지); `Assets/Scripts/Player/ThirdPersonMotor.cs` Update에서 `!characterController.enabled`이면 return
- [X] T006 [P] Write `Assets/Tests/EditMode/Vehicle/VehicleParamsValidatorTests.cs` — 정상→OK; maxSpeed 0→Error; steerSpeedFalloff 1.5→Error; lateralGrip -0.1→Error; suspensionLength 0→Error; exitMaxSpeed -1→Error; flipUpThreshold 2→Error; flipRecoverSeconds -1→Error
- [X] T007 [P] Implement `Assets/Scripts/Vehicle/Model/VehicleParams.cs` — `[Serializable]` 필드 data-model 표 그대로
- [X] T008 Implement pure `Assets/Scripts/Vehicle/Model/VehicleParamsValidator.cs` — `Validate(params) → VehicleValidationResult{IsError, Messages}` (depends on T007)
- [X] T009 Implement `Assets/Scripts/Vehicle/Model/VehicleParamsLoader.cs` — `TryLoad(out params, out result)` 경로 `StreamingAssets/Vehicle/vehicle_params.json`; 실패 시 기본값 인스턴스 반환 + 경고(차량은 그래도 동작) (depends on T008)
- [X] T010 [P] Create `Assets/StreamingAssets/Vehicle/vehicle_params.json` — version "0.1", maxSpeed 14, reverseMaxSpeed 5, acceleration 18, brakeForce 30, steerTorque 6, steerSpeedFalloff 0.6, lateralGrip 0.85, suspensionLength 0.6, suspensionSpring 60, suspensionDamper 6, enterRange 3, exitMaxSpeed 1.5, flipUpThreshold 0.2, flipRecoverSeconds 3, resetY -5, movingSpeedThreshold 0.3
- [X] T011 [P] Implement `Assets/Scripts/Vehicle/Runtime/VehicleEvents.cs` + `Assets/Scripts/Vehicle/Runtime/VehicleState.cs` — `enum SeatKind {Driver, Passenger}`, `struct VehicleState {Speed, IsMoving, EngineOn, DriverId, PassengerId, IsFlipped}`, 이벤트 5개 + Raise*

## Phase 3: US1 타고, 운전하고, 내린다 (P1) 🎯
### Tests
- [X] T012 [P] [US1] Write `Assets/Tests/EditMode/Vehicle/SeatAssignmentTests.cs` — (false,false)→Driver; (true,false)→Passenger; (false,true)→Driver; (true,true)→null
- [X] T013 [P] [US1] Write `Assets/Tests/EditMode/Vehicle/ExitRuleTests.cs` — speed 0, max 1.5→true; 1.5→true; 1.51→false; 음수 속도(후진)는 절댓값 사용 → -1→true, -2→false
### Implementation
- [X] T014 [P] [US1] Implement pure `Assets/Scripts/Vehicle/Model/SeatAssignment.cs` — `static SeatKind? PickSeat(bool driverOccupied, bool passengerOccupied)`
- [X] T015 [P] [US1] Implement pure `Assets/Scripts/Vehicle/Model/ExitRule.cs` — `static bool CanExit(float speed, float maxExitSpeed) => Mathf.Abs(speed) <= maxExitSpeed`
- [X] T016 [US1] Implement `Assets/Scripts/Vehicle/Runtime/VehicleController.cs` — `RequireComponent(Rigidbody)`; Unity 6 API `rb.linearVelocity`/`angularVelocity` 사용(`velocity` 폐기); 싱글톤 `Instance`(차량 1대, FR-001); Awake: 파라미터 로드, `rb.centerOfMass` 낮게(-0.3), 스폰 위치·회전 저장; 서스펜션 4점(로컬 오프셋 ±0.9, ±1.4)에서 `Physics.Raycast(down, suspensionLength)` → 스프링·댐퍼 힘 `AddForceAtPosition`; `IsGrounded` = 접지점 ≥ 2; 입력 `SetInput(throttle, steer, brake)`; FixedUpdate: 접지 시 전진력 = throttle*acceleration(속도 상한 max/reverseMax), 조향 토크 = steer*steerTorque*(1 - falloff*speed/max)*sign(전진), 제동 = 전진 속도 반대 힘, 측면 속도 × (1-lateralGrip) 감쇠; `Speed`(전진 방향 부호 속도), `EngineOn`(외부 set), `Recover()`(US2), `ResetToSpawn()`(US2), `State` 스냅샷; 바퀴 Cylinder 자식 회전 시각화 (depends on T009, T011)
- [X] T017 [US1] Implement `Assets/Scripts/Vehicle/Runtime/VehicleSeats.cs` — 좌석 Transform 2개(운전석 로컬 (-0.5,0.9,0.2), 동승석 (0.5,0.9,0.2)); `Driver`, `Passenger`; `TryEnter(p)`: `SeatAssignment.PickSeat`→ null이면 HUD "좌석이 모두 찼다" false; 성공: `p.IsInVehicle=true`, `CharacterController.enabled=false`, `ThirdPersonMotor.SetMovementEnabled(false)`, `p.transform.SetParent(seat, false)` 원점, `p.Camera?.SetTarget(vehicle.transform, 7f)`, Driver면 `controller.EngineOn=true`; `OnEntered`; `TryExit(p)`: `ExitRule.CanExit(controller.Speed, exitMaxSpeed)` 아니면 `OnExitDenied` + HUD "속도를 줄여야 내릴 수 있다" false; 성공: 좌석 측면 1.6m·바닥 높이(Raycast로 y 보정, 기본 1) 위치, 부모 해제, CharacterController on, 모터 on, `IsInVehicle=false`, 카메라 `SetTarget(p.transform, 4.5f)`, Driver였으면 `EngineOn=false` + 입력 0; `OnExited`; `static bool IsInVehicle(p)` (depends on T014, T015, T016, T005)
- [X] T018 [P] [US1] Implement `Assets/Scripts/Vehicle/Runtime/VehicleEnterInteractable.cs` — IInteractable "탑승"; `InteractionRange = params.enterRange`; `CanInteract`: !IsLocked && !IsInVehicle && !HandsBusy && 빈 좌석 있음; `Interact`: `seats.TryEnter(player)` (depends on T017)
- [X] T019 [US1] Implement `Assets/Scripts/Vehicle/Runtime/VehicleDriverInput.cs` — 차량 컴포넌트; Update: Driver의 `PlayerInput` 액션(Move·Sprint·Jump·Interact) 폴링 → `controller.SetInput(move.y, move.x, sprintPressed?1:0)`; Interact `WasPressedThisFrame` → `seats.TryExit(driver)`; Jump → `controller.Recover()`(뒤집힘 여부 무관 허용, US2); Passenger의 Interact → `seats.TryExit(passenger)`; Passenger의 Move는 무시(FR-007); 운전자 없으면 입력 0 (depends on T017)
- [X] T020 [US1] Implement `Assets/Editor/VehicleSceneBuilder.cs` — 001 빌더 재사용(바닥·벽·빛·HUD·P1 입력·P2 (10,1,10) 입력 없음); 차량 `Truck`: Cube 차체 (2.2,1.0,4.2) at (5,1,0) + `Rigidbody`(mass 1200, drag 0.2, angularDrag 1.5, interpolate) + `BoxCollider`(자동) + 바퀴 Cylinder 4개(자식, 시각용, 콜라이더 제거) + `VehicleController`·`VehicleSeats`·`VehicleEnterInteractable`·`VehicleDriverInput`·`PrimitiveTint`; `Ramp`: Cube (6,0.5,8) 회전 x -15° at (-8,1,8); `Bump`: Cube (4,0.4,1) at (0,0.2,12); NPC 1(`veh_npc`, talkable false, 001만: NpcIdentity·NpcVision·라벨) at (12,1,-6); `VehicleDebugPanel`; 저장 `Assets/Scenes/Test_Vehicle.unity` (depends on T019, T018)

**Checkpoint**: E 탑승 → 주행 → 정지 후 E 하차. 고속 하차 거부.

## Phase 4: US2 부딪혀도 조작이 깨지지 않는다 (P1)
### Tests
- [X] T021 [P] [US2] Write `Assets/Tests/EditMode/Vehicle/FlipDetectorTests.cs` — threshold 0.2, 3초: upY 1.0 → false·IsFlipped false; upY -0.5 2.9초 누적 → false; 3.0초 → true(1회 반환)·IsFlipped; 다시 upY 1.0 → Reset·false; upY 0.1(경계 아래) 지속 → true
### Implementation
- [X] T022 [P] [US2] Implement pure `Assets/Scripts/Vehicle/Model/FlipDetector.cs` — `FlipDetector(threshold, seconds)`, `bool Tick(float upY, float dt)`(임계 이하 누적이 seconds 도달 순간 1회 true), `IsFlipped`, `Reset()`
- [X] T023 [US2] Implement recovery & reset in `Assets/Scripts/Vehicle/Runtime/VehicleController.cs` — FixedUpdate에서 `flip.Tick(transform.up.y, dt)` true면 `Recover()`; `Recover()`: 위치 +0.8y, 회전 = yaw만 유지, velocity/angularVelocity 0, `OnRecovered`, HUD; `transform.position.y < resetY`면 `ResetToSpawn()`: 스폰 위치·회전, 속도 0, `OnReset`(탑승자는 좌석 자식이므로 함께 이동), HUD "차량 경계 이탈 → 복귀"; 충돌 후에도 입력 처리 경로에 상태 의존 없음 확인(FR-011) (depends on T022, T016)
- [X] T024 [US2] Verify exit-in-any-pose in `Assets/Scripts/Vehicle/Runtime/VehicleSeats.cs` — 뒤집힌 상태에서도 속도 조건만 보고 하차 허용, 하차 위치는 Raycast 바닥 보정(뒤집힘 시 차체 위로 나오지 않게 좌우 2.2m로 확장) (depends on T017)

**Checkpoint**: 벽 충돌 후 후진, 뒤집힘 3초 후 복구/Space 즉시, 낙하 리셋.

## Phase 5: US3 차량은 NPC를 해치지 않는다 (P2)
- [X] T025 [US3] Implement NPC contact damping in `Assets/Scripts/Vehicle/Runtime/VehicleController.cs` — `OnCollisionEnter/Stay`: 상대에 `NpcIdentity`가 있으면 `rb.linearVelocity *= 0.3f`(정지/튕김), NPC transform·상태 변경 코드 없음(주석으로 FR-015·FR-016·원칙 V 명시); NPC에는 Rigidbody를 붙이지 않는다(정적 콜라이더) — `Assets/Editor/VehicleSceneBuilder.cs`에서 확인 (depends on T016, T020)

**Checkpoint**: 돌진해도 NPC 위치 불변.

## Phase 6: US4 동승자도 탈 수 있다 (P3)
- [X] T026 [US4] Implement `Assets/Scripts/Vehicle/Debug/VehicleDebugPanel.cs` — 우상단 고정 텍스트: Speed·IsMoving·EngineOn·Grounded·Flipped·Driver·Passenger·최근 로그(VehicleEvents); 키 안내 "E 탑승/하차 · W/S A/D · Shift 제동 · Space 복구 · V P2 동승 토글(디버그)"; `V` 키: P2가 있으면 `seats.TryEnter(P2)`/`TryExit(P2)` 토글(P2는 입력이 없으므로 검증용) (depends on T017, T011)
- [X] T027 [US4] Verify passenger independence in `Assets/Scripts/Vehicle/Runtime/VehicleDriverInput.cs` — Passenger의 PlayerInput이 있어도 Move/Sprint 무시, Interact만 하차; Driver 하차 후 Passenger 유지·EngineOn=false; 세 번째 탑승 시도 → "좌석이 모두 찼다" (depends on T019, T017)

**Checkpoint**: V로 P2 동승, P2만 하차, P1 계속 운전.

## Phase 7: Polish
- [X] T028 [P] Update `Assets/Scripts/README.md` — `Vehicle/` 행(기존 빈 폴더 사용), 노출 상태·입력, 002/007이 읽는 방법
- [X] T029 [P] Update `specs/001-play-foundation/contracts/runtime-api.md` — 가산 표면(`IsInVehicle`, `OrbitCamera.SetTarget`, `HandsBusy`(004), `SpeedMultiplier/SprintAllowed`(004), `ShowSecondaryPrompt`(004), `CreateFixedText`(002)) 추기
- [ ] T030 (에디터 수동) Run `specs/006-vehicle-drive/quickstart.md`
- [X] T031 헌장 최종 점검 — `Vehicle` asmdef가 Suspicion·NpcTypes·Subdue·Report를 참조하지 않음; 코드에 noise/suspicion/encounter 식별자 0; 에셋 0 → `specs/006-vehicle-drive/checklists/requirements.md` Notes

## Dependencies
Phase 1 → 2 → US1 → US2 → US3 → US4 → Polish. `VehicleController.cs`(T016→T023→T025), `VehicleSeats.cs`(T017→T024), `VehicleDriverInput.cs`(T019→T027) 순차.

## Parallel
Phase 2: T006 ∥ T007 ∥ T010 ∥ T011 · US1: T012 ∥ T013 → T014 ∥ T015 · US2: T021 ∥ T022

## Implementation Strategy
MVP = US1 + US2(PrototypePlan 완료 기준 2). 이어 US3(원칙 V), US4(원칙 IV).

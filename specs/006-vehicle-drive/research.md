# Research: 차량 이동

## R-1. 주행 물리
- **Decision**: Rigidbody + 박스 콜라이더 + 4점 레이캐스트 서스펜션(스프링·댐퍼) 아케이드 제어. 접지 시에만 전진력·조향 토크, 측면 마찰 감쇠로
  미끄러짐 억제. 바퀴는 시각용 Cylinder(회전만).
- **Rationale**: WheelCollider는 질량·서스펜션·마찰 곡선 튠이 필요하고 프리미티브 씬에서 튐이 잦다. PrototypePlan 완료 기준 2("충돌 후 조작이 깨지지
  않는다")에는 단순한 힘 기반 제어가 더 예측 가능하다. 경사·둔덕은 레이캐스트 서스펜션이 처리한다.
- **Alternatives**: WheelCollider(com.unity.modules.vehicles 존재) — 사실감은 높지만 튠 비용. 기각. CharacterController 이동 — 충돌 물리 없음. 기각.

## R-2. 입력 매핑
- **Decision**: 기존 액션 재사용. 운전자: Move.y 가속/후진, Move.x 조향, Sprint 제동, Jump 뒤집힘 복구, Interact 하차. 동승자: Interact 하차만. 탑승은
  001 `IInteractable`("탑승 [E]"). 액션 에셋은 수정하지 않는다.

## R-3. 탑승 중 플레이어 상태
- **Decision**: `PlayerEntity.IsInVehicle`(가산 bool). 탑승 시 `CharacterController.enabled=false`, `ThirdPersonMotor.SetMovementEnabled(false)`, 플레이어를 좌석
  트랜스폼 자식으로, `InteractionDetector`는 `IsInVehicle`이면 안내 없음(FR-004). `Lock`은 쓰지 않는다 — 카메라 회전은 허용해야 하므로(FR-009).
- **Rationale**: Lock은 카메라까지 잠근다. 대화(001)는 탑승 중 안내가 뜨지 않아 자연히 차단된다.

## R-4. 카메라
- **Decision**: 001 `OrbitCamera.SetTarget(transform, distance)` 가산. 탑승 시 차량(거리 7), 하차 시 플레이어(거리 4.5)로 되돌린다. 조향은 Move.x이므로
  마우스 회전은 차량 진행에 영향 없음(FR-009).

## R-5. 하차 위치·속도 제한
- **Decision**: 순수 `ExitRule.CanExit(speed, maxExitSpeed)`. 고속이면 거부 + 안내(FR-005). 하차 위치는 좌석 쪽 측면 1.6m, 바닥 높이로 보정. 운전자가 내리면
  엔진 Off(입력 없음 → 관성으로 감속), 동승자는 남는다(Edge case).

## R-6. 뒤집힘 복구·경계 리셋
- **Decision**: 순수 `FlipDetector(thresholdUpY, seconds)`: `transform.up.y < threshold`가 seconds 지속되면 뒤집힘. 복구 = 위로 0.8m 올리고 yaw만 유지해
  세움, 속도 0. Jump로 즉시 복구도 가능. `transform.position.y < resetY`면 차량·탑승자를 스폰으로 리셋(FR-014).

## R-7. NPC 무해
- **Decision**: 001 NPC는 Rigidbody가 없는 캡슐 콜라이더(정적)라 차량이 밀어내지 못한다. 접촉 시 차량 속도를 감쇠(OnCollisionEnter에서 NpcIdentity면 속도 30%)
  하여 "멈추거나 튕김". NPC 위치·상태 변경 코드는 없다(FR-015·FR-016). 기절 등은 존재하지 않는다.

## R-8. 파라미터 파일
- **Decision**: 기반 기능이지만 튠 값을 `vehicle_params.json`으로 분리(001·002 관행). `status` 필드는 두지 않는다(SDD 규칙이 아님).

## R-9. 노출 상태
- **Decision**: `VehicleState { Speed, IsMoving, EngineOn, DriverId, PassengerId, IsFlipped }` + `VehicleEvents.OnEntered/OnExited/OnRecovered/OnReset/OnStateChanged`.
  002 소음·007 인카운터가 읽기만 한다.

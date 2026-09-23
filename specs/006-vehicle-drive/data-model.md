# Data Model: 차량 이동

## VehicleParams (JSON)
| 필드 | 규칙 |
|---|---|
| maxSpeed > 0, reverseMaxSpeed > 0 | m/s |
| acceleration > 0, brakeForce > 0 | 힘 계수 |
| steerTorque > 0, steerSpeedFalloff 0~1 | 고속에서 조향 감쇠 |
| lateralGrip 0~1 | 측면 속도 감쇠 |
| suspensionLength > 0, suspensionSpring > 0, suspensionDamper ≥ 0 | 레이캐스트 서스펜션 |
| enterRange > 0 | 탑승 안내 거리 |
| exitMaxSpeed ≥ 0 | 하차 허용 속도 |
| flipUpThreshold -1~1, flipRecoverSeconds ≥ 0 | 뒤집힘 판정 |
| resetY | 경계 낙하 리셋 높이 |
| movingSpeedThreshold ≥ 0 | IsMoving 판정 |

## 열거형
`SeatKind { Driver, Passenger }`

## 순수 로직
- `SeatAssignment.PickSeat(bool driverOccupied, bool passengerOccupied) → SeatKind?`(운전석 우선, 둘 다 차면 null)
- `ExitRule.CanExit(float speed, float maxExitSpeed) → bool`
- `FlipDetector(threshold, seconds)`: `Tick(upY, dt) → bool flippedNow`, `IsFlipped`, `Reset()`

## 런타임
- `VehicleController`(Rigidbody): 입력값(throttle, steer, brake)을 `VehicleDriverInput`이 넣는다. `Speed`, `IsGrounded`, `Recover()`, `ResetToSpawn()`, `EngineOn`(운전자 있음).
- `VehicleSeats`: `Driver`, `Passenger`(PlayerEntity), 좌석 Transform 2개, `TryEnter(player)`, `TryExit(player)`, 카메라 재타깃, `PlayerEntity.IsInVehicle` 설정.
- `VehicleEnterInteractable`: IInteractable "탑승 [E]"; `CanInteract`: 빈 좌석 있음 && !IsLocked && !IsInVehicle && !HandsBusy.
- `VehicleDriverInput`: 좌석별 PlayerInput 읽기; 운전자만 제어; 동승자·운전자 Interact → 하차; Jump → 복구.
- `VehicleState`: 스냅샷 struct.

## 이벤트 (VehicleEvents)
- `OnEntered(PlayerEntity, SeatKind)`, `OnExited(PlayerEntity, SeatKind)`, `OnRecovered()`, `OnReset()`, `OnExitDenied(PlayerEntity, string reason)`

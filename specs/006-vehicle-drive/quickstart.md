# Quickstart: 차량 이동 검증

## 1. 자동 테스트
EditMode `Vehicle.Tests.EditMode` 4묶음: SeatAssignment, ExitRule, FlipDetector, VehicleParamsValidator.

## 2. 씬
`Tools > PROJECT 1028 > Build Vehicle Test Scene`. Truck(차량 1대), Ramp(경사), Bump(둔덕), NPC 1, P1·P2. 우측 상단 차량 패널.

## 3. 시나리오
| US | 조작 | 기대 |
|---|---|---|
| 1 | 차량 옆에서 E | 운전석 탑승, 카메라가 차량 궤도. W/S 가속·후진, A/D 조향, Shift 제동. 마우스는 카메라만. 정지 후 E → 차량 옆에 하차·도보 복귀. 고속 E → "속도를 줄여야 함" 거부 |
| 2 | 벽에 정면 충돌 → S | 후진 정상. Ramp에서 뒤집기 → 몇 초 후 자동 복구(또는 Space). 정지 상태면 어떤 자세든 E 하차. 바닥 밖 → 시작 위치 복귀 |
| 3 | NPC로 돌진 | 차량이 멈춤/튕김, NPC 위치·상태 불변(001 라벨·위치 확인). 밀어내기 불가 |
| 4 | P2를 동승석에(디버그 키 `V`: P2 탑승/하차 토글) | 패널 Passenger=P2, P2 조작 입력은 차량에 영향 없음(P2 입력 없음), P2만 하차 후 P1 계속 운전 |

## 4. 성공 기준
- [ ] SC-001 60초 내 탑승·주행·하차 · [ ] SC-002 충돌·뒤집힘·둔덕 30/30 조작 재개 · [ ] SC-003 NPC 변화 0 · [ ] SC-004 동승 독립 · [ ] SC-006 에셋 0

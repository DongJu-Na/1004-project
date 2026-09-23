# Implementation Plan: 차량 이동 (Vehicle Drive)

**Branch**: `006-vehicle-drive` | **Date**: 2026-09-23 | **Spec**: [spec.md](spec.md)

## Summary
차량 1대(운전석·동승석)의 탑승·하차·운전·충돌 복구를 **기반 기능**으로 만든다. Rigidbody 아케이드 제어(레이캐스트 서스펜션 4점, 전진·후진·
조향·제동)로 WheelCollider 튠 없이 프리미티브만으로 안정적인 주행을 얻는다. 탑승 시 001 궤도 카메라를 차량으로 재타깃하고 도보 조작을
끈다. 차량은 NPC를 밀거나 다치게 하지 않는다(NPC는 정적 콜라이더). 씬 밖 낙하·뒤집힘은 자동 복구한다. 속도·좌석·엔진 상태만 외부에 노출하고
소음·인카운터 규칙은 적용하지 않는다(002·007이 읽는다).

## Technical Context
**Language/Version**: C# (Unity 6000.6.2f1)
**Primary Dependencies**: `PlayFoundation`(PlayerEntity·OrbitCamera·ThirdPersonMotor·IInteractable·RuntimeHud), com.unity.modules.physics(Rigidbody·Raycast), Input System(Move·Sprint·Jump·Interact 액션 재사용), uGUI
**Storage**: `Assets/StreamingAssets/Vehicle/vehicle_params.json`(속도·가속·조향·제동·서스펜션·탑승 거리·하차 최대 속도·뒤집힘 복구 시간·경계 Y)
**Testing**: EditMode 순수 4묶음(`SeatAssignment`, `ExitRule`, `FlipDetector`, `VehicleParamsValidator`)
**Project Type**: 어셈블리 `Vehicle → PlayFoundation`(002 이후에 의존하지 않음 — 기반 기능)
**Constraints**: 차량 1대, 게임 규칙 코드 금지(소음→의심, 검문, 인카운터 확률 없음), 신규 에셋 0(Cube 차체 + Cylinder 바퀴 4개 시각용), 가산 수정 `PlayerEntity.IsInVehicle`·`InteractionDetector`(탑승 중 안내 없음)·`OrbitCamera.SetTarget`.
**Scale/Scope**: 스크립트 약 12개, JSON 1개, 테스트 4개, 씬 빌더 1개.

## Constitution Check
| 원칙 | 판정 | 근거 |
|---|---|---|
| I. 분류 | PASS | "기반 기능". 근거 PrototypePlan 2단계·완료 기준 2. 게임 규칙 요소 미포함 선언. `Vehicle` 어셈블리는 `Suspicion`을 참조하지 않는다. |
| I. 규칙 혼입 금지 | PASS | 소음 사건·검문·인카운터 확률 코드 없음. `VehicleState.EngineOn/IsMoving/Speed`만 노출. |
| 근거 문서 § 범위 밖 | PASS | 복수 차량·래그돌·섬 규모 맵·최종 아트 미포함. |
| II. 에셋 | PASS | 프리미티브. |
| III. 독립 테스트 | PASS | 순수 4묶음 + 전용 씬 `Test_Vehicle`(001만 의존). |
| IV. 코옵 | PASS | 좌석은 개체 단위, 동승자 입력은 차량에 영향 없음, 독립 하차. |
| V. 폐기 항목 | PASS | NPC 무해: 정적 콜라이더에 막힘·튕김만. 차량 충돌 피해·기절 없음. |

**Gate**: 위반 없음.

## Project Structure
```text
Assets/Scripts/Vehicle/
├── Vehicle.asmdef                      # refs: PlayFoundation, Unity.InputSystem, UnityEngine.UI
├── Model/
│   ├── VehicleParams.cs / VehicleParamsValidator.cs / VehicleParamsLoader.cs
│   ├── SeatAssignment.cs               # 순수: 빈 좌석 배정(운전석 우선), 점유 규칙
│   ├── ExitRule.cs                     # 순수: 속도 ≤ 최대이면 하차 가능
│   └── FlipDetector.cs                 # 순수: up 벡터·경과 시간으로 뒤집힘 판정
├── Runtime/
│   ├── VehicleController.cs            # Rigidbody 아케이드 주행 + 레이캐스트 서스펜션 + 복구 + 경계 리셋
│   ├── VehicleSeats.cs                 # 운전석/동승석 점유·탑승·하차, 카메라 재타깃, 플레이어 상태
│   ├── VehicleEnterInteractable.cs     # IInteractable "탑승"
│   ├── VehicleDriverInput.cs           # 운전자 PlayerInput → 조작(Move.y 가속/후진, Move.x 조향, Sprint 제동, Jump 복구, Interact 하차)
│   ├── VehicleState.cs                 # 외부 노출 상태 스냅샷
│   └── VehicleEvents.cs
└── Debug/
    └── VehicleDebugPanel.cs
Assets/Editor/VehicleSceneBuilder.cs    # Tools/PROJECT 1028/Build Vehicle Test Scene (경사·둔덕·NPC 1)
Assets/Tests/EditMode/Vehicle/*.cs + asmdef
Assets/StreamingAssets/Vehicle/vehicle_params.json
```

가산 수정(001): `PlayerEntity.IsInVehicle`, `InteractionDetector`(IsInVehicle이면 Current=null), `OrbitCamera.SetTarget(Transform, float distance)`.

## Complexity Tracking
없음.

## Constitution Check (Post-Design)
| 원칙 | 판정 | 확인 |
|---|---|---|
| I | PASS | 파일 트리·API에 의심·소음·인카운터 없음. |
| II | PASS | 에셋 0. |
| III | PASS | 순수 4묶음 + 씬. |
| IV | PASS | 좌석 개체 단위. |
| V | PASS | NPC 무해. |

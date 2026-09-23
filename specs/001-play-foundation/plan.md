# Implementation Plan: 플레이 기반 (Play Foundation)

**Branch**: `001-play-foundation` | **Date**: 2026-09-23 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-play-foundation/spec.md`

## Summary

3인칭 도보 이동·궤도 카메라, NPC 뼈대(위치·정면·플레이어별 "보고 있는가"), 근접 상호작용 안내, 데이터 파일
기반 3~5줄 대화와 목적지 수령을 **테스트 씬 하나**에서 검증 가능하게 만든다. 기존 `ThirdPersonMotor`를 출발점으로
삼고, 카메라·시야·대화 UI는 새 외부 패키지 없이 프로젝트에 이미 있는 Input System·uGUI·Test Framework만으로
구현한다. 테스트 씬은 에디터 메뉴 한 번으로 프리미티브만 사용해 자동 생성되며, 판정 로직은 씬 없이 EditMode
테스트로 검증한다.

## Technical Context

**Language/Version**: C# (Unity 6000.6.2f1, .NET Standard 2.1 프로필)

**Primary Dependencies**: com.unity.inputsystem 1.20.0 (기존 `InputSystem_Actions.inputactions`의 Player 맵:
Move·Look·Sprint·Jump·Interact 사용, Attack은 바인딩하지 않음), com.unity.ugui 2.6.0 (레거시 Text 기반 런타임 생성
캔버스), com.unity.render-pipelines.universal 17.6.0 (기존 설정 그대로), com.unity.modules.physics (시야 가림 판정용
Linecast, 상호작용 대상 탐색용 OverlapSphere)

**Storage**: 대화 데이터는 `Assets/StreamingAssets/Dialogue/*.json` (플레인 텍스트, 에디터 없이 편집 가능,
빌드에 포함). 런타임 상태 저장 없음(범위 밖).

**Testing**: com.unity.test-framework 1.8.0. EditMode 테스트로 순수 로직(시야 판정 수학, 대화 데이터 검증,
상호작용 대상 선택)을 검증. PlayMode 수동 검증은 quickstart.md의 시나리오를 따른다.

**Target Platform**: Windows/macOS 데스크톱 에디터 Play 모드 (키보드·마우스). 빌드는 범위 밖.

**Project Type**: Unity 게임 프로젝트(단일 프로젝트, 기능별 폴더)

**Performance Goals**: 에디터 Play 모드 60 fps 유지. 시야 판정은 NPC×플레이어 쌍당 프레임 1회 Linecast 이하.

**Constraints**: 신규 외부 패키지 0개(Cinemachine·TMP Essentials 임포트 불필요), 신규 아트 에셋 0개(프리미티브만),
`.unity` 씬 파일 수작업 편집 없이 에디터 스크립트로 생성, 게임 규칙 요소(의심·진영·위협 반응) 코드 금지.

**Scale/Scope**: 스크립트 약 12~15개, 테스트 씬 1개, 대화 JSON 1~2개, EditMode 테스트 3묶음. 플레이어 개체 1~2,
NPC 1~2.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 원칙 | 판정 | 근거 |
|---|---|---|
| I. 설계 문서 우선 — 분류 | **PASS** | 스펙 첫머리에 "기반 기능" 명시. 근거 `Docs/PrototypePlan.md` 1·3단계, 완료 기준 1·3. 게임 규칙 요소 미포함 선언. |
| I. 설계 문서 우선 — 규칙 혼입 금지 | **PASS** | 설계에 의심 수치·진영·위협 반응 타입/필드 없음. `NpcVision`은 불리언 감지 결과만 노출(§Contracts). Attack 액션 미바인딩. |
| I. 설계 문서 우선 — `[제안]` 의존 | **PASS** | SDD 조항을 사용하지 않으므로 확정 대기 항목 없음. |
| II. 에셋 예산 준수 | **PASS** | 신규 아트·애니메이션·사운드 0개. 프리미티브(Capsule·Cube·Plane·Sphere)와 내장 LegacyRuntime 폰트만 사용. 외부 패키지 추가 없음. |
| III. 시스템 단위 독립 테스트 | **PASS** | 전용 테스트 씬 `Test_PlayFoundation`을 에디터 메뉴로 생성. 시야·대화 검증·대상 선택은 MonoBehaviour와 분리된 순수 C# 클래스로 두고 EditMode 테스트. |
| IV. 코옵 호환 | **PASS** | 카메라·입력·상호작용·대화 잠금·시야 판정을 모두 `PlayerEntity` 단위로 설계. `NpcVision`은 플레이어별 딕셔너리. 테스트 씬에 두 번째 개체(입력 없음) 배치 옵션. |
| V. 폐기 항목 부활 금지 | **PASS** | 무기·공격·체력 타입 없음. 기존 inputactions의 `Attack` 액션은 읽지 않는다(삭제는 하지 않음 — 에셋 변경 최소화). |
| 근거 문서 § — PrototypePlan 범위 밖 | **PASS** | 전투·다수 NPC 스케줄·래그돌·온라인·섬 규모 맵·차량·최종 아트 미포함. NPC는 정지 상태. |

**Gate 결과**: 위반 없음. Complexity Tracking 불필요.

## Project Structure

### Documentation (this feature)

```text
specs/001-play-foundation/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   ├── dialogue-schema.json   # 대화 데이터 파일 형식
│   └── runtime-api.md         # 후속 기능(002~)이 읽는 공개 인터페이스·이벤트
└── tasks.md             # Phase 2 output (/speckit-tasks)
```

### Source Code (repository root)

```text
Assets/
├── Scripts/
│   ├── PlayFoundation.asmdef            # 런타임 어셈블리 (Unity.InputSystem 참조)
│   ├── Player/
│   │   ├── ThirdPersonMotor.cs          # 기존. 이동·달리기·회전 (소폭 조정: MovementState 노출)
│   │   ├── PlayerEntity.cs              # 플레이어 개체 식별·상태 집계·잠금 API
│   │   ├── OrbitCamera.cs               # 궤도 카메라: Look 입력, 피치 클램프, 장애물 당김
│   │   ├── InteractionDetector.cs       # 반경 내 IInteractable 탐색, 카메라 전방 우선 선택
│   │   └── FallRespawn.cs               # 바닥 밖 낙하 시 시작 위치 복귀
│   ├── NPC/
│   │   ├── NpcIdentity.cs               # NPC 식별자·정면·대화 데이터 참조. 이동 없음
│   │   ├── NpcVision.cs                 # 플레이어별 "보고 있는가" + 히스테리시스 + Gizmo
│   │   ├── VisionEvaluator.cs           # 순수 C#: 각도·거리 판정 (테스트 대상)
│   │   └── NpcTalkInteractable.cs       # IInteractable 구현: 대화 시작 요청
│   ├── Dialogue/
│   │   ├── DialogueData.cs              # JSON 매핑 모델 (lines, destination)
│   │   ├── DialogueValidator.cs         # 순수 C#: 3~5줄·필수 필드 검증 (테스트 대상)
│   │   ├── DialogueLoader.cs            # StreamingAssets에서 JSON 로드
│   │   ├── DialogueRunner.cs            # 플레이어별 진행 상태·잠금·진행 입력
│   │   └── DialogueEvents.cs            # 정적 이벤트 허브 (started/advanced/ended/destination)
│   ├── World/
│   │   ├── Destination.cs               # 목적지 데이터·수령 기록(플레이어·NPC별 중복 방지)
│   │   └── DestinationMarker.cs         # 씬 내 마커 표시
│   ├── UI/
│   │   ├── RuntimeHud.cs                # 코드로 캔버스 생성: 안내·대화창·경고 로그
│   │   └── VisionDebugLabel.cs          # NPC 머리 위 "보고 있음/아님" 표시
│   └── Interaction/
│       ├── IInteractable.cs             # 인터페이스: 안내 문구·상호작용 거리·실행
│       └── InteractableSelector.cs      # 순수 C#: 후보 중 카메라 전방 최근접 선택 (테스트 대상)
├── Editor/
│   ├── PlayFoundation.Editor.asmdef
│   └── PlayFoundationSceneBuilder.cs    # 메뉴: Tools/PROJECT 1028/Build Play Foundation Test Scene
├── Tests/
│   └── EditMode/
│       ├── PlayFoundation.Tests.EditMode.asmdef
│       ├── VisionEvaluatorTests.cs
│       ├── DialogueValidatorTests.cs
│       └── InteractableSelectorTests.cs
├── StreamingAssets/
│   └── Dialogue/
│       └── npc_dock_worker.json         # 샘플 대사 4줄 + 목적지
└── Scenes/
    └── Test_PlayFoundation.unity        # 에디터 메뉴로 생성 (수작업 편집 없음)
```

**Structure Decision**: Unity 단일 프로젝트. `Assets/Scripts`를 하나의 런타임 어셈블리(`PlayFoundation.asmdef`)로
묶어 EditMode 테스트 어셈블리가 참조할 수 있게 한다(사전 정의 어셈블리 `Assembly-CSharp`는 asmdef에서 참조
불가). 기능별 하위 폴더는 후속 002~007이 `Suspicion/`, `Subdue/` 등으로 나란히 확장한다. 순수 로직 클래스
(`VisionEvaluator`, `DialogueValidator`, `InteractableSelector`)는 UnityEngine 의존을 `Vector3`·`Mathf` 수준으로
제한하여 씬 없이 테스트한다.

## Complexity Tracking

위반 없음. 해당 없음.

## Phase 0 → Phase 1 요약

- Phase 0 결정 사항은 [research.md](research.md): 카메라 자체 구현(Cinemachine 미도입), 대화 데이터 JSON in
  StreamingAssets, UI 레거시 Text 런타임 생성, 씬 자동 생성 에디터 스크립트, 어셈블리 분리, 히스테리시스 방식,
  두 번째 플레이어 개체 처리.
- Phase 1 산출물: [data-model.md](data-model.md), [contracts/dialogue-schema.json](contracts/dialogue-schema.json),
  [contracts/runtime-api.md](contracts/runtime-api.md), [quickstart.md](quickstart.md).

## Constitution Check (Post-Design 재평가)

| 원칙 | 판정 | 설계 후 확인 |
|---|---|---|
| I | **PASS** | data-model에 의심·진영·위협 필드 없음. `NpcVision`은 `bool IsSeeing(PlayerEntity)`와 변경 이벤트만 노출. |
| II | **PASS** | 파일 트리에 모델·텍스처·오디오 없음. StreamingAssets는 JSON만. |
| III | **PASS** | 순수 로직 3종 EditMode 테스트, 테스트 씬 생성 메뉴. |
| IV | **PASS** | 모든 런타임 컴포넌트가 `PlayerEntity` 참조로 동작. 정적 싱글톤은 이벤트 허브(`DialogueEvents`)만이며 페이로드에 플레이어를 포함. |
| V | **PASS** | Attack 액션 미사용. 무기·체력 타입 없음. |

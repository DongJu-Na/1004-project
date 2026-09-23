# Research: 플레이 기반 (Play Foundation)

**Date**: 2026-09-23 | **Plan**: [plan.md](plan.md)

Technical Context에 NEEDS CLARIFICATION은 없었다. 아래는 설계 선택지가 둘 이상이던 항목의 결정 기록이다.

## R-1. 3인칭 카메라 구현 방식

- **Decision**: 자체 `OrbitCamera` 컴포넌트. Look 액션(마우스 델타)으로 yaw 무제한·pitch 클램프, 대상에서 카메라
  위치로 SphereCast하여 장애물에 맞으면 거리를 줄인다.
- **Rationale**: 스펙 Assumptions가 "새 외부 의존성 없음"을 명시. Cinemachine은 `Packages/manifest.json`에 없다.
  요구사항(FR-004, FR-005)은 궤도 회전·피치 제한·장애물 당김 세 가지뿐이라 100줄 내로 충분하다.
- **Alternatives considered**: Cinemachine 3.x 도입 — 기능은 풍부하나 패키지 추가와 가상 카메라 프리팹 구성이
  필요하고, 코옵에서 플레이어별 카메라를 코드로 만들 때 Brain/채널 설정이 늘어난다. 기각.

## R-2. 대화 데이터 파일 형식과 위치

- **Decision**: JSON 파일을 `Assets/StreamingAssets/Dialogue/<npcId>.json`에 두고 `JsonUtility.FromJson`으로 로드.
  스키마는 [contracts/dialogue-schema.json](contracts/dialogue-schema.json).
- **Rationale**: FR-010 "씬 밖 데이터 파일", SC-004 "파일 내용을 바꾸면 재시작 후 반영"을 에디터 없이 텍스트
  편집기로 충족한다. StreamingAssets는 빌드에도 그대로 포함된다. 007 인카운터의 "대사 3~5줄"이 같은 형식을
  재사용할 수 있다.
- **Alternatives considered**: ScriptableObject — 인스펙터 편집은 편하지만 플레인 텍스트가 아니고 `.asset`
  직렬화가 에디터 의존. Resources 폴더 JSON — 동작하지만 Resources는 빌드 최적화상 비권장. 기각.

## R-3. UI 텍스트 구현

- **Decision**: uGUI 레거시 `UnityEngine.UI.Text` + `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")`.
  캔버스·패널·텍스트를 `RuntimeHud`가 코드로 생성한다.
- **Rationale**: TextMeshPro는 최초 사용 시 "Import TMP Essentials" 수동 임포트가 필요해 자동 생성 흐름이 끊긴다.
  프로토타입 검증 UI는 안내 한 줄·대화창·경고 로그면 충분하다. 코드 생성이므로 씬 파일 수작업이 없다.
- **Alternatives considered**: TextMeshPro — 품질은 좋으나 임포트 단계와 폰트 에셋 필요. UI Toolkit — 런타임
  UIDocument와 UXML 에셋이 필요해 파일 수가 늘어남. 기각. TMP 전환은 후속 UI 정리 기능에서 검토.

## R-4. 테스트 씬 구성 방법

- **Decision**: 에디터 스크립트 `PlayFoundationSceneBuilder`가 메뉴 항목 `Tools/PROJECT 1028/Build Play Foundation
  Test Scene`으로 새 씬을 만들고 바닥·벽·플레이어·카메라·NPC·두 번째 플레이어 개체를 프리미티브로 배치한 뒤
  `Assets/Scenes/Test_PlayFoundation.unity`로 저장한다. 기존 `SampleScene`은 건드리지 않는다.
- **Rationale**: `.unity` YAML을 직접 쓰는 것은 GUID·fileID 관리가 필요해 깨지기 쉽다. 에디터 API(`EditorSceneManager`,
  `GameObject.CreatePrimitive`, `PlayerInput` 컴포넌트 추가와 액션 에셋 연결)는 안정적이며, 사용자는 메뉴 한 번으로
  씬을 재생성할 수 있다. 헌장 원칙 III(독립 검증)과 사용자의 "집에서 실행" 요구를 동시에 충족.
- **Alternatives considered**: 런타임 부트스트랩(빈 씬 + 스포너 1개) — 하이어라키에서 오브젝트를 조정하기 어렵다.
  수작업 씬 구성 — 구현 단계에서 자동화 불가. 기각.

## R-5. 어셈블리 구성

- **Decision**: `Assets/Scripts/PlayFoundation.asmdef`(런타임, 참조: Unity.InputSystem), `Assets/Editor/
  PlayFoundation.Editor.asmdef`(에디터 전용, 참조: PlayFoundation, Unity.InputSystem), `Assets/Tests/EditMode/
  PlayFoundation.Tests.EditMode.asmdef`(EditMode, 참조: PlayFoundation, UnityEngine.TestRunner, UnityEditor.TestRunner,
  nunit).
- **Rationale**: 테스트 어셈블리는 사전 정의 `Assembly-CSharp`를 참조할 수 없으므로 런타임 코드에 asmdef가 필요하다.
  기존 `ThirdPersonMotor.cs`는 `Assets/Scripts/Player/`에 있어 자동으로 런타임 어셈블리에 포함된다.
- **Alternatives considered**: asmdef 없이 PlayMode 테스트만 — 씬 로드가 필요해 헌장 III의 "씬 없이 검증"에 불리. 기각.

## R-6. "보고 있는가" 판정과 깜빡임 방지

- **Decision**: 순수 C# `VisionEvaluator.Evaluate(npcPos, npcForward, targetPos, fovDeg, range)`가 각도·거리를
  판정하고, `NpcVision`이 결과에 `Physics.Linecast(눈 위치 → 대상 중심, 가림 레이어)`를 더한다. 상태 전환은
  히스테리시스: 새 원시 값이 `stableSeconds`(기본 0.15초) 동안 유지될 때만 공개 상태를 바꾼다. 플레이어별
  `Dictionary<PlayerEntity, VisionState>`.
- **Rationale**: FR-016 (a)(b)(c)와 FR-017 안정 시간, SC-005 "1초 안에 2회 이상 바뀌지 않음"을 충족. 각도·거리
  로직을 분리해 EditMode 테스트 가능.
- **Alternatives considered**: Trigger 콜라이더 부채꼴 — 가림 판정이 별도 필요하고 테스트가 어렵다. 기각.

## R-7. 두 번째 플레이어 개체(코옵 동등성 검증)

- **Decision**: 씬 빌더가 `PlayerEntity` 2개를 만든다. 첫 개체에는 `PlayerInput`과 `OrbitCamera`가 연결된 카메라를,
  둘째 개체에는 `PlayerInput`을 붙이지 않는다(입력 없음). 이동·상호작용·대화·시야 판정 컴포넌트는 둘 다 가진다.
  씬 빌더 메뉴에 "두 번째 개체 포함" 토글을 둔다.
- **Rationale**: 스펙 Assumptions "한 개체만 입력을 받는 방식으로 개체 단위 독립성을 검증". User Story 4 시나리오
  1~3을 충족한다. Input System의 다중 플레이어 장치 페어링은 범위 밖.
- **Alternatives considered**: `PlayerInputManager`로 진짜 로컬 2P — 두 번째 장치가 필요하고 스펙 범위 밖. 기각.

## R-8. 상호작용 대상 선택

- **Decision**: `InteractionDetector`가 `Physics.OverlapSphere(플레이어 위치, 탐색 반경)`로 `IInteractable` 후보를
  모으고, 순수 C# `InteractableSelector.Pick(candidates, cameraForward, playerPos)`가 카메라 전방과 후보 방향의
  각도가 가장 작은 것(동률이면 가까운 것)을 고른다. 각 후보의 `InteractionRange` 안에 있어야 한다.
- **Rationale**: Edge case "NPC 2개 이상이면 카메라가 보는 방향에 가장 가까운 하나" 충족. 테스트 가능.
- **Alternatives considered**: 레이캐스트 단일 대상 — 조준이 필요해 3인칭 도보에서 불편. 기각.

## R-9. 대화 진행 입력과 첫 줄 건너뛰기 방지

- **Decision**: 상호작용과 진행을 같은 `Interact` 액션으로 쓴다. `DialogueRunner.Begin()`은 호출된 프레임 번호를
  기록하고, 같은 프레임의 `WasPressedThisFrame()`은 무시한다.
- **Rationale**: Edge case와 Assumptions에 명시된 기본값. 프레임 기록이 가장 단순한 안전장치다.

## R-10. 목적지 표시와 중복 방지

- **Decision**: `Destination`은 (플레이어, NPC) 키로 수령 기록을 갖고, 이미 수령했으면 마커를 새로 만들지 않고
  기존 마커를 유지한다. 마커는 프리미티브 실린더 + 라벨.
- **Rationale**: FR-013 "마커는 하나만 유지". HUD·거리 표시는 PrototypePlan 4단계이므로 제외.

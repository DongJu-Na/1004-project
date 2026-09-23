# Implementation Plan: 의심 시스템과 시간대 위협 (Suspicion System)

**Branch**: `002-suspicion-system` | **Date**: 2026-09-23 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/002-suspicion-system/spec.md`

## Summary

(NPC, 플레이어) 쌍별 개인 의심 0~3과 씬 단일 섬 의심도 0~100을 두고, **데이터 파일의 상승 사건 표**로만 값을 올린다.
개인 의심 단계(무관심·인지·경계·확신)에 따라 NPC가 쳐다보기·따라다니기·말 걸기를 하고, 3에서 "신고 시도" 사건만
외부로 낸다. 섬 의심도는 네 구간으로 해석되어 전환 사건과 "출항 불가" 상태를 노출한다. 전파(2 이상 NPC가 만나면
지연 후 +1), 매우 느린 감소, 런 종료 이월 값, 낮·밤 배율과 밤 이동 기본 의심을 포함한다. 001의 `NpcVision.IsSeeing`이
유일한 감지 입력이며, 별도 어셈블리 `Suspicion`으로 분리해 001에 역방향 의존이 생기지 않게 한다.

## Technical Context

**Language/Version**: C# (Unity 6000.6.2f1)

**Primary Dependencies**: 어셈블리 `PlayFoundation`(001: `PlayerEntity`, `NpcIdentity`, `NpcVision`, `RuntimeHud`, `DialogueEvents`),
com.unity.inputsystem 1.20.0(테스트 콘솔 키 입력만: `Keyboard.current`), com.unity.ugui 2.6.0(디버그 패널은 001 `RuntimeHud`
확장 API 사용), com.unity.modules.physics(전파 거리·반경 판정은 거리 계산만, 물리 질의 없음)

**Storage**: 규칙 데이터 `Assets/StreamingAssets/Suspicion/suspicion_rules.json` (상승 사건 표·감소·전파·시간대·이월 계수,
각 항목에 `status: "확정" | "확정대기"`). 런타임 상태 저장 없음. 이월 값은 계산·노출만.

**Testing**: com.unity.test-framework 1.8.0 EditMode. 순수 로직(`PersonalSuspicion`, `IslandAlert`, `SuspicionRulesValidator`,
`PropagationScheduler`, `DecayCalculator`, `CarryOverCalculator`) 6묶음. Play 수동 검증은 quickstart.md.

**Target Platform**: 데스크톱 에디터 Play 모드

**Project Type**: Unity 게임 프로젝트(기능별 어셈블리)

**Performance Goals**: NPC 3 × 플레이어 2 규모에서 프레임당 판정 비용 무시 가능. 전파 쌍 검사 O(N²)는 N ≤ 20 가정.

**Constraints**: 상승량·감소·전파·배율 **수치는 코드에 박지 않는다**(FR-009). 신규 패키지·에셋 0. NPC 이동은 이 기능이
최소 도입(`NpcMover`: 지점으로 걷기·따라가기, 내비메시 없음). 신고 진행·유형 규칙·구간 효과 연출은 범위 밖.

**Scale/Scope**: 스크립트 약 16개, JSON 1개, EditMode 테스트 6개, 씬 빌더 확장 1개.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 원칙 | 판정 | 근거 |
|---|---|---|
| I. 분류 | **PASS** | 스펙 첫머리 "게임 규칙 기능". 근거 §2.1, §2.2, §2.4, §2.5, §2.6, §2.7, §7, §8.1 모두 `확정`. |
| I. `[제안]` 처리 | **PASS** | §2.3 상승 수치는 JSON `events[]`에 `status:"확정대기"`로 들어가고 코드는 표만 적용. 위장(v0.6 §3.1)은 불리언 입력. 로더가 확정대기 항목 수를 HUD 경고로 표시. |
| I. 근거 인용 | **PASS** | data-model·contracts의 각 규칙에 조항 번호 병기. |
| II. 에셋 예산 | **PASS** | 신규 에셋 0. 디버그 표시는 001 HUD 텍스트·Gizmo. |
| III. 독립 테스트 | **PASS** | 순수 로직 6묶음 EditMode. 테스트 씬 `Test_Suspicion`을 에디터 메뉴로 생성(NPC 3, 플레이어 2, 낮·밤 스위치, 사건 임의 발생 콘솔). 대화·제압·인카운터 불필요. |
| IV. 코옵 호환 | **PASS** | 개인 의심 키 = (NpcIdentity, PlayerEntity). `IslandAlert`는 `SuspicionSystem` 단일 인스턴스의 하나뿐인 값. 밤 이동·전파·상승 모두 플레이어 개체 단위. |
| V. 폐기 항목 | **PASS** | 해당 없음. 무기·공격 없음. |
| 워크플로우 §: 다른 시스템으로 넘기는 것 | **PASS** | 신고 진행(005)·유형 예외(003)·구간 효과(맵/출항)는 이벤트·플래그·구간 조회로만 연결. |

**Gate 결과**: 위반 없음.

## Project Structure

### Documentation (this feature)

```text
specs/002-suspicion-system/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── suspicion-rules-schema.json   # 규칙 데이터 파일 형식
│   └── runtime-api.md                # 003~007이 읽는 공개 표면
└── tasks.md                          # /speckit-tasks
```

### Source Code (repository root)

```text
Assets/
├── Scripts/
│   └── Suspicion/
│       ├── Suspicion.asmdef                  # references: PlayFoundation, Unity.InputSystem, UnityEngine.UI
│       ├── Model/
│       │   ├── PersonalSuspicion.cs          # 순수: 0~3, 단계, Clamp, 변경 결과
│       │   ├── IslandAlert.cs                # 순수: 0~100, 구간, 전환 결과, 출항 불가
│       │   ├── SuspicionRules.cs             # JSON 모델 (events, decay, propagation, timeOfDay, carryOver)
│       │   ├── SuspicionRulesValidator.cs    # 순수: 필수 필드·범위·음수·중복 id
│       │   ├── SuspicionRulesLoader.cs       # StreamingAssets 로드 + 검증 + 확정대기 카운트
│       │   ├── PropagationScheduler.cs       # 순수: 만남 상태·지연 큐
│       │   ├── DecayCalculator.cs            # 순수: 주기 감소 계산
│       │   └── CarryOverCalculator.cs        # 순수: 런 종료 이월 값
│       ├── Runtime/
│       │   ├── SuspicionSystem.cs            # 씬 단일: 상태 보관, 사건 적용, 감소·전파·밤 규칙 틱, 이월
│       │   ├── SuspicionEvents.cs            # 정적 이벤트 허브
│       │   ├── SuspicionEvent.cs             # 사건 페이로드 (eventId, actor, position, targetNpc)
│       │   ├── TimeOfDay.cs                  # 낮·밤 상태 + 전환 이벤트
│       │   ├── PlayerDisguise.cs             # "위장 유효" 외부 불리언 입력
│       │   ├── NpcSuspicionProfile.cs        # 플래그: PropagatesSuspicion, ReportsToManagerImmediately, IsManager, SuppressStageBehaviour
│       │   ├── NpcMover.cs                   # 최소 이동: 지점으로 걷기 / 대상 따라가기 (내비메시 없음)
│       │   └── NpcSuspicionBehaviour.cs      # 단계별 표현: 1 쳐다보기·돌아보기, 2 따라가기·말 걸기, 3 신고 시도 사건
│       └── Debug/
│           ├── SuspicionDebugPanel.cs        # HUD: 모든 (NPC,플레이어) 값, 섬 의심도·구간·시간대·예약 전파
│           └── SuspicionTestConsole.cs       # 키: F1~F7 사건 발생, N 낮/밤, R 런 종료, T 시간 배속, [ ] 섬 ±10
├── Editor/
│   └── SuspicionSceneBuilder.cs              # Tools/PROJECT 1028/Build Suspicion Test Scene (PlayFoundation.Editor asmdef에 Suspicion 참조 추가)
├── Tests/EditMode/Suspicion/
│   ├── Suspicion.Tests.EditMode.asmdef
│   ├── PersonalSuspicionTests.cs
│   ├── IslandAlertTests.cs
│   ├── SuspicionRulesValidatorTests.cs
│   ├── PropagationSchedulerTests.cs
│   ├── DecayCalculatorTests.cs
│   └── CarryOverCalculatorTests.cs
├── StreamingAssets/Suspicion/
│   └── suspicion_rules.json
└── Scenes/
    └── Test_Suspicion.unity                  # 에디터 메뉴로 생성
```

**Structure Decision**: 001과 같은 프로젝트에 **별도 어셈블리 `Suspicion`**을 둔다. `Suspicion → PlayFoundation` 단방향
의존으로 001 코드는 002를 모른다(원칙 III: 001 테스트 씬은 002 없이 계속 동작). 순수 로직은 `Model/`에 모아 EditMode
테스트한다. 기존 `PlayFoundation.Editor.asmdef`에 `Suspicion` 참조를 추가해 씬 빌더를 같은 에디터 어셈블리에 둔다.

## Complexity Tracking

위반 없음. 해당 없음.

## Constitution Check (Post-Design 재평가)

| 원칙 | 판정 | 설계 후 확인 |
|---|---|---|
| I | **PASS** | 수치는 전부 `suspicion_rules.json`. `SuspicionRulesValidator`가 `status` 누락을 오류로 본다. 코드 상수는 범위(0~3, 0~100)와 구간 경계(25/50/75)만 — 이는 §2.1·§2.4 `확정`값. |
| II | **PASS** | 파일 트리에 에셋 없음. |
| III | **PASS** | 순수 로직 6묶음 + 전용 씬 + 사건 임의 발생 콘솔(FR-028). |
| IV | **PASS** | `Dictionary<(NpcIdentity, PlayerEntity), PersonalSuspicion>`, `IslandAlert` 단일. 새 플레이어 등록 시 0에서 시작(Edge case). |
| V | **PASS** | 해당 없음. |

# Implementation Plan: 제압 시스템 (Subdue System)

**Branch**: `004-subdue-system` | **Date**: 2026-09-23 | **Spec**: [spec.md](spec.md)

## Summary

비살상 제압 세 동작(뒤에서 제압·정면 밀치기·물건으로 기절)을 손 상태(빈손·한 손·두 손)와 003 진영 판정 위에 얹는다. 기절한
NPC는 90~180초 뒤 깨어나 제압자에 대한 개인 의심 3 + 신고 시도를 내고, 그 자리에 남으며, 빈손 플레이어가 두 손으로 들어 옮길 수
있다(속도 저하·달리기 불가). 대가는 002 상승 표를 통해서만: 제압 장면 목격 시 섬 +40(1회), 기절자 발견자마다 섬 +30(목격자는
제외), 발견자의 개인 의심 +3(확정대기). 모든 수치·확률·소음 등급은 `subdue_rules.json`과 002 사건 표에 둔다. 무기 타입은 없다.

## Technical Context

**Language/Version**: C# (Unity 6000.6.2f1)
**Primary Dependencies**: `PlayFoundation`(PlayerEntity·NpcIdentity·NpcVision·VisionEvaluator·IInteractable·InteractableSelector·RuntimeHud·ThirdPersonMotor), `Suspicion`(SuspicionSystem.Raise·SuspicionEvents·NpcMover·NpcSuspicionBehaviour), `NpcTypes`(SubdueJudgement), Input System(키), uGUI(HUD)
**Storage**: `Assets/StreamingAssets/Subdue/subdue_rules.json`; 002 `suspicion_rules.json`에 사건 추가(`subdue_witnessed` 섬 40 확정, `unconscious_found_island` 섬 30 확정, `unconscious_wake` 개인 3 확정, `noise_low/medium/high` 확정대기)
**Testing**: EditMode 순수 로직 6묶음(`HandRules`, `BackConeCheck`, `ShoveRoll`, `UnconsciousTimer`, `CostLedger`, `SubdueRulesValidator`)
**Target Platform**: 데스크톱 에디터 Play
**Project Type**: Unity 기능별 어셈블리(`Subdue → NpcTypes → Suspicion → PlayFoundation`)
**Performance Goals**: 기절자 발견 스캔 = 기절자 수 × 관찰 NPC 수 Linecast/프레임(≤ 20×20 무시 가능)
**Constraints**: 무기·체력·살상 타입 금지(원칙 V). 수치는 JSON. 신규 에셋 0(물건은 Cube). 001·002 수정은 가산적(모터 속도 배율·달리기 허용 플래그, PlayerEntity.HandsBusy, 002 `global` scope·`ForceReportAttempt`).
**Scale/Scope**: 스크립트 약 18개, JSON 1개(+002 수정), 테스트 6개, 씬 빌더 1개.

## Constitution Check

| 원칙 | 판정 | 근거 |
|---|---|---|
| I. 분류 | **PASS** | "게임 규칙 기능". §4.1~§4.5, §3.2, §8.3, §1 원칙 4 `확정`. |
| I. `[제안]` | **PASS** | 정면 밀치기 실패 확률·소음→의심 값·발견 개인 +3·운반 속도 배율·기절 시간 분포는 JSON `확정대기`. §4.4 섬 +30/+40, 개인 3, §4.3 90~180초 범위는 `확정`. |
| I. 선행 SDD | **PASS** | §4.2 발췌 손 규칙만 근거. 증거·도구 시스템 미도입, 검증용 일상 물건만. |
| II. 에셋 | **PASS** | 물건·기절 표현은 프리미티브(Cube, 캡슐 눕힘). 신규 에셋 0. |
| III. 독립 테스트 | **PASS** | 순수 6묶음. 전용 씬 `Test_Subdue`(제압 가능 NPC 2, 불가 1, 목격자 1, 물건 2, 플레이어 2, 시간 배속). |
| IV. 코옵 | **PASS** | 손 상태·후면·인지·소음 위치 모두 행위자 개체 기준. 기절자는 다른 개체가 들 수 있음(§8.3). 대가는 공유 섬 값. |
| V. 폐기 항목 | **PASS** | `Weapon`·`Health`·`Damage` 타입 없음. 물건은 `CarriableObject`(무게 등급만). 어떤 동작도 NPC를 제거하지 않음(항상 깨어남). |

**Gate**: 위반 없음.

## Project Structure

```text
Assets/Scripts/Subdue/
├── Subdue.asmdef                          # refs: PlayFoundation, Suspicion, NpcTypes, Unity.InputSystem, UnityEngine.UI
├── Model/
│   ├── SubdueDefinitions.cs               # enum SubdueAction, HandState, NoiseLevel, HeldKind
│   ├── SubdueRules.cs / SubdueRulesValidator.cs / SubdueRulesLoader.cs
│   ├── HandRules.cs                       # 순수: (동작, 손 상태) → 허용 여부·이유 (§4.2)
│   ├── BackConeCheck.cs                   # 순수: 행위자가 대상 후면 부채꼴 안인가
│   ├── ShoveRoll.cs                       # 순수: 실패 확률 판정(주입 난수)
│   ├── UnconsciousTimer.cs                # 순수: 90~180 범위 추출·틱·깨어남 (§4.3)
│   └── CostLedger.cs                      # 순수: 목격자/발견자 1회 규칙 (§4.4)
├── Runtime/
│   ├── PlayerHands.cs                     # 손 상태·들기/내려놓기·HandsBusy·모터 배율
│   ├── CarriableObject.cs                 # IInteractable "들기" (일상 물건, 무기 속성 없음)
│   ├── UnconsciousState.cs                # NPC 기절 상태 컴포넌트(타이머·제압자·운반자·원장)
│   ├── UnconsciousCarryInteractable.cs    # IInteractable "들어 옮기기"
│   ├── SubdueActor.cs                     # 플레이어: 대상 선택·동작 결정·F 입력
│   ├── SubdueSystem.cs                    # 씬 단일: 규칙, TrySubdue, 기절 틱, 발견 스캔, 대가 사건
│   └── SubdueEvents.cs
└── Debug/
    ├── SubdueDebugPanel.cs                # NPC 기절/남은 시간/발견자, 플레이어 손 상태
    └── SubdueTestConsole.cs               # 표시만(F/E/G 안내), 시간 배속은 002 콘솔 T
Assets/Editor/SubdueSceneBuilder.cs        # Tools/PROJECT 1028/Build Subdue Test Scene
Assets/Tests/EditMode/Subdue/*.cs + asmdef
Assets/StreamingAssets/Subdue/subdue_rules.json
```

가산 수정: `ThirdPersonMotor`(SpeedMultiplier, SprintAllowed), `PlayerEntity.HandsBusy`, `NpcTalkInteractable.CanInteract`(HandsBusy 거부),
`SuspicionEventRule` scope `global`(섬 값만), `SuspicionSystem.ForceReportAttempt(npc, player)`, 002 JSON 사건 6개.

## Complexity Tracking
없음.

## Constitution Check (Post-Design)
| 원칙 | 판정 | 확인 |
|---|---|---|
| I | PASS | 수치 전부 JSON. 코드 상수는 손 상태 enum·동작 enum. |
| II | PASS | 에셋 0. |
| III | PASS | 순수 6묶음 + 씬 + 시간 배속(002 콘솔). |
| IV | PASS | 행위자 기준 판정, 타 개체 운반 가능. |
| V | PASS | 무기·체력·살상 타입 없음. |

# Implementation Plan: 신고 시나리오 (Report Scenario)

**Branch**: `005-report-scenario` | **Date**: 2026-09-23 | **Spec**: [spec.md](spec.md)

## Summary
002의 `OnReportAttempt(npc, player)`를 받아 NPC를 가장 가까운 **사용 가능한 신고 지점**(전화기·관리소)으로 걷게 하고, 도착 후 짧은
신고 동작 뒤 섬 의심도 +25(`report_completed`, 002 표)와 "신고 완료" 사건을 낸다. 플레이어는 (a) 내버려두기, (b) 004 제압으로
중단(깨어나면 재시작), (c) 앞질러 전화선 끊기(시간 소요, 도구 요구는 확정대기), (d) 미리 끊어둠(지점 없으면 포기)을 선택한다.
관리소는 끊을 수 없다. 도보 시간은 데이터의 최소값 이상이 되도록 NPC 속도를 낮춘다. 001~004를 통합하는 첫 장면이다.

## Technical Context
**Language/Version**: C# (Unity 6000.6.2f1)
**Primary Dependencies**: `PlayFoundation`(IInteractable·NpcIdentity·RuntimeHud), `Suspicion`(SuspicionEvents.OnReportAttempt·SuspicionSystem.Raise·NpcMover·NpcSuspicionProfile), `NpcTypes`(InformerBehaviour 일시 중지), `Subdue`(UnconsciousState.IsUnconscious·OnUnconsciousWake), Input System, uGUI
**Storage**: `Assets/StreamingAssets/Report/report_rules.json`; 002 JSON에 `report_completed`(섬 25, global, 확정) 추가; 003 배정에 `rep_*` 추가
**Testing**: EditMode 순수 5묶음(`ReportPointSelector`, `ReportFlowState`, `CutProgress`, `ReportSpeedRule`, `ReportRulesValidator`)
**Project Type**: Unity 기능별 어셈블리 `Report → Subdue → NpcTypes → Suspicion → PlayFoundation`
**Constraints**: 신고 지점·시간·도구 요구는 JSON. 신규 에셋 0(전화기 = 기둥+상자 프리미티브, 관리소 = 큰 상자). NPC 경로는 직선(장애물 회피 없음, NavMesh는 후속). 가산 수정: 002 JSON 사건 1개, 003 배정 3건.
**Scale/Scope**: 스크립트 약 15개, JSON 1개, 테스트 5개, 씬 빌더 1개.

## Constitution Check
| 원칙 | 판정 | 근거 |
|---|---|---|
| I. 분류 | PASS | "게임 규칙 기능(통합)". §5.1~§5.3, §2.2, §4.3, §4.4, §3 `확정`. |
| I. `[제안]` | PASS | 도구(v0.6 §3.1)는 `cutRequiresTool` 플래그 + `PlayerToolkit.HasCuttingTool` 불리언 입력, `확정대기`. 최소 도보·신고·끊기 시간은 SDD에 값이 없어 `확정대기`. 섬 +25는 `확정`. |
| II. 에셋 | PASS | 프리미티브만. |
| III. 독립 테스트 | PASS | 순수 5묶음 + 전용 씬 `Test_Report`(전화기 2, 관리소 1, 감시자 2, 침묵자 1, 플레이어 2). |
| IV. 코옵 | PASS | 신고 흐름은 (NPC, 대상 플레이어) 단위, 개입은 어느 개체든, +25는 공유 값. |
| V. 폐기 항목 | PASS | 선택지에 살상 없음. "제압한다"는 004 비살상. |

**Gate**: 위반 없음.

## Project Structure
```text
Assets/Scripts/Report/
├── Report.asmdef                          # refs: PlayFoundation, Suspicion, NpcTypes, Subdue, Unity.InputSystem, UnityEngine.UI
├── Model/
│   ├── ReportDefinitions.cs               # enum ReportPointKind {Phone, Office}, ReportPhase {Moving, Reporting, Completed, Abandoned, Interrupted}
│   ├── ReportRules.cs / ReportRulesValidator.cs / ReportRulesLoader.cs
│   ├── ReportPointSelector.cs             # 순수: 사용 가능한 지점 중 최근접
│   ├── ReportFlowState.cs                 # 순수: 상태기계(Moving→Reporting→Completed / Retarget / Abandon / Interrupt)
│   ├── CutProgress.cs                     # 순수: 홀드 진행·취소
│   └── ReportSpeedRule.cs                 # 순수: 최소 도보 시간을 만족하는 속도
├── Runtime/
│   ├── ReportPoint.cs                     # 신고 지점 컴포넌트(종류·사용 가능·끊기 가능)
│   ├── PhoneCutInteractable.cs            # IInteractable "전화선 끊기"
│   ├── PlayerCutter.cs                    # 플레이어: 끊기 진행(정지 유지·이동 시 취소)
│   ├── PlayerToolkit.cs                   # "도구 보유" 외부 입력
│   ├── ReportFlow.cs                      # NPC 흐름 컴포넌트(동적 부착)
│   ├── ReportSystem.cs                    # 씬 단일: 규칙, OnReportAttempt 구독, 흐름 생성, 완료 이력, 중단·재시작
│   └── ReportEvents.cs
└── Debug/
    ├── ReportDebugPanel.cs                # 지점 상태, NPC별 흐름 상태·목적지, 로그
    └── ReportTestConsole.cs               # K 도구 토글, O 가장 가까운 전화기 즉시 끊기(디버그)
Assets/Editor/ReportSceneBuilder.cs        # Tools/PROJECT 1028/Build Report Test Scene
Assets/Tests/EditMode/Report/*.cs + asmdef
Assets/StreamingAssets/Report/report_rules.json
```

## Complexity Tracking
없음.

## Constitution Check (Post-Design)
| 원칙 | 판정 | 확인 |
|---|---|---|
| I | PASS | 시간·도구·거리 JSON. 코드 상수는 enum. |
| II | PASS | 에셋 0. |
| III | PASS | 순수 5묶음 + 씬. |
| IV | PASS | (NPC, 플레이어) 이력, 개입 개체 무관. |
| V | PASS | 해당 없음. |

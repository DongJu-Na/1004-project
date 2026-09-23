# Implementation Plan: NPC 유형 5종 (NPC Types)

**Branch**: `003-npc-types` | **Date**: 2026-09-23 | **Spec**: [spec.md](spec.md)

## Summary

NPC마다 유형(감시자·밀고자·동조자·침묵자·경계자)·진영(Antagonist·Victim·Neutral)·역할(관리자·중개인)을 **씬 밖 데이터**
(`npc_assignments`)로 지정하고, 유형별 규칙을 002 의심 시스템 위에 얹는다. 감시자는 시야 안 조사에 즉시 +2(시간대 배율
무시), 밀고자는 겉으로 무반응이다가 지연 후 관리자에게 걸어가 전달, 동조자는 섬 의심도 긴장 구간 이상에서 감시자 규칙으로
전환, 침묵자는 신고·전파 없음, 경계자는 소리로 반경 안 NPC를 돌려세운다. 제압 가능 판정은 진영만으로 결정하는 순수 함수로
제공해 004가 소비한다. 002에는 **가산적** 확장 3건만 넣는다(사건 규칙 `ignoresTimeMultiplier`, 프로필 플래그
`SuppressReportAttempt`·`IgnoresSuspicionEvents`, 사건 2개 추가).

## Technical Context

**Language/Version**: C# (Unity 6000.6.2f1)
**Primary Dependencies**: 어셈블리 `PlayFoundation`(001), `Suspicion`(002: `SuspicionSystem.Raise`, `NpcSuspicionProfile`, `NpcMover`, `TimeOfDay`, `IslandAlert.Zone`), Input System(테스트 콘솔 키), uGUI(라벨은 001 `RuntimeHud`)
**Storage**: `Assets/StreamingAssets/NpcTypes/npc_types.json` (유형 규칙 파라미터 + NPC별 배정 목록). 002 `suspicion_rules.json`에 사건 2개 추가.
**Testing**: EditMode 순수 로직 5묶음(`SubdueJudgement`, `NpcTypeRulesValidator`, `SympathizerTurnRule`, `InformerDeliveryState`, `SentinelAlarmGate`).
**Target Platform**: 데스크톱 에디터 Play 모드
**Project Type**: Unity 기능별 어셈블리
**Performance Goals**: NPC ≤ 20, 프레임당 유형 판정 O(N), 경계자 반경 검사 O(N·P).
**Constraints**: 유형·진영·역할·지연·반경 수치는 JSON. 신규 패키지·에셋 0. 밀고자 식별 단서(§9 v0.9)·조사 행동 정의(§2.3) 미포함. 002 코드 변경은 가산적 3건.
**Scale/Scope**: 스크립트 약 14개, JSON 1개(+002 JSON 수정), 테스트 5개, 씬 빌더 1개.

## Constitution Check

| 원칙 | 판정 | 근거 |
|---|---|---|
| I. 분류 | **PASS** | "게임 규칙 기능". 근거 §3 표·§3.1·§3.2 `확정`, §2.4·§2.6 `확정`. |
| I. `[제안]` | **PASS** | 조사 행동 정의(§2.3)는 외부 사건(`NpcTypeSystem.Investigate(actor)`)으로만 받음. 밀고자 식별 단서(§9) 미포함. 경계자 소음 값은 002 `noise`(확정대기) 재사용. 유형 파라미터(지연·반경)는 SDD에 값이 없어 JSON `확정대기`. |
| I. 선행 SDD 인용 | **PASS** | §3.2에 발췌된 v0.4 §3.3 문장만 근거. 진영 3종·판정 규칙을 그대로 옮김. |
| II. 에셋 | **PASS** | 유형은 프리미티브 색·라벨. 신규 에셋 0. |
| III. 독립 테스트 | **PASS** | 순수 로직 5묶음. 전용 씬 `Test_NpcTypes`(유형별 NPC 1개 + 관리자, 플레이어 2, 라벨 숨김 모드, 조사 키). |
| IV. 코옵 | **PASS** | 감시자 +2·밀고자 전달은 "목격된 플레이어" 단위. 동조자 전환은 공유 섬 구간 → 전원 동시. |
| V. 폐기 항목 | **PASS** | 제압 불가 = 물리 반응 없음. 무기 없음. |

**Gate 결과**: 위반 없음.

## Project Structure

### Documentation
```text
specs/003-npc-types/{plan,research,data-model,quickstart,tasks}.md, contracts/{npc-types-schema.json, runtime-api.md}
```

### Source Code
```text
Assets/Scripts/NpcTypes/
├── NpcTypes.asmdef                       # references: PlayFoundation, Suspicion, Unity.InputSystem, UnityEngine.UI
├── Model/
│   ├── NpcTypeDefinitions.cs             # enum NpcType, Faction, 역할 상수
│   ├── NpcTypeRules.cs                   # JSON 모델: rules + assignments
│   ├── NpcTypeRulesValidator.cs          # 순수
│   ├── NpcTypeRulesLoader.cs
│   ├── SubdueJudgement.cs                # 순수: 진영 → 제압 가능 판정 (§3.2)
│   ├── SympathizerTurnRule.cs            # 순수: AlertZone → 전환 여부 (§3 동조자)
│   ├── InformerDeliveryState.cs          # 순수 상태기계: Idle→Waiting→Moving→Delivered / Held
│   └── SentinelAlarmGate.cs              # 순수: 감지·쿨다운
├── Runtime/
│   ├── NpcTypeProfile.cs                 # NPC 컴포넌트: Type, Faction, Roles, IsTurned(동조자)
│   ├── NpcTypeSystem.cs                  # 씬 단일: 로드·배정 적용(002 플래그 설정), Investigate(actor) 릴레이, 동조자 전환 감시
│   ├── NpcTypeEvents.cs
│   ├── InformerBehaviour.cs              # 밀고자: 지연 후 관리자로 이동·전달
│   └── SentinelBehaviour.cs              # 경계자: 감지 → 소리 → 반경 NPC 돌려세움 + noise 사건
└── Debug/
    ├── NpcTypeLabel.cs                   # 유형 라벨 (숨김 모드 지원)
    └── NpcTypeTestConsole.cs             # I 조사, L 라벨 토글
Assets/Editor/NpcTypesSceneBuilder.cs     # Tools/PROJECT 1028/Build NPC Types Test Scene
Assets/Tests/EditMode/NpcTypes/*.cs + asmdef
Assets/StreamingAssets/NpcTypes/npc_types.json
Assets/StreamingAssets/Suspicion/suspicion_rules.json   # +watcher_investigate(확정), +informer_delivery(확정), investigate_in_sight scope→target
```

**Structure Decision**: 002와 같은 패턴. `NpcTypes → Suspicion → PlayFoundation` 단방향. 002 수정은 가산적(새 필드 기본값 false)이라
002 테스트·씬에 영향이 없다.

## Complexity Tracking
없음.

## Constitution Check (Post-Design)

| 원칙 | 판정 | 확인 |
|---|---|---|
| I | **PASS** | 유형 규칙 파라미터 JSON, 감시자 +2는 002 사건 `watcher_investigate`(확정) 데이터. 코드 상수는 enum·구간 비교(Tension 이상)만. |
| II | **PASS** | 에셋 0. |
| III | **PASS** | 순수 5묶음 + 전용 씬. 002 씬 회귀 없음(플래그 기본값). |
| IV | **PASS** | 모든 규칙이 (NPC, 플레이어) 또는 공유 구간 기준. |
| V | **PASS** | `SubdueJudgement`는 판정만. |

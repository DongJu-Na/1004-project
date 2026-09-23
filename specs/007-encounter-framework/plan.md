# Implementation Plan: 인카운터 프레임워크 (Encounter Framework)

**Branch**: `007-encounter-framework` | **Date**: 2026-09-23 | **Spec**: [spec.md](spec.md)

## Summary
인카운터 = 트리거 + NPC 1~2 + 대사 3~5줄 + 결과 플래그(§6.1)를 **데이터로 정의**하고, 이동 중(도보/차량)에만 스폰 규격(§6.4: 런당 2~4, 최소 간격,
차량 확률↑, 섬 전용/공용 풀, 시간대·진행도 조건, 원본 재등장 금지·변주만)에 따라 발생시킨다. 네 유형(§6.2)은 결과 계약이 다르며 D(결과 없음)가
반드시 섞여야 한다. C 유형은 이지선다를 강제한다. 결과 플래그는 런 공유 기록에 남고, 회수 테이블(§6.5)은 플래그로 조회되어 후속 변주를 후보에
넣는다. 개별 20~30개 목록과 회수 내용은 v0.9이므로 샘플 8개(유형별 2)만 `sample:true`로 둔다. 신규 에셋 0(§6.1).

## Technical Context
**Language/Version**: C# (Unity 6000.6.2f1)
**Primary Dependencies**: `PlayFoundation`(PlayerEntity·NpcIdentity·NpcVision·DialogueRunner·DialogueEvents·RuntimeHud·PlayFoundationSceneBuilder 정적 생성), `Suspicion`(SuspicionSystem.Raise·TimeOfDay·IslandAlert.Zone), `Vehicle`(PlayerEntity.IsInVehicle·VehicleController.State), `Subdue`(SubdueEvents.OnSubdued → 중단), Input System, uGUI
**Storage**: `Assets/StreamingAssets/Encounter/encounters.json`(스폰 규격·풀·인카운터 정의·회수 테이블). 002 JSON에 `encounter_stared_at`(B 유형 샘플, 개인 1 확정대기), `encounter_picked_up_runaway`(C 샘플 "태운다" 섬 +20 확정대기) 추가.
**Testing**: EditMode 순수 6묶음(`CandidateFilter`, `WeightedTypePicker`, `SpawnGate`, `ChoiceResolver`, `EncounterRulesValidator`, `RunLedger`)
**Project Type**: 어셈블리 `Encounter → Subdue/Vehicle/Suspicion/PlayFoundation`
**Constraints**: 수치(확률·비율·상한·간격)는 JSON, §6.3 비율 `확정대기`. NPC는 런타임 프리미티브 스폰. 대사는 인라인(001 `DialogueRunner.TryBeginInline` 가산). 저장 없음(이력·플래그는 노출만). 콘텐츠 목록 아님.
**Scale/Scope**: 스크립트 약 18개, JSON 1개(+002 2건), 테스트 6개, 씬 빌더 1개.

## Constitution Check
| 원칙 | 판정 | 근거 |
|---|---|---|
| I. 분류 | PASS | "게임 규칙 기능". §6.1·§6.2·§6.4·§6.5 `확정`, §7·§2.4 조건 `확정`. |
| I. `[제안]` | PASS | §6.3 비율은 `typeWeights`(확정대기). 20~30개 목록·회수 내용은 범위 밖, 샘플은 `sample:true`. B/C 샘플의 상승 수치는 002 표 `확정대기`. |
| II. 에셋 | PASS | §6.1 "신규 제작 0". 샘플 NPC·물건은 프리미티브. 인카운터 정의에 단가(npc 수·줄 수) 검증. |
| III. 독립 테스트 | PASS | 순수 6묶음 + 전용 씬 `Test_Encounter`(도로 1, 차량 1, 트리거 6, 샘플 8) + 런 시뮬레이션 콘솔. |
| IV. 코옵 | PASS | 인카운터는 런 공유 사건, 트리거는 어느 플레이어든, C 선택은 먼저 고른 개체, 플래그 런 공유. |
| V. 폐기 항목 | PASS | 결과에 무기 없음. "개입" 샘플은 004 비살상 제압을 언급만. |

**Gate**: 위반 없음.

## Project Structure
```text
Assets/Scripts/Encounter/
├── Encounter.asmdef                    # refs: PlayFoundation, Suspicion, Vehicle, Subdue, NpcTypes, Unity.InputSystem, UnityEngine.UI
├── Model/
│   ├── EncounterDefinitions.cs         # enum EncounterType {A,B,C,D}, InstancePhase, TriggerKind
│   ├── EncounterRules.cs / EncounterRulesValidator.cs / EncounterRulesLoader.cs
│   ├── CandidateFilter.cs              # 순수: 풀·조건·이력·변주 필터
│   ├── WeightedTypePicker.cs           # 순수: 유형 가중 추출 → 후보 중 하나
│   ├── SpawnGate.cs                    # 순수: 런 상한·최소 간격·이동 중·바쁨·확률
│   ├── ChoiceResolver.cs               # 순수: C 선택/타임아웃 기본
│   └── RunLedger.cs                    # 순수: 플래그·이력·회수 조회·내보내기
├── Runtime/
│   ├── EncounterSystem.cs              # 씬 단일: 로드, 트리거 감시, 스폰, 인스턴스 관리, 중단, 이벤트
│   ├── EncounterTrigger.cs             # 씬 배치 트리거(반경) — 진입 시 1회 판정
│   ├── EncounterInstance.cs            # 인스턴스 상태기계: Spawn → Dialogue → (Choice) → Outcome → End
│   ├── EncounterNpcSpawner.cs          # 프리미티브 NPC 스폰/회수(001 컴포넌트 부착)
│   ├── PlayerActivity.cs               # "조사·정찰 중" 외부 불리언 입력
│   └── EncounterEvents.cs
└── Debug/
    ├── EncounterDebugPanel.cs
    └── EncounterTestConsole.cs         # X 강제 발생(다음 샘플), 1/2 선택, Z 100런 시뮬, S 정찰 토글, N/T 002 콘솔
Assets/Editor/EncounterSceneBuilder.cs
Assets/Tests/EditMode/Encounter/*.cs + asmdef
Assets/StreamingAssets/Encounter/encounters.json
```

가산 수정(001): `DialogueRunner.TryBeginInline(NpcIdentity speaker, string[] lines)`(파일 대신 인라인, 목적지 없음), `DialogueEvents.OnDialogueEnded` 재사용.

## Complexity Tracking
없음.

## Constitution Check (Post-Design)
| 원칙 | 판정 | 확인 |
|---|---|---|
| I | PASS | 확률·비율·상한 JSON. 샘플 `sample:true`. |
| II | PASS | 스폰 NPC 프리미티브, 단가 검증. |
| III | PASS | 순수 6묶음 + 시뮬레이션. |
| IV | PASS | 런 공유 인스턴스·플래그. |
| V | PASS | 해당 없음. |

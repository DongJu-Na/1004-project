# Implementation Plan: 염전섬 그레이박스 수직 슬라이스 (Island Slice)

**Branch**: `008-island-slice` | **Date**: 2026-09-29 | **Spec**: [spec.md](spec.md)

## Summary
001~007을 섬 한 장에 올리고, 플레이어가 "뭘 해야 하는지"와 "지금 얼마나 위험한지"를 항상 알게 만든다. 데이터로 정의한 목표 단계·런 시계
(밤 전환·출항·다음 배)·부두 출항 판정·제한 구역·조사 후 들 수 있는 증거를 추가하고, 기존 시스템의 이벤트 20종을 화면(가장자리 맥동·색조·히트스톱·
카메라 흔들림·문구)과 코드 합성 소리(심박·경보·저음)로 즉시 보여준다. 섬 배치는 JSON에서 읽어 에디터 메뉴 한 번으로 생성한다. 결과 화면과 R 재시작.
신규 에셋 0, 오디오 파일 0.

## Technical Context
**Language/Version**: C# (Unity 6000.6.2f1)
**Primary Dependencies**: `PlayFoundation`·`Suspicion`·`NpcTypes`·`Subdue`·`Report`·`Vehicle`·`Encounter` 전부(이벤트 구독·씬 빌더 정적 메서드 재사용), Input System, uGUI, 내장 Audio 모듈(`AudioClip.Create`로 파형 합성), SceneManagement(재시작)
**Storage**: `Assets/StreamingAssets/Slice/slice_rules.json`(시계·증거·제한구역·출항·목표 단계·반응 매핑·안내 문구), `Assets/StreamingAssets/Slice/island_layout.json`(시설·NPC·트리거·증거·스폰 좌표). 003 `npc_types.json`에 `slice_*` 배정 10건.
**Testing**: EditMode 순수 5묶음(`RunClock`, `ObjectiveTracker`, `DepartureRule`, `HeartbeatRule`, `SliceRulesValidator`). Play 검증은 quickstart.
**Project Type**: 어셈블리 `Slice → (모든 기능 어셈블리)`
**Constraints**: 새 규칙은 시계·출항·제한 구역·증거 조사만. 수치·문구·좌표 전부 JSON. 소리는 코드 합성. 가산 수정: 001 `OrbitCamera.Shake`, 004 `CarriableObject.Locked/IsEvidence`, 006 `VehicleSeats.AllowBusyHands`.
**Scale/Scope**: 스크립트 약 20개, JSON 2개, 테스트 5개, 씬 빌더 1개.

## Constitution Check
| 원칙 | 판정 | 근거 |
|---|---|---|
| I. 분류 | PASS | 통합 기능. 규칙 부분(시계·출항·봉쇄 §2.5, 증거 손 규칙 §4.2, 감시자 조사 §3, 위협 원칙 §1)과 기반 부분(PrototypePlan 4단계 HUD·테스트 씬, 연출)을 스펙 항목마다 표시. |
| I. `[제안]`·선행 조항 | PASS | 출항 시각·다음 배·강제 출도·조사 시간·증거 목격·제한 구역 값은 JSON `확정대기`/해석 표시. 002 사건은 이미 표에 있는 것만 사용(`enter_restricted_area`, `evidence_seen_*`). |
| II. 에셋 | PASS | 프리미티브·코드 합성 소리. 오디오 파일 0. |
| III. 독립 테스트 | PASS | 순수 5묶음. 통합 씬은 이 기능의 목적. |
| IV. 코옵 | PASS | 목표·시계 런 공유, 출항은 전원 부두. 반응 연출은 P1(입력 개체) 기준이나 이벤트 페이로드는 개체별. |
| V. 폐기 항목 | PASS | 실패는 "나갈 수 없음". 무기 없음. |

**Gate**: 위반 없음.

## Project Structure
```text
Assets/Scripts/Slice/
├── Slice.asmdef                          # refs: PlayFoundation, Suspicion, NpcTypes, Subdue, Report, Vehicle, Encounter, Unity.InputSystem, UnityEngine.UI
├── Model/
│   ├── SliceRules.cs / SliceRulesValidator.cs / SliceRulesLoader.cs
│   ├── IslandLayout.cs / IslandLayoutLoader.cs   # 배치 데이터(에디터·런타임 공용 모델)
│   ├── RunClock.cs                       # 순수: 경과·밤·출항·다음 배·단계 전이
│   ├── ObjectiveTracker.cs               # 순수: 단계 목록·완료 조건·진행
│   ├── DepartureRule.cs                  # 순수: 탑승 가능/거부 사유/결과 종류
│   └── HeartbeatRule.cs                  # 순수: (최고 단계, 구간) → 심박 간격
├── Runtime/
│   ├── RunDirector.cs                    # 씬 단일: 규칙 로드, 시계 틱, 밤 전환, 목표, 런 종료·결과, R 재시작
│   ├── ObjectiveHud.cs                   # 상단 목표·거리·시계·안내(30초)
│   ├── DockBoat.cs                       # IInteractable "배 타기" → DepartureRule
│   ├── RestrictedZone.cs                 # 구역 진입 → enter_restricted_area (쿨다운)
│   ├── EvidenceItem.cs                   # IInteractable "조사 (E 길게)" → 완료 시 CarriableObject.Locked=false, 003 Investigate
│   ├── EvidenceWatcher.cs                # 증거 소지 + 보임 → evidence_seen_* (쿨다운)
│   ├── FeedbackDirector.cs               # 이벤트 20종 → 시각·청각 반응 디스패치
│   ├── ScreenFx.cs                       # 가장자리 맥동·색조·중앙 문구·히트스톱
│   ├── ProceduralAudio.cs                # 파형 합성·심박 루프·경보·저음
│   ├── EyeIndicator.cs                   # HUD "보고 있음" 표시
│   └── ResultScreen.cs                   # 결과·통계·R
└── Debug/
    └── SliceTestConsole.cs               # F9 시계 +60s, F10 출항 시각으로, F11 봉쇄 토글, F12 반응 전체 테스트 순환
Assets/Editor/IslandSliceBuilder.cs       # Tools/PROJECT 1028/Build Island Slice (island_layout.json 읽어 생성)
Assets/Tests/EditMode/Slice/*.cs + asmdef
Assets/StreamingAssets/Slice/{slice_rules.json, island_layout.json}
```

가산 수정: 001 `OrbitCamera.Shake(amplitude, seconds)`; 004 `CarriableObject.Locked`(잠김이면 들기 불가)·`IsEvidence`; 006 `VehicleSeats.AllowBusyHands`(정적, 데이터로 설정).

## Complexity Tracking
없음.

## Constitution Check (Post-Design)
| 원칙 | 판정 | 확인 |
|---|---|---|
| I | PASS | 새 규칙 4개는 모두 §2.5·§4.2·§3·§2.3 근거. 연출은 이벤트 소비만. 수치 JSON. |
| II | PASS | 에셋 0, 오디오 파일 0. |
| III | PASS | 순수 5묶음. |
| IV | PASS | 런 공유 시계·목표, 전원 출항. |
| V | PASS | 해당 없음. |

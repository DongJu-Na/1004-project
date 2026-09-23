# Research: 인카운터 프레임워크

## R-1. 대사 표시
- **Decision**: 001 `DialogueRunner`에 `TryBeginInline(NpcIdentity speaker, string[] lines)` 가산. 파일 로드 없이 3~5줄을 같은 UI로 표시하고 `OnDialogueEnded`로 종료를 알린다. 목적지 전달 없음.
- **Rationale**: FR-001 대사는 인카운터 정의 안에 있다. 007 인카운터가 001 표시 방식을 재사용한다는 스펙 의도.

## R-2. C 유형 이지선다
- **Decision**: 대사 진행 중 `choiceAfterLine` 인덱스에서 `EncounterInstance`가 대화를 일시 정지하고 HUD 보조 안내 "1: {A} / 2: {B}"를 띄운다. 키 1/2(콘솔 매핑) → 선택 → 나머지 줄 진행. 순수 `ChoiceResolver`가 타임아웃/이탈 시 `defaultOption`을 적용. 선택 전 진행 불가(FR-007).
- **Alternatives**: 001 대화 UI에 선택지 위젯 추가 — 001 변경 범위 확대. 기각(후속 UI 정리에서 통합 가능).

## R-3. 트리거와 확률
- **Decision**: 씬 배치 `EncounterTrigger`(반경). 플레이어가 반경에 **진입**할 때 1회 판정: `SpawnGate`가 (이동 중: 도보 `MovementState != Idle` 또는 탑승 중 차량 `IsMoving`) && !IsLocked && !대화 중 && !`PlayerActivity.IsScouting` && 진행 중 인스턴스 없음 && 런 상한 미달 && 최소 간격 경과 → 확률(`footChance`/`vehicleChance`) 롤 → 통과 시 후보 선택·스폰. 트리거는 이탈 후 재진입해야 다시 판정.
- **Rationale**: FR-011~FR-014. 프레임마다 롤하면 확률 의미가 없다.

## R-4. 후보 필터와 선택
- **Decision**: 순수 `CandidateFilter.Filter(defs, pools, islandId, phase, zone, flags, history)`: 풀 ∈ {현재 섬, common}, 시간대·구간·플래그 조건, 원본이 이력에 있으면 제외, `variantOf`가 있는 정의는 원본이 이력에 있고 회수 테이블에 (flag ∈ flags → variant) 항목이 있을 때만 포함. 순수 `WeightedTypePicker.Pick(candidates, typeWeights, roll01a, roll01b)`: 존재하는 유형만으로 가중 정규화 → 유형 선택 → 그 유형 후보 중 균등.
- **Rationale**: FR-010·FR-015~FR-017·FR-021.

## R-5. NPC 스폰
- **Decision**: `EncounterNpcSpawner`가 프리미티브 캡슐 + `NpcIdentity`(npcId = `{encounterId}_{i}`) + `NpcVision` + `PrimitiveTint`를 트리거 기준 오프셋에 생성. 종료 후 `despawnDelaySeconds` 뒤 제거. 003 배정은 없음(유형 미지정 → 003 시스템이 있으면 동조자 기본 경고가 뜨므로, 스폰 NPC에 `NpcTypeProfile`을 직접 부착해 Sympathizer/Victim으로 조용히 설정).
- **Rationale**: §6.1 "기존 에셋만". 런타임 생성이라 씬 파일 수정 없음.

## R-6. 유형별 결과 계약
- **Decision**: A: `outcome.flags[]`를 원장에 기록, 상승 사건 금지(검증기가 A에 eventId 있으면 Error). B: `outcome.condition`("in_sight" = 스폰 NPC 중 하나가 플레이어를 `IsSeeing` / "always")이 충족되면 `Raise(Target(eventId, player, npc))`(002 표, 확정대기). C: 선택별 `flags`·선택적 `eventId`(`SuspicionEvent.At` global 등). D: 검증기가 flags·eventId 모두 없음을 강제, 이력만 기록.
- **Rationale**: FR-005~FR-009.

## R-7. 중단
- **Decision**: `SubdueEvents.OnSubdued(actor, npc, ...)`에서 npc가 진행 중 인스턴스의 스폰 NPC면 `Interrupt()`: 대화 종료, 플래그 미기록, 이력만 기록, 004 대가는 그대로(004가 처리).

## R-8. 런 이력·이월
- **Decision**: 순수 `RunLedger`: `Flags`(HashSet), `History`(HashSet), `Import(previousHistory)`, `Export()`. 시스템이 `EndRun` 콘솔(002 R 키)과 별개로 `Snapshot()`을 노출. 저장은 후속.

## R-9. 시뮬레이션
- **Decision**: 콘솔 `Z`: 순수 로직만으로 100런 시뮬(트리거 통과 시퀀스 가정: 도보/차량 교대 12회) → 발생 수 분포·간격 위반·중복·조건 위반 집계를 HUD·콘솔에 출력(SC-001·002·004).

## R-10. 차량 이동 중 판정
- **Decision**: 탑승 중이면 `VehicleController.Instance.State.IsMoving`으로 이동 판정, 확률은 `vehicleChance`. 차량 트리거 진입은 차량 위치가 아닌 플레이어 위치(좌석 자식이라 같다).

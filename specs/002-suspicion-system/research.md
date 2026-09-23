# Research: 의심 시스템과 시간대 위협

**Date**: 2026-09-23 | **Plan**: [plan.md](plan.md)

NEEDS CLARIFICATION 없음. 선택지가 둘 이상이던 항목의 결정 기록.

## R-1. 어셈블리 분리

- **Decision**: `Assets/Scripts/Suspicion/Suspicion.asmdef` 신설, `PlayFoundation` 참조. 에디터 빌더는 기존
  `PlayFoundation.Editor.asmdef`에 `Suspicion` 참조를 추가해 둔다.
- **Rationale**: 헌장 원칙 III — 001 테스트 씬이 002 없이 동작해야 하므로 의존은 단방향이어야 한다. 003~007도 같은
  패턴(`Suspicion` 참조)으로 확장한다.
- **Alternatives**: 같은 어셈블리에 폴더만 추가 — 컴파일은 빠르지만 001이 002 타입을 참조해도 막을 수 없다. 기각.

## R-2. 규칙 데이터 형식

- **Decision**: JSON 한 파일 `suspicion_rules.json`. 상승 사건 표 `events[]` 각 항목에 `id, personalDelta, islandDelta,
  scope("witness"|"radius"|"target"), radius, requiresSight, nightOnly, status("확정"|"확정대기"), note(SDD 조항)`.
  `decay`, `propagation`, `timeOfDay`, `carryOver` 블록 각각에도 `status`.
- **Rationale**: FR-009·FR-010. §2.3 제안값 7개를 초깃값으로 넣되 `확정대기` 표시. §4.4(+30/+40)·§5.2(+25)는 004·005가
  같은 표에 `확정`으로 추가한다. JsonUtility는 딕셔너리를 못 다루므로 배열 + id 조회.
- **Alternatives**: ScriptableObject — 에디터 의존, 텍스트 편집 불가. 기각(001 R-2와 동일 이유).

## R-3. 사건 전달 방식과 대상 범위

- **Decision**: `SuspicionSystem.Instance.Raise(SuspicionEvent)`. 페이로드 `{ EventId, Actor(PlayerEntity), Position,
  TargetNpc(optional) }`. 적용 대상은 표의 `scope`로 결정: `witness` = `NpcVision.IsSeeing(actor)`가 참인 모든 NPC(FR-011),
  `radius` = Position에서 `radius` 안의 모든 NPC(시야 무관, 소음용), `target` = TargetNpc 하나(밀고자 전달·전파용).
  `requiresSight`가 참이면 어느 scope든 시야 조건을 추가한다.
- **Rationale**: §2.3 표의 "시야 안", "근처 NPC만", "반경 내" 세 조건을 데이터로 표현한다. 003·004·005가 코드 변경 없이
  새 사건 id를 JSON에 추가하고 `Raise`만 호출하면 된다.
- **Alternatives**: NPC별 `OnEvent` 콜백 — 대상 선택 로직이 흩어진다. 기각.

## R-4. 시간대 배율 적용 위치

- **Decision**: 개인 의심 상승량에 `timeOfDay.{day,night}Multiplier`를 곱한 뒤 **반올림**해 정수 적용(최소 1, 상승량이
  0이 아닌 경우). 섬 의심도 상승량에는 배율을 적용하지 않는다.
- **Rationale**: §7 "의심 누적: 낮 빠름 / 밤 느림"은 개인 의심(목격자 수·누적)에 관한 표다. 섬 의심도의 큰 값(+25/+30/+40)은
  `확정` 수치이므로 배율로 왜곡하지 않는다. 정수 유지가 §2.1 "0~3" 정의와 맞다.
- **Alternatives**: 실수 누적 후 표시만 정수 — 단계 전환 시점이 불투명해진다. 기각.

## R-5. 밤 이동 기본 의심

- **Decision**: 밤에 NPC가 `IsSeeing(player)`이고 `player.MovementState != Idle`이면 그 (NPC, 플레이어)에 대해
  `night_wander` 사건을 발생시킨다. 같은 쌍에는 `timeOfDay.nightWanderIntervalSeconds`(초깃값 3초) 안에 다시 발생하지
  않는다(쿨다운).
- **Rationale**: §7 "돌아다니는 것 자체가 +1". 프레임마다 +1이면 즉시 3이 되어 의미가 없다. 쿨다운으로 "계속 돌아다니면
  계속 오른다"는 의도를 유지한다.
- **Alternatives**: 시야 진입 1회당 +1 — 서 있어도 오른다. 기각(SDD는 "돌아다님").

## R-6. 전파: "만남"의 정의와 1회성

- **Decision**: 순수 `PropagationScheduler`. NPC 쌍(A,B)이 `propagation.distance` 안으로 들어오면 "만남 시작", 밖으로
  나가면 "만남 종료". 만남 시작 시 A의 개인 의심이 2 이상인 플레이어마다 (A→B, player, executeAt = now+delay)를
  예약하고 그 만남 동안 같은 (A,B,player)는 다시 예약하지 않는다. 예약은 A의 의심이 내려가도 실행된다(Edge case).
  B에는 `target` scope 사건 `propagation`(+1 개인)을 Raise한다. `NpcSuspicionProfile.PropagatesSuspicion == false`(침묵자)면
  A는 예약하지 않는다. `ReportsToManagerImmediately`(밀고자)면 B가 `IsManager`인 경우에만, 지연 0으로 예약한다.
- **Rationale**: §2.6 세 문장을 그대로 데이터·플래그로 옮긴다. FR-017~FR-020.
- **Alternatives**: 주기적 "근처 NPC에 전파" — 붙어 있으면 반복 상승해 FR-018 위반. 기각.

## R-7. 감소

- **Decision**: 순수 `DecayCalculator`. 개인 의심은 마지막 상승 이후 `decay.personalIntervalSeconds`마다 1 감소(0까지).
  섬 의심도는 `decay.islandPerMinute`씩 실수 누적, 정수로 내림해 반영. 감소로 구간이 내려가면 전환 사건 발행(FR-014).
  상승과 감소가 같은 프레임이면 상승 먼저(Edge case).
- **Rationale**: §2.7 "매우 느림". 초깃값은 개인 60초/1, 섬 0.5/분 → 1분 안에 구간 불변(US4 시나리오 1).

## R-8. 런 종료 이월

- **Decision**: 순수 `CarryOverCalculator`. `EndRun()` 호출 시 개인 의심 = `max(0, floor(value * carryOver.personalFactor))`,
  섬 의심도 = `max(carryOver.islandMin, round(value * carryOver.islandFactor))`. 결과를 `CarryOverSnapshot`으로 노출하고
  `SuspicionEvents.OnRunEnded(snapshot)` 발행. 저장은 하지 않는다.
- **Rationale**: §2.7 "부분 감소, 완전 초기화 아님". `islandMin`(초깃값 5)으로 "0이 아닌 값이 남는다"를 보장(FR-022).

## R-9. NPC 단계 표현과 최소 이동

- **Decision**: `NpcMover`(CharacterController 없이 `transform` 이동 + 회전, 목표 지점/대상 추종, 정지 거리, 속도 1.6m/s,
  장애물 회피 없음). `NpcSuspicionBehaviour`가 (NPC의 모든 플레이어 중) **최고 단계**의 플레이어를 기준으로 표현한다:
  1단계 = 몸을 플레이어 쪽으로 회전(쳐다본다), 시야에서 벗어나면 마지막 위치를 향해 회전(돌아본다). 2단계 = 플레이어를
  따라가되 `followDistance`(2.5m) 유지, 접근 시 HUD 라벨에 짧은 말 걸기 문구(데이터 파일 `barks[]`) 표시. 3단계 =
  `OnReportAttempt(npc, player)` 1회 발행 후 표현 중단(신고 흐름은 005). `SuppressStageBehaviour`가 참이면 1·2단계 표현
  억제(003 밀고자).
- **Rationale**: §2.2 행동 표를 최소 비용으로 체감 가능하게 한다. 내비메시(com.unity.ai.navigation은 이미 있음)는 003·005의
  경로 탐색 요구에서 도입한다. 지금은 평지 테스트 씬이라 직선 이동으로 충분하다.
- **Alternatives**: 지금 NavMeshAgent 도입 — 씬 베이크 단계가 생겨 자동 생성 흐름이 복잡해진다. 005로 미룸.

## R-10. 테스트 콘솔과 시간 배속

- **Decision**: `SuspicionTestConsole`이 `Keyboard.current`로 F1~F7(표의 사건 7개를 "카메라가 보는 NPC" 또는 반경으로
  발생), `N` 낮/밤, `R` 런 종료, `T` `Time.timeScale` 1↔10, `[`/`]` 섬 의심도 ±10(디버그 전용 직접 조정). 디버그 직접 조정은
  `SuspicionSystem.DebugAdjustIsland`로 분리해 정상 경로(`Raise`)와 구분한다.
- **Rationale**: FR-027, FR-028, 감소·이월 검증에 시간 배속이 필요하다.

## R-11. 새 플레이어 등록·NPC 소멸

- **Decision**: `PlayerEntity.OnRegistered`로 새 플레이어의 모든 NPC 쌍을 0으로 생성. NPC가 비활성화되어도 딕셔너리
  항목은 유지(Edge case "값은 사라지지 않고 유지"). 키는 `NpcIdentity` 참조이므로 파괴 시 null 키 정리는 `EndRun`/씬 종료에서만.

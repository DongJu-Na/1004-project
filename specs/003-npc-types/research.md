# Research: NPC 유형 5종

## R-1. 감시자 +2와 002 일반 조사 사건의 중복 방지
- **Decision**: `NpcTypeSystem.Investigate(actor)` 릴레이가 actor를 보고 있는 NPC마다 **하나의** 사건만 낸다: 감시자 규칙 대상
  (감시자, 또는 전환된 동조자)이면 `watcher_investigate`(002 JSON, `확정`, personal 2, target, `ignoresTimeMultiplier: true`),
  아니면 `investigate_in_sight`(002 JSON, `확정대기`, target). 이를 위해 002의 `investigate_in_sight` scope를 `witness`→`target`으로
  바꾼다(값·상태 불변, 002 콘솔 F1은 target scope도 처리).
- **Rationale**: witness scope로 두면 감시자가 +2+2=+4를 받는다. §3 "즉시 +2"는 시간대 배율(§7, 확정대기)과 별개의 확정
  규칙이므로 배율을 무시하는 플래그를 사건 규칙에 추가한다(002 가산 확장).
- **Alternatives**: 감시자에게만 별도 경로로 직접 값을 쓰기 — FR-009(표로만 상승) 위반. 기각.

## R-2. 밀고자 전달과 002 전파의 중복 방지
- **Decision**: 밀고자 배정 시 002 플래그를 `PropagatesSuspicion=false`, `ReportsToManagerImmediately=true`(의미 표시),
  `SuppressStageBehaviour=true`로 설정하고, 전달 이동·도착·+1은 003 `InformerBehaviour`가 담당한다. 도착 시
  `informer_delivery`(002 JSON, `확정`, personal 1, target=관리자) 사건을 낸다.
- **Rationale**: 002 전파는 "거리 안 만남"으로만 동작해 걸어가는 연출을 못 만들고, 둘을 함께 켜면 도착 순간 +1이 두 번 된다.
- **Alternatives**: 002 전파에 이동을 넣기 — 002 범위 확대. 기각.

## R-3. 침묵자의 신고 억제와 경계자의 면역
- **Decision**: 002 `NpcSuspicionProfile`에 `SuppressReportAttempt`(신고 시도 사건 미발행), `IgnoresSuspicionEvents`(상승 사건
  무시) 두 플래그를 추가하고 `SuspicionSystem`이 존중한다. 침묵자 = `SuppressReportAttempt + PropagatesSuspicion=false`,
  경계자 = `IgnoresSuspicionEvents + PropagatesSuspicion=false + SuppressReportAttempt`.
- **Rationale**: FR-016·FR-022. 005가 `OnReportAttempt`를 구독하므로 발행 자체를 막는 것이 확실하다. 기본값 false라 002 회귀 없음.

## R-4. 동조자 전환
- **Decision**: 순수 `SympathizerTurnRule.IsTurned(AlertZone) => zone >= Tension`. `NpcTypeSystem`이 `SuspicionEvents.OnIslandZoneChanged`
  구독 + 시작 시 1회 평가해 각 동조자 `NpcTypeProfile.IsTurned`를 갱신, `NpcTypeEvents.OnSympathizerTurned` 발행. 전환 중 조사는
  R-1의 감시자 경로. 진영은 바꾸지 않는다(제압 불가 유지, FR-015).

## R-5. 진영과 제압 판정
- **Decision**: 순수 `SubdueJudgement.Evaluate(Faction) → SubdueVerdict { CanSubdue, PhysicalReaction, Reason }`. Antagonist → 가능,
  Victim/Neutral → 불가·물리 반응 없음. 유형 기본 진영: 감시자 Antagonist, 밀고자·동조자·침묵자 Victim, 경계자 Neutral. 배정에서
  유형과 진영이 상충(감시자+Victim 등)하면 진영 우선 + 경고(Edge case). "중개인"은 역할 문자열이며 Antagonist 권장 경고.

## R-6. 조사 사건 입력
- **Decision**: `NpcTypeSystem.Investigate(PlayerEntity actor)` 공개 메서드 + 테스트 콘솔 `I` 키. 실제 조사 상호작용(E 길게)은
  증거 기능(후속)이 이 메서드를 호출한다.

## R-7. 경계자 소리
- **Decision**: 순수 `SentinelAlarmGate(detectRadius, cooldown)`: 반경 안 플레이어가 있고 쿨다운이 지나면 알람. `SentinelBehaviour`가
  알람 시 `soundRadius` 안 NPC들(`NpcMover.FaceTowards(playerPos)`, 없으면 transform 회전)과 002 `noise` 사건(`At`, radius scope)을
  낸다. 경계자 자신은 `IgnoresSuspicionEvents`.

## R-8. 밀고자 상태기계
- **Decision**: 순수 `InformerDeliveryState`: `Idle → Waiting(since) → Moving → Delivered`, 관리자 없음이면 `Held`(관리자 등장 시 Moving).
  트리거: 어떤 플레이어에 대한 개인 의심 ≥ 2. 대상 플레이어별로 1회 전달, 값이 2 미만으로 내려가면 리셋. 이동 중 재목격은 무시.
  가장 가까운 관리자 선택, 자기 자신 제외(Edge case).

## R-9. 라벨 숨김 모드
- **Decision**: `NpcTypeLabel`이 유형·진영·전환·전달 상태를 표시하고, `NpcTypeTestConsole` `L` 키로 전체 숨김(SC-002 검증). 밀고자는
  숨김 모드에서 002 1·2단계 표현이 억제되어 일반 주민과 구분되지 않는다.

## R-10. 데이터 배정 방식
- **Decision**: `npc_types.json`에 `rules`(유형 파라미터, 각 `status`)와 `assignments[]`(`npcId, type, faction?, roles[]`)를 둔다.
  `NpcTypeSystem.Awake`가 `NpcIdentity.All`을 순회해 `NpcTypeProfile`을 부여·설정한다. 미배정 NPC는 동조자 기본 + 경고.
  테스트 씬 NPC는 대화 파일이 없는 고유 npcId를 쓰므로 001 빌더에 `talkable:false` 오버로드를 추가한다.

# Quickstart: 의심 시스템 검증 가이드

**Date**: 2026-09-23 | [plan.md](plan.md) · [spec.md](spec.md) · [contracts/runtime-api.md](contracts/runtime-api.md)

## 사전 조건

- 001 플레이 기반이 컴파일되고 `Test_PlayFoundation` 씬 검증을 통과했다.
- Unity 6000.6.2f1. 추가 패키지 없음.

## 1. 자동 테스트

`Window > General > Test Runner` → EditMode → Run All. `Suspicion.Tests.EditMode`의 6묶음 녹색:
`PersonalSuspicionTests`(클램프·단계), `IslandAlertTests`(25/50/75 경계·양방향 전환·봉쇄), `SuspicionRulesValidatorTests`
(필수·범위·중복 id·status), `PropagationSchedulerTests`(만남 1회·지연·재만남), `DecayCalculatorTests`, `CarryOverCalculatorTests`(islandMin).

## 2. 테스트 씬

메뉴 `Tools > PROJECT 1028 > Build Suspicion Test Scene` → `Assets/Scenes/Test_Suspicion.unity`.
하이어라키: Ground/Walls/OcclusionWall, RuntimeHud, TimeOfDay, SuspicionSystem, Player_P1(+Camera_P1), Player_P2,
NPC_A(일반), NPC_B(전파 안 함), NPC_M(관리자), SuspicionDebugPanel(HUD), SuspicionTestConsole.
좌상단 패널에 모든 (NPC, 플레이어) 값·단계, 섬 의심도·구간·출항 불가, 시간대, 예약 전파, 확정 대기 건수가 보인다.

## 3. 수동 시나리오

### US1 개인 의심 단계

| 단계 | 조작 | 기대 |
|---|---|---|
| 1 | NPC_A 정면에 서서 F1(`investigate_in_sight`, +2 확정대기) | NPC_A/P1 = 2, NPC_A가 P1을 따라오고 말 걸기 문구 표시 |
| 2 | 뒤로 물러나 NPC_A가 못 보는 곳에서 F1 | 값 변화 없음 (시야 조건) |
| 3 | 다시 정면에서 F1 | 3 → 패널에 "신고 시도 발생" 로그, NPC 표현 중단 |
| 4 | F1 반복 | 3 유지 |
| 5 | 1단계만 올린 뒤(F4 `night_wander`는 밤 전용이므로 F5 `sprint_nearby` 등 +1 사건) 옆으로 지나가기 | NPC가 몸을 돌려 쳐다보고, 시야에서 벗어나면 마지막 위치로 돌아본다 |

### US2 섬 의심도 구간

| 조작 | 기대 |
|---|---|
| `]`를 3회(→30) | 패널 구간 평온→경계, 전환 로그 1회 |
| `]` 계속 76 이상 | 봉쇄, "출항 불가: 예", `OnDepartureBlockedChanged(true)` 로그 |
| `]` 100에서 더 | 100 유지 |
| `[`로 75 이하 | 봉쇄 해제 로그, 출항 불가: 아니오 |
| P2 관점 | P2 슬롯 패널의 섬 의심도가 P1과 같은 값 |

### US3 전파

| 조작 | 기대 |
|---|---|
| NPC_A/P1을 2로 올린 뒤 NPC_A가 P1을 따라 NPC_M 근처(전파 거리 안)로 오게 유도 | 예약 전파 1건 표시 → 지연 후 NPC_M/P1 = 1, `OnPropagated` 로그 |
| 둘이 계속 붙어 있음 | 재전파 없음 |
| NPC_B(전파 안 함)를 2로 올려 NPC_M 근처로 | 전파 없음 |
| NPC_A가 NPC_M에서 멀어졌다 다시 접근 | 새 만남으로 다시 1회 전파 가능 |

### US4 감소·이월

| 조작 | 기대 |
|---|---|
| 섬 60, T로 배속 10, 60초 실제 대기 | 구간 변화 없음(1분 내 불변), 값은 조금 내려감 |
| NPC_A/P1 = 2, 상승 없이 배속 대기 | 주기마다 1씩 내려가 0 |
| 섬 80에서 R | 패널 이월 값: 섬 = max(5, 80×islandFactor), 개인 값 floor(×personalFactor), 0보다 큼 |

### US5 시간대

| 조작 | 기대 |
|---|---|
| N으로 밤 | 패널 "밤" |
| NPC_A 시야 안에서 걷기 | 쿨다운 간격마다 +1 (night_wander) |
| 정지 | 상승 없음 |
| 낮으로 전환 후 걷기 | 상승 없음 |
| P1의 `PlayerDisguise.IsDisguised` 체크 후 밤 | 패널 "위장 유효: 아니오" (밤 무효) |

## 4. 성공 기준

- [ ] EditMode 6묶음 통과
- [ ] SC-001 단계별 NPC 행동 구분 (관찰자 3명)
- [ ] SC-002 경계 전환 30회 정확히 1회씩
- [ ] SC-003 출항 불가 토글 10/10
- [ ] SC-004 전파 10/10, 중복 0
- [ ] SC-005 JSON 값 변경 3건 반영
- [ ] SC-006 P2 공유 섬 의심도 확인, 개인 의심 개체별 상이 사례 1건
- [ ] SC-007 밤/낮 이동 +1 유무 각 10회
- [ ] SC-008 신규 에셋 0

## 문제 시

- 패널에 "규칙 로드 실패": `Assets/StreamingAssets/Suspicion/suspicion_rules.json` 경로·JSON 문법·`status` 누락 확인.
- NPC가 따라오지 않음: `NpcMover`·`NpcSuspicionProfile.SuppressStageBehaviour` 확인.
- F키가 안 먹음: 게임 뷰 포커스(클릭) 후 재시도. macOS는 F키가 시스템 기능키일 수 있어 `fn` 조합 필요.

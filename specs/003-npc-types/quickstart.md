# Quickstart: NPC 유형 검증

## 1. 자동 테스트
Test Runner EditMode → `NpcTypes.Tests.EditMode` 5묶음 녹색: SubdueJudgement, NpcTypeRulesValidator, SympathizerTurnRule, InformerDeliveryState, SentinelAlarmGate.
002 `Suspicion.Tests.EditMode`도 여전히 녹색(가산 확장 회귀 확인).

## 2. 씬
`Tools > PROJECT 1028 > Build NPC Types Test Scene`. NPC: `W 감시자(관리자)`, `I 밀고자`, `S 동조자`, `Q 침묵자`, `D 경계자`, `M 관리자(감시자, 역할 manager)`. 플레이어 P1(입력), P2.

## 3. 시나리오
| US | 조작 | 기대 |
|---|---|---|
| 1 감시자 | W 정면에서 `I` | W/P1 +2 즉시(밤에도 +2). W가 못 보는 곳에서 `I` → 변화 없음. 다시 → 3, 신고 시도 로그 |
| 2 밀고자 | I(밀고자) 정면에서 `I` 두 번(→2) | 밀고자는 쳐다보기·따라오기 없음. 지연 후 M(관리자)로 걸어가 도착 → M/P1 +1, 로그 "전달 완료". `L`로 라벨 숨기면 S와 구분 불가 |
| 3 동조자 | `]`로 섬 51 이상 후 S 정면에서 `I` | +2 즉시(감시자 규칙). 50 이하로 내리면 일반 규칙(`investigate_in_sight`, 배율 적용) |
| 4 침묵자 | Q를 3까지 올림 | 신고 시도 로그 없음. Q(2+)가 다른 NPC 옆에 있어도 전파 없음. 패널 제압 판정 "불가" |
| 5 경계자 | D 감지 반경에 진입 | 로그 "경계자 소리", 반경 안 NPC들이 P1 쪽으로 돌아봄(라벨 "보고 있음"으로 전환), D 자신 의심 0 유지 |
| 6 진영 | 라벨 확인 | W/M=가능, I·S·Q·D=불가. 섬 51+에서도 S 불가 |

## 4. 성공 기준
- [ ] SC-001 유형별 규칙 10회씩 일치 · [ ] SC-002 라벨 숨김 시 밀고자 식별 우연 수준 · [ ] SC-003 제압 판정 표 100% · [ ] SC-004 전환 50↔51 10회 · [ ] SC-005 경계자 소리로 NPC 2개 회전 10/10 · [ ] SC-006 에셋 0

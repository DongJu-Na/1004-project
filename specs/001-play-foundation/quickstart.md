# Quickstart: 플레이 기반 검증 가이드

**Date**: 2026-09-23 | [plan.md](plan.md) · [spec.md](spec.md) · [contracts/runtime-api.md](contracts/runtime-api.md)

## 사전 조건

- Unity **6000.6.2f1** (Unity Hub에서 설치). 프로젝트 폴더 `1004-project-master`를 연다.
- 첫 오픈 시 패키지 임포트를 기다린다. 추가 패키지 설치는 없다.
- 콘솔에 컴파일 오류가 없어야 한다.

## 1. 자동 테스트 (씬 불필요)

1. 메뉴 `Window > General > Test Runner` → **EditMode** 탭 → **Run All**.
2. 기대: `VisionEvaluatorTests`, `DialogueValidatorTests`, `InteractableSelectorTests` 전부 녹색.
   - 시야: 정면·측면 경계·후면·거리 밖 각각 기대값 (FR-016, SC-005)
   - 대화 검증: 2줄→Warning, 3~5줄→OK, 6줄→Warning, 0줄→Error (FR-014)
   - 대상 선택: 카메라 전방 최근접, 거리 밖 제외, 동률 시 최근접 (Edge case)

## 2. 테스트 씬 생성

1. 메뉴 `Tools > PROJECT 1028 > Build Play Foundation Test Scene (2 Players)`.
2. `Assets/Scenes/Test_PlayFoundation.unity`가 열린다. 하이어라키에 Directional Light, Ground, Walls, OcclusionWall, RuntimeHud, Player_P1, Camera_P1,
   Player_P2(2 Players일 때), NPC_dock_worker, NPC_dock_worker_2가 있어야 한다. NPC 2개는 상호작용 대상 선택
   규칙(카메라가 보는 쪽 우선) 확인용이며 같은 대화 파일을 쓴다.
3. 씬은 언제든 같은 메뉴로 재생성할 수 있다(덮어쓰기).

## 3. 수동 검증 시나리오 (Play 모드)

### US1 이동·카메라 (60초)

| 단계 | 조작 | 기대 결과 |
|---|---|---|
| 1 | WASD | 카메라 방향 기준 이동, 캐릭터가 이동 방향으로 부드럽게 회전 (FR-001, FR-003) |
| 2 | Shift + WASD | 눈에 띄게 빨라짐, 떼면 걷기 (FR-002) |
| 3 | 마우스 좌우·상하 | 궤도 회전, 상하는 바닥 아래·머리 위 넘지 않음 (FR-004) |
| 4 | 벽에 등을 대고 카메라를 벽 쪽으로 | 카메라가 캐릭터 쪽으로 당겨짐, 벽 뚫고 보지 않음 (FR-005) |
| 5 | 키를 뗌 | 0.5초 안에 정지, 미끄러짐 없음 (FR-006) |
| 6 | 바닥 가장자리 밖으로 떨어짐 | 시작 위치로 복귀 (Edge case) |

### US2 대화·목적지 (2분 안에 안내 없이)

| 단계 | 조작 | 기대 결과 |
|---|---|---|
| 1 | NPC에 접근 | 화면에 "말하기 [E]" 안내, 멀어지면 사라짐 (FR-008) |
| 2 | E | 첫 줄 표시, 이동·카메라 잠김 (FR-009, FR-012). 첫 줄이 건너뛰어지지 않음 |
| 3 | E 반복 | 파일 순서대로 다음 줄 (FR-011) |
| 4 | 마지막 줄에서 E | 대화창 닫힘, 이동 복귀, 씬에 "관리소" 마커 생성 (FR-013) |
| 5 | 같은 NPC에게 다시 E | 처음부터 다시, 마커는 하나 (FR-013) |
| 6 | Play 종료 → `StreamingAssets/Dialogue/npc_dock_worker.json` 문구 수정 → Play | 바뀐 문구 표시 (SC-004) |
| 7 | 같은 파일의 lines를 2줄로 → Play → 대화 | 화면 경고 로그 "3~5줄 규격 위반", 대화는 진행 (FR-014) |

### US3 시야 (NPC 머리 위 라벨과 Gizmo 확인)

| 배치 | 기대 라벨 |
|---|---|
| NPC 정면, 거리 안, 가림 없음 | "P1: 보고 있음" |
| NPC 정면, 사이에 벽 | "P1: 안 보임" |
| NPC 바로 뒤 | "P1: 안 보임" |
| 정면에서 거리 밖으로 후진 | "보고 있음" → "안 보임" |
| 부채꼴 가장자리에 정지 | 1초 안에 2회 이상 바뀌지 않음 (FR-017) |
| 5초 대기 | NPC 위치·방향·대사 변화 없음 (FR-018) |

씬 뷰에서 NPC 선택 시 시야 부채꼴·거리가 Gizmo로 보인다 (FR-019).

### US4 코옵 동등성 (2 Players 씬)

| 단계 | 기대 결과 |
|---|---|
| P2는 NPC 후면에, P1을 정면으로 이동 | NPC 라벨 "P1: 보고 있음 / P2: 안 보임" (FR-021) |
| P1이 대화 중 | P2의 `IsLocked`는 false (인스펙터 확인). P2는 잠기지 않음 |
| 1 Player 씬으로 재생성 후 같은 시나리오 | P1 결과 동일 (FR-022) |

## 4. 성공 기준 체크

- [ ] EditMode 테스트 전부 통과
- [ ] US1 6단계 전부 기대와 일치, 관찰자 "자연스럽다" 동의 (SC-001, SC-002)
- [ ] US2 처음 시도 2분 내 완료 (SC-003), 파일 수정 반영 (SC-004)
- [ ] US3 네 배치 각 10회 100% 일치, 경계 깜빡임 없음 (SC-005)
- [ ] US4 개체별 다른 판정 재현, 대화 중 다른 개체 이동 가능 (SC-006)
- [ ] `Assets/` 안에 신규 모델·텍스처·오디오·애니메이션 없음 (SC-007)

## 문제 시 확인

- 컴파일 오류: 콘솔 첫 오류를 그대로 Claude Code에 붙여 넣으면 된다. 이 코드는 Unity가 없는 환경에서 작성되어
  최초 컴파일은 집 PC에서 처음 이뤄진다.
- 씬이 어둡거나 오브젝트가 회색: `PrimitiveTint`가 URP Lit 셰이더를 찾지 못한 경우. URP 설정(`Assets/Settings`)이 활성인지 확인.

- 안내가 안 뜸: Player_P1에 `PlayerInput`이 있고 Actions가 `InputSystem_Actions`인지, Default Map이 `Player`인지.
- 대화가 시작 안 됨: 콘솔 경고에 파일 경로. `Assets/StreamingAssets/Dialogue/npc_dock_worker.json` 존재 확인.
- 카메라가 안 돎: 마우스가 게임 뷰 위에 있어야 Look 델타가 들어온다. Play 후 게임 뷰를 한 번 클릭.

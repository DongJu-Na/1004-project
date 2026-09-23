# Assets/Scripts — PROJECT 1028 런타임 코드

어셈블리 `PlayFoundation` (asmdef: `Assets/Scripts/PlayFoundation.asmdef`), 네임스페이스 `Project1028.PlayFoundation`.
에디터 전용 코드는 `Assets/Editor/` (`PlayFoundation.Editor`), EditMode 테스트는 `Assets/Tests/EditMode/`.

## 폴더

| 폴더 | 기능 | 내용 |
|---|---|---|
| `Player/` | 001 | `ThirdPersonMotor`(이동), `PlayerEntity`(개체·잠금·목적지), `OrbitCamera`, `FallRespawn` |
| `NPC/` | 001 | `NpcIdentity`(뼈대), `NpcVision`+`VisionEvaluator`("보고 있는가"), `NpcTalkInteractable` |
| `Interaction/` | 001 | `IInteractable`, `InteractableSelector`(순수 로직), `InteractionDetector` |
| `Dialogue/` | 001 | `DialogueData/Validator/Loader/Runner/Events` |
| `World/` | 001 | `Destination`, `DestinationMarker`, `PrimitiveTint` |
| `UI/` | 001 | `RuntimeHud`(코드 생성 캔버스), `VisionDebugLabel` |
| `Suspicion/` | 002 | 어셈블리 `Suspicion`(→PlayFoundation). `Model/` 순수 로직(개인 의심·섬 의심도·규칙 검증·전파·감소·이월), `Runtime/`(`SuspicionSystem`·`TimeOfDay`·`NpcMover`·`NpcSuspicionBehaviour`·`NpcSuspicionProfile`·`PlayerDisguise`), `Debug/`(패널·콘솔) |
| `NpcTypes/` | 003 | 어셈블리 `NpcTypes`(→Suspicion→PlayFoundation). 유형·진영·역할 데이터 배정, 감시자 조사 릴레이, 밀고자 전달, 동조자 전환, 침묵자·경계자 플래그, `SubdueJudgement`(004 전제) |
| `Subdue/` | 004 | 어셈블리 `Subdue`(→NpcTypes). 손 상태(`PlayerHands`), 일상 물건(`CarriableObject`), 제압 세 동작·기절·운반·대가(`SubdueSystem`, `UnconsciousState`), 순수 `HandRules/BackConeCheck/ShoveRoll/UnconsciousTimer/CostLedger` |
| `Report/` | 005 | 어셈블리 `Report`(→Subdue). 신고 지점(`ReportPoint`), 신고 흐름(`ReportFlow`/`ReportSystem`), 전화선 끊기(`PhoneCutInteractable`/`PlayerCutter`/`PlayerToolkit`), 순수 `ReportPointSelector/ReportFlowState/CutProgress/ReportSpeedRule` |
| `Vehicle/` | 006 | 기반 기능. 어셈블리 `Vehicle`(→PlayFoundation만). Rigidbody 아케이드 주행(`VehicleController`), 좌석(`VehicleSeats`), 탑승(`VehicleEnterInteractable`), 입력(`VehicleDriverInput`), 순수 `SeatAssignment/ExitRule/FlipDetector` |
| `Encounter/` | 007 | 어셈블리 `Encounter`(→Subdue/Vehicle/Suspicion). 인카운터 정의·풀·회수 테이블(JSON), 트리거·스폰 게이트·후보 필터·가중 선택(순수), 인스턴스 상태기계(대화→선택→결과), 런 원장 |
| `Mission/`, `Save/` | 예약 | PrototypePlan 4·5·2단계용 빈 폴더 |

후속 기능은 `Suspicion/`(002), `Subdue/`(004), `Encounter/`(007) 등으로 나란히 추가한다.

## 테스트 씬

메뉴 `Tools > PROJECT 1028 > Build Play Foundation Test Scene` 또는 `(2 Players)` →
`Assets/Scenes/Test_PlayFoundation.unity` 생성. 프리미티브만 사용. 검증 절차는 `specs/001-play-foundation/quickstart.md`.

## 후속 기능이 구독하는 공개 표면

계약 문서: `specs/001-play-foundation/contracts/runtime-api.md`

- `PlayerEntity.All`, `.MovementState`, `.IsLocked`, `.Lock/Unlock(owner)`, `OnRegistered/OnUnregistered`
- `NpcIdentity.All`, `.EyePosition`, `.Forward`
- `NpcVision.IsSeeing(player)`, `.SeenPlayers`, `OnSeeingChanged(npc, player, seeing)`
- `IInteractable` (조사·탑승·끊기 구현 지점)
- `DialogueEvents.OnDialogueStarted/OnLineAdvanced/OnDialogueEnded/OnDestinationReceived`
- `DialogueRunner.TryBegin(npc)` — 007 인카운터가 3~5줄 대사 표시에 재사용
- `RuntimeHud.Warn(msg)`, `OnWarning`

## 데이터

`Assets/StreamingAssets/Dialogue/<npcId>.json` — 스키마 `specs/001-play-foundation/contracts/dialogue-schema.json`.
3~5줄 규격(SDD §6.1)을 벗어나면 경고, 0줄·파일 없음·파싱 실패는 대화 시작 불가.

## 헌장 준수 메모

- 원칙 I: 이 폴더의 001 코드에 의심·진영·위협 관련 타입·필드 없음.
- 원칙 II: 모델·텍스처·오디오·애니메이션 없음. Material은 런타임 생성.
- 원칙 V: 입력 액션 `Attack` 미사용. 무기·체력 타입 없음.

## 002 의심 시스템 사용법 (003~007)

- 상승은 오직 `SuspicionSystem.Instance.Raise(SuspicionEvent.Witness/Target/At(id, actor, ...))`.
- 새 사건은 `Assets/StreamingAssets/Suspicion/suspicion_rules.json`의 `events[]`에 `status:"확정"`으로 추가하고 코드에서는 id만 호출한다.
- 구독: `SuspicionEvents.OnReportAttempt`(005), `OnIslandZoneChanged`/`Island.Zone`(003 동조자·007 조건), `OnDepartureBlockedChanged`(출항).
- NPC 예외 플래그: `NpcSuspicionProfile`(003이 유형에 따라 설정). 없으면 "일반".
- 테스트 씬: `Tools > PROJECT 1028 > Build Suspicion Test Scene`. 계약: `specs/002-suspicion-system/contracts/runtime-api.md`.

## 003 NPC 유형 사용법 (004~007)

- 제압 가능 판정: `SubdueJudgement.Evaluate(npc)` — 진영만 본다(Antagonist만 가능). 004가 사용.
- 조사 입력: `NpcTypeSystem.Instance.Investigate(actor)` — 보고 있는 NPC마다 사건 하나(감시자 규칙이면 `watcher_investigate`, 아니면 `investigate_in_sight`).
- 배정 데이터: `Assets/StreamingAssets/NpcTypes/npc_types.json` `assignments[]`에 npcId별 type/faction/roles.
- 002 가산 확장(003에서 추가): `SuspicionEventRule.ignoresTimeMultiplier`, `NpcSuspicionProfile.SuppressReportAttempt/IgnoresSuspicionEvents`, 사건 `watcher_investigate`·`informer_delivery`, `investigate_in_sight` scope→target, 억제 상태 `NpcSuspicionBehaviour`는 Stop 1회만.
- 테스트 씬: `Tools > PROJECT 1028 > Build NPC Types Test Scene`. 키 `I` 조사, `L` 라벨 토글.

## 004 제압 시스템 사용법 (005~007)

- 기절 여부: `UnconsciousState.IsUnconscious(npc)`. 깨어남: `SubdueEvents.OnUnconsciousWake` 또는 002 `OnReportAttempt`(깨어남 시 002가 발행).
- 손 상태: `PlayerHands.State/HeldKind`. 두 손 점유 시 `PlayerEntity.HandsBusy`가 참 → 대화 등 거부.
- 가산 확장: 001 `ThirdPersonMotor.SpeedMultiplier/SprintAllowed`, `PlayerEntity.HandsBusy`, `InteractionDetector`가 비활성 컴포넌트 건너뜀,
  `RuntimeHud.ShowSecondaryPrompt`; 002 scope `global`(섬 값만), `SuspicionSystem.ForceReportAttempt`, 사건 7개.
- 입력(테스트 매핑): E 들기/옮기기, F 문맥 제압, G 내려놓기. 정식 액션 바인딩은 후속.
- 테스트 씬: `Tools > PROJECT 1028 > Build Subdue Test Scene`.

## 005 신고 시나리오 사용법 (레벨·007)

- 레벨에 `ReportPoint`(Phone/Office)를 배치한다. 전화기에는 `PhoneCutInteractable`을 함께 붙인다. 관리소는 끊을 수 없다.
- 입력은 002 `OnReportAttempt`만. 유형·침묵자 처리는 002/003 플래그. 중단은 004 기절, 재시작은 깨어남 이벤트.
- 002 사건 `report_completed`(섬 25, 확정). 입력 키: E 끊기(정지 유지), K 도구 토글, O 최근접 전화기 즉시 끊기(디버그).
- 테스트 씬: `Tools > PROJECT 1028 > Build Report Test Scene`.

## 006 차량 사용법 (002·007)

- 상태 읽기: `VehicleController.Instance.State`(Speed·IsMoving·EngineOn·DriverId·PassengerId). 규칙 적용은 002/007이 한다.
- 탑승 여부: `PlayerEntity.IsInVehicle`. 탑승 중 001 안내·도보 이동은 꺼지고 카메라는 차량 궤도.
- 가산 확장(001): `PlayerEntity.IsInVehicle`, `OrbitCamera.SetTarget`, `FallRespawn`·`ThirdPersonMotor`의 탑승 중 건너뜀.
- 입력: E 탑승/하차, W/S 가속·후진, A/D 조향, Shift 제동, Space 복구, V(디버그) P2 동승 토글. 테스트 씬: `Build Vehicle Test Scene`.

## 007 인카운터 사용법 (v0.9 콘텐츠·문서·저장)

- 콘텐츠 추가 = `Assets/StreamingAssets/Encounter/encounters.json`의 `encounters[]`·`pools[]`·`callbacks[]`에 항목 추가(`sample:false`). 코드 변경 없음. 단가(NPC 수·줄 수)는 HUD에 집계된다.
- 결과 계약: A 플래그만 / B 조건부 002 사건 / C 이지선다(prompt·optionA/B·defaultOption·choiceAfterLine) / D 무결과. 검증기가 강제.
- 회수: `EncounterSystem.Instance.CallbacksFor(flag)` 조회만. 변주 후보 포함은 필터가 자동 처리. 실행(문서 변화 등)은 후속.
- 런 이월: `Ledger.Export()` / `Import(history)`. 002 런 종료(R)에 스냅샷 이벤트.
- 가산 확장: 001 `DialogueRunner.TryBeginInline/Pause/Resume/Abort`; 006 `VehicleDriverInput`은 대화 중 입력 무시; 002 사건 `encounter_stared_at`·`encounter_picked_up_runaway`.
- 입력: 도로 트리거 진입(도보/차량), X 샘플 강제, 1/2 선택, S 정찰 토글, Z 100런 시뮬. 테스트 씬: `Build Encounter Test Scene`.

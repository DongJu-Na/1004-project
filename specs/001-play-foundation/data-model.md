# Data Model: 플레이 기반 (Play Foundation)

**Date**: 2026-09-23 | **Plan**: [plan.md](plan.md) | **Spec entities**: [spec.md#key-entities](spec.md)

게임 규칙(의심·진영·위협) 관련 필드는 의도적으로 없다(헌장 원칙 I).

## 런타임 엔티티

### PlayerEntity (MonoBehaviour)

| 필드 | 타입 | 설명 | 검증 |
|---|---|---|---|
| Id | string | 씬 내 고유 식별자 ("P1", "P2") | 비어 있지 않음, 씬 내 유일 |
| MovementState | enum {Idle, Walking, Sprinting} | `ThirdPersonMotor`에서 파생 | 매 프레임 갱신 |
| IsLocked | bool | 대화 등으로 이동·카메라 잠김 | Lock/Unlock 쌍으로만 변경 |
| Camera | OrbitCamera (nullable) | 자기 카메라. 입력 없는 개체는 null 가능 | |
| ReceivedDestinations | IReadOnlyCollection<Destination> | 수령한 목적지 | (플레이어, NPC) 중복 없음 |
| SpawnPosition | Vector3 | 낙하 복귀 지점 | 씬 시작 시 기록 |

관계: 1 PlayerEntity → 0..1 OrbitCamera, 1 InteractionDetector, 1 DialogueRunner, 1 FallRespawn, 1 ThirdPersonMotor.

### NpcIdentity (MonoBehaviour)

| 필드 | 타입 | 설명 | 검증 |
|---|---|---|---|
| NpcId | string | 대화 파일명과 일치 (`<NpcId>.json`) | 비어 있지 않음 |
| DisplayName | string | 안내·라벨용 | |
| Forward | Vector3 (파생) | transform.forward | |
| EyeHeight | float | 시야 원점 높이 | > 0 |

이 스펙에서 NPC는 이동하지 않는다. 관계: 1 NpcIdentity → 1 NpcVision, 1 NpcTalkInteractable.

### NpcVision (MonoBehaviour)

| 필드 | 타입 | 설명 | 검증 |
|---|---|---|---|
| FovDegrees | float | 정면 기준 전체 부채꼴 각도 | 0 < x ≤ 180 |
| Range | float | 시야 거리 | > 0 |
| OcclusionMask | LayerMask | 가림 판정 레이어 | |
| StableSeconds | float | 상태 전환 안정 시간 | ≥ 0, 기본 0.15 |
| states | Dictionary<PlayerEntity, VisionState> | 플레이어별 상태 | 플레이어 등장 시 자동 생성 |

**VisionState** (순수 C#): `RawSeeing: bool`, `PublicSeeing: bool`, `RawSince: float`.

상태 전환: `Raw`가 바뀌면 `RawSince = now`. `Raw != Public`이고 `now - RawSince ≥ StableSeconds`이면 `Public = Raw`,
`OnSeeingChanged(npc, player, Public)` 발행.

### VisionEvaluator (순수 C# static)

`Evaluate(Vector3 eyePos, Vector3 forward, Vector3 targetPos, float fovDeg, float range) → VisionGeometryResult
{ InRange, InFov, HorizontalAngleDeg, Distance }`. 수직 성분은 무시하고 수평 각도로 판정한다(둔덕 위 플레이어도
정면이면 본다).

### IInteractable (interface)

| 멤버 | 타입 | 설명 |
|---|---|---|
| PromptText | string | 안내 문구 ("말하기") |
| InteractionRange | float | 이 대상의 상호작용 거리 |
| WorldPosition | Vector3 | 선택 계산용 |
| CanInteract(PlayerEntity) | bool | 잠김 등 사전 조건 |
| Interact(PlayerEntity) | void | 실행 |

구현: `NpcTalkInteractable` (대화 시작 요청). 후속 기능이 조사·탑승 등을 추가한다.

### InteractableSelector (순수 C# static)

`Pick(IReadOnlyList<Candidate> candidates, Vector3 playerPos, Vector3 cameraForward) → Candidate?`
Candidate = { Position, Range, Payload }. 규칙: `distance ≤ Range`인 후보만, 카메라 전방과의 수평 각도 최소, 동률(±1°)
이면 거리 최소.

## 데이터 파일 엔티티

### DialogueData (JSON ↔ C# [Serializable])

| 필드 | 타입 | 설명 | 검증 (DialogueValidator) |
|---|---|---|---|
| npcId | string | NpcIdentity.NpcId와 일치 | 필수 |
| lines | string[] | 대사 줄, 순서 있음 | **3 ≤ length ≤ 5** 아니면 Warning("§6.1 3~5줄 규격 위반"), 0줄이면 Error |
| destination | DestinationData | 대화 종료 시 전달 | 필수 |

### DestinationData

| 필드 | 타입 | 설명 | 검증 |
|---|---|---|---|
| name | string | 표시 이름 | 필수 |
| x, y, z | float | 씬 좌표 | 유한값 |

`DialogueValidator.Validate(DialogueData) → ValidationResult { IsError, Messages[] }`. Error면 대화 시작 불가 + HUD
경고. Warning이면 진행하되 HUD 경고.

스키마 파일: [contracts/dialogue-schema.json](contracts/dialogue-schema.json).

## 런타임 상태 엔티티

### DialogueSession (순수 C# 클래스, DialogueRunner 내부)

| 필드 | 타입 | 설명 |
|---|---|---|
| Player | PlayerEntity | 진행 주체 |
| Npc | NpcIdentity | 상대 |
| Data | DialogueData | 로드된 대화 |
| LineIndex | int | 0..lines.Length-1 |
| StartedFrame | int | 첫 줄 건너뛰기 방지 |

상태 전이: `None → Active(LineIndex=0)` on Begin (플레이어 Lock) → `Active(i+1)` on Advance → `Ended` when
i = last & Advance (플레이어 Unlock, Destination 전달, `OnDialogueEnded`).

### Destination (런타임 레코드)

| 필드 | 타입 | 설명 | 검증 |
|---|---|---|---|
| Name | string | | |
| Position | Vector3 | | |
| SourceNpcId | string | 출처 NPC | |
| Receiver | PlayerEntity | 수령자 | (Receiver, SourceNpcId) 유일 |
| Marker | DestinationMarker | 씬 마커 | 하나만 유지 |

## 이벤트 (DialogueEvents / NpcVision)

후속 기능이 구독하는 공개 이벤트. 시그니처는 [contracts/runtime-api.md](contracts/runtime-api.md).

- `NpcVision.OnSeeingChanged(NpcIdentity npc, PlayerEntity player, bool seeing)`
- `DialogueEvents.OnDialogueStarted(NpcIdentity, PlayerEntity)`
- `DialogueEvents.OnLineAdvanced(NpcIdentity, PlayerEntity, int lineIndex)`
- `DialogueEvents.OnDialogueEnded(NpcIdentity, PlayerEntity)`
- `DialogueEvents.OnDestinationReceived(PlayerEntity, Destination)`
- `RuntimeHud.OnWarning(string)` (테스트 씬 경고 로그)

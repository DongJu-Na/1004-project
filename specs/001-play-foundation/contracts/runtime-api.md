# Runtime API Contract: 플레이 기반 → 후속 기능

**Date**: 2026-09-23 | 이 문서는 002 의심 시스템 이후의 기능이 **읽기만** 하는 공개 표면을 정한다. 여기 없는 내부
멤버는 바뀔 수 있다. 게임 규칙 타입·필드는 이 계약에 없다(헌장 원칙 I).

## 네임스페이스

모든 런타임 타입은 `Project1028.PlayFoundation` 아래에 둔다. 어셈블리: `PlayFoundation`.

## 1. 플레이어 개체

```csharp
public sealed class PlayerEntity : MonoBehaviour
{
    public string Id { get; }
    public MovementState MovementState { get; }        // Idle | Walking | Sprinting
    public bool IsLocked { get; }
    public Vector3 Position { get; }                   // transform.position
    public Vector3 Forward { get; }                    // transform.forward
    public OrbitCamera Camera { get; }                 // null 가능 (입력 없는 개체)
    public IReadOnlyCollection<Destination> ReceivedDestinations { get; }

    public void Lock(object owner);                    // 이동·카메라 잠금 (재진입 안전: 소유자 집합)
    public void Unlock(object owner);

    public static IReadOnlyList<PlayerEntity> All { get; }   // 씬 내 활성 개체 (등록 순)
    public static event Action<PlayerEntity> OnRegistered;
    public static event Action<PlayerEntity> OnUnregistered;
}
public enum MovementState { Idle, Walking, Sprinting }
```

후속 002는 `MovementState`와 `All`을 읽어 "밤에 이동" 사건을 만든다. Lock은 004·005·006이 재사용한다.

## 2. NPC 뼈대와 시야

```csharp
public sealed class NpcIdentity : MonoBehaviour
{
    public string NpcId { get; }
    public string DisplayName { get; }
    public Vector3 Position { get; }
    public Vector3 Forward { get; }
    public Vector3 EyePosition { get; }
    public static IReadOnlyList<NpcIdentity> All { get; }
}

public sealed class NpcVision : MonoBehaviour
{
    public float FovDegrees { get; }
    public float Range { get; }
    public bool IsSeeing(PlayerEntity player);         // 공개(안정화된) 상태
    public bool IsSeeingAny { get; }
    public IEnumerable<PlayerEntity> SeenPlayers { get; }

    /// npc, player, seeing(true=보기 시작, false=놓침). 안정 시간 경과 후에만 발행.
    public event Action<NpcIdentity, PlayerEntity, bool> OnSeeingChanged;
}
```

보장: `IsSeeing`은 이 기능 안에서 NPC의 어떤 행동도 바꾸지 않는다(FR-018). 002가 이 값을 상승 사건의 "시야 안"
조건으로 소비한다. 004는 `IsSeeing(actor) == false`를 "인지하지 못한 상태"로 쓴다.

## 3. 상호작용

```csharp
public interface IInteractable
{
    string PromptText { get; }
    float InteractionRange { get; }
    Vector3 WorldPosition { get; }
    bool CanInteract(PlayerEntity player);
    void Interact(PlayerEntity player);
}

public sealed class InteractionDetector : MonoBehaviour
{
    public IInteractable Current { get; }              // 현재 안내 대상 (없으면 null)
    public event Action<PlayerEntity, IInteractable> OnTargetChanged;
}
```

후속 기능(조사·탑승·끊기)은 `IInteractable`을 구현해 얹는다. 선택 규칙: 각 후보의 `InteractionRange` 안 + 카메라
전방 최근접 각도.

## 4. 대화

```csharp
public static class DialogueEvents
{
    public static event Action<NpcIdentity, PlayerEntity> OnDialogueStarted;
    public static event Action<NpcIdentity, PlayerEntity, int> OnLineAdvanced;   // 새로 표시된 줄 인덱스
    public static event Action<NpcIdentity, PlayerEntity> OnDialogueEnded;
    public static event Action<PlayerEntity, Destination> OnDestinationReceived;
}

public sealed class DialogueRunner : MonoBehaviour     // PlayerEntity당 1개
{
    public bool IsActive { get; }
    public NpcIdentity CurrentNpc { get; }
    public int CurrentLineIndex { get; }
    public bool TryBegin(NpcIdentity npc);             // 로드·검증 실패 시 false + HUD 경고
    public void Advance();                             // 마지막 줄이면 종료
}
```

대화 중 플레이어는 `Lock(runner)` 상태. 007 인카운터는 `DialogueRunner.TryBegin`을 재사용해 3~5줄 대사를 표시한다.

## 5. 목적지

```csharp
public sealed class Destination
{
    public string Name { get; }
    public Vector3 Position { get; }
    public string SourceNpcId { get; }
    public PlayerEntity Receiver { get; }
}
```

(Receiver, SourceNpcId) 중복 수령 시 마커는 새로 만들지 않는다.

## 6. 테스트 씬 진단

```csharp
public sealed class RuntimeHud : MonoBehaviour
{
    public static RuntimeHud Instance { get; }
    public void ShowPrompt(PlayerEntity p, string text);   // null/빈 문자열이면 숨김
    public void ShowDialogueLine(PlayerEntity p, string speaker, string line, int index, int total);
    public void HideDialogue(PlayerEntity p);
    public void Warn(string message);                      // 화면 로그 + Debug.LogWarning
    public static event Action<string> OnWarning;
}
```

## 7. 입력 액션 사용 계약

| 액션 (Player 맵) | 사용처 | 비고 |
|---|---|---|
| Move | ThirdPersonMotor | 기존 |
| Sprint | ThirdPersonMotor | 기존 |
| Jump | ThirdPersonMotor | 기존 유지(스펙 Assumptions) |
| Look | OrbitCamera | 신규 사용 |
| Interact | InteractionDetector / DialogueRunner | 신규 사용. 시작·진행 겸용 |
| Attack | **사용 금지** | 헌장 원칙 V. 바인딩을 읽지 않는다 |
| Crouch, Previous, Next | 미사용 | |

## 8. 데이터 파일 계약

`Assets/StreamingAssets/Dialogue/<npcId>.json` — [dialogue-schema.json](dialogue-schema.json). 로더는 파일 없음·
파싱 실패·0줄을 Error(대화 시작 불가), 3~5줄 밖을 Warning(진행)으로 보고한다.

## 9. 에디터 계약

메뉴 `Tools/PROJECT 1028/Build Play Foundation Test Scene` → `Assets/Scenes/Test_PlayFoundation.unity` 생성·덮어쓰기.
메뉴 `Tools/PROJECT 1028/Build Play Foundation Test Scene (2 Players)` → 두 번째 개체 포함.

## 10. 후속 기능이 추가한 가산 표면 (2026-09-23)
- 002: `RuntimeHud.CreateFixedText(name, anchor, anchoredPosition, size, fontSize, alignment)`, `RuntimeHud.CreateWorldLabel`.
- 003: `PlayFoundationSceneBuilder.CreateNpc(..., bool talkable)`, `RuntimeHud.Start` 사전 대화 검증을 대화 가능 NPC로 한정.
- 004: `ThirdPersonMotor.SpeedMultiplier`·`SprintAllowed`, `PlayerEntity.HandsBusy`, `NpcTalkInteractable.CanInteract`가 HandsBusy 거부, `InteractionDetector`가 비활성 컴포넌트 건너뜀, `RuntimeHud.ShowSecondaryPrompt`.
- 006: `PlayerEntity.IsInVehicle`, `OrbitCamera.SetTarget(Transform, float)`, `InteractionDetector`(탑승 중 안내 없음), `FallRespawn`·`ThirdPersonMotor`(탑승 중 건너뜀).
- 007: `DialogueRunner.TryBeginInline(NpcIdentity, string[])`, `.Pause()/.Resume()/.IsPaused`, `.Abort()`(이벤트 없이 종료).

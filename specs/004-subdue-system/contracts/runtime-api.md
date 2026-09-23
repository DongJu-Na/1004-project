# Runtime API Contract: 제압 → 005~007

어셈블리 `Subdue`, 네임스페이스 `Project1028.Subdue`.

```csharp
public enum SubdueAction { Backstab, Shove, ObjectStrike }
public enum HandState { Empty, OneHand, TwoHands }
public enum HeldKind { None, Object, UnconsciousNpc }

public sealed class PlayerHands : MonoBehaviour            // PlayerEntity와 같은 GameObject
{
    public HandState State { get; }  public HeldKind HeldKind { get; }
    public bool TryPickUp(CarriableObject o);  public bool TryPickUpBody(UnconsciousState body);  public bool Drop();
}

public sealed class UnconsciousState : MonoBehaviour       // 기절 중인 NPC에만 존재
{
    public NpcIdentity Npc { get; }  public PlayerEntity SubduedBy { get; }  public PlayerEntity CarriedBy { get; }
    public float Remaining { get; }  public bool IsCarried { get; }
    public static bool IsUnconscious(NpcIdentity npc);
}

public struct SubdueResult { public bool Success; public SubdueAction Action; public string Reason; }
public sealed class SubdueSystem : MonoBehaviour
{
    public static SubdueSystem Instance { get; }  public bool IsReady { get; }  public SubdueRules Rules { get; }
    public SubdueResult TrySubdue(PlayerEntity actor, NpcIdentity target, SubdueAction action);
    public SubdueAction PlanAction(PlayerEntity actor, NpcIdentity target);   // 문맥 결정 (R-5)
    public IEnumerable<UnconsciousState> Unconscious { get; }
}

public static class SubdueEvents { /* data-model.md 이벤트 목록 */ }
```

005 신고 시나리오: 이동 중 NPC가 `UnconsciousState`를 얻으면 흐름 중단, `SubdueEvents.OnUnconsciousWake` 또는 002 `OnReportAttempt`
(깨어남 시 002가 발행)로 재시작. 007: 인카운터 NPC 기절 시 `OnSubdued` 구독으로 중단 처리.

## 가산 확장
- 001 `ThirdPersonMotor.SpeedMultiplier`(1), `.SprintAllowed`(true); `PlayerEntity.HandsBusy`; `NpcTalkInteractable.CanInteract`가 `HandsBusy` 거부.
- 002 `SuspicionEventRule.scope = "global"`(섬 값만); `SuspicionSystem.ForceReportAttempt(npc, player)`; 사건 6개 추가(data-model 표).

## 데이터
`Assets/StreamingAssets/Subdue/subdue_rules.json` — [subdue-rules-schema.json](subdue-rules-schema.json)

## 입력(테스트 콘솔 매핑, 정식 액션 바인딩은 후속)
| 키 | 동작 |
|---|---|
| E | 들기 / 들어 옮기기 / 말하기 (001 Interact) |
| F | 문맥 제압 (안내 문구에 결정된 동작 표시) |
| G | 내려놓기 |
| T | 시간 배속 (002 콘솔) |

## 에디터
`Tools/PROJECT 1028/Build Subdue Test Scene` → `Assets/Scenes/Test_Subdue.unity`.

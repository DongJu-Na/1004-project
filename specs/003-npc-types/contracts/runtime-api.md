# Runtime API Contract: NPC 유형 → 004~007

어셈블리 `NpcTypes`, 네임스페이스 `Project1028.NpcTypes`.

```csharp
public enum NpcType { Watcher, Informer, Sympathizer, Silent, Sentinel }
public enum Faction { Antagonist, Victim, Neutral }

public sealed class NpcTypeProfile : MonoBehaviour
{
    public NpcType Type { get; }  public Faction Faction { get; }  public IReadOnlyList<string> Roles { get; }
    public bool IsTurned { get; }                 // 동조자 전환 (§3 51+)
    public bool WatcherRuleApplies { get; }       // Watcher || (Sympathizer && IsTurned)
    public bool IsManager { get; }
    public static NpcTypeProfile Get(NpcIdentity npc);   // 없으면 null (미배정 NPC는 시스템이 동조자 기본으로 부여)
}

public struct SubdueVerdict { public bool CanSubdue; public bool PhysicalReaction; public string Reason; }
public static class SubdueJudgement
{
    public static SubdueVerdict Evaluate(Faction f);                 // 순수
    public static SubdueVerdict Evaluate(NpcIdentity npc);          // 프로필 없으면 Victim으로 취급 → 불가
}

public sealed class NpcTypeSystem : MonoBehaviour
{
    public static NpcTypeSystem Instance { get; }
    public bool IsReady { get; }
    public NpcTypeRules Rules { get; }
    public int Investigate(PlayerEntity actor);   // 조사 사건 입력 (후속 증거 기능이 호출). 통지한 NPC 수 반환
    public bool LabelsVisible { get; set; }        // 테스트 씬 라벨 표시/숨김
}

public static class NpcTypeEvents
{
    public static event Action<PlayerEntity, int> OnInvestigate;
    public static event Action<NpcIdentity, bool> OnSympathizerTurned;
    public static event Action<NpcIdentity, NpcIdentity, PlayerEntity> OnInformerDeliveryStarted;
    public static event Action<NpcIdentity, NpcIdentity, PlayerEntity> OnInformerDeliveryCompleted;
    public static event Action<NpcIdentity, PlayerEntity> OnInformerHeld;
    public static event Action<NpcIdentity, Vector3, int> OnSentinelAlarm;
}
```

004는 `SubdueJudgement.Evaluate(npc)`만 사용한다. 005는 침묵자에 대해 `OnReportAttempt`가 오지 않음을 전제한다(002 플래그).
007은 `NpcTypeProfile.Type`을 인카운터 NPC 정의에 쓸 수 있다.

## 002에 추가된 표면 (가산)
- `SuspicionEventRule.ignoresTimeMultiplier`
- `NpcSuspicionProfile.SuppressReportAttempt`, `.IgnoresSuspicionEvents`
- 사건 `watcher_investigate`, `informer_delivery` (확정), `investigate_in_sight` scope → target

## 데이터
`Assets/StreamingAssets/NpcTypes/npc_types.json` — [npc-types-schema.json](npc-types-schema.json)

## 에디터·콘솔
메뉴 `Tools/PROJECT 1028/Build NPC Types Test Scene` → `Assets/Scenes/Test_NpcTypes.unity`.
키: `I` 조사(P1 기준), `L` 라벨 토글. 002 콘솔 키(`]` 섬 +10 등)도 동작.

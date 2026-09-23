# Runtime API Contract: 신고 시나리오 → 006/007 및 레벨 데이터

어셈블리 `Report`, 네임스페이스 `Project1028.Report`.

```csharp
public enum ReportPointKind { Phone, Office }
public enum ReportPhase { Moving, Reporting, Completed, Abandoned, Interrupted }

public sealed class ReportPoint : MonoBehaviour      // 레벨에 배치하는 신고 지점
{
    public string Id { get; }  public ReportPointKind Kind { get; }  public string DisplayName { get; }
    public bool IsUsable { get; }  public bool CanBeCut { get; }   // Phone만 끊기 가능
    public static IReadOnlyList<ReportPoint> All { get; }
}
public sealed class PlayerToolkit : MonoBehaviour { public bool HasCuttingTool { get; set; } }   // 외부 입력(도구 시스템 후속)
public sealed class ReportSystem : MonoBehaviour
{
    public static ReportSystem Instance { get; }  public bool IsReady { get; }  public ReportRules Rules { get; }
    public bool IsCompleted(NpcIdentity npc, PlayerEntity player);
    public IReadOnlyList<ReportFlow> ActiveFlows { get; }
}
public sealed class ReportFlow : MonoBehaviour { public ReportPhase Phase { get; } public PlayerEntity TargetPlayer { get; } public ReportPoint TargetPoint { get; } }
public static class ReportEvents { /* data-model 목록 */ }
```

입력 경로: 002 `SuspicionEvents.OnReportAttempt` 구독(005는 유형을 모른다). 중단: 004 `UnconsciousState`. 재시작: 004 깨어남 → 002 발행.
007 인카운터는 `ReportPoint`를 레벨 요소로 참조할 수 있다("전화기 위치 정찰").

## 데이터
`Assets/StreamingAssets/Report/report_rules.json` — [report-rules-schema.json](report-rules-schema.json). 002 JSON `report_completed`(섬 25 확정).

## 입력(테스트 매핑)
| 키 | 동작 |
|---|---|
| E | 전화선 끊기 시작(정지 유지, 이동 시 취소) |
| K | 도구 보유 토글 (P1) |
| I / F / T | 003 조사 / 004 제압 / 002 배속 |

## 에디터
`Tools/PROJECT 1028/Build Report Test Scene` → `Assets/Scenes/Test_Report.unity`.

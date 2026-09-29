# Runtime API Contract: 수직 슬라이스 → 후속(저장·콘텐츠·미술)

어셈블리 `Slice`, 네임스페이스 `Project1028.Slice`.

```csharp
public enum RunPhase { Day, Night, DepartureOpen, Lockdown, Ended }
public enum RunOutcome { None, Success, LeftWithoutEvidence, ForcedDeparture }
public sealed class RunDirector : MonoBehaviour
{
    public static RunDirector Instance { get; }  public bool IsReady { get; }  public SliceRules Rules { get; }
    public RunClock Clock { get; }  public ObjectiveTracker Objectives { get; }  public RunOutcome Outcome { get; }  public RunStats Stats { get; }
    public void EndRun(RunOutcome outcome);  public void Restart();
}
public static class SliceEvents { OnObjectiveAdvanced(ObjectiveStep); OnRunPhaseChanged(RunPhase); OnDepartureDenied(string); OnRunEnded(RunOutcome, RunStats); OnEvidencePicked(PlayerEntity); OnRestrictedEntered(PlayerEntity, string); }
public sealed class FeedbackDirector : MonoBehaviour { /* 이벤트 → 연출. 후속 미술·사운드 기능은 여기서 코드 합성 소리를 에셋으로 교체 */ }
```

## 가산 확장
- 001 `OrbitCamera.Shake(float amplitude, float seconds)`.
- 004 `CarriableObject.Locked`(true면 들기 불가, 안내 "먼저 조사해야 한다"), `.IsEvidence`.
- 006 `VehicleSeats.AllowBusyHands`(static bool).

## 데이터
`Assets/StreamingAssets/Slice/slice_rules.json`, `Assets/StreamingAssets/Slice/island_layout.json`. 003 `npc_types.json`에 `slice_*` 배정 10건.

## 입력
| 키 | 동작 |
|---|---|
| E | 조사(길게)·들기·배 타기·탑승 (001 Interact) |
| F / G | 제압 / 내려놓기 (004) |
| R | 결과 화면에서 재시작 |
| F9 / F10 / F11 / F12 | 시계 +60s / 출항 시각으로 / 봉쇄 토글(디버그) / 반응 20종 순환 테스트 |
| 기존 콘솔 키 | 002 F1~F7·N·T·[ ], 003 I·L, 005 K·O, 006 V, 007 X·1·2·S·Z |

## 에디터
`Tools/PROJECT 1028/Build Island Slice` → `Assets/Scenes/Island_Slice.unity`.

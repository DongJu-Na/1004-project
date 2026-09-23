# Runtime API Contract: 인카운터 프레임워크 → v0.9 콘텐츠·문서 시스템·저장

어셈블리 `Encounter`, 네임스페이스 `Project1028.Encounter`.

```csharp
public enum EncounterType { A, B, C, D }
public sealed class EncounterSystem : MonoBehaviour
{
    public static EncounterSystem Instance { get; }  public bool IsReady { get; }  public EncounterRules Rules { get; }
    public RunLedger Ledger { get; }                 // Flags, History, Export()
    public int RunCount { get; }                      // 이번 런 발생 수
    public EncounterInstance Active { get; }          // 진행 중(없으면 null)
    public string CurrentIslandId { get; set; }       // 외부 입력(섬 시스템 후속)
    public bool ForceStart(string encounterId, PlayerEntity player);   // 테스트/디버그
    public IReadOnlyList<CallbackEntry> CallbacksFor(string flag);     // §6.5 조회(반환만)
}
public sealed class RunLedger { public IReadOnlyCollection<string> Flags; public IReadOnlyCollection<string> History; public void Import(IEnumerable<string> priorHistory); public (string[] flags, string[] history) Export(); }
public sealed class PlayerActivity : MonoBehaviour { public bool IsScouting { get; set; } }   // 조사·정찰 중 = 인카운터 없음
public static class EncounterEvents { /* data-model 목록 */ }
```

- **콘텐츠 추가(v0.9)**: `encounters.json`의 `encounters[]`·`pools[]`·`callbacks[]`에 항목 추가. `sample:false`. 코드 변경 없음. 각 항목 단가(NPC 수·줄 수)는 검증기가 집계해 HUD에 표시(헌장 II).
- **문서 시스템**: `targetKind=document` 회수 항목은 `CallbacksFor(flag)`로 조회만 된다. 실행은 문서 기능이 담당.
- **저장**: `Ledger.Export()`를 보존하고 다음 런 시작 시 `Import(history)`.
- **002 사건 추가**: `encounter_stared_at`(개인 1, target, 확정대기, B 샘플 "주민이 쳐다본다"), `encounter_picked_up_runaway`(섬 20, global, 확정대기, C 샘플 "태운다 → 섬 의심도 급등").
- **001 가산**: `DialogueRunner.TryBeginInline(NpcIdentity speaker, string[] lines)`.

## 입력(테스트 콘솔)
| 키 | 동작 |
|---|---|
| 자연 발생 | 도로 트리거 진입(도보/차량 이동 중) |
| X | 다음 샘플 강제 발생(P1 위치) |
| 1 / 2 | C 선택 |
| Z | 100런 시뮬레이션 리포트 |
| S | P1 정찰 중 토글 |

## 에디터
`Tools/PROJECT 1028/Build Encounter Test Scene` → `Assets/Scenes/Test_Encounter.unity`(도로·차량·트리거 6·샘플 8).

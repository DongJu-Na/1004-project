# Runtime API Contract: 의심 시스템 → 003~007

**Date**: 2026-09-23 | 어셈블리 `Suspicion`, 네임스페이스 `Project1028.Suspicion`. 후속 기능은 여기 있는 표면만 읽는다.

## 1. 사건 발생 (유일한 상승 경로)

```csharp
public struct SuspicionEvent
{
    public string EventId;        // suspicion_rules.json events[].id
    public PlayerEntity Actor;    // 필수. 개인 의심의 대상 플레이어
    public Vector3 Position;      // scope=radius의 중심. 보통 Actor.Position
    public NpcIdentity TargetNpc; // scope=target일 때 필수
}

public sealed class SuspicionSystem : MonoBehaviour
{
    public static SuspicionSystem Instance { get; }
    public bool IsReady { get; }                         // 규칙 로드 성공
    public SuspicionRules Rules { get; }
    public IslandAlert Island { get; }                   // Value, Zone, DepartureBlocked
    public void Raise(SuspicionEvent e);                 // 큐에 넣고 다음 틱에 적용
    public PersonalSuspicion GetPersonal(NpcIdentity npc, PlayerEntity player);   // 없으면 0 생성
    public SuspicionStage GetStage(NpcIdentity npc, PlayerEntity player);
    public IEnumerable<(NpcIdentity npc, PlayerEntity player, PersonalSuspicion s)> AllPersonal { get; }
    public CarryOverSnapshot EndRun();                   // 이월 값 계산·노출·OnRunEnded
    public void DebugAdjustIsland(int delta);            // 테스트 콘솔 전용
}
```

후속 기능이 추가할 사건 id(JSON에 `status:"확정"`으로 추가하고 `Raise`만 호출):
- 003: `investigate_in_sight`(감시자 +2는 유형 규칙이므로 003이 별도 id `watcher_investigate`로 추가), `noise`(경계자)
- 004: `subdue_witnessed`(섬 +40), `unconscious_found`(섬 +30, 개인 +3 확정대기), `noise`(등급별)
- 005: `report_completed`(섬 +25)

## 2. 값·단계·구간

```csharp
public enum SuspicionStage { Indifferent = 0, Aware = 1, Alert = 2, Certain = 3 }
public enum AlertZone { Calm, Watch, Tension, Lockdown }

public sealed class PersonalSuspicion { public int Value { get; } public SuspicionStage Stage { get; } }
public sealed class IslandAlert { public int Value { get; } public AlertZone Zone { get; } public bool DepartureBlocked { get; } }
```

003 동조자는 `Island.Zone >= AlertZone.Tension`을 읽어 전환한다. 005·007은 `Zone`을 진행도 조건으로 읽는다.

## 3. 이벤트

```csharp
public static class SuspicionEvents
{
    public static event Action<NpcIdentity, PlayerEntity, SuspicionStage, SuspicionStage> OnPersonalStageChanged;
    public static event Action<NpcIdentity, PlayerEntity> OnReportAttempt;            // 개인 의심 3 도달 (005 구독)
    public static event Action<int, int> OnIslandChanged;
    public static event Action<AlertZone, AlertZone> OnIslandZoneChanged;
    public static event Action<bool> OnDepartureBlockedChanged;                       // 봉쇄 진입/해제
    public static event Action<NpcIdentity, NpcIdentity, PlayerEntity> OnPropagated;
    public static event Action<CarryOverSnapshot> OnRunEnded;
}
```

보장: `OnReportAttempt`는 (npc, player)당 3 도달 시 1회. 3 유지 중 재발행 없음. 값이 내려가 다시 3이 되면 다시 발행.

## 4. NPC 플래그 (003이 설정)

```csharp
public sealed class NpcSuspicionProfile : MonoBehaviour
{
    public bool PropagatesSuspicion { get; set; } = true;          // false면 전파 원인이 되지 않음 (침묵자)
    public bool ReportsToManagerImmediately { get; set; } = false;  // 밀고자: 관리자에게만, 지연 0
    public bool IsManager { get; set; } = false;
    public bool SuppressStageBehaviour { get; set; } = false;       // 1·2단계 표현 억제 (밀고자)
}
```

컴포넌트가 없는 NPC는 기본값. 003은 유형에 따라 이 값을 설정하며, 002 코드는 유형을 모른다.

## 5. 시간대·위장

```csharp
public enum DayPhase { Day, Night }
public sealed class TimeOfDay : MonoBehaviour
{
    public static TimeOfDay Instance { get; }
    public DayPhase Phase { get; }
    public void Set(DayPhase phase); public void Toggle();
    public event Action<DayPhase> OnChanged;
}
public sealed class PlayerDisguise : MonoBehaviour   // PlayerEntity와 같은 GameObject
{
    public bool IsDisguised { get; set; }             // 외부 입력 (착용 시스템은 후속)
    public bool IsEffective { get; }                  // IsDisguised && Phase == Day  (§7 밤 무효)
}
```

007은 `TimeOfDay.Phase`를 시간대 조건으로 읽는다. `TimeOfDay`가 없으면 시스템은 Day로 취급한다.

## 6. NPC 최소 이동

```csharp
public sealed class NpcMover : MonoBehaviour
{
    public void MoveTo(Vector3 worldPos); public void Follow(Transform target, float keepDistance);
    public void FaceTowards(Vector3 worldPos); public void Stop();
    public bool IsMoving { get; } public bool HasArrived { get; }
    public float Speed { get; set; }
}
```

003 밀고자 이동·005 신고 이동이 재사용한다. 장애물 회피는 없다(005에서 NavMesh로 교체 가능하도록 API를 지점 기반으로 유지).

## 7. 데이터 파일

`Assets/StreamingAssets/Suspicion/suspicion_rules.json` — [suspicion-rules-schema.json](suspicion-rules-schema.json).
로더는 Error 시 `SuspicionSystem.IsReady=false` + HUD 경고, `확정대기` 항목 수를 HUD에 표시.

## 8. 에디터

메뉴 `Tools/PROJECT 1028/Build Suspicion Test Scene` → `Assets/Scenes/Test_Suspicion.unity`. 001 빌더의 플레이어·NPC 생성을
재사용하고(public static으로 노출), NPC 3(일반·`PropagatesSuspicion=false`·`IsManager=true`), 플레이어 2, `TimeOfDay`,
`SuspicionSystem`, 디버그 패널·콘솔을 추가한다.

## 9. 테스트 콘솔 키 (Play 모드)

| 키 | 동작 |
|---|---|
| F1~F7 | events[0..6] 발생 (witness/target은 "카메라 전방 가장 가까운 NPC" 대상, radius는 P1 위치) |
| N | 낮/밤 전환 |
| R | 런 종료(이월 값 표시) |
| T | 시간 배속 1↔10 |
| [ / ] | 섬 의심도 -10 / +10 (디버그 직접 조정) |

## 10. 003에서 추가된 가산 확장 (2026-09-23)

- `SuspicionEventRule.ignoresTimeMultiplier`(기본 false): 참이면 §7 시간대 배율을 적용하지 않는다(§3 감시자 "즉시 +2").
- `NpcSuspicionProfile.SuppressReportAttempt`(기본 false): 3 도달 시 `OnReportAttempt` 미발행(침묵자). 단계 전환 이벤트는 발행.
- `NpcSuspicionProfile.IgnoresSuspicionEvents`(기본 false): 상승 사건 무시(경계자).
- 사건 추가: `watcher_investigate`(personal 2, target, 확정, 배율 무시), `informer_delivery`(personal 1, target, 확정).
- `investigate_in_sight` scope `witness`→`target`(003 조사 릴레이가 NPC별로 발생).
- `NpcSuspicionBehaviour`: `SuppressStageBehaviour`면 `mover.Stop()`을 억제 진입 시 1회만 호출(003 밀고자 이동 보호).

## 11. 004에서 추가된 가산 확장 (2026-09-23)
- `SuspicionEventRule.scope = "global"`: 대상 NPC 없이 `islandDelta`만 적용(제압 목격 +40, 기절자 발견 +30).
- `SuspicionSystem.ForceReportAttempt(npc, player)`: 이미 3인 NPC의 신고 시도 재발행(기절자 깨어남). `SuppressReportAttempt` 존중.
- 사건: `subdue_witnessed`(섬 40 확정), `unconscious_found_island`(섬 30 확정), `unconscious_wake`(개인 3 확정), `shove_failed`(개인 3 확정대기), `noise_low/medium/high`(개인 1, 반경 4/8/14, 확정대기).

## 12. 005에서 추가된 사건
- `report_completed`(섬 25, global, 확정): 신고 완료. 005 `ReportSystem.Complete`가 발생.

## 13. 007에서 추가된 사건
- `encounter_stared_at`(개인 1, target, requiresSight, 확정대기): B 유형 샘플. `encounter_picked_up_runaway`(섬 20, global, 확정대기): C 유형 샘플 "태운다".

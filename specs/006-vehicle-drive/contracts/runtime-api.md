# Runtime API Contract: 차량 → 002(소음)·007(인카운터)

어셈블리 `Vehicle`, 네임스페이스 `Project1028.Vehicle`. `Vehicle`은 `Suspicion`을 참조하지 않는다(기반 기능). 규칙 기능이 이 표면을 읽는다.

```csharp
public enum SeatKind { Driver, Passenger }
public struct VehicleState { public float Speed; public bool IsMoving; public bool EngineOn; public string DriverId; public string PassengerId; public bool IsFlipped; }

public sealed class VehicleController : MonoBehaviour
{
    public static VehicleController Instance { get; }       // 씬에 1대
    public VehicleState State { get; }
    public float Speed { get; }  public bool IsGrounded { get; }  public bool EngineOn { get; }
    public void Recover();  public void ResetToSpawn();
}
public sealed class VehicleSeats : MonoBehaviour
{
    public PlayerEntity Driver { get; }  public PlayerEntity Passenger { get; }
    public bool TryEnter(PlayerEntity p);  public bool TryExit(PlayerEntity p);
    public static bool IsInVehicle(PlayerEntity p);          // == p.IsInVehicle
}
public static class VehicleEvents { OnEntered(PlayerEntity, SeatKind); OnExited(PlayerEntity, SeatKind); OnRecovered(); OnReset(); OnExitDenied(PlayerEntity, string); }
```

002는 `State.EngineOn && State.IsMoving`으로 차량 소음 사건(§2.3 확정대기)을 만들 수 있다. 007은 `PlayerEntity.IsInVehicle`로 "차량 이동 중" 확률을 적용한다.

## 가산 수정(001)
- `PlayerEntity.IsInVehicle`(bool): 탑승 중 `InteractionDetector`가 안내를 내지 않는다. `OrbitCamera.SetTarget(Transform target, float distance)`.

## 입력
| 액션/키 | 운전자 | 동승자 |
|---|---|---|
| Move.y | 가속/후진 | - |
| Move.x | 조향 | - |
| Sprint | 제동 | - |
| Jump | 뒤집힘 복구 | - |
| Interact(E) | 하차(정지·저속) | 하차 |
| 마우스 | 카메라 궤도(조향 무관) | 카메라 |

## 데이터·에디터
`Assets/StreamingAssets/Vehicle/vehicle_params.json` — [vehicle-params-schema.json](vehicle-params-schema.json). 메뉴 `Tools/PROJECT 1028/Build Vehicle Test Scene` → `Assets/Scenes/Test_Vehicle.unity`.

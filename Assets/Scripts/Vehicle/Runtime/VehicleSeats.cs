using UnityEngine;
using Project1028.PlayFoundation;

namespace Project1028.Vehicle
{
    /// <summary>운전석·동승석 점유, 탑승·하차, 카메라 재타깃, 플레이어 상태 전환. 좌석은 개체 단위 (원칙 IV).</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VehicleController))]
    public sealed class VehicleSeats : MonoBehaviour
    {
        [SerializeField] private Vector3 driverSeatLocal = new Vector3(-0.5f, 0.9f, 0.2f);
        [SerializeField] private Vector3 passengerSeatLocal = new Vector3(0.5f, 0.9f, 0.2f);
        [SerializeField] private float exitSideDistance = 2.2f;
        [SerializeField] private float vehicleCameraDistance = 7f;
        [SerializeField] private float walkCameraDistance = 4.5f;

        private VehicleController controller;
        private Transform driverSeat, passengerSeat;

        public PlayerEntity Driver { get; private set; }
        public PlayerEntity Passenger { get; private set; }
        public bool HasFreeSeat => Driver == null || Passenger == null;
        public static bool IsInVehicle(PlayerEntity p) => p != null && p.IsInVehicle;

        private void Awake()
        {
            controller = GetComponent<VehicleController>();
            driverSeat = MakeSeat("Seat_Driver", driverSeatLocal);
            passengerSeat = MakeSeat("Seat_Passenger", passengerSeatLocal);
        }

        private Transform MakeSeat(string name, Vector3 local)
        {
            var t = transform.Find(name);
            if (t == null) { var go = new GameObject(name); go.transform.SetParent(transform, false); t = go.transform; }
            t.localPosition = local;
            t.localRotation = Quaternion.identity;
            return t;
        }

        public bool TryEnter(PlayerEntity player)
        {
            if (player == null || player.IsInVehicle || player.IsLocked || player.HandsBusy) return false;
            var seat = SeatAssignment.PickSeat(Driver != null, Passenger != null);
            if (seat == null) { RuntimeHud.Instance?.Warn($"{player.Id}: 좌석이 모두 찼다"); return false; }

            Transform seatT = seat == SeatKind.Driver ? driverSeat : passengerSeat;
            if (seat == SeatKind.Driver) Driver = player; else Passenger = player;

            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.GetComponent<ThirdPersonMotor>()?.SetMovementEnabled(false);
            player.IsInVehicle = true;
            player.transform.SetParent(seatT, false);
            player.transform.localPosition = Vector3.zero;
            player.transform.localRotation = Quaternion.identity;
            player.Camera?.SetTarget(transform, vehicleCameraDistance);
            if (seat == SeatKind.Driver) controller.EngineOn = true;

            VehicleEvents.RaiseEntered(player, seat.Value);
            RuntimeHud.Instance?.Warn($"{player.Id}: {(seat == SeatKind.Driver ? "운전석" : "동승석")} 탑승");
            return true;
        }

        public bool TryExit(PlayerEntity player)
        {
            if (player == null) return false;
            SeatKind? seat = player == Driver ? SeatKind.Driver : player == Passenger ? SeatKind.Passenger : (SeatKind?)null;
            if (seat == null) return false;

            if (!ExitRule.CanExit(controller.Speed, controller.Params.exitMaxSpeed))
            {
                VehicleEvents.RaiseExitDenied(player, "속도를 줄여야 내릴 수 있다");
                RuntimeHud.Instance?.Warn($"{player.Id}: 속도를 줄여야 내릴 수 있다");
                return false;
            }

            Vector3 side = seat == SeatKind.Driver ? -transform.right : transform.right;
            side.y = 0f;
            if (side.sqrMagnitude < 1e-4f) side = Vector3.left; // 옆으로 누운 경우 등
            Vector3 exitPos = transform.position + side.normalized * exitSideDistance;
            exitPos.y = Physics.Raycast(exitPos + Vector3.up * 3f, Vector3.down, out RaycastHit hit, 10f, ~0, QueryTriggerInteraction.Ignore) && !hit.transform.IsChildOf(transform)
                ? hit.point.y + 1f : 1f;

            player.transform.SetParent(null, true);
            player.transform.SetPositionAndRotation(exitPos, Quaternion.Euler(0f, transform.eulerAngles.y, 0f));
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = true;
            player.GetComponent<ThirdPersonMotor>()?.SetMovementEnabled(!player.IsLocked);
            player.IsInVehicle = false;
            player.Camera?.SetTarget(player.transform, walkCameraDistance);

            if (seat == SeatKind.Driver) { Driver = null; controller.EngineOn = false; controller.SetInput(0f, 0f, 0f); }
            else Passenger = null;

            VehicleEvents.RaiseExited(player, seat.Value);
            RuntimeHud.Instance?.Warn($"{player.Id}: 하차");
            return true;
        }
    }
}

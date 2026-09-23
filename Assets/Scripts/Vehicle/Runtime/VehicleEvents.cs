using System;
using Project1028.PlayFoundation;

namespace Project1028.Vehicle
{
    public struct VehicleState
    {
        public float Speed;
        public bool IsMoving;
        public bool EngineOn;
        public string DriverId;
        public string PassengerId;
        public bool IsFlipped;
        public bool IsGrounded;
    }

    public static class VehicleEvents
    {
        public static event Action<PlayerEntity, SeatKind> OnEntered;
        public static event Action<PlayerEntity, SeatKind> OnExited;
        public static event Action OnRecovered;
        public static event Action OnReset;
        public static event Action<PlayerEntity, string> OnExitDenied;

        internal static void RaiseEntered(PlayerEntity p, SeatKind s) => OnEntered?.Invoke(p, s);
        internal static void RaiseExited(PlayerEntity p, SeatKind s) => OnExited?.Invoke(p, s);
        internal static void RaiseRecovered() => OnRecovered?.Invoke();
        internal static void RaiseReset() => OnReset?.Invoke();
        internal static void RaiseExitDenied(PlayerEntity p, string r) => OnExitDenied?.Invoke(p, r);
    }
}

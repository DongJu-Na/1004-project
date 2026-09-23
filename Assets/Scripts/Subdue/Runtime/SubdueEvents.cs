using System;
using UnityEngine;
using Project1028.PlayFoundation;

namespace Project1028.Subdue
{
    public static class SubdueEvents
    {
        public static event Action<PlayerEntity, NpcIdentity, SubdueAction, int> OnSubdued;
        public static event Action<PlayerEntity, NpcIdentity, SubdueAction, string> OnSubdueRejected;
        public static event Action<PlayerEntity, NpcIdentity> OnShoveFailed;
        public static event Action<NpcIdentity, PlayerEntity> OnUnconsciousWake;
        public static event Action<NpcIdentity, NpcIdentity, PlayerEntity> OnBodyDiscovered;
        public static event Action<PlayerEntity, HeldKind> OnPickedUp;
        public static event Action<PlayerEntity, HeldKind> OnDropped;
        public static event Action<NoiseLevel, Vector3, PlayerEntity> OnNoise;

        internal static void RaiseSubdued(PlayerEntity a, NpcIdentity n, SubdueAction s, int w) => OnSubdued?.Invoke(a, n, s, w);
        internal static void RaiseRejected(PlayerEntity a, NpcIdentity n, SubdueAction s, string r) => OnSubdueRejected?.Invoke(a, n, s, r);
        internal static void RaiseShoveFailed(PlayerEntity a, NpcIdentity n) => OnShoveFailed?.Invoke(a, n);
        internal static void RaiseWake(NpcIdentity n, PlayerEntity p) => OnUnconsciousWake?.Invoke(n, p);
        internal static void RaiseDiscovered(NpcIdentity o, NpcIdentity b, PlayerEntity p) => OnBodyDiscovered?.Invoke(o, b, p);
        internal static void RaisePickedUp(PlayerEntity p, HeldKind k) => OnPickedUp?.Invoke(p, k);
        internal static void RaiseDropped(PlayerEntity p, HeldKind k) => OnDropped?.Invoke(p, k);
        internal static void RaiseNoise(NoiseLevel l, Vector3 pos, PlayerEntity p) => OnNoise?.Invoke(l, pos, p);
    }
}

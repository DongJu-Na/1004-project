using System;
using UnityEngine;
using Project1028.PlayFoundation;

namespace Project1028.NpcTypes
{
    public static class NpcTypeEvents
    {
        public static event Action<PlayerEntity, int> OnInvestigate;
        public static event Action<NpcIdentity, bool> OnSympathizerTurned;
        public static event Action<NpcIdentity, NpcIdentity, PlayerEntity> OnInformerDeliveryStarted;
        public static event Action<NpcIdentity, NpcIdentity, PlayerEntity> OnInformerDeliveryCompleted;
        public static event Action<NpcIdentity, PlayerEntity> OnInformerHeld;
        public static event Action<NpcIdentity, Vector3, int> OnSentinelAlarm;

        internal static void RaiseInvestigate(PlayerEntity p, int n) => OnInvestigate?.Invoke(p, n);
        internal static void RaiseSympathizerTurned(NpcIdentity n, bool t) => OnSympathizerTurned?.Invoke(n, t);
        internal static void RaiseDeliveryStarted(NpcIdentity i, NpcIdentity m, PlayerEntity p) => OnInformerDeliveryStarted?.Invoke(i, m, p);
        internal static void RaiseDeliveryCompleted(NpcIdentity i, NpcIdentity m, PlayerEntity p) => OnInformerDeliveryCompleted?.Invoke(i, m, p);
        internal static void RaiseHeld(NpcIdentity i, PlayerEntity p) => OnInformerHeld?.Invoke(i, p);
        internal static void RaiseSentinelAlarm(NpcIdentity s, Vector3 pos, int n) => OnSentinelAlarm?.Invoke(s, pos, n);
    }
}

using System;
using Project1028.PlayFoundation;

namespace Project1028.Suspicion
{
    /// <summary>후속 기능(003~007)이 구독하는 이벤트 허브. 페이로드에 항상 플레이어를 포함한다 (원칙 IV).</summary>
    public static class SuspicionEvents
    {
        public static event Action<NpcIdentity, PlayerEntity, SuspicionStage, SuspicionStage> OnPersonalStageChanged;
        public static event Action<NpcIdentity, PlayerEntity> OnReportAttempt;
        public static event Action<int, int> OnIslandChanged;
        public static event Action<AlertZone, AlertZone> OnIslandZoneChanged;
        public static event Action<bool> OnDepartureBlockedChanged;
        public static event Action<NpcIdentity, NpcIdentity, PlayerEntity> OnPropagated;
        public static event Action<CarryOverSnapshot> OnRunEnded;
        public static event Action<SuspicionEvent, int> OnEventApplied;

        internal static void RaisePersonalStageChanged(NpcIdentity n, PlayerEntity p, SuspicionStage o, SuspicionStage nw) => OnPersonalStageChanged?.Invoke(n, p, o, nw);
        internal static void RaiseReportAttempt(NpcIdentity n, PlayerEntity p) => OnReportAttempt?.Invoke(n, p);
        internal static void RaiseIslandChanged(int o, int n) => OnIslandChanged?.Invoke(o, n);
        internal static void RaiseIslandZoneChanged(AlertZone o, AlertZone n) => OnIslandZoneChanged?.Invoke(o, n);
        internal static void RaiseDepartureBlockedChanged(bool b) => OnDepartureBlockedChanged?.Invoke(b);
        internal static void RaisePropagated(NpcIdentity f, NpcIdentity t, PlayerEntity p) => OnPropagated?.Invoke(f, t, p);
        internal static void RaiseRunEnded(CarryOverSnapshot s) => OnRunEnded?.Invoke(s);
        internal static void RaiseEventApplied(SuspicionEvent e, int count) => OnEventApplied?.Invoke(e, count);
    }
}

using System;
using Project1028.PlayFoundation;

namespace Project1028.Report
{
    public static class ReportEvents
    {
        public static event Action<NpcIdentity, PlayerEntity, ReportPoint> OnReportStarted;
        public static event Action<NpcIdentity, PlayerEntity, ReportPoint> OnReportRetargeted;
        public static event Action<NpcIdentity, PlayerEntity, string> OnReportAbandoned;
        public static event Action<NpcIdentity, PlayerEntity, ReportPoint> OnReportCompleted;
        public static event Action<NpcIdentity, PlayerEntity> OnReportInterrupted;
        public static event Action<ReportPoint, PlayerEntity> OnPhoneCut;
        public static event Action<ReportPoint, PlayerEntity> OnCutCancelled;

        internal static void RaiseStarted(NpcIdentity n, PlayerEntity p, ReportPoint r) => OnReportStarted?.Invoke(n, p, r);
        internal static void RaiseRetargeted(NpcIdentity n, PlayerEntity p, ReportPoint r) => OnReportRetargeted?.Invoke(n, p, r);
        internal static void RaiseAbandoned(NpcIdentity n, PlayerEntity p, string why) => OnReportAbandoned?.Invoke(n, p, why);
        internal static void RaiseCompleted(NpcIdentity n, PlayerEntity p, ReportPoint r) => OnReportCompleted?.Invoke(n, p, r);
        internal static void RaiseInterrupted(NpcIdentity n, PlayerEntity p) => OnReportInterrupted?.Invoke(n, p);
        internal static void RaisePhoneCut(ReportPoint r, PlayerEntity p) => OnPhoneCut?.Invoke(r, p);
        internal static void RaiseCutCancelled(ReportPoint r, PlayerEntity p) => OnCutCancelled?.Invoke(r, p);
    }
}

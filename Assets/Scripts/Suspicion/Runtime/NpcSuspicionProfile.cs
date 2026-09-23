using UnityEngine;
using Project1028.PlayFoundation;

namespace Project1028.Suspicion
{
    /// <summary>
    /// §2.6 예외 플래그. 003 NPC 유형이 값을 설정한다. 002 코드는 유형을 모른다.
    /// 컴포넌트가 없는 NPC는 기본값("일반")으로 취급한다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NpcIdentity))]
    public sealed class NpcSuspicionProfile : MonoBehaviour
    {
        [SerializeField] private bool propagatesSuspicion = true;
        [SerializeField] private bool reportsToManagerImmediately = false;
        [SerializeField] private bool isManager = false;
        [SerializeField] private bool suppressStageBehaviour = false;
        [SerializeField] private bool suppressReportAttempt = false;   // 침묵자: 3이 되어도 신고 시도 사건 없음
        [SerializeField] private bool ignoresSuspicionEvents = false;  // 경계자: 상승 사건 무시

        public bool PropagatesSuspicion { get => propagatesSuspicion; set => propagatesSuspicion = value; }
        public bool ReportsToManagerImmediately { get => reportsToManagerImmediately; set => reportsToManagerImmediately = value; }
        public bool IsManager { get => isManager; set => isManager = value; }
        public bool SuppressStageBehaviour { get => suppressStageBehaviour; set => suppressStageBehaviour = value; }
        public bool SuppressReportAttempt { get => suppressReportAttempt; set => suppressReportAttempt = value; }
        public bool IgnoresSuspicionEvents { get => ignoresSuspicionEvents; set => ignoresSuspicionEvents = value; }

        public readonly struct Snapshot
        {
            public readonly bool PropagatesSuspicion;
            public readonly bool ReportsToManagerImmediately;
            public readonly bool IsManager;
            public readonly bool SuppressStageBehaviour;
            public readonly bool SuppressReportAttempt;
            public readonly bool IgnoresSuspicionEvents;
            public Snapshot(bool p, bool r, bool m, bool s, bool sr = false, bool ig = false)
            { PropagatesSuspicion = p; ReportsToManagerImmediately = r; IsManager = m; SuppressStageBehaviour = s; SuppressReportAttempt = sr; IgnoresSuspicionEvents = ig; }
            public static Snapshot Default => new Snapshot(true, false, false, false);
        }

        public Snapshot ToSnapshot() => new Snapshot(propagatesSuspicion, reportsToManagerImmediately, isManager, suppressStageBehaviour, suppressReportAttempt, ignoresSuspicionEvents);

        public static Snapshot GetOrDefault(NpcIdentity npc)
        {
            if (npc == null) return Snapshot.Default;
            var profile = npc.GetComponent<NpcSuspicionProfile>();
            return profile != null ? profile.ToSnapshot() : Snapshot.Default;
        }
    }
}

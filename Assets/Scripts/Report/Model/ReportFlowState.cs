namespace Project1028.Report
{
    /// <summary>§5.1 신고 흐름 상태기계. 순수 로직. 종단(Completed/Abandoned/Interrupted) 이후 전이 없음.</summary>
    public sealed class ReportFlowState
    {
        public ReportPhase Phase { get; private set; } = ReportPhase.Moving;
        public string TargetId { get; private set; }
        public float ReportingSince { get; private set; }
        public bool IsTerminal => Phase == ReportPhase.Completed || Phase == ReportPhase.Abandoned || Phase == ReportPhase.Interrupted;

        public void Start(string targetId)
        {
            Phase = ReportPhase.Moving;
            TargetId = targetId;
        }

        public void Arrive(float now)
        {
            if (Phase != ReportPhase.Moving) return;
            Phase = ReportPhase.Reporting;
            ReportingSince = now;
        }

        /// <summary>Reporting 중 신고 동작 시간이 지나면 Completed로 전이하고 true.</summary>
        public bool Tick(float now, float reportDuration)
        {
            if (Phase != ReportPhase.Reporting) return false;
            if (now - ReportingSince < reportDuration) return false;
            Phase = ReportPhase.Completed;
            return true;
        }

        public void Retarget(string newTargetId)
        {
            if (IsTerminal) return;
            if (string.IsNullOrEmpty(newTargetId)) { Phase = ReportPhase.Abandoned; TargetId = null; return; }
            Phase = ReportPhase.Moving;
            TargetId = newTargetId;
        }

        public void Interrupt()
        {
            if (IsTerminal) return;
            Phase = ReportPhase.Interrupted;
        }
    }
}

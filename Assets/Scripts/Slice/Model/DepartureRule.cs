namespace Project1028.Slice
{
    public enum RunOutcome { None, Success, LeftWithoutEvidence, ForcedDeparture }

    public struct DepartureVerdict
    {
        public bool CanBoard;
        public string Reason;
        public RunOutcome Outcome;
    }

    /// <summary>§2.5 봉쇄 / §1 원칙 2 "나갈 수 없게 됨". 순수 로직. 강제 출도는 해석(확정대기).</summary>
    public static class DepartureRule
    {
        public static DepartureVerdict Evaluate(float elapsed, float departureAt, float nextBoatAt, bool blocked, bool hasEvidence, bool allAtDock, bool requireEvidence)
        {
            if (elapsed < departureAt) return new DepartureVerdict { CanBoard = false, Reason = "아직 배가 뜨지 않는다", Outcome = RunOutcome.None };
            if (blocked) return new DepartureVerdict { CanBoard = false, Reason = "배가 뜨지 않는다 — 봉쇄", Outcome = RunOutcome.None };
            if (!allAtDock) return new DepartureVerdict { CanBoard = false, Reason = "전원이 부두에 있어야 한다", Outcome = RunOutcome.None };
            if (requireEvidence && !hasEvidence) return new DepartureVerdict { CanBoard = true, Reason = "증거 없이 떠난다", Outcome = RunOutcome.LeftWithoutEvidence };
            return new DepartureVerdict { CanBoard = true, Reason = "출항", Outcome = RunOutcome.Success };
        }

        /// <summary>봉쇄 상태로 다음 배 시각에 도달하면 강제 출도.</summary>
        public static RunOutcome? Forced(float elapsed, float nextBoatAt, bool blocked)
        {
            if (blocked && elapsed >= nextBoatAt) return RunOutcome.ForcedDeparture;
            return null;
        }
    }
}

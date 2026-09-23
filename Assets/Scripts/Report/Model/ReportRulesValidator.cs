using System;
using System.Collections.Generic;
using Project1028.Suspicion;

namespace Project1028.Report
{
    public sealed class ReportValidationResult
    {
        public bool IsError;
        public int PendingCount;
        public readonly List<string> Messages = new List<string>();
        public void Error(string m) { IsError = true; Messages.Add("오류: " + m); }
    }

    public static class ReportRulesValidator
    {
        public static ReportValidationResult Validate(ReportRules r, Func<string, bool> eventExists)
        {
            var res = new ReportValidationResult();
            eventExists = eventExists ?? (_ => true);
            if (r == null) { res.Error("규칙 데이터가 null입니다."); return res; }
            if (r.minWalkSeconds <= 0f) res.Error("minWalkSeconds는 > 0 이어야 합니다.");
            if (r.reportDurationSeconds < 0f) res.Error("reportDurationSeconds는 ≥ 0 이어야 합니다.");
            if (r.cutDurationSeconds <= 0f) res.Error("cutDurationSeconds는 > 0 이어야 합니다.");
            if (r.cutRange <= 0f) res.Error("cutRange는 > 0 이어야 합니다.");
            if (r.arriveDistance <= 0f) res.Error("arriveDistance는 > 0 이어야 합니다.");
            if (string.IsNullOrEmpty(r.completedEventId) || !eventExists(r.completedEventId)) res.Error($"completedEventId '{r.completedEventId}'가 의심 규칙에 없습니다.");
            if (r.status == SuspicionRules.StatusPending) res.PendingCount++;
            else if (r.status != SuspicionRules.StatusConfirmed) res.Error("status는 '확정' 또는 '확정대기' 이어야 합니다.");
            return res;
        }
    }
}

using System;
using System.Collections.Generic;
using Project1028.Suspicion;

namespace Project1028.Subdue
{
    public sealed class SubdueValidationResult
    {
        public bool IsError;
        public int PendingCount;
        public readonly List<string> Messages = new List<string>();
        public void Error(string m) { IsError = true; Messages.Add("오류: " + m); }
        public void Warning(string m) { Messages.Add("경고: " + m); }
    }

    public static class SubdueRulesValidator
    {
        public static SubdueValidationResult Validate(SubdueRules r, Func<string, bool> eventExists)
        {
            var res = new SubdueValidationResult();
            eventExists = eventExists ?? (_ => true);
            if (r == null) { res.Error("규칙 데이터가 null입니다."); return res; }

            if (r.actions == null) res.Error("actions 블록이 없습니다.");
            else
            {
                Action(res, r.actions.backstab, "actions.backstab", eventExists, needsCone: true, needsFail: false);
                Action(res, r.actions.shove, "actions.shove", eventExists, needsCone: false, needsFail: true);
                Action(res, r.actions.objectStrike, "actions.objectStrike", eventExists, needsCone: false, needsFail: false);
            }

            if (r.unconscious == null) res.Error("unconscious 블록이 없습니다.");
            else
            {
                if (r.unconscious.minSeconds <= 0f) res.Error("unconscious.minSeconds는 > 0 이어야 합니다.");
                if (r.unconscious.maxSeconds < r.unconscious.minSeconds) res.Error("unconscious.maxSeconds는 minSeconds 이상이어야 합니다.");
                Status(res, r.unconscious.status, "unconscious");
            }

            if (r.carry == null) res.Error("carry 블록이 없습니다.");
            else
            {
                if (r.carry.speedMultiplier <= 0f || r.carry.speedMultiplier >= 1f) res.Error("carry.speedMultiplier는 0과 1 사이(배타)여야 합니다.");
                Status(res, r.carry.status, "carry");
            }

            if (r.costs == null) res.Error("costs 블록이 없습니다.");
            else
            {
                foreach (var (id, name) in new[] { (r.costs.witnessedEventId, "witnessedEventId"), (r.costs.foundIslandEventId, "foundIslandEventId"), (r.costs.foundPersonalEventId, "foundPersonalEventId"), (r.costs.wakeEventId, "wakeEventId"), (r.costs.shoveFailEventId, "shoveFailEventId") })
                {
                    if (string.IsNullOrEmpty(id) || !eventExists(id)) res.Error($"costs.{name} '{id}'가 의심 규칙에 없습니다.");
                }
                Status(res, r.costs.status, "costs");
            }
            return res;
        }

        private static void Action(SubdueValidationResult res, ActionRule a, string label, Func<string, bool> exists, bool needsCone, bool needsFail)
        {
            if (a == null) { res.Error($"{label}가 없습니다."); return; }
            if (a.range <= 0f) res.Error($"{label}.range는 > 0 이어야 합니다.");
            if (needsCone && (a.backConeDegrees <= 0f || a.backConeDegrees > 180f)) res.Error($"{label}.backConeDegrees는 0~180 이어야 합니다.");
            if (needsFail && (a.failChance < 0f || a.failChance > 1f)) res.Error($"{label}.failChance는 0~1 이어야 합니다.");
            if (!Enum.TryParse<NoiseLevel>(a.noiseLevel, false, out _)) res.Error($"{label}.noiseLevel은 Low|Medium|High 이어야 합니다. (현재 '{a.noiseLevel}')");
            if (string.IsNullOrEmpty(a.noiseEventId) || !exists(a.noiseEventId)) res.Error($"{label}.noiseEventId '{a.noiseEventId}'가 의심 규칙에 없습니다.");
            Status(res, a.status, label);
        }

        private static void Status(SubdueValidationResult res, string status, string label)
        {
            if (status == SuspicionRules.StatusPending) res.PendingCount++;
            else if (status != SuspicionRules.StatusConfirmed) res.Error($"{label}: status는 '확정' 또는 '확정대기' 이어야 합니다.");
        }
    }
}

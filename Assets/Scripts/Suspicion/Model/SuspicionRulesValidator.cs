using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Project1028.Suspicion
{
    public sealed class RulesValidationResult
    {
        public bool IsError;
        public int PendingCount;
        public readonly List<string> Messages = new List<string>();
        public void Error(string m) { IsError = true; Messages.Add("오류: " + m); }
        public void Warning(string m) { Messages.Add("경고: " + m); }
    }

    /// <summary>순수 로직: 규칙 데이터 검증. Error면 시스템 비활성, Warning(음수 delta 등)은 클램프 후 진행.</summary>
    public static class SuspicionRulesValidator
    {
        private static readonly Regex IdPattern = new Regex("^[a-z][a-z0-9_]*$", RegexOptions.Compiled);

        public static RulesValidationResult Validate(SuspicionRules r)
        {
            var res = new RulesValidationResult();
            if (r == null) { res.Error("규칙 데이터가 null입니다."); return res; }

            // events
            if (r.events == null || r.events.Length == 0)
            {
                res.Error("events가 비어 있습니다.");
            }
            else
            {
                var ids = new HashSet<string>();
                for (int i = 0; i < r.events.Length; i++)
                {
                    var e = r.events[i];
                    string label = e?.id ?? $"events[{i}]";
                    if (e == null) { res.Error($"{label}: null 항목"); continue; }
                    if (string.IsNullOrEmpty(e.id) || !IdPattern.IsMatch(e.id)) res.Error($"{label}: id는 ^[a-z][a-z0-9_]*$ 이어야 합니다.");
                    else if (!ids.Add(e.id)) res.Error($"{label}: id 중복");
                    if (e.personalDelta < 0) { res.Warning($"{label}: personalDelta 음수 → 0으로 클램프"); e.personalDelta = 0; }
                    if (e.islandDelta < 0) { res.Warning($"{label}: islandDelta 음수 → 0으로 클램프"); e.islandDelta = 0; }
                    if (e.scope != SuspicionEventRule.ScopeWitness && e.scope != SuspicionEventRule.ScopeRadius && e.scope != SuspicionEventRule.ScopeTarget && e.scope != SuspicionEventRule.ScopeGlobal)
                        res.Error($"{label}: scope는 witness|radius|target|global 이어야 합니다. (현재 '{e.scope}')");
                    if (e.scope == SuspicionEventRule.ScopeGlobal && e.personalDelta > 0) res.Warning($"{label}: global scope는 개인 의심에 적용되지 않습니다 (personalDelta 무시).");
                    if (e.scope == SuspicionEventRule.ScopeRadius && e.radius <= 0f) res.Error($"{label}: radius scope는 radius > 0 이어야 합니다.");
                    CheckStatus(res, e.status, label);
                }
                if (r.FindEvent(SuspicionRules.PropagationEventId) == null) res.Error($"events에 '{SuspicionRules.PropagationEventId}' 항목이 없습니다 (전파 +1의 유일한 출처).");
            }

            // decay
            if (r.decay == null) res.Error("decay 블록이 없습니다.");
            else
            {
                if (r.decay.personalIntervalSeconds <= 0f) res.Error("decay.personalIntervalSeconds는 > 0 이어야 합니다.");
                if (r.decay.islandPerMinute < 0f) res.Error("decay.islandPerMinute는 ≥ 0 이어야 합니다.");
                CheckStatus(res, r.decay.status, "decay");
            }

            // propagation
            if (r.propagation == null) res.Error("propagation 블록이 없습니다.");
            else
            {
                if (r.propagation.distance <= 0f) res.Error("propagation.distance는 > 0 이어야 합니다.");
                if (r.propagation.delaySeconds < 0f) res.Error("propagation.delaySeconds는 ≥ 0 이어야 합니다.");
                CheckStatus(res, r.propagation.status, "propagation");
            }

            // timeOfDay
            if (r.timeOfDay == null) res.Error("timeOfDay 블록이 없습니다.");
            else
            {
                if (r.timeOfDay.dayMultiplier <= 0f || r.timeOfDay.nightMultiplier <= 0f) res.Error("timeOfDay 배율은 > 0 이어야 합니다.");
                if (r.timeOfDay.nightWanderIntervalSeconds <= 0f) res.Error("timeOfDay.nightWanderIntervalSeconds는 > 0 이어야 합니다.");
                if (r.events != null && r.FindEvent(r.timeOfDay.nightWanderEventId) == null) res.Error($"timeOfDay.nightWanderEventId '{r.timeOfDay.nightWanderEventId}'가 events에 없습니다.");
                CheckStatus(res, r.timeOfDay.status, "timeOfDay");
            }

            // carryOver
            if (r.carryOver == null) res.Error("carryOver 블록이 없습니다.");
            else
            {
                if (r.carryOver.personalFactor < 0f || r.carryOver.personalFactor > 1f) res.Error("carryOver.personalFactor는 0~1 이어야 합니다.");
                if (r.carryOver.islandFactor < 0f || r.carryOver.islandFactor > 1f) res.Error("carryOver.islandFactor는 0~1 이어야 합니다.");
                if (r.carryOver.islandMin < 0 || r.carryOver.islandMin > 100) res.Error("carryOver.islandMin은 0~100 이어야 합니다.");
                CheckStatus(res, r.carryOver.status, "carryOver");
            }

            if (r.barks == null || r.barks.Length == 0) res.Error("barks가 비어 있습니다.");
            return res;
        }

        private static void CheckStatus(RulesValidationResult res, string status, string label)
        {
            if (status == SuspicionRules.StatusPending) res.PendingCount++;
            else if (status != SuspicionRules.StatusConfirmed) res.Error($"{label}: status는 '확정' 또는 '확정대기' 이어야 합니다. (현재 '{status}')");
        }
    }
}

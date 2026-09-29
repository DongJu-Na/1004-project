using System;
using System.Collections.Generic;
using Project1028.Suspicion;

namespace Project1028.Slice
{
    public sealed class SliceValidationResult
    {
        public bool IsError;
        public int PendingCount;
        public readonly List<string> Messages = new List<string>();
        public void Error(string m) { IsError = true; Messages.Add("오류: " + m); }
    }

    public static class SliceRulesValidator
    {
        public static SliceValidationResult Validate(SliceRules r, Func<string, bool> eventExists)
        {
            var res = new SliceValidationResult();
            eventExists = eventExists ?? (_ => true);
            if (r == null) { res.Error("규칙 데이터가 null입니다."); return res; }

            if (r.clock == null) res.Error("clock 없음");
            else
            {
                if (r.clock.nightAtSeconds <= 0f) res.Error("clock.nightAtSeconds > 0");
                if (r.clock.departureAtSeconds <= r.clock.nightAtSeconds) res.Error("clock.departureAtSeconds는 nightAtSeconds보다 커야 합니다.");
                if (r.clock.nextBoatDelaySeconds <= 0f) res.Error("clock.nextBoatDelaySeconds > 0");
                Status(res, r.clock.status, "clock");
            }
            if (r.evidence == null) res.Error("evidence 없음");
            else
            {
                if (r.evidence.investigateSeconds <= 0f) res.Error("evidence.investigateSeconds > 0");
                if (r.evidence.seenCooldownSeconds <= 0f) res.Error("evidence.seenCooldownSeconds > 0");
                foreach (var id in new[] { r.evidence.oneHandEventId, r.evidence.twoHandEventId })
                    if (string.IsNullOrEmpty(id) || !eventExists(id)) res.Error($"evidence 사건 '{id}'가 의심 규칙에 없습니다.");
                Status(res, r.evidence.status, "evidence");
            }
            if (r.restricted == null) res.Error("restricted 없음");
            else
            {
                if (r.restricted.cooldownSeconds < 0f) res.Error("restricted.cooldownSeconds ≥ 0");
                if (string.IsNullOrEmpty(r.restricted.eventId) || !eventExists(r.restricted.eventId)) res.Error($"restricted.eventId '{r.restricted.eventId}'가 의심 규칙에 없습니다.");
                Status(res, r.restricted.status, "restricted");
            }
            if (r.dock == null) res.Error("dock 없음");
            else { if (r.dock.boardRadius <= 0f) res.Error("dock.boardRadius > 0"); Status(res, r.dock.status, "dock"); }
            if (r.vehicle == null) res.Error("vehicle 없음");
            if (r.objectives == null || r.objectives.Length == 0) res.Error("objectives 비어 있음");
            else foreach (var o in r.objectives)
            {
                if (o == null || string.IsNullOrEmpty(o.id) || string.IsNullOrEmpty(o.text) || string.IsNullOrEmpty(o.markerFacilityId)) res.Error("objective: id/text/markerFacilityId 필수");
                else if (!ObjectiveConditions.IsKnown(o.condition)) res.Error($"objective {o.id}: condition '{o.condition}' 알 수 없음");
            }
            if (r.hints == null || r.hints.Length == 0) res.Error("hints 비어 있음");
            if (r.hintSeconds <= 0f) res.Error("hintSeconds > 0");
            if (r.feedback == null) res.Error("feedback 없음");
            else
            {
                var f = r.feedback;
                if (f.heartbeat == null || f.heartbeat.intervalsByStage == null || f.heartbeat.intervalsByStage.Length != 4 || f.heartbeat.zoneMultiplier == null || f.heartbeat.zoneMultiplier.Length != 4)
                    res.Error("feedback.heartbeat intervalsByStage/zoneMultiplier는 길이 4");
                else
                {
                    foreach (var v in f.heartbeat.intervalsByStage) if (v <= 0f) res.Error("heartbeat.intervalsByStage > 0");
                    foreach (var v in f.heartbeat.zoneMultiplier) if (v <= 0f) res.Error("heartbeat.zoneMultiplier > 0");
                    if (f.heartbeat.volume < 0f || f.heartbeat.volume > 1f) res.Error("heartbeat.volume 0~1");
                }
                if (f.pulse == null || f.pulse.seconds <= 0f || f.pulse.alpha < 0f || f.pulse.alpha > 1f) res.Error("feedback.pulse seconds>0, alpha 0~1");
                if (f.tint == null || f.tint.alphaByZone == null || f.tint.alphaByZone.Length != 4) res.Error("feedback.tint.alphaByZone 길이 4");
                if (f.hitstop == null || f.hitstop.seconds < 0f) res.Error("feedback.hitstop.seconds ≥ 0");
                if (f.shake == null || f.shake.amplitude < 0f || f.shake.seconds < 0f) res.Error("feedback.shake ≥ 0");
                if (f.bigText == null || f.bigText.seconds <= 0f) res.Error("feedback.bigText.seconds > 0");
            }
            return res;
        }

        private static void Status(SliceValidationResult res, string status, string label)
        {
            if (status == SuspicionRules.StatusPending) res.PendingCount++;
            else if (status != SuspicionRules.StatusConfirmed) res.Error($"{label}: status는 '확정' 또는 '확정대기'");
        }
    }
}

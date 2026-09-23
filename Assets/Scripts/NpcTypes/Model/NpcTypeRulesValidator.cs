using System;
using System.Collections.Generic;
using Project1028.Suspicion;

namespace Project1028.NpcTypes
{
    public sealed class TypeRulesValidationResult
    {
        public bool IsError;
        public int PendingCount;
        public readonly List<string> Messages = new List<string>();
        public void Error(string m) { IsError = true; Messages.Add("오류: " + m); }
        public void Warning(string m) { Messages.Add("경고: " + m); }
    }

    /// <summary>순수 로직. faction 누락은 유형 기본값으로 채우고, 상충은 진영 우선 + 경고.</summary>
    public static class NpcTypeRulesValidator
    {
        public static TypeRulesValidationResult Validate(NpcTypeRules r, Func<string, bool> suspicionEventExists)
        {
            var res = new TypeRulesValidationResult();
            if (r == null) { res.Error("규칙 데이터가 null입니다."); return res; }
            suspicionEventExists = suspicionEventExists ?? (_ => true);

            if (r.rules == null) { res.Error("rules 블록이 없습니다."); }
            else
            {
                if (r.rules.watcher == null) res.Error("rules.watcher가 없습니다.");
                else
                {
                    if (string.IsNullOrEmpty(r.rules.watcher.investigateEventId) || !suspicionEventExists(r.rules.watcher.investigateEventId))
                        res.Error($"rules.watcher.investigateEventId '{r.rules.watcher.investigateEventId}'가 의심 규칙에 없습니다.");
                    Status(res, r.rules.watcher.status, "rules.watcher");
                }
                if (r.rules.informer == null) res.Error("rules.informer가 없습니다.");
                else
                {
                    if (r.rules.informer.deliveryDelaySeconds <= 0f) res.Error("rules.informer.deliveryDelaySeconds는 > 0 이어야 합니다.");
                    if (r.rules.informer.arriveDistance <= 0f) res.Error("rules.informer.arriveDistance는 > 0 이어야 합니다.");
                    if (string.IsNullOrEmpty(r.rules.informer.deliveryEventId) || !suspicionEventExists(r.rules.informer.deliveryEventId))
                        res.Error($"rules.informer.deliveryEventId '{r.rules.informer.deliveryEventId}'가 의심 규칙에 없습니다.");
                    Status(res, r.rules.informer.status, "rules.informer");
                }
                if (r.rules.sympathizer == null) res.Error("rules.sympathizer가 없습니다.");
                else
                {
                    if (!NpcTypeDefinitions.TryParseZone(r.rules.sympathizer.turnZone, out var z) || z == AlertZone.Calm)
                        res.Error($"rules.sympathizer.turnZone은 Watch|Tension|Lockdown 이어야 합니다. (현재 '{r.rules.sympathizer.turnZone}')");
                    Status(res, r.rules.sympathizer.status, "rules.sympathizer");
                }
                if (r.rules.sentinel == null) res.Error("rules.sentinel이 없습니다.");
                else
                {
                    if (r.rules.sentinel.detectRadius <= 0f) res.Error("rules.sentinel.detectRadius는 > 0 이어야 합니다.");
                    if (r.rules.sentinel.soundRadius <= 0f) res.Error("rules.sentinel.soundRadius는 > 0 이어야 합니다.");
                    if (r.rules.sentinel.cooldownSeconds < 0f) res.Error("rules.sentinel.cooldownSeconds는 ≥ 0 이어야 합니다.");
                    if (string.IsNullOrEmpty(r.rules.sentinel.noiseEventId) || !suspicionEventExists(r.rules.sentinel.noiseEventId))
                        res.Error($"rules.sentinel.noiseEventId '{r.rules.sentinel.noiseEventId}'가 의심 규칙에 없습니다.");
                    Status(res, r.rules.sentinel.status, "rules.sentinel");
                }
            }

            if (r.assignments == null) { res.Error("assignments가 없습니다."); return res; }
            var ids = new HashSet<string>();
            for (int i = 0; i < r.assignments.Length; i++)
            {
                var a = r.assignments[i];
                string label = a?.npcId ?? $"assignments[{i}]";
                if (a == null) { res.Error($"{label}: null 항목"); continue; }
                if (string.IsNullOrWhiteSpace(a.npcId)) res.Error($"{label}: npcId 비어 있음");
                else if (!ids.Add(a.npcId)) res.Error($"{label}: npcId 중복");

                if (!NpcTypeDefinitions.TryParseType(a.type, out var type))
                {
                    res.Error($"{label}: type '{a.type}' 알 수 없음 (Watcher|Informer|Sympathizer|Silent|Sentinel)");
                    continue;
                }

                var defaultFaction = NpcTypeDefinitions.DefaultFactionOf(type);
                if (string.IsNullOrEmpty(a.faction))
                {
                    a.faction = defaultFaction.ToString();
                }
                else if (!NpcTypeDefinitions.TryParseFaction(a.faction, out var faction))
                {
                    res.Error($"{label}: faction '{a.faction}' 알 수 없음");
                }
                else if (faction != defaultFaction)
                {
                    res.Warning($"{label}: 유형 {type}의 기본 진영은 {defaultFaction}이나 {faction}으로 지정됨 — 진영을 우선합니다.");
                }

                if (a.roles != null && Array.IndexOf(a.roles, Roles.Broker) >= 0 && a.faction != Faction.Antagonist.ToString())
                    res.Warning($"{label}: 중개인(broker) 역할은 Antagonist를 권장합니다.");
            }
            return res;
        }

        private static void Status(TypeRulesValidationResult res, string status, string label)
        {
            if (status == SuspicionRules.StatusPending) res.PendingCount++;
            else if (status != SuspicionRules.StatusConfirmed) res.Error($"{label}: status는 '확정' 또는 '확정대기' 이어야 합니다.");
        }
    }
}

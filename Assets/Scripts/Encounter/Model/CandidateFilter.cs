using System;
using System.Collections.Generic;
using Project1028.Suspicion;

namespace Project1028.Encounter
{
    /// <summary>§6.4 후보 필터: 풀(현재 섬 + 공용), 시간대·구간·플래그 조건, 원본 재등장 금지, 변주는 원본 이력 + 회수 플래그 보유 시. 순수 로직.</summary>
    public static class CandidateFilter
    {
        public const string CommonIsland = "common";

        public static List<EncounterDef> Filter(EncounterDef[] defs, PoolDef[] pools, string islandId, DayPhase phase, AlertZone zone,
            ISet<string> flags, ISet<string> history, CallbackEntry[] callbacks)
        {
            var result = new List<EncounterDef>();
            if (defs == null) return result;
            var inPool = new HashSet<string>();
            if (pools != null) foreach (var p in pools)
            {
                if (p == null || p.encounterIds == null) continue;
                if (p.islandId != CommonIsland && p.islandId != islandId) continue;
                foreach (var id in p.encounterIds) inPool.Add(id);
            }

            foreach (var d in defs)
            {
                if (d == null || !inPool.Contains(d.id)) continue;
                if (history != null && history.Contains(d.id)) continue; // 동일 인카운터 재등장 금지

                var c = d.conditions ?? new ConditionDef();
                if (c.timeOfDay == "day" && phase != DayPhase.Day) continue;
                if (c.timeOfDay == "night" && phase != DayPhase.Night) continue;
                if (Enum.TryParse<AlertZone>(c.minZone ?? "Calm", out var minZone) && zone < minZone) continue;
                if (!string.IsNullOrEmpty(c.requiresFlag) && (flags == null || !flags.Contains(c.requiresFlag))) continue;
                if (!string.IsNullOrEmpty(c.forbidsFlag) && flags != null && flags.Contains(c.forbidsFlag)) continue;

                if (!string.IsNullOrEmpty(d.variantOf))
                {
                    // 변주: 원본을 봤고, 회수 테이블에 (보유 플래그 → 이 변주) 항목이 있을 때만
                    if (history == null || !history.Contains(d.variantOf)) continue;
                    bool unlocked = false;
                    if (callbacks != null && flags != null)
                        foreach (var cb in callbacks)
                            if (cb != null && cb.targetKind == "encounter_variant" && cb.targetId == d.id && flags.Contains(cb.flag)) { unlocked = true; break; }
                    if (!unlocked) continue;
                }
                result.Add(d);
            }
            return result;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Project1028.Encounter
{
    /// <summary>§6.3 유형 비율(확정대기)을 가중치로 유형을 고르고, 그 유형 후보 중 균등 선택. 존재하는 유형만으로 정규화. 순수 로직.</summary>
    public static class WeightedTypePicker
    {
        public static EncounterDef Pick(List<EncounterDef> candidates, TypeWeights weights, float rollType, float rollIndex)
        {
            if (candidates == null || candidates.Count == 0) return null;
            var byType = new Dictionary<EncounterType, List<EncounterDef>>();
            foreach (var d in candidates)
            {
                var t = EncounterRules.TypeOf(d);
                if (!byType.TryGetValue(t, out var list)) { list = new List<EncounterDef>(); byType[t] = list; }
                list.Add(d);
            }

            float total = 0f;
            foreach (var kv in byType) total += Mathf.Max(0f, weights != null ? weights.Of(kv.Key) : 1f);
            EncounterType chosen = EncounterType.D;
            if (total <= 0f)
            {
                foreach (var kv in byType) { chosen = kv.Key; break; }
            }
            else
            {
                float r = Mathf.Clamp01(rollType) * total;
                float acc = 0f;
                bool set = false;
                foreach (EncounterType t in new[] { EncounterType.A, EncounterType.B, EncounterType.C, EncounterType.D })
                {
                    if (!byType.ContainsKey(t)) continue;
                    acc += Mathf.Max(0f, weights != null ? weights.Of(t) : 1f);
                    if (r < acc || Mathf.Approximately(acc, total)) { chosen = t; set = true; break; }
                }
                if (!set) foreach (var kv in byType) { chosen = kv.Key; break; }
            }

            var pool = byType[chosen];
            int idx = Mathf.Clamp(Mathf.FloorToInt(Mathf.Clamp01(rollIndex) * pool.Count), 0, pool.Count - 1);
            return pool[idx];
        }
    }
}

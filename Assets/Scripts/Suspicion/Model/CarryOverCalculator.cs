using System.Collections.Generic;
using UnityEngine;

namespace Project1028.Suspicion
{
    public sealed class CarryOverSnapshot
    {
        public readonly Dictionary<(string npcId, string playerId), int> Personal = new Dictionary<(string, string), int>();
        public int Island;
    }

    /// <summary>§2.7 런 종료 이월. 순수 로직. 완전 초기화 금지: Island는 islandMin 이상.</summary>
    public static class CarryOverCalculator
    {
        public static CarryOverSnapshot Compute(IEnumerable<(string npcId, string playerId, int value)> personal, int island, CarryOverRule rule)
        {
            var snap = new CarryOverSnapshot();
            float pf = rule != null ? rule.personalFactor : 0f;
            float inf = rule != null ? rule.islandFactor : 0f;
            int min = rule != null ? rule.islandMin : 0;

            if (personal != null)
            {
                foreach (var (npcId, playerId, value) in personal)
                {
                    snap.Personal[(npcId, playerId)] = Mathf.Max(0, Mathf.FloorToInt(value * pf));
                }
            }
            snap.Island = Mathf.Clamp(Mathf.Max(min, Mathf.RoundToInt(island * inf)), IslandAlert.Min, IslandAlert.Max);
            return snap;
        }
    }
}

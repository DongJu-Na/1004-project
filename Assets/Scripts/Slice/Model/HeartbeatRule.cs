using UnityEngine;

namespace Project1028.Slice
{
    /// <summary>심박 간격 = 최고 개인 단계별 간격 × 섬 구간 배율. 순수 로직.</summary>
    public static class HeartbeatRule
    {
        public static float IntervalFor(int maxStage, int zoneIndex, float[] intervalsByStage, float[] zoneMultiplier)
        {
            if (intervalsByStage == null || intervalsByStage.Length == 0) return 2f;
            int s = Mathf.Clamp(maxStage, 0, intervalsByStage.Length - 1);
            float mult = 1f;
            if (zoneMultiplier != null && zoneMultiplier.Length > 0) mult = zoneMultiplier[Mathf.Clamp(zoneIndex, 0, zoneMultiplier.Length - 1)];
            return Mathf.Max(0.05f, intervalsByStage[s] * mult);
        }
    }
}

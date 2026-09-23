using UnityEngine;

namespace Project1028.Suspicion
{
    /// <summary>§2.7 감소 계산. 순수 로직.</summary>
    public static class DecayCalculator
    {
        /// <summary>마지막 상승 또는 마지막 감소 중 늦은 시점부터 interval마다 1단계.</summary>
        public static int PersonalSteps(float lastRaisedAt, float lastDecayAt, float now, float interval)
        {
            if (interval <= 0f) return 0;
            float basis = Mathf.Max(lastRaisedAt, lastDecayAt);
            float elapsed = now - basis;
            if (elapsed < interval) return 0;
            return Mathf.FloorToInt(elapsed / interval);
        }

        public static float IslandAmount(float deltaSeconds, float perMinute)
        {
            if (deltaSeconds <= 0f || perMinute <= 0f) return 0f;
            return perMinute * (deltaSeconds / 60f);
        }
    }
}

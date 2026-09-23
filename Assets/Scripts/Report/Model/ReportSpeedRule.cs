using UnityEngine;

namespace Project1028.Report
{
    /// <summary>§5.1 "플레이어에게 시간이 있다": 도보 시간이 최소값 이상이 되도록 속도를 낮춘다. 순수 로직.</summary>
    public static class ReportSpeedRule
    {
        public const float MinSpeed = 0.1f;

        public static float SpeedFor(float distance, float normalSpeed, float minWalkSeconds)
        {
            if (minWalkSeconds <= 0f) return normalSpeed;
            return Mathf.Max(MinSpeed, Mathf.Min(normalSpeed, distance / minWalkSeconds));
        }
    }
}

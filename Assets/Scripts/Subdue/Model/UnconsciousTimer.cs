using UnityEngine;

namespace Project1028.Subdue
{
    /// <summary>§4.3 기절 타이머: min~max에서 추출, 운반 중에도 흐른다.</summary>
    public sealed class UnconsciousTimer
    {
        public float Duration { get; }
        public float Remaining { get; private set; }
        public bool IsAwake => Remaining <= 0f;

        public UnconsciousTimer(float minSeconds, float maxSeconds, float roll01)
        {
            float lo = Mathf.Max(0f, minSeconds);
            float hi = Mathf.Max(lo, maxSeconds);
            Duration = Mathf.Lerp(lo, hi, Mathf.Clamp01(roll01));
            Remaining = Duration;
        }

        public void Tick(float deltaSeconds)
        {
            if (deltaSeconds <= 0f) return;
            Remaining = Mathf.Max(0f, Remaining - deltaSeconds);
        }
    }
}

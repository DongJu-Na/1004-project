namespace Project1028.Vehicle
{
    /// <summary>up.y가 임계 이하로 seconds 지속되면 뒤집힘. 도달 순간 1회 true. 순수 로직.</summary>
    public sealed class FlipDetector
    {
        private readonly float threshold;
        private readonly float seconds;
        private float accumulated;

        public bool IsFlipped { get; private set; }

        public FlipDetector(float upThreshold, float requiredSeconds)
        {
            threshold = upThreshold;
            seconds = requiredSeconds < 0f ? 0f : requiredSeconds;
        }

        public bool Tick(float upY, float dt)
        {
            if (upY > threshold)
            {
                Reset();
                return false;
            }
            if (IsFlipped) return false;
            accumulated += dt < 0f ? 0f : dt;
            if (accumulated >= seconds)
            {
                IsFlipped = true;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            accumulated = 0f;
            IsFlipped = false;
        }
    }
}

using UnityEngine;

namespace Project1028.Slice
{
    public enum RunPhase { Day, Night, DepartureOpen, Lockdown, Ended }

    /// <summary>§2.5 "시계를 보며 의심을 관리": 밤 전환·출항 시각·다음 배. 순수 로직.</summary>
    public sealed class RunClock
    {
        private readonly float nightAt, departureAt, nextBoatDelay;

        public float Elapsed { get; private set; }
        public RunPhase Phase { get; private set; } = RunPhase.Day;
        public bool IsNight => Elapsed >= nightAt;
        public bool IsDepartureOpen => Elapsed >= departureAt;
        public float NextBoatAt => departureAt + nextBoatDelay;
        public float RemainingToDeparture => Mathf.Max(0f, departureAt - Elapsed);
        public float RemainingToNextBoat => Mathf.Max(0f, NextBoatAt - Elapsed);
        public float DepartureAt => departureAt;

        public RunClock(float nightAtSeconds, float departureAtSeconds, float nextBoatDelaySeconds)
        {
            nightAt = nightAtSeconds; departureAt = departureAtSeconds; nextBoatDelay = nextBoatDelaySeconds;
        }

        /// <summary>전이가 있으면 새 단계를 반환.</summary>
        public RunPhase? Tick(float dt, bool blocked)
        {
            if (Phase == RunPhase.Ended) return null;
            Elapsed += dt < 0f ? 0f : dt;
            RunPhase next = Compute(blocked);
            if (next == Phase) return null;
            Phase = next;
            return next;
        }

        public void Skip(float seconds) { if (Phase != RunPhase.Ended) Elapsed += Mathf.Max(0f, seconds); }

        private RunPhase Compute(bool blocked)
        {
            if (Elapsed >= departureAt) return blocked ? RunPhase.Lockdown : RunPhase.DepartureOpen;
            if (Elapsed >= nightAt) return RunPhase.Night;
            return RunPhase.Day;
        }

        public void End() => Phase = RunPhase.Ended;
    }
}

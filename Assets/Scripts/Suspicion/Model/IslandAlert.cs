using UnityEngine;

namespace Project1028.Suspicion
{
    /// <summary>§2.4 네 구간.</summary>
    public enum AlertZone { Calm, Watch, Tension, Lockdown }

    public struct ZoneChange
    {
        public AlertZone Old;
        public AlertZone New;
    }

    /// <summary>§2.1/§2.4/§2.5 섬 의심도 0~100. 씬에 하나. 순수 로직.</summary>
    public sealed class IslandAlert
    {
        public const int Min = 0;
        public const int Max = 100;
        public const int WatchFrom = 26;
        public const int TensionFrom = 51;
        public const int LockdownFrom = 76;

        private float fractional;

        public int Value { get; private set; }
        public AlertZone Zone => ZoneOf(Value);
        public bool DepartureBlocked => Zone == AlertZone.Lockdown;

        public IslandAlert(int initial = Min) { Value = Mathf.Clamp(initial, Min, Max); }

        public static AlertZone ZoneOf(int value)
        {
            if (value >= LockdownFrom) return AlertZone.Lockdown;
            if (value >= TensionFrom) return AlertZone.Tension;
            if (value >= WatchFrom) return AlertZone.Watch;
            return AlertZone.Calm;
        }

        public ZoneChange? Apply(int delta)
        {
            int before = Value;
            Value = Mathf.Clamp(Value + delta, Min, Max);
            if (delta > 0) fractional = 0f; // 상승 시 감소 소수 누적 초기화
            return ChangeOf(before);
        }

        /// <summary>감소량을 소수로 누적하고 정수 단위로만 값에 반영한다 (§2.7 매우 느림).</summary>
        public ZoneChange? ApplyDecay(float amount)
        {
            if (amount <= 0f || Value <= Min) return null;
            fractional += amount;
            int whole = Mathf.FloorToInt(fractional);
            if (whole <= 0) return null;
            fractional -= whole;
            int before = Value;
            Value = Mathf.Clamp(Value - whole, Min, Max);
            return ChangeOf(before);
        }

        private ZoneChange? ChangeOf(int before)
        {
            var oldZone = ZoneOf(before);
            var newZone = Zone;
            if (oldZone == newZone) return null;
            return new ZoneChange { Old = oldZone, New = newZone };
        }
    }
}

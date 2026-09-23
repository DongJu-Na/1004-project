using UnityEngine;

namespace Project1028.Suspicion
{
    /// <summary>§2.2 네 단계.</summary>
    public enum SuspicionStage { Indifferent = 0, Aware = 1, Alert = 2, Certain = 3 }

    public struct StageChange
    {
        public SuspicionStage Old;
        public SuspicionStage New;
    }

    /// <summary>§2.1 개인 의심 0~3. (NPC, 플레이어) 쌍마다 하나. 순수 로직.</summary>
    public sealed class PersonalSuspicion
    {
        public const int Min = 0;
        public const int Max = 3;

        public int Value { get; private set; }
        public SuspicionStage Stage => (SuspicionStage)Value;
        public float LastRaisedAt { get; private set; }
        public float LastDecayAt { get; set; }

        public PersonalSuspicion(float now = 0f)
        {
            Value = Min;
            LastRaisedAt = now;
            LastDecayAt = now;
        }

        public StageChange? Apply(int delta, float now)
        {
            int before = Value;
            Value = Mathf.Clamp(Value + delta, Min, Max);
            if (delta > 0) LastRaisedAt = now;
            if (Value == before) return null;
            return new StageChange { Old = (SuspicionStage)before, New = (SuspicionStage)Value };
        }
    }
}

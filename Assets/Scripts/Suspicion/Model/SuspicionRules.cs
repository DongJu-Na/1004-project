using System;

namespace Project1028.Suspicion
{
    /// <summary>suspicion_rules.json 모델. 모든 수치는 이 파일에만 있다 (FR-009). 스키마: contracts/suspicion-rules-schema.json</summary>
    [Serializable]
    public sealed class SuspicionRules
    {
        public const string StatusConfirmed = "확정";
        public const string StatusPending = "확정대기";
        public const string PropagationEventId = "propagation";

        public string version;
        public SuspicionEventRule[] events;
        public DecayRule decay;
        public PropagationRule propagation;
        public TimeOfDayRule timeOfDay;
        public CarryOverRule carryOver;
        public string[] barks;

        public SuspicionEventRule FindEvent(string id)
        {
            if (events == null || string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < events.Length; i++)
            {
                if (events[i] != null && events[i].id == id) return events[i];
            }
            return null;
        }
    }

    [Serializable]
    public sealed class SuspicionEventRule
    {
        public const string ScopeWitness = "witness";
        public const string ScopeRadius = "radius";
        public const string ScopeTarget = "target";
        public const string ScopeGlobal = "global"; // 대상 NPC 없음, 섬 의심도만 (004 대가)

        public string id;
        public int personalDelta;
        public int islandDelta;
        public string scope;
        public float radius;
        public bool requiresSight;
        public bool nightOnly;
        public bool ignoresTimeMultiplier; // §3 감시자 '즉시 +2' 등 시간대 배율(§7)을 무시하는 확정 규칙용
        public string status;
        public string note;
    }

    [Serializable] public sealed class DecayRule { public float personalIntervalSeconds; public float islandPerMinute; public string status; }
    [Serializable] public sealed class PropagationRule { public float distance; public float delaySeconds; public string status; }
    [Serializable] public sealed class TimeOfDayRule { public float dayMultiplier; public float nightMultiplier; public string nightWanderEventId; public float nightWanderIntervalSeconds; public string status; }
    [Serializable] public sealed class CarryOverRule { public float personalFactor; public float islandFactor; public int islandMin; public string status; }
}

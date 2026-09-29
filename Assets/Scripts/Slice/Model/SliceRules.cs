using System;

namespace Project1028.Slice
{
    /// <summary>slice_rules.json 모델. 스키마: contracts/slice-rules-schema.json</summary>
    [Serializable]
    public sealed class SliceRules
    {
        public string version;
        public ClockRule clock;
        public EvidenceRule evidence;
        public RestrictedRule restricted;
        public DockRule dock;
        public VehicleRule vehicle;
        public ObjectiveStep[] objectives;
        public string[] hints;
        public float hintSeconds;
        public FeedbackRule feedback;
    }

    [Serializable] public sealed class ClockRule { public float nightAtSeconds; public float departureAtSeconds; public float nextBoatDelaySeconds; public string status; }
    [Serializable] public sealed class EvidenceRule { public float investigateSeconds; public bool twoHanded; public float seenCooldownSeconds; public string oneHandEventId; public string twoHandEventId; public string status; }
    [Serializable] public sealed class RestrictedRule { public float cooldownSeconds; public string eventId; public string status; }
    [Serializable] public sealed class DockRule { public float boardRadius; public bool requireEvidenceForSuccess; public string status; }
    [Serializable] public sealed class VehicleRule { public bool allowEvidenceAboard; }
    [Serializable] public sealed class ObjectiveStep { public string id; public string text; public string markerFacilityId; public string condition; }
    [Serializable] public sealed class FeedbackRule { public HeartbeatSetting heartbeat; public PulseSetting pulse; public TintSetting tint; public HitstopSetting hitstop; public ShakeSetting shake; public BigTextSetting bigText; }
    [Serializable] public sealed class HeartbeatSetting { public float[] intervalsByStage; public float[] zoneMultiplier; public float volume; }
    [Serializable] public sealed class PulseSetting { public float seconds; public float alpha; }
    [Serializable] public sealed class TintSetting { public float[] alphaByZone; public float nightAlpha; }
    [Serializable] public sealed class HitstopSetting { public float seconds; }
    [Serializable] public sealed class ShakeSetting { public float amplitude; public float seconds; }
    [Serializable] public sealed class BigTextSetting { public float seconds; }

    public static class ObjectiveConditions
    {
        public const string EvidencePicked = "evidence_picked";
        public const string AtDockWithEvidence = "at_dock_with_evidence";
        public static bool IsKnown(string c) => c == EvidencePicked || c == AtDockWithEvidence;
    }
}

using System;
using Project1028.PlayFoundation;

namespace Project1028.Slice
{
    public sealed class RunStats
    {
        public int ReportsCompleted;
        public int Subdues;
        public int Encounters;
        public System.Collections.Generic.List<string> Choices = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> Flags = new System.Collections.Generic.List<string>();
        public float ElapsedSeconds;
        public int IslandValue;
        public string IslandZone = "";
    }

    public static class SliceEvents
    {
        public static event Action<ObjectiveStep> OnObjectiveAdvanced;
        public static event Action<RunPhase> OnRunPhaseChanged;
        public static event Action<string> OnDepartureDenied;
        public static event Action<RunOutcome, RunStats> OnRunEnded;
        public static event Action<PlayerEntity> OnEvidencePicked;
        public static event Action<PlayerEntity, string> OnRestrictedEntered;

        internal static void RaiseObjective(ObjectiveStep s) => OnObjectiveAdvanced?.Invoke(s);
        internal static void RaisePhase(RunPhase p) => OnRunPhaseChanged?.Invoke(p);
        internal static void RaiseDenied(string r) => OnDepartureDenied?.Invoke(r);
        internal static void RaiseEnded(RunOutcome o, RunStats s) => OnRunEnded?.Invoke(o, s);
        internal static void RaiseEvidence(PlayerEntity p) => OnEvidencePicked?.Invoke(p);
        internal static void RaiseRestricted(PlayerEntity p, string z) => OnRestrictedEntered?.Invoke(p, z);
    }
}

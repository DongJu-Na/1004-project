using System;
using System.Collections.Generic;
using Project1028.PlayFoundation;

namespace Project1028.Encounter
{
    public static class EncounterEvents
    {
        public static event Action<PlayerEntity, string, bool, string> OnTriggerEvaluated;
        public static event Action<EncounterDef, PlayerEntity> OnEncounterStarted;
        public static event Action<EncounterDef, PlayerEntity> OnChoiceRequested;
        public static event Action<EncounterDef, PlayerEntity, string, bool> OnChoiceMade;
        public static event Action<EncounterDef, PlayerEntity, IReadOnlyList<string>> OnEncounterEnded;
        public static event Action<EncounterDef, PlayerEntity> OnEncounterInterrupted;
        public static event Action<string> OnFlagRecorded;
        public static event Action<string[], string[]> OnRunSnapshot;

        internal static void RaiseTrigger(PlayerEntity p, string t, bool s, string r) => OnTriggerEvaluated?.Invoke(p, t, s, r);
        internal static void RaiseStarted(EncounterDef d, PlayerEntity p) => OnEncounterStarted?.Invoke(d, p);
        internal static void RaiseChoiceRequested(EncounterDef d, PlayerEntity p) => OnChoiceRequested?.Invoke(d, p);
        internal static void RaiseChoiceMade(EncounterDef d, PlayerEntity p, string o, bool t) => OnChoiceMade?.Invoke(d, p, o, t);
        internal static void RaiseEnded(EncounterDef d, PlayerEntity p, IReadOnlyList<string> f) => OnEncounterEnded?.Invoke(d, p, f);
        internal static void RaiseInterrupted(EncounterDef d, PlayerEntity p) => OnEncounterInterrupted?.Invoke(d, p);
        internal static void RaiseFlag(string f) => OnFlagRecorded?.Invoke(f);
        internal static void RaiseSnapshot(string[] f, string[] h) => OnRunSnapshot?.Invoke(f, h);
    }
}

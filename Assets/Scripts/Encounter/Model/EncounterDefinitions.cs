namespace Project1028.Encounter
{
    /// <summary>§6.2 네 유형.</summary>
    public enum EncounterType { A, B, C, D }

    public enum InstancePhase { Spawning, Dialogue, Choice, Outcome, Ended, Interrupted }

    public static class EncounterNames
    {
        public static string Korean(EncounterType t)
        {
            switch (t)
            {
                case EncounterType.A: return "기회형";
                case EncounterType.B: return "위협형";
                case EncounterType.C: return "도덕적 선택형";
                default: return "세계 채색형";
            }
        }
    }
}

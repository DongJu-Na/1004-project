using System;

namespace Project1028.Encounter
{
    /// <summary>encounters.json 모델. 스키마: contracts/encounters-schema.json. sample:true는 콘텐츠가 아니다.</summary>
    [Serializable]
    public sealed class EncounterRules
    {
        public string version;
        public SpawnRule spawn;
        public PoolDef[] pools;
        public EncounterDef[] encounters;
        public CallbackEntry[] callbacks;

        public EncounterDef Find(string id)
        {
            if (encounters == null || string.IsNullOrEmpty(id)) return null;
            foreach (var e in encounters) if (e != null && e.id == id) return e;
            return null;
        }

        public static EncounterType TypeOf(EncounterDef d)
        {
            Enum.TryParse(d?.type ?? "D", out EncounterType t);
            return t;
        }
    }

    [Serializable] public sealed class SpawnRule { public int perRunMin; public int perRunMax; public float minIntervalSeconds; public float footChance; public float vehicleChance; public float despawnDelaySeconds; public float choiceTimeoutSeconds; public TypeWeights typeWeights; public string status; }
    [Serializable] public sealed class TypeWeights { public float A; public float B; public float C; public float D; public float Of(EncounterType t) => t == EncounterType.A ? A : t == EncounterType.B ? B : t == EncounterType.C ? C : D; }
    [Serializable] public sealed class PoolDef { public string poolId; public string islandId; public string[] encounterIds; }
    [Serializable] public sealed class Vec3 { public float x; public float y; public float z; }
    [Serializable] public sealed class TriggerDef { public string kind = "zone"; public float radius = 3f; }
    [Serializable] public sealed class NpcDef { public string displayName; public Vec3 offset; public Vec3 facing; }
    [Serializable] public sealed class ChoiceOption { public string label; public string[] flags; public string eventId; }
    [Serializable] public sealed class OutcomeDef { public string[] flags; public string condition; public string eventId; public string prompt; public ChoiceOption optionA; public ChoiceOption optionB; public string defaultOption; }
    [Serializable] public sealed class ConditionDef { public string timeOfDay = "any"; public string minZone = "Calm"; public string requiresFlag; public string forbidsFlag; }
    [Serializable]
    public sealed class EncounterDef
    {
        public string id; public string type; public bool sample; public string variantOf; public int choiceAfterLine = -1;
        public TriggerDef trigger; public NpcDef[] npcs; public string[] lines; public OutcomeDef outcome; public ConditionDef conditions;
    }
    [Serializable] public sealed class CallbackEntry { public string flag; public string targetKind; public string targetId; public string description; public bool sample; }
}

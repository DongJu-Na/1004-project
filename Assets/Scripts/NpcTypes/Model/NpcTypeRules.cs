using System;

namespace Project1028.NpcTypes
{
    /// <summary>npc_types.json 모델. 스키마: contracts/npc-types-schema.json</summary>
    [Serializable]
    public sealed class NpcTypeRules
    {
        public string version;
        public TypeRuleSet rules;
        public NpcAssignment[] assignments;

        public NpcAssignment Find(string npcId)
        {
            if (assignments == null || string.IsNullOrEmpty(npcId)) return null;
            for (int i = 0; i < assignments.Length; i++)
            {
                if (assignments[i] != null && assignments[i].npcId == npcId) return assignments[i];
            }
            return null;
        }
    }

    [Serializable] public sealed class TypeRuleSet { public WatcherRule watcher; public InformerRule informer; public SympathizerRule sympathizer; public SentinelRule sentinel; }
    [Serializable] public sealed class WatcherRule { public string investigateEventId; public string status; }
    [Serializable] public sealed class InformerRule { public float deliveryDelaySeconds; public string deliveryEventId; public float arriveDistance; public string status; }
    [Serializable] public sealed class SympathizerRule { public string turnZone; public string status; }
    [Serializable] public sealed class SentinelRule { public float detectRadius; public float soundRadius; public float cooldownSeconds; public string noiseEventId; public string status; }
    [Serializable] public sealed class NpcAssignment { public string npcId; public string type; public string faction; public string[] roles; }
}

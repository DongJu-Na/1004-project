using System;
using Project1028.Suspicion;

namespace Project1028.NpcTypes
{
    /// <summary>§3 표의 다섯 유형.</summary>
    public enum NpcType { Watcher, Informer, Sympathizer, Silent, Sentinel }

    /// <summary>§3.2 (v0.4 §3.3 발췌) 진영. 제압 가능 판정의 유일한 근거.</summary>
    public enum Faction { Antagonist, Victim, Neutral }

    public static class Roles
    {
        public const string Manager = "manager";
        public const string Broker = "broker";
    }

    public static class NpcTypeDefinitions
    {
        /// <summary>FR-002: 감시자 Antagonist, 밀고자·동조자·침묵자 Victim, 경계자 Neutral.</summary>
        public static Faction DefaultFactionOf(NpcType type)
        {
            switch (type)
            {
                case NpcType.Watcher: return Faction.Antagonist;
                case NpcType.Sentinel: return Faction.Neutral;
                default: return Faction.Victim;
            }
        }

        public static bool TryParseType(string s, out NpcType type) => Enum.TryParse(s, false, out type);
        public static bool TryParseFaction(string s, out Faction faction) => Enum.TryParse(s, false, out faction);
        public static bool TryParseZone(string s, out AlertZone zone) => Enum.TryParse(s, false, out zone);

        public static string KoreanName(NpcType t)
        {
            switch (t)
            {
                case NpcType.Watcher: return "감시자";
                case NpcType.Informer: return "밀고자";
                case NpcType.Sympathizer: return "동조자";
                case NpcType.Silent: return "침묵자";
                default: return "경계자";
            }
        }
    }
}

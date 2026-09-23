using System;
using System.Collections.Generic;
using UnityEngine;
using Project1028.PlayFoundation;

namespace Project1028.NpcTypes
{
    /// <summary>NPC의 유형·진영·역할. 003 시스템이 데이터 배정으로 부여한다.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NpcIdentity))]
    public sealed class NpcTypeProfile : MonoBehaviour
    {
        [SerializeField] private NpcType type = NpcType.Sympathizer;
        [SerializeField] private Faction faction = Faction.Victim;
        [SerializeField] private string[] roles = Array.Empty<string>();

        public NpcType Type => type;
        public Faction Faction => faction;
        public IReadOnlyList<string> Roles => roles;
        public bool IsTurned { get; internal set; }
        public bool WatcherRuleApplies => type == NpcType.Watcher || (type == NpcType.Sympathizer && IsTurned);
        public bool IsManager => Array.IndexOf(roles, Project1028.NpcTypes.Roles.Manager) >= 0;
        public bool IsBroker => Array.IndexOf(roles, Project1028.NpcTypes.Roles.Broker) >= 0;
        public NpcIdentity Identity => GetComponent<NpcIdentity>();

        public void Configure(NpcType newType, Faction newFaction, string[] newRoles)
        {
            type = newType;
            faction = newFaction;
            roles = newRoles ?? Array.Empty<string>();
        }

        public static NpcTypeProfile Get(NpcIdentity npc) => npc != null ? npc.GetComponent<NpcTypeProfile>() : null;
    }
}

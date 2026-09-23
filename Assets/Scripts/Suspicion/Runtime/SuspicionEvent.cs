using UnityEngine;
using Project1028.PlayFoundation;

namespace Project1028.Suspicion
{
    /// <summary>의심 상승의 유일한 입력. EventId는 suspicion_rules.json events[].id.</summary>
    public struct SuspicionEvent
    {
        public string EventId;
        public PlayerEntity Actor;
        public Vector3 Position;
        public NpcIdentity TargetNpc;

        public static SuspicionEvent Witness(string id, PlayerEntity actor) =>
            new SuspicionEvent { EventId = id, Actor = actor, Position = actor != null ? actor.Position : Vector3.zero };

        public static SuspicionEvent Target(string id, PlayerEntity actor, NpcIdentity npc) =>
            new SuspicionEvent { EventId = id, Actor = actor, TargetNpc = npc, Position = actor != null ? actor.Position : Vector3.zero };

        public static SuspicionEvent At(string id, PlayerEntity actor, Vector3 position) =>
            new SuspicionEvent { EventId = id, Actor = actor, Position = position };
    }
}

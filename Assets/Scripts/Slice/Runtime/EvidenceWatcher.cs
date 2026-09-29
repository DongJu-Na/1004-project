using UnityEngine;
using Project1028.PlayFoundation;
using Project1028.Suspicion;
using Project1028.Subdue;

namespace Project1028.Slice
{
    /// <summary>증거를 든 채 보이면 쿨다운마다 002 사건(§2.3 확정대기). 부두 반경 + 증거 소지 → 목표 완료 조건.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerEntity))]
    public sealed class EvidenceWatcher : MonoBehaviour
    {
        private PlayerEntity owner;
        private PlayerHands hands;
        private float nextSeenAt;

        private void Awake() { owner = GetComponent<PlayerEntity>(); hands = GetComponent<PlayerHands>(); }

        private void Update()
        {
            var d = RunDirector.Instance;
            if (d == null || !d.IsReady || hands == null) return;
            bool holdingEvidence = hands.HeldKind == HeldKind.Object && hands.HeldObject != null && hands.HeldObject.IsEvidence;
            if (!holdingEvidence) return;

            if (Time.time >= nextSeenAt)
            {
                bool seen = false;
                foreach (var npc in NpcIdentity.All) { var v = npc.GetComponent<NpcVision>(); if (v != null && v.enabled && v.IsSeeing(owner)) { seen = true; break; } }
                if (seen)
                {
                    nextSeenAt = Time.time + d.Rules.evidence.seenCooldownSeconds;
                    string id = hands.HeldObject.IsTwoHanded ? d.Rules.evidence.twoHandEventId : d.Rules.evidence.oneHandEventId;
                    SuspicionSystem.Instance?.Raise(SuspicionEvent.Witness(id, owner));
                }
            }

            var boat = FindFirstObjectByType<DockBoat>();
            if (boat != null && Vector3.Distance(owner.Position, boat.transform.position) <= boat.InteractionRange)
                d.Satisfy(ObjectiveConditions.AtDockWithEvidence);
        }
    }
}

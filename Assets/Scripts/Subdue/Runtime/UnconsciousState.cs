using System.Collections.Generic;
using UnityEngine;
using Project1028.PlayFoundation;
using Project1028.Suspicion;
using Project1028.NpcTypes;

namespace Project1028.Subdue
{
    /// <summary>
    /// §4.3 기절 상태. 부착 시 시야·이동·행동을 끄고 눕힌다. 깨어나면 복구한다. 옮길 수 있다. 영구 제거되지 않는다 (원칙 V).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UnconsciousState : MonoBehaviour
    {
        public NpcIdentity Npc { get; private set; }
        public PlayerEntity SubduedBy { get; private set; }
        public PlayerEntity CarriedBy { get; private set; }
        public UnconsciousTimer Timer { get; private set; }
        public float Remaining => Timer != null ? Timer.Remaining : 0f;
        public bool IsCarried => CarriedBy != null;
        public readonly HashSet<string> WitnessIds = new HashSet<string>();

        private readonly List<Behaviour> disabled = new List<Behaviour>();
        private bool savedPropagates;
        private NpcSuspicionProfile profile;
        private Quaternion standingRotation;
        private UnconsciousCarryInteractable carry;

        public static bool IsUnconscious(NpcIdentity npc) => npc != null && npc.GetComponent<UnconsciousState>() != null;

        public static UnconsciousState Attach(NpcIdentity npc, PlayerEntity subduer, UnconsciousTimer timer)
        {
            var state = npc.gameObject.AddComponent<UnconsciousState>();
            state.Npc = npc;
            state.SubduedBy = subduer;
            state.Timer = timer;
            state.standingRotation = npc.transform.rotation;

            state.DisableIfPresent<NpcVision>();
            state.DisableIfPresent<NpcMover>();
            state.DisableIfPresent<NpcSuspicionBehaviour>();
            state.DisableIfPresent<InformerBehaviour>();
            state.DisableIfPresent<SentinelBehaviour>();
            state.DisableIfPresent<NpcTalkInteractable>();

            state.profile = npc.GetComponent<NpcSuspicionProfile>();
            if (state.profile != null)
            {
                state.savedPropagates = state.profile.PropagatesSuspicion;
                state.profile.PropagatesSuspicion = false; // 기절 중 전파 없음 (FR-010)
            }

            // 눕힘 (프리미티브 표현)
            npc.transform.rotation = Quaternion.Euler(90f, npc.transform.eulerAngles.y, 0f);
            npc.transform.position = new Vector3(npc.transform.position.x, 0.5f, npc.transform.position.z);

            state.carry = npc.gameObject.AddComponent<UnconsciousCarryInteractable>();
            return state;
        }

        private void DisableIfPresent<T>() where T : Behaviour
        {
            var b = GetComponent<T>();
            if (b != null && b.enabled) { b.enabled = false; disabled.Add(b); }
        }

        internal void SetCarried(PlayerEntity carrier) => CarriedBy = carrier;

        /// <summary>깨어남: 운반 중이면 내려지고, 컴포넌트 복구 후 이 상태를 제거한다.</summary>
        public void Wake()
        {
            if (CarriedBy != null)
            {
                var hands = CarriedBy.GetComponent<PlayerHands>();
                hands?.ForceReleaseBody(this);
            }
            foreach (var b in disabled) if (b != null) b.enabled = true;
            disabled.Clear();
            if (profile != null) profile.PropagatesSuspicion = savedPropagates;

            transform.SetParent(null, true);
            transform.rotation = Quaternion.Euler(0f, standingRotation.eulerAngles.y, 0f);
            transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
            foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = true;

            if (carry != null) Destroy(carry);
            SubdueEvents.RaiseWake(Npc, SubduedBy);
            Destroy(this);
        }
    }
}

using UnityEngine;
using Project1028.PlayFoundation;

namespace Project1028.Subdue
{
    /// <summary>기절자 "들어 옮기기" (§4.3). 빈손 플레이어만, 두 손 점유.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UnconsciousState))]
    public sealed class UnconsciousCarryInteractable : MonoBehaviour, IInteractable
    {
        private UnconsciousState state;
        private void Awake() => state = GetComponent<UnconsciousState>();

        public string PromptText => "들어 옮기기 (두 손)";
        public float InteractionRange => 2f;
        public Vector3 WorldPosition => transform.position;

        public bool CanInteract(PlayerEntity player)
        {
            if (player == null || player.IsLocked || state == null || state.IsCarried) return false;
            var hands = player.GetComponent<PlayerHands>();
            return hands != null && HandRules.CanPickUpBody(hands.State);
        }

        public void Interact(PlayerEntity player)
        {
            var hands = player != null ? player.GetComponent<PlayerHands>() : null;
            if (hands == null) return;
            hands.TryPickUpBody(state);
        }
    }
}

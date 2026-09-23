using UnityEngine;
using Project1028.PlayFoundation;

namespace Project1028.Subdue
{
    /// <summary>
    /// 들 수 있는 일상 물건(상자·병·도구). 무기 분류·내구도·살상 판정은 없다 (헌장 원칙 V, §10).
    /// 한 손/두 손 여부만 가진다 (§4.2 손 규칙 검증용).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CarriableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool isTwoHanded;
        [SerializeField] private string displayName = "상자";
        [SerializeField, Min(0.5f)] private float interactionRange = 2f;

        public bool IsTwoHanded => isTwoHanded;
        public string DisplayName => displayName;
        public string PromptText => $"{displayName} 들기 ({(isTwoHanded ? "두 손" : "한 손")})";
        public float InteractionRange => interactionRange;
        public Vector3 WorldPosition => transform.position;
        public bool IsHeld { get; internal set; }

        public void Configure(string name, bool twoHanded)
        {
            displayName = name;
            isTwoHanded = twoHanded;
        }

        public bool CanInteract(PlayerEntity player)
        {
            if (player == null || player.IsLocked || IsHeld) return false;
            var hands = player.GetComponent<PlayerHands>();
            return hands != null && HandRules.CanPickUpObject(hands.State);
        }

        public void Interact(PlayerEntity player)
        {
            var hands = player != null ? player.GetComponent<PlayerHands>() : null;
            if (hands == null) { RuntimeHud.Instance?.Warn($"{player?.Id}에 PlayerHands가 없습니다."); return; }
            hands.TryPickUp(this);
        }
    }
}

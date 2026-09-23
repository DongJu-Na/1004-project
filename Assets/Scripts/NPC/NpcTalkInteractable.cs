using UnityEngine;

namespace Project1028.PlayFoundation
{
    /// <summary>NPC에게 말 걸기. 유형·의심 등 게임 규칙 필드는 두지 않는다 (헌장 원칙 I).</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NpcIdentity))]
    public sealed class NpcTalkInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string promptText = "말하기";
        [SerializeField, Min(0.5f)] private float interactionRange = 2.5f;

        private NpcIdentity identity;

        public string PromptText => promptText;
        public float InteractionRange => interactionRange;
        public Vector3 WorldPosition => transform.position;

        private void Awake() => identity = GetComponent<NpcIdentity>();

        public bool CanInteract(PlayerEntity player) => player != null && !player.IsLocked && !player.HandsBusy && isActiveAndEnabled;

        public void Interact(PlayerEntity player)
        {
            if (player == null) return;
            var runner = player.GetComponent<DialogueRunner>();
            if (runner == null)
            {
                RuntimeHud.Instance?.Warn($"{player.Id}에 DialogueRunner가 없습니다.");
                return;
            }
            runner.TryBegin(identity);
        }
    }
}

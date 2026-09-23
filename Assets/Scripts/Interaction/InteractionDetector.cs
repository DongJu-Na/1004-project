using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project1028.PlayFoundation
{
    /// <summary>
    /// 플레이어 개체별 상호작용 대상 탐색·안내·실행 (FR-008, FR-009). PlayerInput이 없으면 입력만 건너뛴다(US4).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerEntity))]
    public sealed class InteractionDetector : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float searchRadius = 4f;
        [SerializeField] private LayerMask searchMask = ~0;
        [SerializeField] private string promptSuffix = " [E]";

        private PlayerEntity owner;
        private PlayerInput playerInput;
        private DialogueRunner dialogueRunner;
        private InputAction interactAction;
        private readonly Collider[] hits = new Collider[32];
        private readonly List<InteractableSelector.Candidate> candidates = new List<InteractableSelector.Candidate>();

        public IInteractable Current { get; private set; }
        public event Action<PlayerEntity, IInteractable> OnTargetChanged;

        private void Awake()
        {
            owner = GetComponent<PlayerEntity>();
            playerInput = GetComponent<PlayerInput>();
            dialogueRunner = GetComponent<DialogueRunner>();
        }

        private void OnEnable()
        {
            interactAction = playerInput != null && playerInput.actions != null ? playerInput.actions.FindAction("Interact", false) : null;
        }

        private void Update()
        {
            IInteractable next = (owner.IsLocked || owner.IsInVehicle) ? null : FindBest();
            if (!ReferenceEquals(next, Current))
            {
                Current = next;
                RuntimeHud.Instance?.ShowPrompt(owner, Current != null ? Current.PromptText + promptSuffix : null);
                OnTargetChanged?.Invoke(owner, Current);
            }

            if (Current == null || interactAction == null) return;
            if (dialogueRunner != null && Time.frameCount == dialogueRunner.LastEndedFrame) return; // 종료 프레임 재시작 방지
            if (interactAction.WasPressedThisFrame())
            {
                Current.Interact(owner);
            }
        }

        private IInteractable FindBest()
        {
            candidates.Clear();
            int count = Physics.OverlapSphereNonAlloc(owner.Position, searchRadius, hits, searchMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                // 같은 오브젝트에 여러 IInteractable이 있을 수 있다(말하기 + 들어 옮기기). 비활성 컴포넌트는 건너뛴다.
                var found = hits[i].GetComponentsInParent<IInteractable>(false);
                for (int k = 0; k < found.Length; k++)
                {
                    var interactable = found[k];
                    if (interactable is Behaviour b && !b.isActiveAndEnabled) continue;
                    if (!interactable.CanInteract(owner)) continue;
                    bool duplicate = false;
                    for (int j = 0; j < candidates.Count; j++)
                    {
                        if (ReferenceEquals(candidates[j].Payload, interactable)) { duplicate = true; break; }
                    }
                    if (duplicate) continue;
                    candidates.Add(new InteractableSelector.Candidate(interactable.WorldPosition, interactable.InteractionRange, interactable));
                }
            }

            Vector3 forward = owner.Camera != null ? owner.Camera.transform.forward : owner.Forward;
            var picked = InteractableSelector.Pick(candidates, owner.Position, forward);
            return picked?.Payload as IInteractable;
        }
    }
}

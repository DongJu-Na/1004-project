using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Project1028.PlayFoundation;
using Project1028.NpcTypes;

namespace Project1028.Subdue
{
    /// <summary>플레이어 개체별 제압 입력. 카메라 전방 최근접 NPC를 대상으로 문맥 동작을 안내하고 F로 실행한다.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerEntity))]
    [RequireComponent(typeof(PlayerHands))]
    public sealed class SubdueActor : MonoBehaviour
    {
        private PlayerEntity owner;
        private PlayerHands hands;
        private PlayerInput input;
        private readonly List<InteractableSelector.Candidate> cands = new List<InteractableSelector.Candidate>();
        private string lastPrompt;

        public NpcIdentity CurrentTarget { get; private set; }
        public SubdueAction PlannedAction { get; private set; }

        private void Awake()
        {
            owner = GetComponent<PlayerEntity>();
            hands = GetComponent<PlayerHands>();
            input = GetComponent<PlayerInput>();
        }

        private void Update()
        {
            var system = SubdueSystem.Instance;
            if (system == null || !system.IsReady) return;

            CurrentTarget = owner.IsLocked ? null : PickTarget(system);
            string prompt = null;
            if (CurrentTarget != null)
            {
                var verdict = SubdueJudgement.Evaluate(CurrentTarget);
                if (!verdict.CanSubdue) prompt = $"{CurrentTarget.DisplayName}: 제압 불가 ({verdict.Reason})";
                else if (UnconsciousState.IsUnconscious(CurrentTarget)) prompt = null;
                else if (hands.HeldKind == HeldKind.UnconsciousNpc) prompt = "두 손 점유 — 기절자를 내려놓아야 함 [G]";
                else
                {
                    PlannedAction = system.PlanAction(owner, CurrentTarget);
                    var (ok, reason) = HandRules.CanPerform(PlannedAction, hands.State, hands.HeldKind);
                    prompt = ok ? $"{SubdueNames.Korean(PlannedAction)} [F]" : $"{SubdueNames.Korean(PlannedAction)} 불가: {reason}";
                }
            }
            if (prompt != lastPrompt)
            {
                RuntimeHud.Instance?.ShowSecondaryPrompt(owner, prompt);
                lastPrompt = prompt;
            }

            if (input == null || CurrentTarget == null || owner.IsLocked) return;
            var kb = Keyboard.current;
            if (kb != null && kb.fKey.wasPressedThisFrame)
            {
                system.TrySubdue(owner, CurrentTarget, PlannedAction);
            }
        }

        private NpcIdentity PickTarget(SubdueSystem system)
        {
            float range = Mathf.Max(system.Rules.actions.backstab.range, system.Rules.actions.shove.range, system.Rules.actions.objectStrike.range) + 0.5f;
            cands.Clear();
            foreach (var npc in NpcIdentity.All) cands.Add(new InteractableSelector.Candidate(npc.Position, range, npc));
            Vector3 fwd = owner.Camera != null ? owner.Camera.transform.forward : owner.Forward;
            var picked = InteractableSelector.Pick(cands, owner.Position, fwd);
            return picked?.Payload as NpcIdentity;
        }
    }
}

using UnityEngine;
using Project1028.PlayFoundation;
using Project1028.NpcTypes;
using Project1028.Report;
using Project1028.Subdue;

namespace Project1028.Slice
{
    /// <summary>증거 "조사 (E 길게)". 완료하면 004 CarriableObject의 잠금이 풀려 들 수 있다. 조사 시작 시 003 조사 릴레이 1회.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CarriableObject))]
    public sealed class EvidenceItem : MonoBehaviour, IInteractable
    {
        private CarriableObject carriable;
        private CutProgress progress;
        private PlayerEntity investigator;
        private bool investigated;

        private void Awake() { carriable = GetComponent<CarriableObject>(); SubdueEvents.OnPickedUp += OnPickedUp; }
        private void OnDestroy() => SubdueEvents.OnPickedUp -= OnPickedUp; // 조사 완료 후에도 픽업 이벤트를 받아야 하므로 OnDisable이 아닌 OnDestroy

        private RunDirector D => RunDirector.Instance;
        public string PromptText => D != null && D.IsReady ? $"{carriable.DisplayName} 조사 (E 후 {D.Rules.evidence.investigateSeconds:0.#}초 멈춰 있기)" : "조사";
        public float InteractionRange => 2f;
        public Vector3 WorldPosition => transform.position;
        public bool CanInteract(PlayerEntity player) => !investigated && carriable.Locked && player != null && !player.IsLocked && !player.HandsBusy && !player.IsInVehicle && progress == null;

        public void Interact(PlayerEntity player)
        {
            var d = D;
            if (d == null || !d.IsReady) return;
            investigator = player;
            progress = new CutProgress(d.Rules.evidence.investigateSeconds);
            NpcTypeSystem.Instance?.Investigate(player); // §3 감시자: 시야 안 조사 → 즉시 +2
            RuntimeHud.Instance?.Warn($"{player.Id}: {carriable.DisplayName} 조사 시작 — 움직이면 취소");
        }

        private void Update()
        {
            if (progress == null || investigator == null) return;
            bool valid = Vector3.Distance(investigator.Position, transform.position) <= InteractionRange + 0.5f && investigator.MovementState == MovementState.Idle && !investigator.IsLocked;
            var r = progress.Tick(Time.deltaTime, valid);
            switch (r)
            {
                case CutTickResult.Progressed:
                    RuntimeHud.Instance?.ShowSecondaryPrompt(investigator, $"조사 중 {progress.Progress01 * 100f:0}%");
                    break;
                case CutTickResult.Cancelled:
                case CutTickResult.Idle:
                    if (!valid) { RuntimeHud.Instance?.ShowSecondaryPrompt(investigator, null); RuntimeHud.Instance?.Warn("조사 취소 (이동)"); progress = null; investigator = null; }
                    break;
                case CutTickResult.Completed:
                    RuntimeHud.Instance?.ShowSecondaryPrompt(investigator, null);
                    carriable.Locked = false;
                    RuntimeHud.Instance?.Warn($"★ {carriable.DisplayName}을(를) 확인했다 — E로 든다 (두 손)");
                    progress = null; investigator = null;
                    investigated = true; // 다시 조사할 필요 없음 (FR-014). 컴포넌트는 켜둔다(픽업 이벤트 수신)
                    break;
            }
        }

        private void OnPickedUp(PlayerEntity p, HeldKind kind)
        {
            if (kind != HeldKind.Object) return;
            var hands = p.GetComponent<PlayerHands>();
            if (hands == null || hands.HeldObject != carriable) return;
            SliceEvents.RaiseEvidence(p);
            RunDirector.Instance?.Satisfy(ObjectiveConditions.EvidencePicked);
        }
    }
}

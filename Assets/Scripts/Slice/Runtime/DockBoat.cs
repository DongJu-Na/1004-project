using UnityEngine;
using Project1028.PlayFoundation;
using Project1028.Suspicion;
using Project1028.Subdue;

namespace Project1028.Slice
{
    /// <summary>부두의 배 "배 타기". §2.5 봉쇄면 뜨지 않는다. 증거 없이 떠나면 확인 후 부분 실패.</summary>
    [DisallowMultipleComponent]
    public sealed class DockBoat : MonoBehaviour, IInteractable
    {
        private float confirmUntil;

        private RunDirector D => RunDirector.Instance;
        public string PromptText => D != null && D.IsReady && D.Clock.IsDepartureOpen ? "배 타기" : "배 (아직 뜨지 않는다)";
        public float InteractionRange => D != null && D.IsReady ? D.Rules.dock.boardRadius : 6f;
        public Vector3 WorldPosition => transform.position;
        public bool CanInteract(PlayerEntity player) => player != null && !player.IsLocked && !player.IsInVehicle && D != null && D.IsReady && !D.IsEnded;

        public static bool AnyoneHasEvidence()
        {
            foreach (var p in PlayerEntity.All)
            {
                var h = p.GetComponent<PlayerHands>();
                if (h != null && h.HeldKind == HeldKind.Object && h.HeldObject != null && h.HeldObject.IsEvidence) return true;
            }
            return false;
        }

        public bool AllAtDock()
        {
            float r = InteractionRange;
            foreach (var p in PlayerEntity.All) if (Vector3.Distance(p.Position, transform.position) > r) return false;
            return true;
        }

        public void Interact(PlayerEntity player)
        {
            var d = D;
            bool blocked = SuspicionSystem.Instance != null && SuspicionSystem.Instance.Island.DepartureBlocked;
            var v = DepartureRule.Evaluate(d.Clock.Elapsed, d.Clock.DepartureAt, d.Clock.NextBoatAt, blocked, AnyoneHasEvidence(), AllAtDock(), d.Rules.dock.requireEvidenceForSuccess);
            if (!v.CanBoard)
            {
                string extra = v.Reason.Contains("아직") ? $" ({RunDirector.Mmss(d.Clock.RemainingToDeparture)})" : v.Reason.Contains("봉쇄") ? $" — 다음 배 {RunDirector.Mmss(d.Clock.RemainingToNextBoat)}" : "";
                SliceEvents.RaiseDenied(v.Reason + extra);
                RuntimeHud.Instance?.Warn(v.Reason + extra);
                return;
            }
            if (v.Outcome == RunOutcome.LeftWithoutEvidence && Time.time > confirmUntil)
            {
                confirmUntil = Time.time + 5f;
                RuntimeHud.Instance?.ShowSecondaryPrompt(player, "빈손으로 떠날까? 다시 E");
                RuntimeHud.Instance?.Warn("증거가 없다. 그래도 떠나려면 5초 안에 다시 E");
                return;
            }
            RuntimeHud.Instance?.ShowSecondaryPrompt(player, null);
            d.EndRun(v.Outcome);
        }
    }
}

using UnityEngine;
using Project1028.PlayFoundation;

namespace Project1028.Report
{
    /// <summary>전화기 "전화선 끊기" 상호작용. 관리소에는 부착하지 않는다.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ReportPoint))]
    public sealed class PhoneCutInteractable : MonoBehaviour, IInteractable
    {
        private ReportPoint point;
        private void Awake() => point = GetComponent<ReportPoint>();

        private ReportRules Rules => ReportSystem.Instance != null && ReportSystem.Instance.IsReady ? ReportSystem.Instance.Rules : null;

        public string PromptText => Rules != null ? $"전화선 끊기 ({Rules.cutDurationSeconds:0}초)" : "전화선 끊기";
        public float InteractionRange => Rules != null ? Rules.cutRange : 2f;
        public Vector3 WorldPosition => transform.position;

        public bool CanInteract(PlayerEntity player)
        {
            if (player == null || player.IsLocked || player.HandsBusy) return false;
            if (point == null || !point.CanBeCut || !point.IsUsable) return false;
            var cutter = player.GetComponent<PlayerCutter>();
            if (cutter == null || cutter.IsCutting) return false;
            var rules = Rules;
            if (rules != null && rules.cutRequiresTool)
            {
                var kit = player.GetComponent<PlayerToolkit>();
                if (kit == null || !kit.HasCuttingTool) return true; // 안내는 보이되 Begin에서 이유를 알려준다
            }
            return true;
        }

        public void Interact(PlayerEntity player)
        {
            var cutter = player != null ? player.GetComponent<PlayerCutter>() : null;
            cutter?.Begin(point);
        }
    }
}

using UnityEngine;
using Project1028.PlayFoundation;

namespace Project1028.Report
{
    /// <summary>§5.2 전화선 끊기 진행. 정지 상태를 유지해야 하며 이동·잠금·두 손 점유·거리 이탈 시 취소된다. 다른 지점에 Begin하면 리셋.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerEntity))]
    public sealed class PlayerCutter : MonoBehaviour
    {
        private PlayerEntity owner;
        private PlayerToolkit toolkit;
        private ReportPoint point;
        private CutProgress progress;

        public bool IsCutting => point != null && progress != null && !progress.IsDone;
        public ReportPoint Target => point;
        public float Progress01 => progress != null ? progress.Progress01 : 0f;

        private void Awake()
        {
            owner = GetComponent<PlayerEntity>();
            toolkit = GetComponent<PlayerToolkit>();
        }

        public bool Begin(ReportPoint target)
        {
            var system = ReportSystem.Instance;
            if (system == null || !system.IsReady || target == null) return false;
            if (!target.CanBeCut) { RuntimeHud.Instance?.Warn($"{owner.Id}: {target.DisplayName}은(는) 끊을 수 없다 (관리소)"); return false; }
            if (!target.IsUsable) { RuntimeHud.Instance?.Warn($"{owner.Id}: {target.DisplayName}은(는) 이미 끊겨 있다"); return false; }
            if (system.Rules.cutRequiresTool && (toolkit == null || !toolkit.HasCuttingTool))
            {
                RuntimeHud.Instance?.Warn($"{owner.Id}: 전화선을 끊으려면 도구가 필요하다 (확정 대기 — K로 도구 토글)");
                return false;
            }
            point = target;
            progress = new CutProgress(system.Rules.cutDurationSeconds);
            RuntimeHud.Instance?.Warn($"{owner.Id}: {target.DisplayName} 전화선 끊기 시작 — 움직이면 취소");
            return true;
        }

        private void Update()
        {
            if (!IsCutting) return;
            var system = ReportSystem.Instance;
            bool valid = system != null && system.IsReady && point != null && point.IsUsable
                         && Vector3.Distance(owner.Position, point.Position) <= system.Rules.cutRange
                         && owner.MovementState == MovementState.Idle && !owner.IsLocked && !owner.HandsBusy;

            var r = progress.Tick(Time.deltaTime, valid);
            switch (r)
            {
                case CutTickResult.Progressed:
                    RuntimeHud.Instance?.ShowSecondaryPrompt(owner, $"전화선 끊는 중 {progress.Progress01 * 100f:0}%");
                    break;
                case CutTickResult.Cancelled:
                    RuntimeHud.Instance?.ShowSecondaryPrompt(owner, null);
                    RuntimeHud.Instance?.Warn($"{owner.Id}: 끊기 취소 (이동/이탈)");
                    ReportEvents.RaiseCutCancelled(point, owner);
                    point = null; progress = null;
                    break;
                case CutTickResult.Completed:
                    RuntimeHud.Instance?.ShowSecondaryPrompt(owner, null);
                    point.SetUsable(false);
                    ReportEvents.RaisePhoneCut(point, owner);
                    RuntimeHud.Instance?.Warn($"★ {owner.Id}: {point.DisplayName} 전화선을 끊었다 — 이 런 동안 사용 불가 (§5.2)");
                    point = null; progress = null;
                    break;
                case CutTickResult.Idle:
                    if (!valid) { point = null; progress = null; RuntimeHud.Instance?.ShowSecondaryPrompt(owner, null); }
                    break;
            }
        }
    }
}

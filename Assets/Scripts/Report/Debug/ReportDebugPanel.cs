using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Project1028.PlayFoundation;

namespace Project1028.Report
{
    /// <summary>FR-016: 지점 상태, 흐름 상태·목적지, 완료 이력, 로그.</summary>
    public sealed class ReportDebugPanel : MonoBehaviour
    {
        private Text text;
        private float nextRefresh;
        private readonly Queue<string> log = new Queue<string>();
        private readonly StringBuilder sb = new StringBuilder();

        private void OnEnable()
        {
            ReportEvents.OnReportStarted += Started; ReportEvents.OnReportRetargeted += Retargeted; ReportEvents.OnReportAbandoned += Abandoned;
            ReportEvents.OnReportCompleted += Completed; ReportEvents.OnReportInterrupted += Interrupted; ReportEvents.OnPhoneCut += Cut; ReportEvents.OnCutCancelled += CutCancelled;
        }
        private void OnDisable()
        {
            ReportEvents.OnReportStarted -= Started; ReportEvents.OnReportRetargeted -= Retargeted; ReportEvents.OnReportAbandoned -= Abandoned;
            ReportEvents.OnReportCompleted -= Completed; ReportEvents.OnReportInterrupted -= Interrupted; ReportEvents.OnPhoneCut -= Cut; ReportEvents.OnCutCancelled -= CutCancelled;
        }
        private void Started(NpcIdentity n, PlayerEntity p, ReportPoint r) => Push($"시작 {n.DisplayName}→{r.DisplayName} ({p.Id})");
        private void Retargeted(NpcIdentity n, PlayerEntity p, ReportPoint r) => Push($"재타깃 {n.DisplayName}→{r.DisplayName}");
        private void Abandoned(NpcIdentity n, PlayerEntity p, string w) => Push($"포기 {n.DisplayName}: {w}");
        private void Completed(NpcIdentity n, PlayerEntity p, ReportPoint r) => Push($"★ 완료 {n.DisplayName}@{r?.DisplayName} ({p.Id})");
        private void Interrupted(NpcIdentity n, PlayerEntity p) => Push($"중단 {n.DisplayName}");
        private void Cut(ReportPoint r, PlayerEntity p) => Push($"끊음 {r.DisplayName} ({p.Id})");
        private void CutCancelled(ReportPoint r, PlayerEntity p) => Push($"끊기 취소 {r.DisplayName}");

        private void Push(string s) { log.Enqueue($"[{Time.time:0.0}] {s}"); while (log.Count > 6) log.Dequeue(); }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.2f;
            if (text == null)
            {
                if (RuntimeHud.Instance == null) return;
                text = RuntimeHud.Instance.CreateFixedText("ReportPanel", new Vector2(1f, 0f), new Vector2(-16f, 16f), new Vector2(620f, 380f), 20, TextAnchor.LowerRight);
                text.color = new Color(1f, 0.8f, 0.8f);
            }
            var system = ReportSystem.Instance;
            sb.Clear();
            if (system == null || !system.IsReady) { sb.AppendLine("신고 시스템: 미준비"); text.text = sb.ToString(); return; }
            sb.Append("신고 확정 대기 ").Append(system.PendingRuleCount).Append("건 · 완료 이력 ").Append(system.CompletedCount).AppendLine();
            sb.AppendLine("— 지점 —");
            foreach (var p in ReportPoint.All) sb.Append(p.DisplayName).Append(" [").Append(ReportNames.Korean(p.Kind)).Append("] ").AppendLine(p.IsUsable ? "사용 가능" : "끊김/불가");
            sb.AppendLine("— 흐름 —");
            if (system.ActiveFlows.Count == 0) sb.AppendLine("(없음)");
            foreach (var f in system.ActiveFlows) if (f != null) sb.Append(f.Npc.DisplayName).Append(" → ").Append(f.TargetPoint?.DisplayName).Append(' ').Append(ReportNames.Korean(f.Phase)).Append(" (").Append(f.TargetPlayer?.Id).AppendLine(")");
            sb.AppendLine("— 로그 —");
            foreach (var l in log) sb.AppendLine(l);
            sb.AppendLine("키: E 끊기(정지 유지) · K 도구 토글 · O 최근접 전화기 즉시 끊기 · I 조사 · F 제압 · T 배속");
            text.text = sb.ToString();
        }
    }
}

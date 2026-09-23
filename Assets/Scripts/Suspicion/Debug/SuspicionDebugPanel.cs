using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Project1028.PlayFoundation;

namespace Project1028.Suspicion
{
    /// <summary>FR-027: 모든 (NPC, 플레이어) 값·단계, 섬 의심도·구간·출항 불가, 시간대, 위장, 예약 전파, 최근 이벤트 로그.</summary>
    public sealed class SuspicionDebugPanel : MonoBehaviour
    {
        [SerializeField] private float refreshInterval = 0.2f;

        private Text text;
        private float nextRefresh;
        private readonly Queue<string> log = new Queue<string>();
        private readonly StringBuilder sb = new StringBuilder();
        private const int MaxLog = 6;

        private void OnEnable()
        {
            SuspicionEvents.OnPersonalStageChanged += OnStage;
            SuspicionEvents.OnReportAttempt += OnReport;
            SuspicionEvents.OnIslandZoneChanged += OnZone;
            SuspicionEvents.OnDepartureBlockedChanged += OnBlocked;
            SuspicionEvents.OnPropagated += OnPropagated;
            SuspicionEvents.OnRunEnded += OnRunEnded;
        }

        private void OnDisable()
        {
            SuspicionEvents.OnPersonalStageChanged -= OnStage;
            SuspicionEvents.OnReportAttempt -= OnReport;
            SuspicionEvents.OnIslandZoneChanged -= OnZone;
            SuspicionEvents.OnDepartureBlockedChanged -= OnBlocked;
            SuspicionEvents.OnPropagated -= OnPropagated;
            SuspicionEvents.OnRunEnded -= OnRunEnded;
        }

        private void OnStage(NpcIdentity n, PlayerEntity p, SuspicionStage o, SuspicionStage nw) => Push($"{n.DisplayName}/{p.Id}: {(int)o}→{(int)nw} ({nw})");
        private void OnReport(NpcIdentity n, PlayerEntity p) => Push($"★ 신고 시도 발생: {n.DisplayName} → {p.Id}");
        private void OnZone(AlertZone o, AlertZone n) => Push($"섬 구간 {o} → {n}");
        private void OnBlocked(bool b) => Push(b ? "★ 봉쇄: 배가 뜨지 않는다 (출항 불가)" : "봉쇄 해제");
        private void OnPropagated(NpcIdentity f, NpcIdentity t, PlayerEntity p) => Push($"전파 {f.DisplayName} → {t.DisplayName} ({p.Id})");
        private void OnRunEnded(CarryOverSnapshot s) => Push($"런 종료 이월: 섬 {s.Island}, 개인 {s.Personal.Count}쌍");

        private void Push(string line)
        {
            log.Enqueue($"[{Time.time:0.0}] {line}");
            while (log.Count > MaxLog) log.Dequeue();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + refreshInterval;

            if (text == null)
            {
                if (RuntimeHud.Instance == null) return;
                text = RuntimeHud.Instance.CreateFixedText("SuspicionPanel", new Vector2(0f, 1f), new Vector2(16f, -240f), new Vector2(760f, 600f), 20, TextAnchor.UpperLeft);
                text.color = new Color(0.85f, 0.95f, 1f);
            }

            var system = SuspicionSystem.Instance;
            sb.Clear();
            if (system == null || !system.IsReady)
            {
                sb.AppendLine("의심 시스템: 규칙 로드 실패 또는 없음");
                text.text = sb.ToString();
                return;
            }

            sb.Append("섬 의심도 ").Append(system.Island.Value).Append(" [").Append(system.Island.Zone).Append("]  출항 불가: ")
              .Append(system.Island.DepartureBlocked ? "예" : "아니오")
              .Append("  |  시간대 ").Append(TimeOfDay.CurrentOrDay)
              .Append("  |  확정 대기 ").Append(system.PendingRuleCount).Append("건").AppendLine();

            foreach (var p in PlayerEntity.All)
            {
                var d = p.GetComponent<PlayerDisguise>();
                sb.Append(p.Id).Append(" 이동:").Append(p.MovementState);
                if (d != null) sb.Append("  위장:").Append(d.IsDisguised ? "착용" : "미착용").Append(" 유효:").Append(d.IsEffective ? "예" : "아니오");
                sb.AppendLine();
            }

            sb.AppendLine("— 개인 의심 —");
            foreach (var npc in NpcIdentity.All)
            {
                sb.Append(npc.DisplayName).Append(": ");
                foreach (var p in PlayerEntity.All)
                {
                    var s = system.GetPersonal(npc, p);
                    sb.Append(p.Id).Append('=').Append(s.Value).Append('(').Append(s.Stage).Append(") ");
                }
                var prof = NpcSuspicionProfile.GetOrDefault(npc);
                if (!prof.PropagatesSuspicion) sb.Append("[전파 안 함]");
                if (prof.IsManager) sb.Append("[관리자]");
                if (prof.ReportsToManagerImmediately) sb.Append("[밀고]");
                sb.AppendLine();
            }

            var pending = system.PendingPropagations;
            if (pending.Count > 0)
            {
                sb.AppendLine("— 전파 예약 —");
                foreach (var r in pending) sb.Append(r.From).Append(" → ").Append(r.To).Append(" (").Append(r.PlayerId).Append(") ").Append(Mathf.Max(0f, r.ExecuteAt - Time.time).ToString("0.0")).AppendLine("s");
            }

            sb.AppendLine("— 로그 —");
            foreach (var l in log) sb.AppendLine(l);
            sb.AppendLine("키: F1~F7 사건 | N 낮/밤 | R 런 종료 | T 배속 | [ ] 섬 ±10");
            text.text = sb.ToString();
        }
    }
}

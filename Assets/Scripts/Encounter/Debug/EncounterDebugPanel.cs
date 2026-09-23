using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Project1028.PlayFoundation;

namespace Project1028.Encounter
{
    /// <summary>FR-024: 후보·발생 수·다음 가능 시점·플래그·이력·회수 조회·로그.</summary>
    public sealed class EncounterDebugPanel : MonoBehaviour
    {
        private Text text;
        private float nextRefresh;
        private readonly Queue<string> log = new Queue<string>();
        private readonly Queue<string> callbacks = new Queue<string>();
        private readonly StringBuilder sb = new StringBuilder();

        private void OnEnable()
        {
            EncounterEvents.OnTriggerEvaluated += OnTrigger;
            EncounterEvents.OnEncounterStarted += OnStarted;
            EncounterEvents.OnChoiceMade += OnChoice;
            EncounterEvents.OnEncounterEnded += OnEnded;
            EncounterEvents.OnEncounterInterrupted += OnInterrupted;
            EncounterEvents.OnFlagRecorded += OnFlag;
        }
        private void OnDisable()
        {
            EncounterEvents.OnTriggerEvaluated -= OnTrigger;
            EncounterEvents.OnEncounterStarted -= OnStarted;
            EncounterEvents.OnChoiceMade -= OnChoice;
            EncounterEvents.OnEncounterEnded -= OnEnded;
            EncounterEvents.OnEncounterInterrupted -= OnInterrupted;
            EncounterEvents.OnFlagRecorded -= OnFlag;
        }

        private void OnTrigger(PlayerEntity p, string t, bool s, string r) => Push($"{(s ? "발생" : "스킵")} @{t}: {r}");
        private void OnStarted(EncounterDef d, PlayerEntity p) => Push($"시작 {d.id} [{d.type}]");
        private void OnChoice(EncounterDef d, PlayerEntity p, string o, bool t) => Push($"선택 {d.id}: {o}{(t ? "(기본)" : "")}");
        private void OnEnded(EncounterDef d, PlayerEntity p, IReadOnlyList<string> f) => Push($"종료 {d.id} 플래그 {(f.Count == 0 ? "-" : string.Join(",", f))}");
        private void OnInterrupted(EncounterDef d, PlayerEntity p) => Push($"중단 {d.id}");
        private void OnFlag(string f)
        {
            var system = EncounterSystem.Instance;
            if (system == null) return;
            foreach (var c in system.CallbacksFor(f)) { callbacks.Enqueue($"{f} → {c.targetKind}:{c.targetId} — {c.description}"); while (callbacks.Count > 3) callbacks.Dequeue(); }
        }
        private void Push(string s) { log.Enqueue($"[{Time.time:0.0}] {s}"); while (log.Count > 6) log.Dequeue(); }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.5f;
            if (text == null)
            {
                if (RuntimeHud.Instance == null) return;
                text = RuntimeHud.Instance.CreateFixedText("EncounterPanel", new Vector2(0f, 0f), new Vector2(16f, 16f), new Vector2(760f, 460f), 19, TextAnchor.LowerLeft);
                text.color = new Color(1f, 0.95f, 0.8f);
            }
            var system = EncounterSystem.Instance;
            sb.Clear();
            if (system == null || !system.IsReady) { sb.AppendLine("인카운터 시스템: 미준비"); text.text = sb.ToString(); return; }
            var sp = system.Rules.spawn;
            float next = Mathf.Max(0f, sp.minIntervalSeconds - (Time.time - system.LastEndedAt));
            sb.Append("인카운터 확정 대기 ").Append(system.PendingRuleCount).Append("건 · 샘플 ").Append(system.SampleCount).Append(" · 단가 NPC ").Append(system.TotalNpcs).Append('/').Append("대사 ").Append(system.TotalLines).AppendLine("줄");
            sb.Append("발생 ").Append(system.RunCount).Append('/').Append(sp.perRunMax).Append("  다음 가능 ").Append(next.ToString("0")).Append("s  후보 ").Append(system.Candidates().Count).Append("  활성 ").Append(system.Active != null ? $"{system.Active.Def.id} ({system.Active.Phase})" : "-").AppendLine();
            sb.Append("플래그: ").AppendLine(system.Ledger.Flags.Count == 0 ? "-" : string.Join(", ", system.Ledger.Flags));
            sb.Append("이력: ").AppendLine(system.Ledger.History.Count == 0 ? "-" : string.Join(", ", system.Ledger.History));
            if (callbacks.Count > 0) { sb.AppendLine("— 회수 조회(§6.5, 실행은 후속) —"); foreach (var c in callbacks) sb.AppendLine(c); }
            sb.AppendLine("— 로그 —");
            foreach (var l in log) sb.AppendLine(l);
            if (!string.IsNullOrEmpty(system.LastReport)) sb.AppendLine(system.LastReport);
            sb.AppendLine("키: X 샘플 강제 · 1/2 선택 · S 정찰 토글 · Z 100런 시뮬 · N 낮/밤 · E 탑승");
            text.text = sb.ToString();
        }
    }
}

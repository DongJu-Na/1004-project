using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Project1028.PlayFoundation;

namespace Project1028.Subdue
{
    /// <summary>FR-021: 플레이어 손 상태, 기절자별 제압자·남은 시간·운반자·목격/발견 수, 최근 로그.</summary>
    public sealed class SubdueDebugPanel : MonoBehaviour
    {
        private Text text;
        private float nextRefresh;
        private readonly Queue<string> log = new Queue<string>();
        private readonly StringBuilder sb = new StringBuilder();

        private void OnEnable()
        {
            SubdueEvents.OnSubdued += OnSubdued;
            SubdueEvents.OnSubdueRejected += OnRejected;
            SubdueEvents.OnShoveFailed += OnShoveFailed;
            SubdueEvents.OnUnconsciousWake += OnWake;
            SubdueEvents.OnBodyDiscovered += OnDiscovered;
            SubdueEvents.OnPickedUp += OnPickedUp;
            SubdueEvents.OnDropped += OnDropped;
        }
        private void OnDisable()
        {
            SubdueEvents.OnSubdued -= OnSubdued;
            SubdueEvents.OnSubdueRejected -= OnRejected;
            SubdueEvents.OnShoveFailed -= OnShoveFailed;
            SubdueEvents.OnUnconsciousWake -= OnWake;
            SubdueEvents.OnBodyDiscovered -= OnDiscovered;
            SubdueEvents.OnPickedUp -= OnPickedUp;
            SubdueEvents.OnDropped -= OnDropped;
        }

        private void OnSubdued(PlayerEntity a, NpcIdentity n, SubdueAction s, int w) => Push($"{a.Id} {SubdueNames.Korean(s)} → {n.DisplayName} (목격 {w})");
        private void OnRejected(PlayerEntity a, NpcIdentity n, SubdueAction s, string r) => Push($"{a?.Id} {SubdueNames.Korean(s)} 거부: {r}");
        private void OnShoveFailed(PlayerEntity a, NpcIdentity n) => Push($"{a.Id} 밀치기 실패 → {n.DisplayName} 확신");
        private void OnWake(NpcIdentity n, PlayerEntity p) => Push($"{n.DisplayName} 깨어남 (제압자 {p?.Id})");
        private void OnDiscovered(NpcIdentity o, NpcIdentity b, PlayerEntity p) => Push($"{o.DisplayName}이(가) {b.DisplayName} 발견");
        private void OnPickedUp(PlayerEntity p, HeldKind k) => Push($"{p.Id} 들기: {k}");
        private void OnDropped(PlayerEntity p, HeldKind k) => Push($"{p.Id} 내려놓기: {k}");
        private void Push(string s) { log.Enqueue($"[{Time.time:0.0}] {s}"); while (log.Count > 6) log.Dequeue(); }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.2f;
            if (text == null)
            {
                if (RuntimeHud.Instance == null) return;
                text = RuntimeHud.Instance.CreateFixedText("SubduePanel", new Vector2(1f, 1f), new Vector2(-16f, -16f), new Vector2(640f, 520f), 20, TextAnchor.UpperRight);
                text.color = new Color(1f, 0.9f, 0.85f);
            }
            var system = SubdueSystem.Instance;
            sb.Clear();
            if (system == null || !system.IsReady) { sb.AppendLine("제압 시스템: 미준비"); text.text = sb.ToString(); return; }

            sb.Append("제압 확정 대기 ").Append(system.PendingRuleCount).AppendLine("건");
            foreach (var p in PlayerEntity.All)
            {
                var h = p.GetComponent<PlayerHands>();
                sb.Append(p.Id).Append(" 손: ").Append(h != null ? SubdueNames.Korean(h.State) : "-");
                if (h != null && h.HeldKind != HeldKind.None) sb.Append(" (").Append(h.HeldKind == HeldKind.Object ? h.HeldObject?.DisplayName : h.HeldBody?.Npc.DisplayName).Append(')');
                sb.AppendLine();
            }
            sb.AppendLine("— 기절 —");
            bool any = false;
            foreach (var u in system.Unconscious)
            {
                if (u == null) continue;
                any = true;
                sb.Append(u.Npc.DisplayName).Append(" 제압자 ").Append(u.SubduedBy?.Id).Append(" 남은 ").Append(u.Remaining.ToString("0")).Append("s")
                  .Append(u.IsCarried ? $" 운반:{u.CarriedBy.Id}" : "")
                  .Append(" 목격 ").Append(system.Ledger.WitnessCount(u.Npc.NpcId)).Append(" 발견 ").Append(system.Ledger.DiscovererCount(u.Npc.NpcId)).AppendLine();
            }
            if (!any) sb.AppendLine("(없음)");
            sb.AppendLine("— 로그 —");
            foreach (var l in log) sb.AppendLine(l);
            sb.AppendLine("키: E 들기/옮기기 · F 제압 · G 내려놓기 · T 배속");
            text.text = sb.ToString();
        }
    }
}

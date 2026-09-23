using System.Collections.Generic;
using UnityEngine;
using Project1028.PlayFoundation;
using Project1028.Suspicion;
using Project1028.Subdue;

namespace Project1028.Report
{
    /// <summary>
    /// 씬 단일. 002 OnReportAttempt를 소비해 신고 흐름을 만들고, 완료 시 섬 +25(002 표)와 이력을 기록한다. 유형은 보지 않는다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ReportSystem : MonoBehaviour
    {
        public static ReportSystem Instance { get; private set; }
        public bool IsReady { get; private set; }
        public ReportRules Rules { get; private set; }
        public int PendingRuleCount { get; private set; }

        private readonly HashSet<(string, string)> completed = new HashSet<(string, string)>();
        private readonly List<ReportFlow> flows = new List<ReportFlow>();
        private readonly List<ReportPointSelector.PointInfo> pointBuffer = new List<ReportPointSelector.PointInfo>();
        private ReportValidationResult loadResult;

        public IReadOnlyList<ReportFlow> ActiveFlows => flows;
        public int CompletedCount => completed.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable() => SuspicionEvents.OnReportAttempt += HandleAttempt;
        private void OnDisable() => SuspicionEvents.OnReportAttempt -= HandleAttempt;
        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void Start()
        {
            var suspicion = SuspicionSystem.Instance;
            if (suspicion == null || !suspicion.IsReady) { RuntimeHud.Instance?.Warn("[신고] 의심 시스템 미준비 — 신고 흐름 비활성"); return; }
            if (!ReportRulesLoader.TryLoad(suspicion.Rules, out var rules, out loadResult))
            {
                foreach (var m in loadResult.Messages) RuntimeHud.Instance?.Warn($"[신고 규칙] {m}");
                Debug.LogError("[ReportSystem] 규칙 로드 실패.");
                return;
            }
            Rules = rules;
            PendingRuleCount = loadResult.PendingCount;
            IsReady = true;
            RuntimeHud.Instance?.Warn($"신고 규칙 확정 대기 {PendingRuleCount}건 (StreamingAssets/Report/report_rules.json)");
        }

        public bool IsCompleted(NpcIdentity npc, PlayerEntity player) =>
            npc != null && player != null && completed.Contains((npc.NpcId, player.Id));

        private ReportFlow FlowOf(NpcIdentity npc)
        {
            for (int i = 0; i < flows.Count; i++) if (flows[i] != null && flows[i].Npc == npc) return flows[i];
            return null;
        }

        // 002가 발행: 3 도달, 004 깨어남. 침묵자는 002 플래그로 발행되지 않는다.
        private void HandleAttempt(NpcIdentity npc, PlayerEntity player)
        {
            if (!IsReady || npc == null || player == null) return;
            if (IsCompleted(npc, player)) return;                 // FR-009 한 플레이어당 1회
            if (FlowOf(npc) != null) return;                      // 이미 진행 중
            if (UnconsciousState.IsUnconscious(npc)) return;      // 기절 중엔 시작 불가
            TryStart(npc, player);
        }

        public bool TryStart(NpcIdentity npc, PlayerEntity player)
        {
            var point = PickPoint(npc);
            if (point == null)
            {
                ReportEvents.RaiseAbandoned(npc, player, "신고 지점 없음");
                RuntimeHud.Instance?.Warn($"{npc.DisplayName}: 신고할 곳이 없다 → 아무 일도 일어나지 않는다 (§5.2 미리 끊어뒀다)");
                return false;
            }
            var flow = npc.gameObject.AddComponent<ReportFlow>();
            flow.Begin(player, point, Rules);
            flows.Add(flow);
            ReportEvents.RaiseStarted(npc, player, point);
            RuntimeHud.Instance?.Warn($"★ {npc.DisplayName}이(가) {point.DisplayName}로 신고하러 간다 ({player.Id}) — 시간이 있다 (§5.1)");
            return true;
        }

        /// <summary>사용 가능한 최근접 신고 지점. 없으면 null.</summary>
        public ReportPoint PickPoint(NpcIdentity npc)
        {
            pointBuffer.Clear();
            foreach (var p in ReportPoint.All) pointBuffer.Add(new ReportPointSelector.PointInfo(p.Id, p.Position, p.IsUsable));
            string id = ReportPointSelector.PickNearestUsable(pointBuffer, npc.Position);
            if (id == null) return null;
            foreach (var p in ReportPoint.All) if (p.Id == id) return p;
            return null;
        }

        public void Complete(ReportFlow flow)
        {
            if (flow == null) return;
            var npc = flow.Npc;
            var player = flow.TargetPlayer;
            completed.Add((npc.NpcId, player.Id));
            SuspicionSystem.Instance?.Raise(SuspicionEvent.At(Rules.completedEventId, player, flow.TargetPoint != null ? flow.TargetPoint.Position : npc.Position)); // 섬 +25
            ReportEvents.RaiseCompleted(npc, player, flow.TargetPoint);
            RuntimeHud.Instance?.Warn($"★★ 신고 완료: {npc.DisplayName} → {flow.TargetPoint?.DisplayName} ({player.Id}). 섬 의심도 +25 — 이번 런이 위태롭다 (§5.2)");
        }

        internal void Unregister(ReportFlow flow) => flows.Remove(flow);
    }
}

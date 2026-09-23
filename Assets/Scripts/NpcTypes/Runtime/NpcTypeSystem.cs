using System.Collections.Generic;
using UnityEngine;
using Project1028.PlayFoundation;
using Project1028.Suspicion;

namespace Project1028.NpcTypes
{
    /// <summary>
    /// 씬 단일. 유형 규칙 로드, NPC 배정(002 플래그 설정·행동 컴포넌트 부착), 조사 릴레이, 동조자 전환 감시.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NpcTypeSystem : MonoBehaviour
    {
        public static NpcTypeSystem Instance { get; private set; }

        public bool IsReady { get; private set; }
        public NpcTypeRules Rules { get; private set; }
        public int PendingRuleCount { get; private set; }
        public bool LabelsVisible { get; set; } = true;
        public AlertZone TurnZone { get; private set; } = AlertZone.Tension;

        private readonly List<NpcTypeProfile> profiles = new List<NpcTypeProfile>();
        private bool started;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            SuspicionEvents.OnIslandZoneChanged -= HandleZoneChanged;
        }

        private void Start()
        {
            var suspicion = SuspicionSystem.Instance;
            if (suspicion == null || !suspicion.IsReady)
            {
                RuntimeHud.Instance?.Warn("[유형] 의심 시스템이 준비되지 않아 NPC 유형을 적용할 수 없습니다.");
                return;
            }

            if (!NpcTypeRulesLoader.TryLoad(suspicion.Rules, out var rules, out var result))
            {
                foreach (var m in result.Messages) RuntimeHud.Instance?.Warn($"[유형 규칙] {m}");
                Debug.LogError("[NpcTypeSystem] 유형 규칙 로드 실패. 시스템 비활성.");
                return;
            }
            foreach (var m in result.Messages) RuntimeHud.Instance?.Warn($"[유형 규칙] {m}");

            Rules = rules;
            PendingRuleCount = result.PendingCount;
            if (NpcTypeDefinitions.TryParseZone(rules.rules.sympathizer.turnZone, out var z)) TurnZone = z;
            IsReady = true;
            RuntimeHud.Instance?.Warn($"유형 규칙 확정 대기 {PendingRuleCount}건 (StreamingAssets/NpcTypes/npc_types.json)");

            AssignAll();
            SuspicionEvents.OnIslandZoneChanged += HandleZoneChanged;
            EvaluateTurns(suspicion.Island.Zone);
            started = true;
        }

        // ---------------- 배정 ----------------

        private void AssignAll()
        {
            profiles.Clear();
            foreach (var npc in NpcIdentity.All)
            {
                var a = Rules.Find(npc.NpcId);
                NpcType type;
                Faction faction;
                string[] roles;
                if (a == null)
                {
                    type = NpcType.Sympathizer;
                    faction = NpcTypeDefinitions.DefaultFactionOf(type);
                    roles = new string[0];
                    RuntimeHud.Instance?.Warn($"미배정 NPC '{npc.NpcId}' → 동조자(일반 주민) 기본 적용");
                }
                else
                {
                    NpcTypeDefinitions.TryParseType(a.type, out type);
                    if (!NpcTypeDefinitions.TryParseFaction(a.faction, out faction)) faction = NpcTypeDefinitions.DefaultFactionOf(type);
                    roles = a.roles ?? new string[0];
                }

                var profile = npc.GetComponent<NpcTypeProfile>() ?? npc.gameObject.AddComponent<NpcTypeProfile>();
                profile.Configure(type, faction, roles);
                profiles.Add(profile);

                ApplyProfileFlags(npc, type);
                if (npc.GetComponent<NpcTypeLabel>() == null) npc.gameObject.AddComponent<NpcTypeLabel>();
            }
        }

        /// <summary>유형 → 002 플래그·003 행동 컴포넌트. 002 코드는 유형을 모른다.</summary>
        private static void ApplyProfileFlags(NpcIdentity npc, NpcType type)
        {
            var flags = npc.GetComponent<NpcSuspicionProfile>() ?? npc.gameObject.AddComponent<NpcSuspicionProfile>();
            var typeProfile = npc.GetComponent<NpcTypeProfile>();
            flags.IsManager = typeProfile != null && typeProfile.IsManager;

            switch (type)
            {
                case NpcType.Informer:
                    flags.PropagatesSuspicion = false;          // 전달은 003이 전담 (이중 +1 방지)
                    flags.ReportsToManagerImmediately = true;   // 의미 표시
                    flags.SuppressStageBehaviour = true;        // §3.1 그 자리에서는 아무 일 없음
                    if (npc.GetComponent<NpcMover>() == null) npc.gameObject.AddComponent<NpcMover>();
                    if (npc.GetComponent<InformerBehaviour>() == null) npc.gameObject.AddComponent<InformerBehaviour>();
                    break;
                case NpcType.Silent:
                    flags.PropagatesSuspicion = false;          // §2.6 침묵자는 전파하지 않는다
                    flags.SuppressReportAttempt = true;         // §3 신고하지 않는다
                    break;
                case NpcType.Sentinel:
                    flags.IgnoresSuspicionEvents = true;        // 자신은 의심을 쌓지 않는다
                    flags.PropagatesSuspicion = false;
                    flags.SuppressReportAttempt = true;
                    flags.SuppressStageBehaviour = true;
                    if (npc.GetComponent<SentinelBehaviour>() == null) npc.gameObject.AddComponent<SentinelBehaviour>();
                    break;
                default: // Watcher, Sympathizer: 002 기본 규칙
                    break;
            }
        }

        // ---------------- 조사 릴레이 (FR-006, FR-007) ----------------

        /// <summary>조사 사건 입력. actor를 보고 있는 NPC마다 사건 하나만 낸다: 감시자 규칙이면 watcher_investigate, 아니면 일반 규칙.</summary>
        public int Investigate(PlayerEntity actor)
        {
            if (!IsReady || actor == null || SuspicionSystem.Instance == null) return 0;
            int count = 0;
            foreach (var npc in NpcIdentity.All)
            {
                var vision = npc.GetComponent<NpcVision>();
                if (vision == null || !vision.IsSeeing(actor)) continue;
                var profile = NpcTypeProfile.Get(npc);
                string eventId = profile != null && profile.WatcherRuleApplies ? Rules.rules.watcher.investigateEventId : "investigate_in_sight";
                SuspicionSystem.Instance.Raise(SuspicionEvent.Target(eventId, actor, npc));
                count++;
            }
            NpcTypeEvents.RaiseInvestigate(actor, count);
            return count;
        }

        // ---------------- 동조자 전환 (FR-013, FR-014) ----------------

        private void HandleZoneChanged(AlertZone oldZone, AlertZone newZone) => EvaluateTurns(newZone);

        private void EvaluateTurns(AlertZone zone)
        {
            bool turned = SympathizerTurnRule.IsTurned(zone, TurnZone);
            foreach (var p in profiles)
            {
                if (p == null || p.Type != NpcType.Sympathizer) continue;
                if (p.IsTurned == turned) continue;
                p.IsTurned = turned;
                NpcTypeEvents.RaiseSympathizerTurned(p.Identity, turned);
                RuntimeHud.Instance?.Warn(turned ? $"{p.Identity.DisplayName}: 동조자 적대 전환 (섬 {zone})" : $"{p.Identity.DisplayName}: 동조자 전환 해제 (섬 {zone})");
            }
        }

        public IReadOnlyList<NpcTypeProfile> Profiles => profiles;
    }
}

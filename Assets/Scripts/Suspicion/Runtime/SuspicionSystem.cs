using System.Collections.Generic;
using UnityEngine;
using Project1028.PlayFoundation;

namespace Project1028.Suspicion
{
    /// <summary>
    /// 씬 단일 의심 시스템. 상태 보관(개인·섬), 사건 적용(표 기반), 전파, 감소, 밤 이동, 이월.
    /// 틱 순서 고정: 밤 이동 사건 생성 → 큐 적용(상승) → 전파 → 감소.  (상승이 감소보다 먼저: Edge case)
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SuspicionSystem : MonoBehaviour
    {
        public static SuspicionSystem Instance { get; private set; }

        public bool IsReady { get; private set; }
        public SuspicionRules Rules { get; private set; }
        public IslandAlert Island { get; private set; } = new IslandAlert();
        public int PendingRuleCount { get; private set; }

        private readonly Dictionary<(NpcIdentity, PlayerEntity), PersonalSuspicion> personal = new Dictionary<(NpcIdentity, PlayerEntity), PersonalSuspicion>();
        private readonly Queue<SuspicionEvent> queue = new Queue<SuspicionEvent>();
        private readonly PropagationScheduler scheduler = new PropagationScheduler();
        private readonly Dictionary<(NpcIdentity, PlayerEntity), float> nightWanderCooldown = new Dictionary<(NpcIdentity, PlayerEntity), float>();
        private readonly HashSet<string> warnedUnknownIds = new HashSet<string>();
        private readonly Dictionary<string, NpcIdentity> npcById = new Dictionary<string, NpcIdentity>();
        private readonly Dictionary<string, PlayerEntity> playerById = new Dictionary<string, PlayerEntity>();
        private readonly List<NpcIdentity> npcBuffer = new List<NpcIdentity>();
        private bool warnedNotReady;
        private RulesValidationResult loadResult;

        public IReadOnlyList<PropagationScheduler.Reservation> PendingPropagations => scheduler.Pending;

        public IEnumerable<(NpcIdentity npc, PlayerEntity player, PersonalSuspicion s)> AllPersonal
        {
            get { foreach (var kv in personal) yield return (kv.Key.Item1, kv.Key.Item2, kv.Value); }
        }

        // ---------------- lifecycle ----------------

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (SuspicionRulesLoader.TryLoad(out var rules, out var result))
            {
                Rules = rules;
                IsReady = true;
                PendingRuleCount = result.PendingCount;
            }
            else
            {
                IsReady = false;
                Debug.LogError("[SuspicionSystem] 규칙 로드 실패. 시스템 비활성.");
            }
            loadResult = result;
        }

        private void Start()
        {
            // RuntimeHud.Awake 순서에 의존하지 않도록 Start에서 경고를 낸다.
            if (loadResult != null) foreach (var m in loadResult.Messages) RuntimeHud.Instance?.Warn($"[의심 규칙] {m}");
            if (IsReady) RuntimeHud.Instance?.Warn($"의심 규칙 확정 대기 {PendingRuleCount}건 (수치는 StreamingAssets/Suspicion/suspicion_rules.json)");
        }

        private void OnEnable()
        {
            PlayerEntity.OnRegistered += HandlePlayerRegistered;
            foreach (var p in PlayerEntity.All) HandlePlayerRegistered(p);
        }

        private void OnDisable()
        {
            PlayerEntity.OnRegistered -= HandlePlayerRegistered;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void HandlePlayerRegistered(PlayerEntity p)
        {
            if (p == null) return;
            playerById[p.Id] = p;
            // 새 플레이어(코옵 합류)는 모든 NPC에 대해 0에서 시작. 섬 의심도는 그대로 본다.
            foreach (var npc in NpcIdentity.All) GetPersonal(npc, p);
        }

        // ---------------- public API ----------------

        public void Raise(SuspicionEvent e)
        {
            if (!IsReady)
            {
                if (!warnedNotReady) { RuntimeHud.Instance?.Warn("의심 시스템이 준비되지 않아 사건을 무시합니다."); warnedNotReady = true; }
                return;
            }
            if (e.Actor == null) { RuntimeHud.Instance?.Warn($"사건 '{e.EventId}': Actor가 없습니다."); return; }
            queue.Enqueue(e);
        }

        public PersonalSuspicion GetPersonal(NpcIdentity npc, PlayerEntity player)
        {
            var key = (npc, player);
            if (!personal.TryGetValue(key, out var s))
            {
                s = new PersonalSuspicion(Time.time);
                personal[key] = s;
                if (npc != null) npcById[npc.NpcId] = npc;
                if (player != null) playerById[player.Id] = player;
            }
            return s;
        }

        public SuspicionStage GetStage(NpcIdentity npc, PlayerEntity player) => GetPersonal(npc, player).Stage;

        public void DebugAdjustIsland(int delta) => ApplyIsland(delta);

        /// <summary>이미 3인 NPC가 다시 신고를 시도해야 할 때(004 깨어남 등). SuppressReportAttempt(침묵자)는 존중.</summary>
        public void ForceReportAttempt(NpcIdentity npc, PlayerEntity player)
        {
            if (npc == null || player == null) return;
            if (NpcSuspicionProfile.GetOrDefault(npc).SuppressReportAttempt) return;
            SuspicionEvents.RaiseReportAttempt(npc, player);
        }

        public CarryOverSnapshot EndRun()
        {
            var list = new List<(string, string, int)>();
            foreach (var kv in personal)
            {
                var (npc, player) = kv.Key;
                if (npc == null || player == null) continue;
                list.Add((npc.NpcId, player.Id, kv.Value.Value));
            }
            var snap = CarryOverCalculator.Compute(list, Island.Value, Rules?.carryOver);
            SuspicionEvents.RaiseRunEnded(snap);
            RuntimeHud.Instance?.Warn($"런 종료 이월: 섬 {Island.Value}→{snap.Island}, 개인 {snap.Personal.Count}쌍 (저장은 후속 기능)");
            return snap;
        }

        // ---------------- tick ----------------

        private void Update()
        {
            if (!IsReady) return;
            float now = Time.time;
            TickNightWander(now);
            ApplyQueuedEvents(now);
            TickPropagation(now);
            TickDecay(now);
        }

        // §7 밤에 돌아다니면 +1 (쿨다운)
        private void TickNightWander(float now)
        {
            if (TimeOfDay.CurrentOrDay != DayPhase.Night) return;
            var rule = Rules.timeOfDay;
            foreach (var npc in NpcIdentity.All)
            {
                var vision = npc.GetComponent<NpcVision>();
                if (vision == null) continue;
                foreach (var p in PlayerEntity.All)
                {
                    if (p.MovementState == MovementState.Idle || !vision.IsSeeing(p)) continue;
                    var key = (npc, p);
                    if (nightWanderCooldown.TryGetValue(key, out float until) && until > now) continue;
                    nightWanderCooldown[key] = now + rule.nightWanderIntervalSeconds;
                    Raise(SuspicionEvent.Target(rule.nightWanderEventId, p, npc));
                }
            }
        }

        private void ApplyQueuedEvents(float now)
        {
            while (queue.Count > 0)
            {
                var e = queue.Dequeue();
                var rule = Rules.FindEvent(e.EventId);
                if (rule == null)
                {
                    if (warnedUnknownIds.Add(e.EventId ?? "(null)")) RuntimeHud.Instance?.Warn($"알 수 없는 사건 id '{e.EventId}' — suspicion_rules.json에 추가하세요.");
                    continue;
                }
                if (rule.nightOnly && TimeOfDay.CurrentOrDay != DayPhase.Night) continue;
                if (e.Actor == null) continue;

                npcBuffer.Clear();
                CollectTargets(rule, e, npcBuffer);

                int applied = 0;
                int personalDelta = ScaledPersonalDelta(rule);
                foreach (var npc in npcBuffer)
                {
                    var profile = NpcSuspicionProfile.GetOrDefault(npc);
                    if (profile.IgnoresSuspicionEvents) continue; // 003 경계자
                    var s = GetPersonal(npc, e.Actor);
                    var change = s.Apply(personalDelta, now);
                    applied++;
                    if (change.HasValue)
                    {
                        SuspicionEvents.RaisePersonalStageChanged(npc, e.Actor, change.Value.Old, change.Value.New);
                        if (change.Value.New == SuspicionStage.Certain && !profile.SuppressReportAttempt)
                            SuspicionEvents.RaiseReportAttempt(npc, e.Actor); // §2.2 3 도달 1회 (침묵자는 억제)
                    }
                }

                if (rule.islandDelta > 0) ApplyIsland(rule.islandDelta); // §8.1 공유 값, 배율 미적용(R-4)
                SuspicionEvents.RaiseEventApplied(e, applied);
            }
        }

        private void CollectTargets(SuspicionEventRule rule, SuspicionEvent e, List<NpcIdentity> into)
        {
            switch (rule.scope)
            {
                case SuspicionEventRule.ScopeGlobal:
                    break; // 대상 없음: islandDelta만 적용
                case SuspicionEventRule.ScopeTarget:
                    if (e.TargetNpc != null && (!rule.requiresSight || Sees(e.TargetNpc, e.Actor))) into.Add(e.TargetNpc);
                    break;
                case SuspicionEventRule.ScopeRadius:
                    foreach (var npc in NpcIdentity.All)
                    {
                        if (Vector3.Distance(npc.Position, e.Position) > rule.radius) continue;
                        if (rule.requiresSight && !Sees(npc, e.Actor)) continue;
                        into.Add(npc);
                    }
                    break;
                default: // witness: 시야 안 NPC 전부 (FR-011)
                    foreach (var npc in NpcIdentity.All)
                    {
                        if (Sees(npc, e.Actor)) into.Add(npc);
                    }
                    break;
            }
        }

        private static bool Sees(NpcIdentity npc, PlayerEntity player)
        {
            var vision = npc != null ? npc.GetComponent<NpcVision>() : null;
            return vision != null && vision.IsSeeing(player);
        }

        // R-4: 개인 의심 상승량에만 시간대 배율, 반올림, 원래 0이 아니면 최소 1. ignoresTimeMultiplier(확정 규칙)면 원값.
        private int ScaledPersonalDelta(SuspicionEventRule rule)
        {
            int baseDelta = rule.personalDelta;
            if (baseDelta <= 0) return 0;
            if (rule.ignoresTimeMultiplier) return baseDelta;
            float mult = TimeOfDay.CurrentOrDay == DayPhase.Night ? Rules.timeOfDay.nightMultiplier : Rules.timeOfDay.dayMultiplier;
            return Mathf.Max(1, Mathf.RoundToInt(baseDelta * mult));
        }

        private void ApplyIsland(int delta)
        {
            int oldValue = Island.Value;
            bool oldBlocked = Island.DepartureBlocked;
            var change = Island.Apply(delta);
            EmitIsland(oldValue, oldBlocked, change);
        }

        private void EmitIsland(int oldValue, bool oldBlocked, ZoneChange? change)
        {
            if (Island.Value != oldValue) SuspicionEvents.RaiseIslandChanged(oldValue, Island.Value);
            if (change.HasValue) SuspicionEvents.RaiseIslandZoneChanged(change.Value.Old, change.Value.New);
            if (Island.DepartureBlocked != oldBlocked) SuspicionEvents.RaiseDepartureBlockedChanged(Island.DepartureBlocked); // §2.5
        }

        // §2.6 전파
        private void TickPropagation(float now)
        {
            var npcs = NpcIdentity.All;
            float dist = Rules.propagation.distance;
            float delay = Rules.propagation.delaySeconds;

            for (int i = 0; i < npcs.Count; i++)
            {
                for (int j = i + 1; j < npcs.Count; j++)
                {
                    var a = npcs[i]; var b = npcs[j];
                    bool inRange = Vector3.Distance(a.Position, b.Position) <= dist;
                    scheduler.SetMeeting(a.NpcId, b.NpcId, inRange, now);
                    if (!inRange) continue;
                    TryReserveDirection(a, b, now, delay);
                    TryReserveDirection(b, a, now, delay);
                }
            }

            foreach (var r in scheduler.Drain(now))
            {
                if (!npcById.TryGetValue(r.From, out var from) || !npcById.TryGetValue(r.To, out var to) || !playerById.TryGetValue(r.PlayerId, out var player)) continue;
                if (from == null || to == null || player == null) continue;
                Raise(SuspicionEvent.Target(SuspicionRules.PropagationEventId, player, to));
                SuspicionEvents.RaisePropagated(from, to, player);
            }
        }

        private void TryReserveDirection(NpcIdentity from, NpcIdentity to, float now, float delay)
        {
            var pf = NpcSuspicionProfile.GetOrDefault(from);
            if (!pf.PropagatesSuspicion) return; // 침묵자
            var pt = NpcSuspicionProfile.GetOrDefault(to);
            bool immediate = pf.ReportsToManagerImmediately;
            if (immediate && !pt.IsManager) return; // 밀고자는 관리자에게만

            foreach (var p in PlayerEntity.All)
            {
                if (GetPersonal(from, p).Value < (int)SuspicionStage.Alert) continue;
                npcById[from.NpcId] = from; npcById[to.NpcId] = to; playerById[p.Id] = p;
                scheduler.TryReserve(from.NpcId, to.NpcId, p.Id, now + (immediate ? 0f : delay));
            }
        }

        // §2.7 감소
        private void TickDecay(float now)
        {
            float interval = Rules.decay.personalIntervalSeconds;
            foreach (var kv in personal)
            {
                var (npc, player) = kv.Key;
                if (npc == null || player == null) continue;
                var s = kv.Value;
                if (s.Value <= PersonalSuspicion.Min) continue;
                int steps = DecayCalculator.PersonalSteps(s.LastRaisedAt, s.LastDecayAt, now, interval);
                if (steps <= 0) continue;
                s.LastDecayAt = now;
                var change = s.Apply(-steps, now);
                if (change.HasValue) SuspicionEvents.RaisePersonalStageChanged(npc, player, change.Value.Old, change.Value.New);
            }

            int oldValue = Island.Value;
            bool oldBlocked = Island.DepartureBlocked;
            var zc = Island.ApplyDecay(DecayCalculator.IslandAmount(Time.deltaTime, Rules.decay.islandPerMinute));
            EmitIsland(oldValue, oldBlocked, zc);
        }
    }
}

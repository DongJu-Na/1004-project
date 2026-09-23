using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Project1028.PlayFoundation;
using Project1028.Suspicion;
using Project1028.Subdue;
using Project1028.Vehicle;

namespace Project1028.Encounter
{
    /// <summary>씬 단일: 규칙 로드, 트리거 판정(§6.4), 스폰, 인스턴스 관리, 중단(004), 회수 조회(§6.5), 런 스냅샷.</summary>
    [DisallowMultipleComponent]
    public sealed class EncounterSystem : MonoBehaviour
    {
        public static EncounterSystem Instance { get; private set; }

        [SerializeField] private string currentIslandId = "island_test";

        public bool IsReady { get; private set; }
        public EncounterRules Rules { get; private set; }
        public RunLedger Ledger { get; } = new RunLedger();
        public int RunCount { get; private set; }
        public EncounterInstance Active { get; private set; }
        public float LastEndedAt { get; private set; } = float.NegativeInfinity;
        public int PendingRuleCount { get; private set; }
        public int SampleCount { get; private set; }
        public int TotalNpcs { get; private set; }
        public int TotalLines { get; private set; }
        public string CurrentIslandId { get => currentIslandId; set => currentIslandId = value; }
        public string LastReport { get; private set; } = string.Empty;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            SubdueEvents.OnSubdued += HandleSubdued;
            SuspicionEvents.OnRunEnded += HandleRunEnded;
        }

        private void OnDisable()
        {
            SubdueEvents.OnSubdued -= HandleSubdued;
            SuspicionEvents.OnRunEnded -= HandleRunEnded;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void Start()
        {
            var suspicion = SuspicionSystem.Instance;
            if (suspicion == null || !suspicion.IsReady) { RuntimeHud.Instance?.Warn("[인카운터] 의심 시스템 미준비 — 비활성"); return; }
            if (!EncounterRulesLoader.TryLoad(suspicion.Rules, out var rules, out var result))
            {
                foreach (var m in result.Messages) RuntimeHud.Instance?.Warn($"[인카운터 규칙] {m}");
                Debug.LogError("[EncounterSystem] 규칙 로드 실패.");
                return;
            }
            foreach (var m in result.Messages) RuntimeHud.Instance?.Warn($"[인카운터 규칙] {m}");
            Rules = rules;
            PendingRuleCount = result.PendingCount; SampleCount = result.SampleCount; TotalNpcs = result.TotalNpcs; TotalLines = result.TotalLines;
            IsReady = true;
            RuntimeHud.Instance?.Warn($"인카운터: 정의 {rules.encounters.Length}개(샘플 {SampleCount}) · 단가 합계 NPC {TotalNpcs}·대사 {TotalLines}줄 · 확정 대기 {PendingRuleCount}건 (§6.1, 헌장 II)");
        }

        // ---------------- 트리거 (§6.4) ----------------

        public void TryTrigger(PlayerEntity player, EncounterTrigger trigger)
        {
            if (!IsReady || player == null || trigger == null) return;
            var runner = player.GetComponent<DialogueRunner>();
            var activity = player.GetComponent<PlayerActivity>();
            bool inVehicle = player.IsInVehicle;
            bool moving = inVehicle
                ? (VehicleController.Instance != null && VehicleController.Instance.IsMoving)
                : player.MovementState != MovementState.Idle;
            var ctx = new SpawnGate.SpawnContext
            {
                RunCount = RunCount, PerRunMax = Rules.spawn.perRunMax,
                SinceLastEnd = Time.time - LastEndedAt, MinInterval = Rules.spawn.minIntervalSeconds,
                Moving = moving, Busy = player.IsLocked || (runner != null && runner.IsActive) || player.HandsBusy,
                Scouting = activity != null && activity.IsScouting, ActiveInstance = Active != null,
            };
            var (ok, reason) = SpawnGate.CanSpawn(ctx);
            if (!ok) { Evaluate(player, trigger.TriggerId, false, reason); return; }
            if (!SpawnGate.Roll(inVehicle, Rules.spawn.footChance, Rules.spawn.vehicleChance, Random.value)) { Evaluate(player, trigger.TriggerId, false, inVehicle ? "확률(차량)" : "확률(도보)"); return; }

            var candidates = Candidates();
            if (candidates.Count == 0) { Evaluate(player, trigger.TriggerId, false, "후보 없음"); return; }
            var def = WeightedTypePicker.Pick(candidates, Rules.spawn.typeWeights, Random.value, Random.value);
            if (def == null) { Evaluate(player, trigger.TriggerId, false, "선택 실패"); return; }
            Evaluate(player, trigger.TriggerId, true, $"{def.id} ({(inVehicle ? "차량" : "도보")})");
            StartInstance(def, player, trigger.transform.position);
        }

        private void Evaluate(PlayerEntity p, string triggerId, bool spawned, string reason)
        {
            EncounterEvents.RaiseTrigger(p, triggerId, spawned, reason);
            RuntimeHud.Instance?.Warn(spawned ? $"★ 인카운터 발생 @{triggerId}: {reason}" : $"인카운터 스킵 @{triggerId}: {reason}");
        }

        public List<EncounterDef> Candidates()
        {
            var zone = SuspicionSystem.Instance != null ? SuspicionSystem.Instance.Island.Zone : AlertZone.Calm;
            return CandidateFilter.Filter(Rules.encounters, Rules.pools, currentIslandId, TimeOfDay.CurrentOrDay, zone,
                new HashSet<string>(Ledger.Flags), new HashSet<string>(Ledger.History), Rules.callbacks);
        }

        public bool ForceStart(string encounterId, PlayerEntity player)
        {
            if (!IsReady || player == null) return false;
            if (Active != null) { RuntimeHud.Instance?.Warn("진행 중인 인카운터가 있다"); return false; }
            var def = Rules.Find(encounterId);
            if (def == null) { RuntimeHud.Instance?.Warn($"인카운터 '{encounterId}' 없음"); return false; }
            StartInstance(def, player, player.Position + player.Forward * 3f);
            return true;
        }

        private void StartInstance(EncounterDef def, PlayerEntity player, Vector3 anchor)
        {
            var go = new GameObject($"Encounter_{def.id}");
            go.transform.position = anchor;
            var inst = go.AddComponent<EncounterInstance>();
            Active = inst;
            RunCount++;
            EncounterEvents.RaiseStarted(def, player);
            RuntimeHud.Instance?.Warn($"인카운터 시작: {def.id} [{EncounterNames.Korean(EncounterRules.TypeOf(def))}]{(def.sample ? " (샘플)" : "")} — 런 {RunCount}/{Rules.spawn.perRunMax}");
            inst.Begin(def, player, anchor, this);
        }

        internal void NotifyEnded(EncounterInstance inst)
        {
            if (Active == inst) Active = null;
            LastEndedAt = Time.time;
        }

        private void HandleSubdued(PlayerEntity actor, NpcIdentity npc, SubdueAction action, int witnesses)
        {
            if (Active != null && Active.SpawnedNpcs.Contains(npc)) Active.Interrupt();
        }

        // ---------------- 회수 (§6.5) ----------------

        public IReadOnlyList<CallbackEntry> CallbacksFor(string flag) => IsReady ? RunLedger.CallbacksFor(flag, Rules.callbacks) : new List<CallbackEntry>();

        private void HandleRunEnded(CarryOverSnapshot _)
        {
            var (flags, history) = Ledger.Export();
            EncounterEvents.RaiseSnapshot(flags, history);
            RuntimeHud.Instance?.Warn($"인카운터 이월: 플래그 {flags.Length}, 이력 {history.Length} (저장은 후속 기능)");
        }

        // ---------------- 시뮬레이션 (SC-001·002·004) ----------------

        public string Simulate(int runs)
        {
            if (!IsReady) return "미준비";
            var sp = Rules.spawn;
            var hist = new int[sp.perRunMax + 2];
            int intervalViolations = 0, duplicates = 0, conditionViolations = 0, vehicleSpawns = 0, footSpawns = 0;
            var rng = new System.Random(12345);
            for (int run = 0; run < runs; run++)
            {
                int count = 0; float t = 0f, lastEnd = float.NegativeInfinity;
                var history = new HashSet<string>(); var flags = new HashSet<string>();
                for (int i = 0; i < 12; i++)
                {
                    t += 30f;
                    bool vehicle = i % 2 == 1;
                    var ctx = new SpawnGate.SpawnContext { RunCount = count, PerRunMax = sp.perRunMax, SinceLastEnd = t - lastEnd, MinInterval = sp.minIntervalSeconds, Moving = true };
                    if (!SpawnGate.CanSpawn(ctx).ok) continue;
                    if (!SpawnGate.Roll(vehicle, sp.footChance, sp.vehicleChance, (float)rng.NextDouble())) continue;
                    var cands = CandidateFilter.Filter(Rules.encounters, Rules.pools, currentIslandId, DayPhase.Day, AlertZone.Calm, flags, history, Rules.callbacks);
                    if (cands.Count == 0) continue;
                    var def = WeightedTypePicker.Pick(cands, sp.typeWeights, (float)rng.NextDouble(), (float)rng.NextDouble());
                    if (def == null) continue;
                    if (t - lastEnd < sp.minIntervalSeconds) intervalViolations++;
                    if (!history.Add(def.id)) duplicates++;
                    if (def.conditions != null && def.conditions.timeOfDay == "night") conditionViolations++;
                    if (def.outcome?.flags != null) foreach (var f in def.outcome.flags) flags.Add(f);
                    if (vehicle) vehicleSpawns++; else footSpawns++;
                    count++; lastEnd = t;
                }
                hist[Mathf.Clamp(count, 0, hist.Length - 1)]++;
            }
            var sb = new StringBuilder();
            sb.Append($"시뮬 {runs}런: 발생 수 분포 ");
            for (int i = 0; i < hist.Length; i++) if (hist[i] > 0) sb.Append($"{i}개×{hist[i]} ");
            sb.Append($"| 간격 위반 {intervalViolations} | 원본 중복 {duplicates} | 조건 위반 {conditionViolations} | 차량 {vehicleSpawns} vs 도보 {footSpawns}");
            LastReport = sb.ToString();
            Debug.Log("[Encounter] " + LastReport);
            RuntimeHud.Instance?.Warn(LastReport);
            return LastReport;
        }
    }
}

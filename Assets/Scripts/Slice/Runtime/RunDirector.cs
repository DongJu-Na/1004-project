using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Project1028.PlayFoundation;
using Project1028.Suspicion;
using Project1028.Subdue;
using Project1028.Report;
using Project1028.Vehicle;
using Project1028.Encounter;

namespace Project1028.Slice
{
    /// <summary>씬 단일: 규칙·배치 로드, 시계 틱과 밤 전환, 목표, 통계, 런 종료·재시작.</summary>
    [DisallowMultipleComponent]
    public sealed class RunDirector : MonoBehaviour
    {
        public static RunDirector Instance { get; private set; }

        public bool IsReady { get; private set; }
        public SliceRules Rules { get; private set; }
        public IslandLayout Layout { get; private set; }
        public RunClock Clock { get; private set; }
        public ObjectiveTracker Objectives { get; private set; }
        public RunOutcome Outcome { get; private set; } = RunOutcome.None;
        public RunStats Stats { get; } = new RunStats();
        public int PendingRuleCount { get; private set; }
        public bool IsEnded => Outcome != RunOutcome.None;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Time.timeScale = 1f;
        }

        private void OnEnable()
        {
            ReportEvents.OnReportCompleted += OnReport;
            SubdueEvents.OnSubdued += OnSubdued;
            EncounterEvents.OnEncounterEnded += OnEncounterEnded;
            EncounterEvents.OnChoiceMade += OnChoice;
            EncounterEvents.OnFlagRecorded += OnFlag;
        }

        private void OnDisable()
        {
            ReportEvents.OnReportCompleted -= OnReport;
            SubdueEvents.OnSubdued -= OnSubdued;
            EncounterEvents.OnEncounterEnded -= OnEncounterEnded;
            EncounterEvents.OnChoiceMade -= OnChoice;
            EncounterEvents.OnFlagRecorded -= OnFlag;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void Start()
        {
            var suspicion = SuspicionSystem.Instance;
            if (suspicion == null || !suspicion.IsReady) { RuntimeHud.Instance?.Warn("[슬라이스] 의심 시스템 미준비"); return; }
            if (!SliceRulesLoader.TryLoad(suspicion.Rules, out var rules, out var result))
            {
                foreach (var m in result.Messages) RuntimeHud.Instance?.Warn($"[슬라이스 규칙] {m}");
                Debug.LogError("[RunDirector] 규칙 로드 실패."); return;
            }
            if (!IslandLayoutLoader.TryLoad(out var layout, out var err)) { RuntimeHud.Instance?.Warn($"[섬 배치] {err}"); Debug.LogError(err); return; }

            Rules = rules; Layout = layout;
            PendingRuleCount = result.PendingCount;
            Clock = new RunClock(rules.clock.nightAtSeconds, rules.clock.departureAtSeconds, rules.clock.nextBoatDelaySeconds);
            Objectives = new ObjectiveTracker(rules.objectives);
            VehicleSeats.AllowBusyHands = rules.vehicle.allowEvidenceAboard;
            IsReady = true;
            RuntimeHud.Instance?.Warn($"런 시작 — 출항 {rules.clock.departureAtSeconds / 60f:0}분 후, 밤 {rules.clock.nightAtSeconds / 60f:0}분 후 (확정 대기 {PendingRuleCount}건)");
        }

        private void Update()
        {
            if (!IsReady || IsEnded) return;
            bool blocked = SuspicionSystem.Instance != null && SuspicionSystem.Instance.Island.DepartureBlocked;
            var transition = Clock.Tick(Time.deltaTime, blocked);
            if (transition.HasValue)
            {
                if (transition.Value == RunPhase.Night && TimeOfDay.Instance != null) TimeOfDay.Instance.Set(DayPhase.Night);
                SliceEvents.RaisePhase(transition.Value);
            }
            var forced = DepartureRule.Forced(Clock.Elapsed, Clock.NextBoatAt, blocked);
            if (forced.HasValue) EndRun(forced.Value);
        }

        /// <summary>목표 조건 충족 시도. 전진하면 이벤트.</summary>
        public bool Satisfy(string condition)
        {
            if (!IsReady || IsEnded) return false;
            var step = Objectives.Current;
            if (!Objectives.Satisfy(condition)) return false;
            SliceEvents.RaiseObjective(step);
            RuntimeHud.Instance?.Warn($"목표 완료: {step.text}");
            return true;
        }

        public void EndRun(RunOutcome outcome)
        {
            if (!IsReady || IsEnded) return;
            Outcome = outcome;
            Clock.End();
            var s = SuspicionSystem.Instance;
            Stats.ElapsedSeconds = Clock.Elapsed;
            Stats.IslandValue = s != null ? s.Island.Value : 0;
            Stats.IslandZone = s != null ? s.Island.Zone.ToString() : "";
            s?.EndRun();
            SliceEvents.RaiseEnded(outcome, Stats);
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            var scene = SceneManager.GetActiveScene();
            if (scene.buildIndex >= 0) { SceneManager.LoadScene(scene.buildIndex); return; }
#if UNITY_EDITOR
            // 빌드 설정에 없는 테스트 씬: 에디터에서는 경로로 다시 로드한다
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
            SceneManager.LoadScene(0);
#endif
        }

        private void OnReport(NpcIdentity n, PlayerEntity p, ReportPoint r) => Stats.ReportsCompleted++;
        private void OnSubdued(PlayerEntity a, NpcIdentity n, SubdueAction s, int w) => Stats.Subdues++;
        private void OnEncounterEnded(EncounterDef d, PlayerEntity p, IReadOnlyList<string> f) => Stats.Encounters++;
        private void OnChoice(EncounterDef d, PlayerEntity p, string o, bool t) => Stats.Choices.Add($"{d.id}:{o}");
        private void OnFlag(string f) => Stats.Flags.Add(f);

        public static string Mmss(float seconds) { int s = Mathf.Max(0, Mathf.RoundToInt(seconds)); return $"{s / 60:0}:{s % 60:00}"; }
    }
}

using System.Collections.Generic;
using UnityEngine;
using Project1028.PlayFoundation;
using Project1028.Suspicion;
using Project1028.Subdue;
using Project1028.Report;
using Project1028.Encounter;

namespace Project1028.Slice
{
    /// <summary>
    /// 기존 시스템의 이벤트를 시각·청각 반응으로 바꾼다. 규칙은 없다 — 보여줄 뿐이다. 강도·색·간격은 slice_rules.feedback.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FeedbackDirector : MonoBehaviour
    {
        private readonly HashSet<NpcVision> hooked = new HashSet<NpcVision>();
        private readonly Dictionary<NpcIdentity, UnityEngine.UI.Text> bangs = new Dictionary<NpcIdentity, UnityEngine.UI.Text>();
        private readonly Dictionary<NpcIdentity, float> bangUntil = new Dictionary<NpcIdentity, float>();
        private float nextRescan;
        private float nextHeart;
        private bool reportPulse;
        private int testCursor;

        private FeedbackRule F => RunDirector.Instance != null && RunDirector.Instance.IsReady ? RunDirector.Instance.Rules.feedback : null;
        private ScreenFx Fx => ScreenFx.Instance;
        private ProceduralAudio Au => ProceduralAudio.Instance;
        private PlayerEntity P1 => PlayerEntity.All.Count > 0 ? PlayerEntity.All[0] : null;

        private void OnEnable()
        {
            SuspicionEvents.OnPersonalStageChanged += OnStage;
            SuspicionEvents.OnIslandZoneChanged += OnZone;
            SuspicionEvents.OnDepartureBlockedChanged += OnBlocked;
            SuspicionEvents.OnReportAttempt += OnReportAttempt;
            ReportEvents.OnReportStarted += OnReportStarted;
            ReportEvents.OnReportCompleted += OnReportCompleted;
            ReportEvents.OnReportInterrupted += OnReportInterrupted;
            ReportEvents.OnReportAbandoned += OnReportAbandoned;
            ReportEvents.OnPhoneCut += OnPhoneCut;
            SubdueEvents.OnSubdued += OnSubdued;
            SubdueEvents.OnShoveFailed += OnShoveFailed;
            SubdueEvents.OnBodyDiscovered += OnDiscovered;
            SubdueEvents.OnUnconsciousWake += OnWake;
            EncounterEvents.OnEncounterStarted += OnEncStarted;
            EncounterEvents.OnChoiceRequested += OnChoiceReq;
            EncounterEvents.OnEncounterEnded += OnEncEnded;
            SliceEvents.OnObjectiveAdvanced += OnObjective;
            SliceEvents.OnDepartureDenied += OnDenied;
            SliceEvents.OnRunEnded += OnRunEnded;
            SliceEvents.OnRestrictedEntered += OnRestricted;
            SliceEvents.OnRunPhaseChanged += OnPhase;
        }

        private void OnDisable()
        {
            SuspicionEvents.OnPersonalStageChanged -= OnStage;
            SuspicionEvents.OnIslandZoneChanged -= OnZone;
            SuspicionEvents.OnDepartureBlockedChanged -= OnBlocked;
            SuspicionEvents.OnReportAttempt -= OnReportAttempt;
            ReportEvents.OnReportStarted -= OnReportStarted;
            ReportEvents.OnReportCompleted -= OnReportCompleted;
            ReportEvents.OnReportInterrupted -= OnReportInterrupted;
            ReportEvents.OnReportAbandoned -= OnReportAbandoned;
            ReportEvents.OnPhoneCut -= OnPhoneCut;
            SubdueEvents.OnSubdued -= OnSubdued;
            SubdueEvents.OnShoveFailed -= OnShoveFailed;
            SubdueEvents.OnBodyDiscovered -= OnDiscovered;
            SubdueEvents.OnUnconsciousWake -= OnWake;
            EncounterEvents.OnEncounterStarted -= OnEncStarted;
            EncounterEvents.OnChoiceRequested -= OnChoiceReq;
            EncounterEvents.OnEncounterEnded -= OnEncEnded;
            SliceEvents.OnObjectiveAdvanced -= OnObjective;
            SliceEvents.OnDepartureDenied -= OnDenied;
            SliceEvents.OnRunEnded -= OnRunEnded;
            SliceEvents.OnRestrictedEntered -= OnRestricted;
            SliceEvents.OnRunPhaseChanged -= OnPhase;
            foreach (var v in hooked) if (v != null) v.OnSeeingChanged -= OnSeeing;
            hooked.Clear();
            if (TimeOfDay.Instance != null) TimeOfDay.Instance.OnChanged -= OnTimeOfDay;
        }

        private void Start() { if (TimeOfDay.Instance != null) TimeOfDay.Instance.OnChanged += OnTimeOfDay; }

        private void Update()
        {
            if (F == null) return;
            if (Time.unscaledTime >= nextRescan)
            {
                nextRescan = Time.unscaledTime + 0.5f;
                foreach (var npc in NpcIdentity.All) { var v = npc.GetComponent<NpcVision>(); if (v != null && hooked.Add(v)) v.OnSeeingChanged += OnSeeing; }
                if (Time.unscaledTime >= nextHeart) { nextHeart = Time.unscaledTime + 0.5f; UpdateHeartbeat(); }
            }
            if (reportPulse && Fx != null) Fx.Pulse(new Color(1f, 0.5f, 0f), 0.6f, F.pulse.alpha * 0.6f);
            foreach (var kv in bangs)
            {
                var t = kv.Value; var npc = kv.Key;
                if (t == null) continue;
                bool show = npc != null && bangUntil.TryGetValue(npc, out float until) && Time.unscaledTime < until;
                t.enabled = show && Camera.main != null;
                if (!show) continue;
                Vector3 screen = Camera.main.WorldToScreenPoint(npc.EyePosition + Vector3.up * 1.4f);
                if (screen.z <= 0f) { t.enabled = false; continue; }
                var canvasRt = t.canvas.transform as RectTransform;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screen, null, out Vector2 local);
                t.rectTransform.anchoredPosition = local + canvasRt.rect.size * 0.5f;
            }
        }

        private void UpdateHeartbeat()
        {
            var s = SuspicionSystem.Instance; var p1 = P1;
            if (s == null || !s.IsReady || p1 == null || Au == null) return;
            int max = 0;
            foreach (var npc in NpcIdentity.All) max = Mathf.Max(max, (int)s.GetStage(npc, p1));
            float interval = HeartbeatRule.IntervalFor(max, (int)s.Island.Zone, F.heartbeat.intervalsByStage, F.heartbeat.zoneMultiplier);
            bool calm = max == 0 && s.Island.Zone == AlertZone.Calm;
            Au.SetHeartbeat(calm ? 0f : interval, F.heartbeat.volume);
        }

        private void Bang(NpcIdentity npc, string text, Color color, float seconds)
        {
            if (npc == null || RuntimeHud.Instance == null) return;
            if (!bangs.TryGetValue(npc, out var t) || t == null) { t = RuntimeHud.Instance.CreateWorldLabel(text); t.fontSize = 34; bangs[npc] = t; }
            t.text = text; t.color = color;
            bangUntil[npc] = Time.unscaledTime + seconds;
        }

        // ---------- 반응 매핑 ----------

        private void OnSeeing(NpcIdentity npc, PlayerEntity p, bool seeing)
        {
            if (p != P1) return;
            if (seeing) { Au?.Tone(1200f, 0.08f, WaveKind.Sine, 0.2f); Bang(npc, "◉", new Color(1f, 0.8f, 0.4f), 0.6f); }
        }

        private void OnStage(NpcIdentity npc, PlayerEntity p, SuspicionStage o, SuspicionStage n)
        {
            if (p != P1 || F == null) return;
            if (n > o)
            {
                switch (n)
                {
                    case SuspicionStage.Aware: Au?.Tone(1000f, 0.12f, WaveKind.Triangle, 0.35f); Bang(npc, "?", new Color(1f, 0.9f, 0.4f), 1.5f); break;
                    case SuspicionStage.Alert: Au?.Tone(300f, 0.35f, WaveKind.Sine, 0.5f); Bang(npc, "!", new Color(1f, 0.5f, 0.2f), 1.5f); Fx?.Pulse(Color.red, F.pulse.seconds, F.pulse.alpha * 0.7f); break;
                    case SuspicionStage.Certain: Au?.Alarm(); Bang(npc, "!!", Color.red, 2f); Fx?.Pulse(Color.red, F.pulse.seconds, F.pulse.alpha); break;
                }
            }
            UpdateHeartbeat();
        }

        private void OnZone(AlertZone o, AlertZone n)
        {
            if (F == null) return;
            float a = F.tint.alphaByZone[Mathf.Clamp((int)n, 0, 3)];
            if (TimeOfDay.CurrentOrDay == DayPhase.Night) a = Mathf.Max(a, F.tint.nightAlpha);
            Fx?.SetTint(n >= AlertZone.Tension ? new Color(0.25f, 0f, 0f) : Color.black, a);
            if (n > o) { Au?.Tone(220f, 0.5f, WaveKind.Sine, 0.5f); Fx?.BigText(n == AlertZone.Watch ? "섬이 경계한다" : n == AlertZone.Tension ? "섬이 긴장한다 — 감시자가 따라붙는다" : "봉쇄", F.bigText.seconds, new Color(1f, 0.6f, 0.5f)); }
            else Fx?.BigText("섬이 조금 잠잠해졌다", F.bigText.seconds * 0.8f, new Color(0.7f, 0.9f, 0.7f));
            UpdateHeartbeat();
        }

        private void OnBlocked(bool blocked)
        {
            if (F == null) return;
            if (blocked) { Fx?.BigText("배가 뜨지 않는다 — 봉쇄", F.bigText.seconds * 2f, Color.red); Au?.Alarm(); Au?.SetDrone(true); }
            else { Fx?.BigText("봉쇄가 풀렸다", F.bigText.seconds, new Color(0.7f, 1f, 0.7f)); if (TimeOfDay.CurrentOrDay != DayPhase.Night) Au?.SetDrone(false); }
        }

        private void OnReportAttempt(NpcIdentity npc, PlayerEntity p) { if (p == P1) Au?.Alarm(); }
        private void OnReportStarted(NpcIdentity npc, PlayerEntity p, ReportPoint r) { if (F == null) return; reportPulse = true; Fx?.BigText($"{npc.DisplayName}이(가) 신고하러 간다 → {r.DisplayName}", F.bigText.seconds * 1.5f, new Color(1f, 0.6f, 0.3f)); Au?.Alarm(); }
        private void OnReportCompleted(NpcIdentity npc, PlayerEntity p, ReportPoint r) { if (F == null) return; reportPulse = false; Fx?.BigText("신고됐다", F.bigText.seconds * 1.5f, Color.red); Au?.Alarm(); Fx?.Pulse(Color.red, F.pulse.seconds * 2f, F.pulse.alpha); }
        private void OnReportInterrupted(NpcIdentity npc, PlayerEntity p) { reportPulse = false; Au?.Tone(200f, 0.3f, WaveKind.Sine, 0.4f); }
        private void OnReportAbandoned(NpcIdentity npc, PlayerEntity p, string why) { if (F == null) return; reportPulse = false; Fx?.BigText("신고할 곳이 없다", F.bigText.seconds, new Color(0.7f, 0.9f, 0.7f)); Au?.Tone(520f, 0.25f, WaveKind.Triangle, 0.3f); }
        private void OnPhoneCut(ReportPoint r, PlayerEntity p) { if (F == null) return; Fx?.BigText($"{r.DisplayName} 전화선을 끊었다", F.bigText.seconds, new Color(0.7f, 0.9f, 1f)); Au?.Noise(0.25f, 0.4f); }

        private void OnSubdued(PlayerEntity a, NpcIdentity n, SubdueAction s, int w)
        {
            if (F == null) return;
            Fx?.HitStop(F.hitstop.seconds);
            a.Camera?.Shake(F.shake.amplitude, F.shake.seconds);
            Au?.Noise(0.18f, 0.6f); Au?.Tone(90f, 0.25f, WaveKind.Sine, 0.6f);
            if (w > 0) { Fx?.BigText($"목격당했다 ×{w}", F.bigText.seconds, Color.red); Fx?.Pulse(Color.red, F.pulse.seconds, F.pulse.alpha); }
        }
        private void OnShoveFailed(PlayerEntity a, NpcIdentity n) { if (F == null) return; a.Camera?.Shake(F.shake.amplitude * 0.5f, F.shake.seconds); Fx?.BigText("밀치기 실패", F.bigText.seconds * 0.7f, new Color(1f, 0.7f, 0.4f)); Au?.Tone(150f, 0.2f, WaveKind.Square, 0.35f); }
        private void OnDiscovered(NpcIdentity o, NpcIdentity b, PlayerEntity p) { if (F == null) return; Fx?.Pulse(Color.red, F.pulse.seconds, F.pulse.alpha); Fx?.BigText($"{o.DisplayName}이(가) 쓰러진 {b.DisplayName}을(를) 발견했다", F.bigText.seconds, new Color(1f, 0.6f, 0.5f)); Au?.Tone(260f, 0.4f, WaveKind.Sine, 0.5f); }
        private void OnWake(NpcIdentity n, PlayerEntity p) { if (F == null) return; Fx?.BigText($"{n.DisplayName}이(가) 깨어났다", F.bigText.seconds, new Color(1f, 0.8f, 0.5f)); Au?.Tone(400f, 0.2f, WaveKind.Triangle, 0.35f); }

        private void OnEncStarted(EncounterDef d, PlayerEntity p) { if (F == null) return; Au?.Tone(700f, 0.15f, WaveKind.Triangle, 0.3f); Fx?.BigText("…누군가 있다", F.bigText.seconds, new Color(0.9f, 0.9f, 1f)); }
        private void OnChoiceReq(EncounterDef d, PlayerEntity p) { Au?.Tone(500f, 0.3f, WaveKind.Sine, 0.4f); }
        private void OnEncEnded(EncounterDef d, PlayerEntity p, IReadOnlyList<string> f) { Au?.Tone(600f, 0.12f, WaveKind.Triangle, 0.25f); }

        private void OnObjective(ObjectiveStep s) { if (F == null) return; Au?.Chord(true); Fx?.BigText("목표 완료", F.bigText.seconds, new Color(1f, 0.95f, 0.5f)); }
        private void OnDenied(string r) { if (F == null) return; Au?.Tone(120f, 0.5f, WaveKind.Square, 0.3f); Fx?.BigText(r, F.bigText.seconds, new Color(1f, 0.7f, 0.6f)); }
        private void OnRunEnded(RunOutcome o, RunStats s) { if (F == null) return; Au?.SetHeartbeat(0f, 0f); Au?.SetDrone(false); Au?.Chord(o == RunOutcome.Success); }
        private void OnRestricted(PlayerEntity p, string z) { if (F == null || p != P1) return; Fx?.Pulse(new Color(1f, 0.3f, 0f), F.pulse.seconds, F.pulse.alpha * 0.8f); Au?.Tone(180f, 0.3f, WaveKind.Square, 0.3f); }
        private void OnPhase(RunPhase ph)
        {
            if (F == null) return;
            if (ph == RunPhase.DepartureOpen) { Fx?.BigText("배가 떠난다 — 부두로", F.bigText.seconds * 1.5f, new Color(0.8f, 1f, 0.8f)); Au?.Chord(true); }
        }
        private void OnTimeOfDay(DayPhase ph)
        {
            if (F == null) return;
            if (ph == DayPhase.Night) { Fx?.SetTint(Color.black, F.tint.nightAlpha); Au?.SetDrone(true); Fx?.BigText("밤이다 — 작업복이 통하지 않는다", F.bigText.seconds * 1.5f, new Color(0.7f, 0.7f, 1f)); Au?.Tone(110f, 0.8f, WaveKind.Sine, 0.4f); }
            else { Fx?.SetTint(Color.black, 0f); Au?.SetDrone(false); }
        }

        /// <summary>F12: 반응을 이벤트 없이 순환 재생 (SC-004 점검).</summary>
        public void TestNext()
        {
            if (F == null) return;
            var p1 = P1; var npc = NpcIdentity.All.Count > 0 ? NpcIdentity.All[0] : null;
            string[] names = { "보기 시작", "단계1", "단계2", "단계3", "구간↑", "봉쇄", "신고 시작", "신고 완료", "제압", "밀치기 실패", "발견", "밤", "인카운터", "목표", "출항 거부", "성공", "실패" };
            int i = testCursor++ % names.Length;
            Fx?.BigText($"[반응 테스트 {i + 1}/{names.Length}] {names[i]}", 1f, new Color(0.8f, 0.8f, 0.8f));
            switch (i)
            {
                case 0: if (npc != null && p1 != null) OnSeeing(npc, p1, true); break;
                case 1: if (npc != null && p1 != null) OnStage(npc, p1, SuspicionStage.Indifferent, SuspicionStage.Aware); break;
                case 2: if (npc != null && p1 != null) OnStage(npc, p1, SuspicionStage.Aware, SuspicionStage.Alert); break;
                case 3: if (npc != null && p1 != null) OnStage(npc, p1, SuspicionStage.Alert, SuspicionStage.Certain); break;
                case 4: OnZone(AlertZone.Calm, AlertZone.Tension); break;
                case 5: OnBlocked(true); break;
                case 6: if (npc != null && p1 != null && ReportPoint.All.Count > 0) OnReportStarted(npc, p1, ReportPoint.All[0]); break;
                case 7: if (npc != null && p1 != null) OnReportCompleted(npc, p1, null); break;
                case 8: if (npc != null && p1 != null) OnSubdued(p1, npc, SubdueAction.Backstab, 1); break;
                case 9: if (npc != null && p1 != null) OnShoveFailed(p1, npc); break;
                case 10: if (npc != null && p1 != null) OnDiscovered(npc, npc, p1); break;
                case 11: OnTimeOfDay(DayPhase.Night); break;
                case 12: if (p1 != null) OnEncStarted(null, p1); break;
                case 13: OnObjective(null); break;
                case 14: OnDenied("배가 뜨지 않는다 — 봉쇄 (테스트)"); break;
                case 15: Au?.Chord(true); Fx?.BigText("성공", 1f, Color.green); break;
                case 16: Au?.Chord(false); Fx?.BigText("강제 출도", 1f, Color.red); break;
            }
        }
    }
}

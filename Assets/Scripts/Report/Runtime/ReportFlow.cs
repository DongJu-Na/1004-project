using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Project1028.PlayFoundation;
using Project1028.Suspicion;
using Project1028.NpcTypes;
using Project1028.Subdue;

namespace Project1028.Report
{
    /// <summary>
    /// §5.1 NPC 하나의 신고 흐름. 사용 가능한 최근접 지점으로 걷고(최소 도보 시간 보장), 도착 후 신고 동작, 완료.
    /// 목적지가 끊기면 재타깃/포기, 기절하면 중단. 흐름 중 002 따라다니기와 003 행동은 억제한다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NpcIdentity))]
    public sealed class ReportFlow : MonoBehaviour
    {
        private NpcIdentity npc;
        private NpcMover mover;
        private NpcSuspicionProfile profile;
        private ReportRules rules;
        private readonly ReportFlowState state = new ReportFlowState();
        private readonly List<Behaviour> disabledBehaviours = new List<Behaviour>();
        private float savedSpeed;
        private bool savedSuppress;
        private Text label;
        private bool finished;

        public ReportPhase Phase => state.Phase;
        public PlayerEntity TargetPlayer { get; private set; }
        public ReportPoint TargetPoint { get; private set; }
        public NpcIdentity Npc => npc;

        public void Begin(PlayerEntity player, ReportPoint point, ReportRules r)
        {
            npc = GetComponent<NpcIdentity>();
            mover = GetComponent<NpcMover>() ?? gameObject.AddComponent<NpcMover>();
            profile = GetComponent<NpcSuspicionProfile>() ?? gameObject.AddComponent<NpcSuspicionProfile>();
            rules = r;
            TargetPlayer = player;

            savedSpeed = mover.Speed;
            savedSuppress = profile.SuppressStageBehaviour;
            profile.SuppressStageBehaviour = true; // FR-013: 신고 이동이 단계 표현을 대체
            Disable<InformerBehaviour>();
            Disable<SentinelBehaviour>();

            SetTarget(point);
            state.Start(point.Id);
        }

        private void Disable<T>() where T : Behaviour
        {
            var b = GetComponent<T>();
            if (b != null && b.enabled) { b.enabled = false; disabledBehaviours.Add(b); }
        }

        private void SetTarget(ReportPoint point)
        {
            TargetPoint = point;
            float dist = Vector3.Distance(transform.position, point.Position);
            mover.Speed = ReportSpeedRule.SpeedFor(dist, savedSpeed, rules.minWalkSeconds); // §5.1 플레이어에게 시간이 있다
            mover.MoveTo(point.Position);
            UpdateLabel();
        }

        private void Update()
        {
            if (finished || rules == null) return;
            var system = ReportSystem.Instance;

            // 004: 기절 → 중단 (깨어나면 002가 신고 시도를 다시 발행한다)
            if (UnconsciousState.IsUnconscious(npc))
            {
                state.Interrupt();
                ReportEvents.RaiseInterrupted(npc, TargetPlayer);
                RuntimeHud.Instance?.Warn($"{npc.DisplayName}: 신고 중단 (기절)");
                Finish();
                return;
            }

            // 목적지 사용 불가 → 재타깃 또는 포기 (Moving·Reporting 모두: 끊긴 전화로는 신고를 마칠 수 없다)
            if (TargetPoint == null || !TargetPoint.IsUsable)
            {
                var next = system != null ? system.PickPoint(npc) : null;
                if (next == null)
                {
                    state.Retarget(null);
                    mover.Stop();
                    ReportEvents.RaiseAbandoned(npc, TargetPlayer, "사용 가능한 신고 지점 없음");
                    RuntimeHud.Instance?.Warn($"{npc.DisplayName}: 신고 포기 — 사용 가능한 지점 없음 (§5.2 미리 끊어뒀다)");
                    Finish();
                    return;
                }
                state.Retarget(next.Id);
                SetTarget(next);
                ReportEvents.RaiseRetargeted(npc, TargetPlayer, next);
                RuntimeHud.Instance?.Warn($"{npc.DisplayName}: 목적지 변경 → {next.DisplayName}");
            }

            switch (state.Phase)
            {
                case ReportPhase.Moving:
                    mover.MoveTo(TargetPoint.Position); // 매 프레임 재발행: 002 억제 진입 Stop과의 프레임 순서 문제 방지
                    if (Vector3.Distance(transform.position, TargetPoint.Position) <= rules.arriveDistance)
                    {
                        state.Arrive(Time.time);
                        mover.Stop();
                        mover.FaceTowards(TargetPoint.Position);
                        UpdateLabel();
                        RuntimeHud.Instance?.Warn($"{npc.DisplayName}: {TargetPoint.DisplayName} 도착 — 신고 중 ({rules.reportDurationSeconds:0}초)");
                    }
                    break;
                case ReportPhase.Reporting:
                    if (state.Tick(Time.time, rules.reportDurationSeconds))
                    {
                        system?.Complete(this);
                        Finish();
                    }
                    break;
            }
        }

        private void UpdateLabel()
        {
            if (label == null && RuntimeHud.Instance != null)
            {
                label = RuntimeHud.Instance.CreateWorldLabel(string.Empty);
                label.fontSize = 22;
                label.color = new Color(1f, 0.5f, 0.5f);
            }
            if (label != null) label.text = TargetPoint != null ? $"→ {TargetPoint.DisplayName} ({ReportNames.Korean(state.Phase)})" : string.Empty;
        }

        private void LateUpdate()
        {
            if (label == null) return;
            var cam = Camera.main;
            if (cam == null || finished) { label.enabled = false; return; }
            Vector3 screen = cam.WorldToScreenPoint(npc.EyePosition + Vector3.up * 2.2f);
            if (screen.z <= 0f) { label.enabled = false; return; }
            label.enabled = true;
            var canvasRt = label.canvas.transform as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screen, null, out Vector2 local);
            label.rectTransform.anchoredPosition = local + canvasRt.rect.size * 0.5f;
        }

        /// <summary>원값 복구 후 컴포넌트 제거. 완료·포기·중단 공통.</summary>
        public void Finish()
        {
            if (finished) return;
            finished = true;
            if (mover != null) { mover.Stop(); mover.Speed = savedSpeed; }
            if (profile != null) profile.SuppressStageBehaviour = savedSuppress;
            foreach (var b in disabledBehaviours) if (b != null) b.enabled = true;
            disabledBehaviours.Clear();
            if (label != null) Destroy(label.gameObject);
            ReportSystem.Instance?.Unregister(this);
            Destroy(this);
        }
    }
}

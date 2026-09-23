using System.Collections.Generic;
using UnityEngine;
using Project1028.PlayFoundation;
using Project1028.Suspicion;
using Project1028.NpcTypes;

namespace Project1028.Subdue
{
    public struct SubdueResult
    {
        public bool Success;
        public SubdueAction Action;
        public string Reason;
    }

    /// <summary>
    /// 씬 단일. 규칙 로드, 제압 판정·적용, 기절 틱·깨어남, 기절자 발견 스캔, 대가 사건(002 표를 통해서만).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SubdueSystem : MonoBehaviour
    {
        public static SubdueSystem Instance { get; private set; }
        public bool IsReady { get; private set; }
        public SubdueRules Rules { get; private set; }
        public int PendingRuleCount { get; private set; }
        public CostLedger Ledger { get; } = new CostLedger();

        private readonly List<UnconsciousState> unconscious = new List<UnconsciousState>();
        private readonly List<string> observerBuffer = new List<string>();
        private SubdueValidationResult loadResult;

        public IEnumerable<UnconsciousState> Unconscious => unconscious;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void Start()
        {
            var suspicion = SuspicionSystem.Instance;
            if (suspicion == null || !suspicion.IsReady)
            {
                RuntimeHud.Instance?.Warn("[제압] 의심 시스템이 준비되지 않아 제압 규칙을 적용할 수 없습니다.");
                return;
            }
            if (!SubdueRulesLoader.TryLoad(suspicion.Rules, out var rules, out loadResult))
            {
                foreach (var m in loadResult.Messages) RuntimeHud.Instance?.Warn($"[제압 규칙] {m}");
                Debug.LogError("[SubdueSystem] 규칙 로드 실패. 시스템 비활성.");
                return;
            }
            foreach (var m in loadResult.Messages) RuntimeHud.Instance?.Warn($"[제압 규칙] {m}");
            Rules = rules;
            PendingRuleCount = loadResult.PendingCount;
            IsReady = true;
            RuntimeHud.Instance?.Warn($"제압 규칙 확정 대기 {PendingRuleCount}건 (StreamingAssets/Subdue/subdue_rules.json)");
        }

        private void Update()
        {
            if (!IsReady) return;
            TickUnconscious();
            ScanDiscovery();
        }

        // ---------------- 계획 (R-5) ----------------

        /// <summary>문맥 동작: 물건 들면 물건으로 기절, 빈손이고 대상이 나를 못 보며 후면이면 뒤에서 제압, 아니면 정면 밀치기.</summary>
        public SubdueAction PlanAction(PlayerEntity actor, NpcIdentity target)
        {
            var hands = actor != null ? actor.GetComponent<PlayerHands>() : null;
            if (hands != null && hands.HeldKind == HeldKind.Object) return SubdueAction.ObjectStrike;
            if (target != null && IsUnaware(target, actor) && IsBehind(target, actor)) return SubdueAction.Backstab;
            return SubdueAction.Shove;
        }

        // ---------------- 판정·적용 ----------------

        public SubdueResult TrySubdue(PlayerEntity actor, NpcIdentity target, SubdueAction action)
        {
            var result = new SubdueResult { Action = action };
            if (!IsReady || actor == null || target == null) return Reject(actor, target, action, "제압 시스템 미준비");

            // §3.2 진영 판정 — 불가 대상은 물리 반응 없이 안내만
            var verdict = SubdueJudgement.Evaluate(target);
            if (!verdict.CanSubdue) return Reject(actor, target, action, $"제압 불가: {verdict.Reason}");
            if (UnconsciousState.IsUnconscious(target)) return Reject(actor, target, action, "이미 기절한 대상");

            var rule = Rules.ForAction(action);
            if (rule == null) return Reject(actor, target, action, "동작 규칙 없음");
            if (Vector3.Distance(actor.Position, target.Position) > rule.range) return Reject(actor, target, action, "너무 멀다");

            var hands = actor.GetComponent<PlayerHands>();
            var (ok, reason) = HandRules.CanPerform(action, hands != null ? hands.State : HandState.Empty, hands != null ? hands.HeldKind : HeldKind.None);
            if (!ok) return Reject(actor, target, action, reason);

            switch (action)
            {
                case SubdueAction.Backstab:
                    if (!IsUnaware(target, actor)) return Reject(actor, target, action, "상대가 나를 인지하고 있다 (§4.1)");
                    if (!IsBehind(target, actor)) return Reject(actor, target, action, "후면이 아니다");
                    ApplySubdue(actor, target, action, rule);
                    result.Success = true;
                    return result;

                case SubdueAction.Shove:
                    EmitNoise(rule, actor, target.Position);
                    if (ShoveRoll.Succeeds(rule.failChance, Random.value))
                    {
                        ApplySubdue(actor, target, action, rule, noiseAlreadyEmitted: true);
                        result.Success = true;
                        return result;
                    }
                    // 실패: 시도당한 NPC는 확신(가정, 확정대기 사건)
                    SuspicionSystem.Instance.Raise(SuspicionEvent.Target(Rules.costs.shoveFailEventId, actor, target));
                    SubdueEvents.RaiseShoveFailed(actor, target);
                    RuntimeHud.Instance?.Warn($"{actor.Id}: 정면 밀치기 실패 — {target.DisplayName}이(가) 확신한다");
                    result.Reason = "밀치기 실패";
                    return result;

                default: // ObjectStrike: 인지 무관, 100%, 물건 유지
                    ApplySubdue(actor, target, action, rule);
                    result.Success = true;
                    return result;
            }
        }

        private SubdueResult Reject(PlayerEntity actor, NpcIdentity target, SubdueAction action, string reason)
        {
            SubdueEvents.RaiseRejected(actor, target, action, reason);
            if (actor != null) RuntimeHud.Instance?.Warn($"{actor.Id}: {SubdueNames.Korean(action)} 거부 — {reason}");
            return new SubdueResult { Success = false, Action = action, Reason = reason };
        }

        private void ApplySubdue(PlayerEntity actor, NpcIdentity target, SubdueAction action, ActionRule rule, bool noiseAlreadyEmitted = false)
        {
            // §4.4 목격자: 제압 순간 행위자를 보고 있는 다른 NPC (대상·기절자 제외)
            observerBuffer.Clear();
            foreach (var npc in NpcIdentity.All)
            {
                if (npc == target || UnconsciousState.IsUnconscious(npc)) continue;
                var vision = npc.GetComponent<NpcVision>();
                if (vision != null && vision.enabled && vision.IsSeeing(actor)) observerBuffer.Add(npc.NpcId);
            }
            Ledger.RecordWitnesses(target.NpcId, observerBuffer);

            var timer = new UnconsciousTimer(Rules.unconscious.minSeconds, Rules.unconscious.maxSeconds, Random.value);
            var state = UnconsciousState.Attach(target, actor, timer);
            foreach (var id in observerBuffer) state.WitnessIds.Add(id);
            unconscious.Add(state);

            if (!noiseAlreadyEmitted) EmitNoise(rule, actor, target.Position);
            if (observerBuffer.Count > 0)
            {
                SuspicionSystem.Instance.Raise(SuspicionEvent.At(Rules.costs.witnessedEventId, actor, target.Position)); // 섬 +40, 1회
                RuntimeHud.Instance?.Warn($"★ 제압 장면을 {observerBuffer.Count}명이 목격 — 섬 의심도 상승 (§4.4)");
            }

            SubdueEvents.RaiseSubdued(actor, target, action, observerBuffer.Count);
            RuntimeHud.Instance?.Warn($"{actor.Id}: {SubdueNames.Korean(action)} 성공 → {target.DisplayName} 기절 ({timer.Duration:0}초)");
        }

        private void EmitNoise(ActionRule rule, PlayerEntity actor, Vector3 position)
        {
            System.Enum.TryParse<NoiseLevel>(rule.noiseLevel, false, out var level);
            SuspicionSystem.Instance.Raise(SuspicionEvent.At(rule.noiseEventId, actor, position));
            SubdueEvents.RaiseNoise(level, position, actor);
            RuntimeHud.Instance?.Warn($"소음 {SubdueNames.Korean(level)} ({rule.noiseEventId})");
        }

        private static bool IsUnaware(NpcIdentity target, PlayerEntity actor)
        {
            var vision = target.GetComponent<NpcVision>();
            return vision == null || !vision.enabled || !vision.IsSeeing(actor);
        }

        private bool IsBehind(NpcIdentity target, PlayerEntity actor) =>
            BackConeCheck.IsBehind(target.Position, target.Forward, actor.Position, Rules.actions.backstab.backConeDegrees);

        // ---------------- 기절 틱 (§4.3) ----------------

        private void TickUnconscious()
        {
            for (int i = unconscious.Count - 1; i >= 0; i--)
            {
                var s = unconscious[i];
                if (s == null) { unconscious.RemoveAt(i); continue; }
                s.Timer.Tick(Time.deltaTime); // 운반 중에도 흐른다 (FR-014)
                if (!s.Timer.IsAwake) continue;

                var npc = s.Npc;
                var subduer = s.SubduedBy;
                var suspicion = SuspicionSystem.Instance;
                bool alreadyCertain = suspicion.GetStage(npc, subduer) == SuspicionStage.Certain;
                suspicion.Raise(SuspicionEvent.Target(Rules.costs.wakeEventId, subduer, npc)); // 개인 3 (§4.4)
                if (alreadyCertain) suspicion.ForceReportAttempt(npc, subduer);               // 이미 3이면 신고 시도 재발행

                Ledger.Clear(npc.NpcId);
                unconscious.RemoveAt(i);
                s.Wake();
                RuntimeHud.Instance?.Warn($"{npc.DisplayName} 깨어남 → {subduer.Id}에 대해 확신, 신고 시도 (§4.3)");
            }
        }

        // ---------------- 발견 스캔 (§4.4) ----------------

        private void ScanDiscovery()
        {
            foreach (var body in unconscious)
            {
                if (body == null) continue;
                Vector3 bodyCenter = body.transform.position + Vector3.up * 0.3f;
                foreach (var observer in NpcIdentity.All)
                {
                    if (observer == body.Npc || UnconsciousState.IsUnconscious(observer)) continue;
                    var vision = observer.GetComponent<NpcVision>();
                    if (vision == null || !vision.enabled) continue;

                    var geo = VisionEvaluator.Evaluate(observer.EyePosition, observer.Forward, bodyCenter, vision.FovDegrees, vision.Range);
                    if (!geo.InCone) continue;
                    if (Physics.Linecast(observer.EyePosition, bodyCenter, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore))
                    {
                        if (!hit.transform.IsChildOf(body.transform) && !hit.transform.IsChildOf(observer.transform)
                            && !(body.CarriedBy != null && hit.transform.IsChildOf(body.CarriedBy.transform))) continue;
                    }

                    if (!Ledger.TryRecordDiscovery(body.Npc.NpcId, observer.NpcId)) continue; // 목격자·중복 제외
                    var suspicion = SuspicionSystem.Instance;
                    suspicion.Raise(SuspicionEvent.At(Rules.costs.foundIslandEventId, body.SubduedBy, body.transform.position));     // 섬 +30
                    suspicion.Raise(SuspicionEvent.Target(Rules.costs.foundPersonalEventId, body.SubduedBy, observer));               // 개인 +3 (확정대기)
                    SubdueEvents.RaiseDiscovered(observer, body.Npc, body.SubduedBy);
                    RuntimeHud.Instance?.Warn($"★ {observer.DisplayName}이(가) 기절한 {body.Npc.DisplayName}을(를) 발견 — 섬 의심도 상승 (§4.4)");
                }
            }
        }
    }
}

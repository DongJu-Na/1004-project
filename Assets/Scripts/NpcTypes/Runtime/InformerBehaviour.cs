using System.Collections.Generic;
using UnityEngine;
using Project1028.PlayFoundation;
using Project1028.Suspicion;

namespace Project1028.NpcTypes
{
    /// <summary>
    /// §3.1 밀고자: 개인 의심 2 이상이면 지연 후 가장 가까운 관리자에게 걸어가 전달(관리자 +1). 관리자가 없으면 보류.
    /// 겉모습 표현(쳐다보기·따라가기)은 002 플래그로 억제되어 있다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NpcIdentity))]
    [RequireComponent(typeof(NpcMover))]
    public sealed class InformerBehaviour : MonoBehaviour
    {
        private NpcIdentity identity;
        private NpcMover mover;
        private readonly Dictionary<PlayerEntity, InformerDeliveryState> states = new Dictionary<PlayerEntity, InformerDeliveryState>();
        private PlayerEntity active;
        private NpcIdentity targetManager;

        public DeliveryPhase ActivePhase => active != null && states.TryGetValue(active, out var s) ? s.Phase : DeliveryPhase.Idle;
        public PlayerEntity ActivePlayer => active;

        private void Awake()
        {
            identity = GetComponent<NpcIdentity>();
            mover = GetComponent<NpcMover>();
        }

        private void Update()
        {
            var suspicion = SuspicionSystem.Instance;
            var types = NpcTypeSystem.Instance;
            if (suspicion == null || !suspicion.IsReady || types == null || !types.IsReady) return;
            var rule = types.Rules.rules.informer;
            float now = Time.time;

            // 트리거/리셋
            foreach (var p in PlayerEntity.All)
            {
                if (!states.TryGetValue(p, out var st)) { st = new InformerDeliveryState(); states[p] = st; }
                int v = suspicion.GetPersonal(identity, p).Value;
                if (v >= (int)SuspicionStage.Alert) { if (st.Trigger(now) && active == null) active = p; }
                else if (st.Phase != DeliveryPhase.Idle) { st.Reset(); if (active == p) { active = null; targetManager = null; mover.Stop(); } }
            }
            if (active == null)
            {
                foreach (var kv in states) if (kv.Value.Phase == DeliveryPhase.Waiting) { active = kv.Key; break; }
                if (active == null) return;
            }

            var state = states[active];
            var manager = targetManager != null ? targetManager : FindNearestManager();
            bool managerAvailable = manager != null;

            var transition = state.Tick(now, rule.deliveryDelaySeconds, managerAvailable);
            if (transition.HasValue)
            {
                switch (transition.Value)
                {
                    case DeliveryPhase.Moving:
                        targetManager = manager;
                        mover.MoveTo(manager.Position);
                        NpcTypeEvents.RaiseDeliveryStarted(identity, manager, active);
                        RuntimeHud.Instance?.Warn($"{identity.DisplayName}: 관리자 {manager.DisplayName}에게 전달 이동 ({active.Id})");
                        break;
                    case DeliveryPhase.Held:
                        targetManager = null;
                        mover.Stop();
                        NpcTypeEvents.RaiseHeld(identity, active);
                        RuntimeHud.Instance?.Warn($"{identity.DisplayName}: 관리자 없음 → 전달 보류 ({active.Id})");
                        break;
                }
            }

            if (state.Phase == DeliveryPhase.Moving && targetManager != null)
            {
                if (Vector3.Distance(transform.position, targetManager.Position) <= rule.arriveDistance)
                {
                    state.Arrive();
                    mover.Stop();
                    suspicion.Raise(SuspicionEvent.Target(rule.deliveryEventId, active, targetManager));
                    NpcTypeEvents.RaiseDeliveryCompleted(identity, targetManager, active);
                    RuntimeHud.Instance?.Warn($"{identity.DisplayName}: 전달 완료 → {targetManager.DisplayName} +1 ({active.Id})");
                    targetManager = null;
                    active = null; // 다음 Waiting 플레이어가 있으면 이어서 처리
                }
                else
                {
                    mover.MoveTo(targetManager.Position); // 관리자가 움직였을 수 있음
                }
            }
        }

        private NpcIdentity FindNearestManager()
        {
            NpcIdentity best = null;
            float bestDist = float.MaxValue;
            foreach (var npc in NpcIdentity.All)
            {
                if (npc == identity) continue; // 자기 자신 제외
                var p = NpcTypeProfile.Get(npc);
                if (p == null || !p.IsManager) continue;
                float d = Vector3.Distance(transform.position, npc.Position);
                if (d < bestDist) { bestDist = d; best = npc; }
            }
            return best;
        }
    }
}

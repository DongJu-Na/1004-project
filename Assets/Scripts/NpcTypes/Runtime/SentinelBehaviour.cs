using UnityEngine;
using Project1028.PlayFoundation;
using Project1028.Suspicion;

namespace Project1028.NpcTypes
{
    /// <summary>§3 경계자(개·거위·아이): 감지 반경 안 플레이어 → 소리 → 소리 반경 안 NPC가 그쪽을 본다 + noise 사건. 자신은 의심을 쌓지 않는다.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NpcIdentity))]
    public sealed class SentinelBehaviour : MonoBehaviour
    {
        private NpcIdentity identity;
        private SentinelAlarmGate gate;

        private void Awake() => identity = GetComponent<NpcIdentity>();

        private void Update()
        {
            var types = NpcTypeSystem.Instance;
            var suspicion = SuspicionSystem.Instance;
            if (types == null || !types.IsReady || suspicion == null) return;
            var rule = types.Rules.rules.sentinel;
            if (gate == null) gate = new SentinelAlarmGate(rule.cooldownSeconds);

            PlayerEntity nearest = null;
            float nearestDist = float.MaxValue;
            foreach (var p in PlayerEntity.All)
            {
                float d = Vector3.Distance(transform.position, p.Position);
                if (d < nearestDist) { nearestDist = d; nearest = p; }
            }
            bool inRadius = nearest != null && nearestDist <= rule.detectRadius;
            if (!gate.TryAlarm(inRadius, Time.time)) return;

            int turned = 0;
            foreach (var npc in NpcIdentity.All)
            {
                if (npc == identity) continue;
                if (Vector3.Distance(npc.Position, transform.position) > rule.soundRadius) continue;
                var mover = npc.GetComponent<NpcMover>();
                if (mover != null) mover.FaceTowards(nearest.Position);
                else
                {
                    Vector3 dir = nearest.Position - npc.Position; dir.y = 0f;
                    if (dir.sqrMagnitude > 1e-4f) npc.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
                }
                turned++;
            }

            suspicion.Raise(SuspicionEvent.At(rule.noiseEventId, nearest, transform.position));
            NpcTypeEvents.RaiseSentinelAlarm(identity, transform.position, turned);
            RuntimeHud.Instance?.Warn($"{identity.DisplayName}: 경계자 소리! 주변 NPC {turned}명이 {nearest.Id} 쪽을 본다");
        }
    }
}

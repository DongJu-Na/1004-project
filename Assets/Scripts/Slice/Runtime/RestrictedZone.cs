using System.Collections.Generic;
using UnityEngine;
using Project1028.PlayFoundation;
using Project1028.Suspicion;

namespace Project1028.Slice
{
    /// <summary>제한 구역(§2.3 확정대기). 진입 시 002 사건 발생 — 시야 조건은 002 표(witness·requiresSight)가 처리한다.</summary>
    [DisallowMultipleComponent]
    public sealed class RestrictedZone : MonoBehaviour
    {
        [SerializeField] private string zoneId = "zone";
        [SerializeField] private Vector3 size = new Vector3(10f, 3f, 10f);

        private readonly HashSet<PlayerEntity> inside = new HashSet<PlayerEntity>();
        private readonly Dictionary<PlayerEntity, float> cooldown = new Dictionary<PlayerEntity, float>();

        public void Configure(string id, Vector3 s) { zoneId = id; size = s; }

        private bool Contains(Vector3 p)
        {
            Vector3 l = transform.InverseTransformPoint(p);
            return Mathf.Abs(l.x) <= size.x * 0.5f && Mathf.Abs(l.y) <= size.y * 0.5f + 1f && Mathf.Abs(l.z) <= size.z * 0.5f;
        }

        private void Update()
        {
            var d = RunDirector.Instance;
            if (d == null || !d.IsReady) return;
            foreach (var p in PlayerEntity.All)
            {
                bool now = Contains(p.Position);
                bool was = inside.Contains(p);
                if (now && !was)
                {
                    inside.Add(p);
                    if (!cooldown.TryGetValue(p, out float until) || Time.time >= until)
                    {
                        cooldown[p] = Time.time + d.Rules.restricted.cooldownSeconds;
                        SuspicionSystem.Instance?.Raise(SuspicionEvent.Witness(d.Rules.restricted.eventId, p));
                        SliceEvents.RaiseRestricted(p, zoneId);
                        RuntimeHud.Instance?.Warn($"{p.Id}: 제한 구역 진입 ({zoneId}) — 보고 있는 사람이 있으면 의심이 오른다");
                    }
                }
                else if (!now && was) inside.Remove(p);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
    }
}

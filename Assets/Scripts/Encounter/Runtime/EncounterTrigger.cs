using System.Collections.Generic;
using UnityEngine;
using Project1028.PlayFoundation;

namespace Project1028.Encounter
{
    /// <summary>씬 배치 트리거(반경). 플레이어가 진입하는 순간 1회 판정을 요청한다. 이탈 후 재진입해야 다시 판정.</summary>
    [DisallowMultipleComponent]
    public sealed class EncounterTrigger : MonoBehaviour
    {
        [SerializeField] private string triggerId = "trigger";
        [SerializeField, Min(0.5f)] private float radius = 3f;

        private readonly HashSet<PlayerEntity> inside = new HashSet<PlayerEntity>();

        public string TriggerId => triggerId;
        public float Radius => radius;

        public void Configure(string id, float r) { triggerId = id; radius = Mathf.Max(0.5f, r); }

        private void Update()
        {
            var system = EncounterSystem.Instance;
            foreach (var p in PlayerEntity.All)
            {
                bool now = Vector3.Distance(p.Position, transform.position) <= radius;
                bool was = inside.Contains(p);
                if (now && !was) { inside.Add(p); system?.TryTrigger(p, this); }
                else if (!now && was) inside.Remove(p);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.6f);
            const int seg = 32;
            Vector3 prev = transform.position + new Vector3(radius, 0.05f, 0f);
            for (int i = 1; i <= seg; i++)
            {
                float a = i / (float)seg * Mathf.PI * 2f;
                Vector3 p = transform.position + new Vector3(Mathf.Cos(a) * radius, 0.05f, Mathf.Sin(a) * radius);
                Gizmos.DrawLine(prev, p); prev = p;
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using Project1028.PlayFoundation;
using Project1028.Suspicion;
using Project1028.NpcTypes;

namespace Project1028.Encounter
{
    /// <summary>§6.1 "기존 에셋만": 인카운터 NPC를 프리미티브로 런타임 스폰·회수한다. 유형은 동조자(일반 주민)·Victim으로 조용히 부여.</summary>
    public static class EncounterNpcSpawner
    {
        public static List<NpcIdentity> Spawn(EncounterDef def, Vector3 anchor, Quaternion facingBase)
        {
            var list = new List<NpcIdentity>();
            if (def?.npcs == null) return list;
            for (int i = 0; i < def.npcs.Length; i++)
            {
                var n = def.npcs[i];
                Vector3 offset = n.offset != null ? new Vector3(n.offset.x, n.offset.y, n.offset.z) : Vector3.zero;
                Vector3 pos = anchor + facingBase * offset;
                pos.y = 1f;
                if (!TryFindFreeSpot(ref pos)) { Debug.LogWarning($"[Encounter] {def.id} NPC {i} 등장 위치가 막혀 건너뜀"); continue; }

                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = $"Enc_{def.id}_{i}";
                go.transform.position = pos;
                Vector3 facing = n.facing != null ? new Vector3(n.facing.x, 0f, n.facing.z) : Vector3.back;
                if (facing.sqrMagnitude < 1e-4f) facing = Vector3.back;
                go.transform.rotation = Quaternion.LookRotation(facing.normalized, Vector3.up);
                var tint = go.AddComponent<PrimitiveTint>();
                tint.SetColor(new Color(0.85f, 0.75f, 0.5f));

                var id = go.AddComponent<NpcIdentity>();
                id.Configure($"{def.id}_{i}", n.displayName ?? $"NPC {i + 1}", 0.6f);
                var vision = go.AddComponent<NpcVision>();
                vision.Configure(100f, 10f, 0.15f);
                go.AddComponent<VisionDebugLabel>();
                go.AddComponent<NpcSuspicionProfile>();
                var profile = go.AddComponent<NpcTypeProfile>();
                profile.Configure(NpcType.Sympathizer, Faction.Victim, new string[0]);
                list.Add(id);
            }
            return list;
        }

        private static bool TryFindFreeSpot(ref Vector3 pos)
        {
            foreach (var dx in new[] { 0f, 1f, -1f, 2f, -2f })
            {
                var p = pos + Vector3.right * dx;
                if (!Physics.CheckCapsule(p + Vector3.up * 0.5f, p - Vector3.up * 0.5f, 0.4f, ~0, QueryTriggerInteraction.Ignore)) { pos = p; return true; }
            }
            return false;
        }

        public static void Despawn(List<NpcIdentity> npcs, float delaySeconds)
        {
            if (npcs == null) return;
            foreach (var n in npcs) if (n != null) Object.Destroy(n.gameObject, Mathf.Max(0f, delaySeconds));
            npcs.Clear();
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Project1028.PlayFoundation
{
    /// <summary>
    /// NPC 뼈대: 식별자·표시 이름·위치·정면·눈 위치. 이 기능에서 NPC는 이동하지 않는다 (PrototypePlan 범위).
    /// 유형·진영·의심 등 게임 규칙 필드는 두지 않는다 (헌장 원칙 I).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NpcIdentity : MonoBehaviour
    {
        private static readonly List<NpcIdentity> all = new List<NpcIdentity>();
        public static IReadOnlyList<NpcIdentity> All => all;

        [SerializeField] private string npcId = "npc_dock_worker";
        [SerializeField] private string displayName = "부두 노동자";
        [Tooltip("pivot 기준 눈 높이. 프리미티브 Capsule(pivot=중심, 높이 2)은 0.6 → 월드 1.6m.")]
        [SerializeField, Min(0.01f)] private float eyeHeight = 0.6f;

        public string NpcId => npcId;
        public string DisplayName => displayName;
        public float EyeHeight => eyeHeight;
        public Vector3 Position => transform.position;
        public Vector3 Forward => transform.forward;
        public Vector3 EyePosition => transform.position + Vector3.up * eyeHeight;

        public void Configure(string newNpcId, string newDisplayName, float newEyeHeight)
        {
            npcId = newNpcId;
            displayName = newDisplayName;
            eyeHeight = Mathf.Max(0.01f, newEyeHeight);
        }

        private void Awake()
        {
            if (string.IsNullOrEmpty(npcId))
            {
                Debug.LogError($"[NpcIdentity] NpcId가 비어 있습니다: {name}", this);
            }
        }

        private void OnEnable() => all.Add(this);
        private void OnDisable() => all.Remove(this);
    }
}

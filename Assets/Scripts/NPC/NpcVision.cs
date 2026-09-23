using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project1028.PlayFoundation
{
    /// <summary>
    /// 플레이어별 "보고 있는가" 판정 (FR-016): 부채꼴·거리(VisionEvaluator) + 가림(Linecast) + 안정 시간(FR-017).
    /// 이 컴포넌트는 NPC의 위치·방향·행동을 절대 바꾸지 않는다 (FR-018). 결과는 읽기·이벤트로만 노출된다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NpcIdentity))]
    public sealed class NpcVision : MonoBehaviour
    {
        private sealed class VisionState
        {
            public bool RawSeeing;
            public bool PublicSeeing;
            public float RawSince;
        }

        [SerializeField, Range(1f, 180f)] private float fovDegrees = 100f;
        [SerializeField, Min(0.1f)] private float range = 10f;
        [SerializeField] private LayerMask occlusionMask = ~0;
        [SerializeField, Min(0f)] private float stableSeconds = 0.15f;

        private NpcIdentity identity;
        private readonly Dictionary<PlayerEntity, VisionState> states = new Dictionary<PlayerEntity, VisionState>();
        private readonly List<PlayerEntity> seen = new List<PlayerEntity>();

        public float FovDegrees => fovDegrees;
        public float Range => range;
        public float StableSeconds => stableSeconds;
        public NpcIdentity Identity => identity;

        /// <summary>npc, player, seeing(true=보기 시작, false=놓침). 안정 시간 경과 후에만 발행.</summary>
        public event Action<NpcIdentity, PlayerEntity, bool> OnSeeingChanged;

        public void Configure(float fov, float newRange, float newStableSeconds)
        {
            fovDegrees = Mathf.Clamp(fov, 1f, 180f);
            range = Mathf.Max(0.1f, newRange);
            stableSeconds = Mathf.Max(0f, newStableSeconds);
        }

        private void Awake() => identity = GetComponent<NpcIdentity>();

        private void OnEnable()
        {
            PlayerEntity.OnRegistered += HandleRegistered;
            PlayerEntity.OnUnregistered += HandleUnregistered;
            foreach (var p in PlayerEntity.All) HandleRegistered(p);
        }

        private void OnDisable()
        {
            PlayerEntity.OnRegistered -= HandleRegistered;
            PlayerEntity.OnUnregistered -= HandleUnregistered;
            states.Clear();
            seen.Clear();
        }

        private void HandleRegistered(PlayerEntity player)
        {
            if (player != null && !states.ContainsKey(player))
            {
                states[player] = new VisionState { RawSince = Time.time };
            }
        }

        private void HandleUnregistered(PlayerEntity player)
        {
            if (player == null) return;
            states.Remove(player);
            seen.Remove(player);
        }

        private void Update()
        {
            float now = Time.time;
            foreach (var kv in states)
            {
                var player = kv.Key;
                var state = kv.Value;
                if (player == null) continue;

                bool raw = ComputeRaw(player);
                if (raw != state.RawSeeing)
                {
                    state.RawSeeing = raw;
                    state.RawSince = now;
                }

                if (state.RawSeeing != state.PublicSeeing && now - state.RawSince >= stableSeconds)
                {
                    state.PublicSeeing = state.RawSeeing;
                    if (state.PublicSeeing) { if (!seen.Contains(player)) seen.Add(player); }
                    else seen.Remove(player);
                    OnSeeingChanged?.Invoke(identity, player, state.PublicSeeing);
                }
            }
        }

        private bool ComputeRaw(PlayerEntity player)
        {
            Vector3 eye = identity.EyePosition;
            Vector3 target = player.CenterPosition;
            var geo = VisionEvaluator.Evaluate(eye, identity.Forward, target, fovDegrees, range);
            if (!geo.InCone) return false;

            // 가림: 눈 → 대상 중심. 플레이어 자신의 콜라이더 또는 NPC 자신은 가림으로 치지 않는다.
            if (Physics.Linecast(eye, target, out RaycastHit hit, occlusionMask, QueryTriggerInteraction.Ignore))
            {
                Transform t = hit.transform;
                if (t.IsChildOf(player.transform) || t.IsChildOf(transform)) return true;
                return false;
            }
            return true;
        }

        public bool IsSeeing(PlayerEntity player) => player != null && states.TryGetValue(player, out var s) && s.PublicSeeing;
        public bool IsSeeingAny => seen.Count > 0;
        public IEnumerable<PlayerEntity> SeenPlayers => seen;

        // T030: 시야 부채꼴·거리 Gizmo (FR-019)
        private void OnDrawGizmosSelected()
        {
            var id = identity != null ? identity : GetComponent<NpcIdentity>();
            if (id == null) return;

            Vector3 eye = id.EyePosition;
            Vector3 fwd = new Vector3(id.Forward.x, 0f, id.Forward.z).normalized;
            if (fwd.sqrMagnitude < 1e-6f) fwd = Vector3.forward;

            Gizmos.color = Application.isPlaying && IsSeeingAny ? Color.red : Color.yellow;
            float half = fovDegrees * 0.5f;
            Vector3 left = Quaternion.Euler(0f, -half, 0f) * fwd;
            Vector3 right = Quaternion.Euler(0f, half, 0f) * fwd;
            Gizmos.DrawLine(eye, eye + left * range);
            Gizmos.DrawLine(eye, eye + right * range);

            const int segments = 24;
            Vector3 prev = eye + left * range;
            for (int i = 1; i <= segments; i++)
            {
                float a = -half + (fovDegrees * i / segments);
                Vector3 p = eye + (Quaternion.Euler(0f, a, 0f) * fwd) * range;
                Gizmos.DrawLine(prev, p);
                prev = p;
            }
        }
    }
}

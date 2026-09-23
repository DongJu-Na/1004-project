using System.Collections.Generic;
using UnityEngine;

namespace Project1028.PlayFoundation
{
    /// <summary>
    /// 순수 로직: 후보 중 하나를 고른다. 규칙 — 수평 거리 ≤ Range인 후보만, 카메라 전방(수평)과의 각도 최소,
    /// 동률(±1°)이면 거리 최소. UnityEngine 의존은 Vector3/Mathf만 (EditMode 테스트 대상).
    /// </summary>
    public static class InteractableSelector
    {
        public const float AngleTieToleranceDeg = 1f;

        public struct Candidate
        {
            public Vector3 Position;
            public float Range;
            public object Payload;

            public Candidate(Vector3 position, float range, object payload)
            {
                Position = position;
                Range = range;
                Payload = payload;
            }
        }

        public static Candidate? Pick(IReadOnlyList<Candidate> candidates, Vector3 playerPos, Vector3 cameraForward)
        {
            if (candidates == null || candidates.Count == 0) return null;

            Vector3 forward = Flatten(cameraForward);
            if (forward.sqrMagnitude < 1e-6f) forward = Vector3.forward;

            Candidate? best = null;
            float bestAngle = float.MaxValue;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                var c = candidates[i];
                Vector3 toTarget = Flatten(c.Position - playerPos);
                float dist = toTarget.magnitude;
                if (dist > c.Range) continue;

                float angle = dist < 1e-4f ? 0f : Vector3.Angle(forward, toTarget);

                bool better;
                if (best == null) better = true;
                else if (Mathf.Abs(angle - bestAngle) <= AngleTieToleranceDeg) better = dist < bestDistance;
                else better = angle < bestAngle;

                if (better)
                {
                    best = c;
                    bestAngle = angle;
                    bestDistance = dist;
                }
            }
            return best;
        }

        private static Vector3 Flatten(Vector3 v) => new Vector3(v.x, 0f, v.z);
    }
}

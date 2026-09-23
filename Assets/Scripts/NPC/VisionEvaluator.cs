using UnityEngine;

namespace Project1028.PlayFoundation
{
    public struct VisionGeometryResult
    {
        public bool InRange;
        public bool InFov;
        public float HorizontalAngleDeg;
        public float Distance;
        public bool InCone => InRange && InFov;
    }

    /// <summary>
    /// 순수 로직: 눈 위치·정면·대상으로 부채꼴(수평 각도)·거리 판정. 가림 판정은 NpcVision이 Physics로 덧붙인다.
    /// 수직 성분은 무시한다 — 둔덕 위 플레이어도 정면이면 본다 (EditMode 테스트 대상).
    /// </summary>
    public static class VisionEvaluator
    {
        public static VisionGeometryResult Evaluate(Vector3 eyePos, Vector3 forward, Vector3 targetPos, float fovDeg, float range)
        {
            var result = new VisionGeometryResult();

            Vector3 toTarget = targetPos - eyePos;
            result.Distance = toTarget.magnitude;
            result.InRange = result.Distance <= range;

            Vector3 flatForward = new Vector3(forward.x, 0f, forward.z);
            Vector3 flatToTarget = new Vector3(toTarget.x, 0f, toTarget.z);

            if (flatForward.sqrMagnitude < 1e-8f)
            {
                // 정면이 수직이면 수평 판정 불가 → 각도 0으로 취급하되 시야 밖.
                result.HorizontalAngleDeg = 0f;
                result.InFov = false;
                return result;
            }

            if (flatToTarget.sqrMagnitude < 1e-8f)
            {
                // 바로 위/아래(수평 거리 0)는 정면으로 본다.
                result.HorizontalAngleDeg = 0f;
                result.InFov = true;
                return result;
            }

            result.HorizontalAngleDeg = Vector3.Angle(flatForward, flatToTarget);
            result.InFov = result.HorizontalAngleDeg <= fovDeg * 0.5f;
            return result;
        }
    }
}

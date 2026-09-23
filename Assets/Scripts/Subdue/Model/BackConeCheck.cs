using UnityEngine;

namespace Project1028.Subdue
{
    /// <summary>행위자가 대상의 후면 부채꼴 안에 있는가. 수평 각도만 사용.</summary>
    public static class BackConeCheck
    {
        public static bool IsBehind(Vector3 targetPos, Vector3 targetForward, Vector3 actorPos, float coneDegrees)
        {
            Vector3 back = new Vector3(-targetForward.x, 0f, -targetForward.z);
            Vector3 toActor = actorPos - targetPos;
            toActor.y = 0f;
            if (back.sqrMagnitude < 1e-8f || toActor.sqrMagnitude < 1e-8f) return false;
            return Vector3.Angle(back, toActor) <= coneDegrees * 0.5f;
        }
    }
}

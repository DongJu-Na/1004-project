using UnityEngine;

namespace Project1028.Vehicle
{
    /// <summary>정지 또는 저속에서만 하차. 후진은 절댓값. 순수 로직.</summary>
    public static class ExitRule
    {
        public static bool CanExit(float speed, float maxExitSpeed) => Mathf.Abs(speed) <= maxExitSpeed;
    }
}

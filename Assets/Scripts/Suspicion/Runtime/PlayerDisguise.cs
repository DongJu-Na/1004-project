using UnityEngine;
using Project1028.PlayFoundation;

namespace Project1028.Suspicion
{
    /// <summary>§7 "위장 유효" 외부 입력. 효과량은 확정 대기(v0.6 §3.1)이므로 값 노출만. 밤에는 무효.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerEntity))]
    public sealed class PlayerDisguise : MonoBehaviour
    {
        [SerializeField] private bool isDisguised;

        public bool IsDisguised { get => isDisguised; set => isDisguised = value; }
        public bool IsEffective => isDisguised && TimeOfDay.CurrentOrDay == DayPhase.Day;
    }
}

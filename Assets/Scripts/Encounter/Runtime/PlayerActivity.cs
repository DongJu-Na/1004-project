using UnityEngine;
using Project1028.PlayFoundation;

namespace Project1028.Encounter
{
    /// <summary>§6.4 "조사·정찰 중에는 뜨지 않는다" — 외부 불리언 입력(증거 시스템 후속).</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerEntity))]
    public sealed class PlayerActivity : MonoBehaviour
    {
        [SerializeField] private bool isScouting;
        public bool IsScouting { get => isScouting; set => isScouting = value; }
    }
}

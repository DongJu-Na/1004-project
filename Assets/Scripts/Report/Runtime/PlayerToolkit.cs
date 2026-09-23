using UnityEngine;
using Project1028.PlayFoundation;

namespace Project1028.Report
{
    /// <summary>"도구 보유" 외부 입력. 도구 5종(v0.6 §3.1)은 확정대기이므로 불리언만.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerEntity))]
    public sealed class PlayerToolkit : MonoBehaviour
    {
        [SerializeField] private bool hasCuttingTool;
        public bool HasCuttingTool { get => hasCuttingTool; set => hasCuttingTool = value; }
    }
}

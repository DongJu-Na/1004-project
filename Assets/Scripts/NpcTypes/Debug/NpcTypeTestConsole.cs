using UnityEngine;
using UnityEngine.InputSystem;
using Project1028.PlayFoundation;

namespace Project1028.NpcTypes
{
    /// <summary>키: I 조사(P1), L 라벨 토글. 002 콘솔 키와 함께 쓴다.</summary>
    public sealed class NpcTypeTestConsole : MonoBehaviour
    {
        private void Update()
        {
            var kb = Keyboard.current;
            var types = NpcTypeSystem.Instance;
            if (kb == null || types == null) return;

            if (kb.iKey.wasPressedThisFrame)
            {
                var actor = PlayerEntity.All.Count > 0 ? PlayerEntity.All[0] : null;
                int n = actor != null ? types.Investigate(actor) : 0;
                RuntimeHud.Instance?.Warn($"조사(I): {n}명의 NPC에 통지");
            }
            if (kb.lKey.wasPressedThisFrame)
            {
                types.LabelsVisible = !types.LabelsVisible;
                RuntimeHud.Instance?.Warn(types.LabelsVisible ? "유형 라벨 표시" : "유형 라벨 숨김 (밀고자 식별 검증 모드)");
            }
        }
    }
}

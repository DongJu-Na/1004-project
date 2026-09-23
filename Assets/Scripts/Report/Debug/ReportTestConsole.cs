using UnityEngine;
using UnityEngine.InputSystem;
using Project1028.PlayFoundation;

namespace Project1028.Report
{
    /// <summary>키: K 도구 토글(P1), O 가장 가까운 사용 가능 전화기 즉시 끊기(디버그, 사전 차단 시나리오용).</summary>
    public sealed class ReportTestConsole : MonoBehaviour
    {
        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            var p1 = PlayerEntity.All.Count > 0 ? PlayerEntity.All[0] : null;

            if (kb.kKey.wasPressedThisFrame && p1 != null)
            {
                var kit = p1.GetComponent<PlayerToolkit>();
                if (kit != null) { kit.HasCuttingTool = !kit.HasCuttingTool; RuntimeHud.Instance?.Warn($"도구 보유: {(kit.HasCuttingTool ? "예" : "아니오")}"); }
            }
            if (kb.oKey.wasPressedThisFrame && p1 != null)
            {
                ReportPoint best = null; float bestD = float.MaxValue;
                foreach (var p in ReportPoint.All)
                {
                    if (!p.CanBeCut || !p.IsUsable) continue;
                    float d = Vector3.Distance(p.Position, p1.Position);
                    if (d < bestD) { bestD = d; best = p; }
                }
                if (best != null) { best.SetUsable(false); RuntimeHud.Instance?.Warn($"(디버그) {best.DisplayName} 즉시 끊음"); }
                else RuntimeHud.Instance?.Warn("(디버그) 끊을 전화기가 없다");
            }
        }
    }
}

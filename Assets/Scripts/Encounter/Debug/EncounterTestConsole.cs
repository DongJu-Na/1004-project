using UnityEngine;
using UnityEngine.InputSystem;
using Project1028.PlayFoundation;

namespace Project1028.Encounter
{
    /// <summary>키: X 다음 샘플 강제 발생, 1/2 C 선택, S P1 정찰 토글, Z 100런 시뮬레이션.</summary>
    public sealed class EncounterTestConsole : MonoBehaviour
    {
        private int cursor;

        private void Update()
        {
            var kb = Keyboard.current;
            var system = EncounterSystem.Instance;
            if (kb == null || system == null || !system.IsReady) return;
            var p1 = PlayerEntity.All.Count > 0 ? PlayerEntity.All[0] : null;

            if (kb.xKey.wasPressedThisFrame && p1 != null)
            {
                var defs = system.Rules.encounters;
                if (defs.Length > 0)
                {
                    var def = defs[cursor % defs.Length];
                    cursor++;
                    system.ForceStart(def.id, p1);
                }
            }
            if (kb.digit1Key.wasPressedThisFrame) system.Active?.Choose("A");
            if (kb.digit2Key.wasPressedThisFrame) system.Active?.Choose("B");
            if (kb.sKey.wasPressedThisFrame && p1 != null)
            {
                var act = p1.GetComponent<PlayerActivity>();
                if (act != null) { act.IsScouting = !act.IsScouting; RuntimeHud.Instance?.Warn($"P1 조사·정찰 중: {(act.IsScouting ? "예 (인카운터 없음)" : "아니오")}"); }
            }
            if (kb.zKey.wasPressedThisFrame) system.Simulate(100);
        }
    }
}

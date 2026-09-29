using UnityEngine;
using UnityEngine.InputSystem;
using Project1028.PlayFoundation;
using Project1028.Suspicion;

namespace Project1028.Slice
{
    /// <summary>F9 시계 +60s · F10 출항 직전 · F11 봉쇄 토글 · F12 반응 순환 테스트.</summary>
    public sealed class SliceTestConsole : MonoBehaviour
    {
        private void Update()
        {
            var kb = Keyboard.current; var d = RunDirector.Instance;
            if (kb == null || d == null || !d.IsReady) return;
            if (kb.f9Key.wasPressedThisFrame) { d.Clock.Skip(60f); RuntimeHud.Instance?.Warn("시계 +60초"); }
            if (kb.f10Key.wasPressedThisFrame) { d.Clock.Skip(Mathf.Max(0f, d.Clock.RemainingToDeparture - 5f)); RuntimeHud.Instance?.Warn("출항 5초 전으로"); }
            if (kb.f11Key.wasPressedThisFrame && SuspicionSystem.Instance != null)
            {
                bool blocked = SuspicionSystem.Instance.Island.DepartureBlocked;
                SuspicionSystem.Instance.DebugAdjustIsland(blocked ? -80 : 80);
                RuntimeHud.Instance?.Warn(blocked ? "(디버그) 봉쇄 해제" : "(디버그) 봉쇄");
            }
            if (kb.f12Key.wasPressedThisFrame) FindFirstObjectByType<FeedbackDirector>()?.TestNext();
        }
    }
}

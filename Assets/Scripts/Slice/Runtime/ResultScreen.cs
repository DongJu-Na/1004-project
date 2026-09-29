using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Project1028.PlayFoundation;

namespace Project1028.Slice
{
    /// <summary>런 결과·통계 + R 재시작. 표시 중 시간 정지.</summary>
    public sealed class ResultScreen : MonoBehaviour
    {
        private Image panel;
        private Text text;
        private bool shown;

        private void OnEnable() => SliceEvents.OnRunEnded += Show;
        private void OnDisable() { SliceEvents.OnRunEnded -= Show; if (shown) Time.timeScale = 1f; }

        private void Show(RunOutcome outcome, RunStats s)
        {
            if (RuntimeHud.Instance == null) return;
            var canvas = RuntimeHud.Instance.GetComponentInChildren<Canvas>();
            if (panel == null && canvas != null)
            {
                var go = new GameObject("ResultPanel"); go.transform.SetParent(canvas.transform, false);
                panel = go.AddComponent<Image>(); panel.color = new Color(0f, 0f, 0f, 0.82f); panel.raycastTarget = false;
                var rt = panel.rectTransform; rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = rt.anchorMin; rt.pivot = rt.anchorMin; rt.sizeDelta = new Vector2(900f, 560f);
                text = RuntimeHud.Instance.CreateFixedText("ResultText", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(840f, 520f), 26, TextAnchor.UpperLeft);
                text.transform.SetAsLastSibling();
            }
            var sb = new StringBuilder();
            string title = outcome == RunOutcome.Success ? "출항 — 증거를 가지고 섬을 떠났다" : outcome == RunOutcome.LeftWithoutEvidence ? "출항 — 빈손으로 떠났다" : "강제 출도 — 봉쇄 상태로 배를 놓쳤다";
            sb.AppendLine($"<size=40>{title}</size>").AppendLine();
            sb.AppendLine($"걸린 시간  {RunDirector.Mmss(s.ElapsedSeconds)}");
            sb.AppendLine($"최종 섬 의심도  {s.IslandValue} [{s.IslandZone}]");
            sb.AppendLine($"신고 완료  {s.ReportsCompleted}회   제압  {s.Subdues}회   인카운터  {s.Encounters}회");
            if (s.Choices.Count > 0) sb.AppendLine("선택  " + string.Join(", ", s.Choices));
            if (s.Flags.Count > 0) sb.AppendLine("플래그  " + string.Join(", ", s.Flags));
            sb.AppendLine().AppendLine("(이월 값과 이력은 저장 기능이 붙기 전까지 표시만 한다)");
            sb.AppendLine().AppendLine("<size=30>R — 다시 시작</size>");
            text.text = sb.ToString();
            text.supportRichText = true;
            panel.gameObject.SetActive(true); text.gameObject.SetActive(true);
            shown = true;
            Time.timeScale = 0f;
        }

        private void Update()
        {
            if (!shown) return;
            if (Time.timeScale != 0f) Time.timeScale = 0f; // 다른 콘솔(T 배속 등)이 풀어도 결과 화면 동안 정지 유지
            var kb = Keyboard.current;
            if (kb != null && kb.rKey.wasPressedThisFrame) { Time.timeScale = 1f; RunDirector.Instance?.Restart(); }
        }
    }
}

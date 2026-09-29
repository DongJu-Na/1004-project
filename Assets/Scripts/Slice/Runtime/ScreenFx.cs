using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Project1028.PlayFoundation;

namespace Project1028.Slice
{
    /// <summary>화면 반응: 색조, 가장자리 맥동, 중앙 큰 문구, 히트스톱. 전부 uGUI 코드 생성.</summary>
    [DisallowMultipleComponent]
    public sealed class ScreenFx : MonoBehaviour
    {
        public static ScreenFx Instance { get; private set; }

        private Image tint;
        private Image[] edges;
        private Text big;
        private float pulseUntil, pulseDuration, pulseAlpha;
        private Color pulseColor = Color.red;
        private float bigUntil, bigDuration;
        private float tintTarget;
        private Color tintColor = Color.black;
        private bool hitstopping;

        private void Awake() { if (Instance != null && Instance != this) { Destroy(gameObject); return; } Instance = this; }
        private void OnDestroy() { if (Instance == this) Instance = null; Time.timeScale = 1f; }

        private void EnsureUi()
        {
            if (tint != null || RuntimeHud.Instance == null) return;
            var canvas = RuntimeHud.Instance.GetComponentInChildren<Canvas>();
            if (canvas == null) return;
            var root = canvas.transform as RectTransform;

            tint = MakeImage("Tint", root); Full(tint.rectTransform); tint.color = new Color(0f, 0f, 0f, 0f);
            tint.transform.SetAsFirstSibling();

            edges = new Image[4];
            for (int i = 0; i < 4; i++)
            {
                edges[i] = MakeImage($"Edge_{i}", root);
                var rt = edges[i].rectTransform;
                rt.anchorMin = i == 0 ? new Vector2(0, 0) : i == 1 ? new Vector2(0, 1) : i == 2 ? new Vector2(0, 0) : new Vector2(1, 0);
                rt.anchorMax = i == 0 ? new Vector2(1, 0) : i == 1 ? new Vector2(1, 1) : i == 2 ? new Vector2(0, 1) : new Vector2(1, 1);
                rt.pivot = rt.anchorMin;
                rt.sizeDelta = i < 2 ? new Vector2(0f, 90f) : new Vector2(90f, 0f);
                edges[i].color = new Color(1f, 0f, 0f, 0f);
            }

            big = RuntimeHud.Instance.CreateFixedText("BigText", new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(1200f, 90f), 48, TextAnchor.MiddleCenter);
            big.color = new Color(1f, 1f, 1f, 0f);
        }

        private static Image MakeImage(string name, RectTransform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.raycastTarget = false;
            return img;
        }

        private static void Full(RectTransform rt) { rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; }

        private void Update()
        {
            EnsureUi();
            if (tint == null) return;
            float now = Time.unscaledTime;

            tint.color = Color.Lerp(tint.color, new Color(tintColor.r, tintColor.g, tintColor.b, tintTarget), Time.unscaledDeltaTime * 3f);

            float a = pulseUntil > now && pulseDuration > 0f ? pulseAlpha * Mathf.Clamp01((pulseUntil - now) / pulseDuration) : 0f;
            foreach (var e in edges) e.color = new Color(pulseColor.r, pulseColor.g, pulseColor.b, a);

            float b = bigUntil > now && bigDuration > 0f ? Mathf.Clamp01((bigUntil - now) / (bigDuration * 0.4f)) : 0f;
            big.color = new Color(big.color.r, big.color.g, big.color.b, b);
        }

        public void Pulse(Color color, float seconds, float alpha) { pulseColor = color; pulseDuration = Mathf.Max(0.01f, seconds); pulseAlpha = alpha; pulseUntil = Time.unscaledTime + seconds; }
        public void SetTint(Color color, float alpha) { tintColor = color; tintTarget = Mathf.Clamp01(alpha); }
        public void BigText(string text, float seconds, Color? color = null)
        {
            EnsureUi();
            if (big == null) return;
            big.text = text;
            var c = color ?? Color.white;
            big.color = new Color(c.r, c.g, c.b, 1f);
            bigDuration = Mathf.Max(0.05f, seconds);
            bigUntil = Time.unscaledTime + seconds;
        }

        public void HitStop(float seconds)
        {
            if (hitstopping || seconds <= 0f || Time.timeScale <= 0f) return; // 결과 화면 등 정지 중에는 건너뛴다
            if (RunDirector.Instance != null && RunDirector.Instance.IsEnded) return;
            StartCoroutine(HitStopRoutine(seconds));
        }

        private IEnumerator HitStopRoutine(float seconds)
        {
            hitstopping = true;
            float saved = Time.timeScale;
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(seconds);
            if (Time.timeScale == 0f) Time.timeScale = saved <= 0f ? 1f : saved;
            hitstopping = false;
        }
    }
}

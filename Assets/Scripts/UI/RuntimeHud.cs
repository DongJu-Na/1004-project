using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Project1028.PlayFoundation
{
    /// <summary>
    /// 테스트 씬용 HUD. 캔버스·텍스트를 전부 코드로 생성한다 (신규 에셋 0, 헌장 원칙 II).
    /// 플레이어 개체별 슬롯(안내·대화)과 공용 경고 로그를 제공한다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RuntimeHud : MonoBehaviour
    {
        public static RuntimeHud Instance { get; private set; }
        public static event Action<string> OnWarning;

        private const int MaxWarnings = 5;
        private const float WarningLifetime = 8f;

        private sealed class PlayerSlot
        {
            public RectTransform Root;
            public Text Prompt;
            public Text SecondaryPrompt;
            public GameObject DialoguePanel;
            public Text DialogueSpeaker;
            public Text DialogueLine;
            public Text DialogueCounter;
        }

        private sealed class WarningEntry
        {
            public string Message;
            public float ExpiresAt;
        }

        private Canvas canvas;
        private Font font;
        private Text warningText;
        private readonly Dictionary<PlayerEntity, PlayerSlot> slots = new Dictionary<PlayerEntity, PlayerSlot>();
        private readonly List<WarningEntry> warnings = new List<WarningEntry>();
        private readonly List<Text> worldLabels = new List<Text>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildCanvas();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            // T036: NPC 대화 파일을 미리 검증해 규격 위반을 조기에 경고한다. Start를 써서 등록 순서에 의존하지 않는다.
            foreach (var npc in NpcIdentity.All)
            {
                if (npc.GetComponent<NpcTalkInteractable>() == null) continue; // 대화 없는 NPC는 검증 대상 아님
                if (DialogueLoader.TryLoad(npc.NpcId, out _, out var result))
                {
                    foreach (var msg in result.Messages) Warn($"[{npc.NpcId}] {msg}");
                }
                else
                {
                    foreach (var msg in result.Messages) Warn($"[{npc.NpcId}] {msg}");
                }
            }
        }

        private void Update()
        {
            bool changed = false;
            for (int i = warnings.Count - 1; i >= 0; i--)
            {
                if (Time.unscaledTime >= warnings[i].ExpiresAt)
                {
                    warnings.RemoveAt(i);
                    changed = true;
                }
            }
            if (changed) RefreshWarnings();
        }

        // ---------- 공개 API ----------

        public void ShowPrompt(PlayerEntity player, string text)
        {
            var slot = GetSlot(player);
            if (slot == null) return;
            bool show = !string.IsNullOrEmpty(text);
            slot.Prompt.gameObject.SetActive(show);
            if (show) slot.Prompt.text = text;
        }

        /// <summary>두 번째 안내 줄(예: 제압 동작 안내). null/빈 문자열이면 숨김.</summary>
        public void ShowSecondaryPrompt(PlayerEntity player, string text)
        {
            var slot = GetSlot(player);
            if (slot == null) return;
            bool show = !string.IsNullOrEmpty(text);
            slot.SecondaryPrompt.gameObject.SetActive(show);
            if (show) slot.SecondaryPrompt.text = text;
        }

        public void ShowDialogueLine(PlayerEntity player, string speaker, string line, int index, int total)
        {
            var slot = GetSlot(player);
            if (slot == null) return;
            slot.DialoguePanel.SetActive(true);
            slot.DialogueSpeaker.text = speaker;
            slot.DialogueLine.text = line;
            slot.DialogueCounter.text = $"{index + 1}/{total}";
            slot.Prompt.gameObject.SetActive(false);
        }

        public void HideDialogue(PlayerEntity player)
        {
            var slot = GetSlot(player);
            if (slot == null) return;
            slot.DialoguePanel.SetActive(false);
        }

        public void Warn(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            Debug.LogWarning($"[PlayFoundation] {message}");
            warnings.Add(new WarningEntry { Message = message, ExpiresAt = Time.unscaledTime + WarningLifetime });
            while (warnings.Count > MaxWarnings) warnings.RemoveAt(0);
            RefreshWarnings();
            OnWarning?.Invoke(message);
        }

        /// <summary>월드 좌표 위에 띄우는 라벨(NPC 시야 디버그 등)을 만든다. 호출자가 위치를 갱신한다.</summary>
        public Text CreateWorldLabel(string initialText)
        {
            var text = CreateText("WorldLabel", canvas.transform as RectTransform, 20, TextAnchor.LowerCenter, new Color(1f, 0.95f, 0.6f));
            var rt = text.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(360f, 80f);
            text.text = initialText;
            worldLabels.Add(text);
            return text;
        }

        /// <summary>화면 고정 텍스트(디버그 패널 등). anchor/pivot은 같은 값, anchoredPosition은 anchor 기준 오프셋.</summary>
        public Text CreateFixedText(string name, Vector2 anchor, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment)
        {
            var text = CreateText(name, canvas.transform as RectTransform, fontSize, alignment, Color.white);
            var rt = text.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = size;
            text.text = string.Empty;
            return text;
        }

        // ---------- 내부 ----------

        private PlayerSlot GetSlot(PlayerEntity player)
        {
            if (player == null || canvas == null) return null;
            if (slots.TryGetValue(player, out var slot)) return slot;
            slot = BuildSlot(player, slots.Count);
            slots[player] = slot;
            return slot;
        }

        private void BuildCanvas()
        {
            var canvasGo = new GameObject("HudCanvas");
            canvasGo.transform.SetParent(transform, false);
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            warningText = CreateText("Warnings", canvas.transform as RectTransform, 22, TextAnchor.UpperLeft, new Color(1f, 0.75f, 0.3f));
            var wrt = warningText.rectTransform;
            wrt.anchorMin = new Vector2(0f, 1f);
            wrt.anchorMax = new Vector2(0f, 1f);
            wrt.pivot = new Vector2(0f, 1f);
            wrt.anchoredPosition = new Vector2(16f, -16f);
            wrt.sizeDelta = new Vector2(900f, 220f);
            warningText.text = string.Empty;
        }

        private PlayerSlot BuildSlot(PlayerEntity player, int slotIndex)
        {
            // 첫 슬롯(입력 있는 개체)은 하단 중앙 크게, 이후 슬롯은 우측에 축소 (T035).
            bool primary = slotIndex == 0;
            float scale = primary ? 1f : 0.6f;

            var rootGo = new GameObject($"Slot_{player.Id}");
            rootGo.transform.SetParent(canvas.transform, false);
            var root = rootGo.AddComponent<RectTransform>();
            root.anchorMin = primary ? new Vector2(0.5f, 0f) : new Vector2(1f, 0f);
            root.anchorMax = root.anchorMin;
            root.pivot = primary ? new Vector2(0.5f, 0f) : new Vector2(1f, 0f);
            root.anchoredPosition = primary ? new Vector2(0f, 40f) : new Vector2(-24f, 40f + (slotIndex - 1) * 200f);
            root.sizeDelta = new Vector2(1000f * scale, 260f * scale);
            root.localScale = Vector3.one * scale;

            var slot = new PlayerSlot { Root = root };

            slot.Prompt = CreateText("Prompt", root, 30, TextAnchor.MiddleCenter, Color.white);
            var prt = slot.Prompt.rectTransform;
            prt.anchorMin = new Vector2(0.5f, 0f);
            prt.anchorMax = new Vector2(0.5f, 0f);
            prt.pivot = new Vector2(0.5f, 0f);
            prt.anchoredPosition = new Vector2(0f, 80f);
            prt.sizeDelta = new Vector2(600f, 48f);
            slot.Prompt.gameObject.SetActive(false);

            slot.SecondaryPrompt = CreateText("SecondaryPrompt", root, 24, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.6f));
            var srt = slot.SecondaryPrompt.rectTransform;
            srt.anchorMin = new Vector2(0.5f, 0f);
            srt.anchorMax = new Vector2(0.5f, 0f);
            srt.pivot = new Vector2(0.5f, 0f);
            srt.anchoredPosition = new Vector2(0f, 130f);
            srt.sizeDelta = new Vector2(700f, 40f);
            slot.SecondaryPrompt.gameObject.SetActive(false);

            var panelGo = new GameObject("DialoguePanel");
            panelGo.transform.SetParent(root, false);
            var panel = panelGo.AddComponent<Image>();
            panel.color = new Color(0f, 0f, 0f, 0.7f);
            var pprt = panel.rectTransform;
            pprt.anchorMin = new Vector2(0.5f, 0f);
            pprt.anchorMax = new Vector2(0.5f, 0f);
            pprt.pivot = new Vector2(0.5f, 0f);
            pprt.anchoredPosition = new Vector2(0f, 0f);
            pprt.sizeDelta = new Vector2(1000f, 200f);
            slot.DialoguePanel = panelGo;

            slot.DialogueSpeaker = CreateText("Speaker", pprt, 26, TextAnchor.UpperLeft, new Color(0.9f, 0.85f, 0.6f));
            SetInset(slot.DialogueSpeaker.rectTransform, 24f, 16f, 24f, 140f);
            slot.DialogueLine = CreateText("Line", pprt, 30, TextAnchor.UpperLeft, Color.white);
            SetInset(slot.DialogueLine.rectTransform, 24f, 56f, 24f, 16f);
            slot.DialogueCounter = CreateText("Counter", pprt, 22, TextAnchor.LowerRight, new Color(0.7f, 0.7f, 0.7f));
            SetInset(slot.DialogueCounter.rectTransform, 24f, 16f, 24f, 12f);
            slot.DialogueCounter.text = string.Empty;

            panelGo.SetActive(false);
            if (!primary)
            {
                var tag = CreateText("Tag", root, 22, TextAnchor.UpperRight, new Color(0.6f, 0.8f, 1f));
                var trt = tag.rectTransform;
                trt.anchorMin = new Vector2(1f, 1f);
                trt.anchorMax = new Vector2(1f, 1f);
                trt.pivot = new Vector2(1f, 1f);
                trt.anchoredPosition = Vector2.zero;
                trt.sizeDelta = new Vector2(200f, 30f);
                tag.text = $"{player.Id} (입력 없음)";
            }
            return slot;
        }

        private static void SetInset(RectTransform rt, float left, float top, float right, float bottom)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        private Text CreateText(string name, RectTransform parent, int size, TextAnchor anchor, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            return text;
        }

        private void RefreshWarnings()
        {
            if (warningText == null) return;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < warnings.Count; i++)
            {
                sb.Append("⚠ ").AppendLine(warnings[i].Message);
            }
            warningText.text = sb.ToString();
        }
    }
}

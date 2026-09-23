using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Project1028.PlayFoundation
{
    /// <summary>NPC 머리 위에 플레이어별 "보고 있음/안 보임"을 표시한다 (FR-019). 표시만 하며 NPC 행동에 영향 없음.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NpcVision))]
    public sealed class VisionDebugLabel : MonoBehaviour
    {
        [SerializeField] private float labelHeight = 0.6f;

        private NpcVision vision;
        private NpcIdentity identity;
        private Text label;
        private readonly StringBuilder sb = new StringBuilder();

        private void Awake()
        {
            vision = GetComponent<NpcVision>();
            identity = GetComponent<NpcIdentity>();
        }

        private void LateUpdate()
        {
            if (label == null)
            {
                if (RuntimeHud.Instance == null) return;
                label = RuntimeHud.Instance.CreateWorldLabel(identity.DisplayName);
            }

            var cam = Camera.main;
            if (cam == null) { label.enabled = false; return; }

            Vector3 world = identity.EyePosition + Vector3.up * labelHeight;
            Vector3 screen = cam.WorldToScreenPoint(world);
            if (screen.z <= 0f) { label.enabled = false; return; }

            label.enabled = true;
            var canvasRt = label.canvas.transform as RectTransform;
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screen, null, out local);
            label.rectTransform.anchoredPosition = local + canvasRt.rect.size * 0.5f;

            sb.Clear();
            sb.Append(identity.DisplayName);
            foreach (var p in PlayerEntity.All)
            {
                sb.Append('\n').Append(p.Id).Append(": ").Append(vision.IsSeeing(p) ? "보고 있음" : "안 보임");
            }
            label.text = sb.ToString();
            label.color = vision.IsSeeingAny ? new Color(1f, 0.45f, 0.4f) : new Color(1f, 0.95f, 0.6f);
        }
    }
}

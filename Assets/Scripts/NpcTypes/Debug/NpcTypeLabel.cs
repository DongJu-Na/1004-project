using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Project1028.PlayFoundation;

namespace Project1028.NpcTypes
{
    /// <summary>유형·진영·제압 판정·전환·전달 상태 라벨. 숨김 모드(FR-011)에서는 표시하지 않는다.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NpcIdentity))]
    public sealed class NpcTypeLabel : MonoBehaviour
    {
        [SerializeField] private float labelHeight = 1.7f;

        private NpcIdentity identity;
        private Text label;
        private readonly StringBuilder sb = new StringBuilder();

        private void Awake() => identity = GetComponent<NpcIdentity>();

        private void LateUpdate()
        {
            var types = NpcTypeSystem.Instance;
            if (label == null)
            {
                if (RuntimeHud.Instance == null) return;
                label = RuntimeHud.Instance.CreateWorldLabel(string.Empty);
                label.fontSize = 18;
                label.color = new Color(0.8f, 1f, 0.8f);
            }
            if (types == null || !types.IsReady || !types.LabelsVisible) { label.enabled = false; return; }

            var cam = Camera.main;
            if (cam == null) { label.enabled = false; return; }
            Vector3 screen = cam.WorldToScreenPoint(identity.EyePosition + Vector3.up * labelHeight);
            if (screen.z <= 0f) { label.enabled = false; return; }
            label.enabled = true;
            var canvasRt = label.canvas.transform as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screen, null, out Vector2 local);
            label.rectTransform.anchoredPosition = local + canvasRt.rect.size * 0.5f;

            var p = NpcTypeProfile.Get(identity);
            sb.Clear();
            if (p == null) { sb.Append("(유형 없음)"); }
            else
            {
                var verdict = SubdueJudgement.Evaluate(p.Faction);
                sb.Append(NpcTypeDefinitions.KoreanName(p.Type)).Append('/').Append(p.Faction).Append(" 제압:").Append(verdict.CanSubdue ? "가능" : "불가");
                if (p.IsTurned) sb.Append(" [전환]");
                if (p.IsManager) sb.Append(" [관리자]");
                if (p.IsBroker) sb.Append(" [중개인]");
                if (p.Type == NpcType.Silent) sb.Append(" [무신고][무전파]");
                var informer = GetComponent<InformerBehaviour>();
                if (informer != null && informer.ActivePhase != DeliveryPhase.Idle) sb.Append(" 전달:").Append(informer.ActivePhase);
            }
            label.text = sb.ToString();
        }
    }
}

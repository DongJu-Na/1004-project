using UnityEngine;
using UnityEngine.UI;
using Project1028.PlayFoundation;

namespace Project1028.Slice
{
    /// <summary>HUD "◉ 보고 있음 ×N" — P1을 보는 NPC 수. 켜질 때 짧은 소리.</summary>
    public sealed class EyeIndicator : MonoBehaviour
    {
        private Text text;
        private float nextCheck;
        private int last;

        private void Update()
        {
            if (Time.unscaledTime < nextCheck) return;
            nextCheck = Time.unscaledTime + 0.1f;
            if (text == null)
            {
                if (RuntimeHud.Instance == null) return;
                text = RuntimeHud.Instance.CreateFixedText("EyeIndicator", new Vector2(0.5f, 1f), new Vector2(0f, -96f), new Vector2(500f, 40f), 28, TextAnchor.UpperCenter);
                text.color = new Color(1f, 0.35f, 0.3f);
            }
            var p1 = PlayerEntity.All.Count > 0 ? PlayerEntity.All[0] : null;
            int n = 0;
            if (p1 != null) foreach (var npc in NpcIdentity.All) { var v = npc.GetComponent<NpcVision>(); if (v != null && v.enabled && v.IsSeeing(p1)) n++; }
            if (n > 0 && last == 0) ProceduralAudio.Instance?.Tone(1200f, 0.08f, WaveKind.Sine, 0.25f);
            text.text = n > 0 ? $"◉ 보고 있음 ×{n}" : string.Empty;
            last = n;
        }
    }
}

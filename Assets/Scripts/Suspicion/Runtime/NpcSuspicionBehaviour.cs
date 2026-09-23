using UnityEngine;
using UnityEngine.UI;
using Project1028.PlayFoundation;

namespace Project1028.Suspicion
{
    /// <summary>
    /// §2.2 단계별 표현. 1 인지: 쳐다본다·돌아본다. 2 경계: 따라다니며 말을 건다. 3 확신: 표현 중단(신고는 005).
    /// SuppressStageBehaviour(003 밀고자)면 1·2단계 표현을 억제한다. 값은 SuspicionSystem이, 표현만 여기서.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NpcIdentity))]
    [RequireComponent(typeof(NpcMover))]
    public sealed class NpcSuspicionBehaviour : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float followDistance = 2.5f;
        [SerializeField, Min(0f)] private float barkCooldown = 6f;
        [SerializeField] private float barkLabelHeight = 1.1f;

        private NpcIdentity identity;
        private NpcMover mover;
        private NpcVision vision;
        private PlayerEntity focus;
        private Vector3 lastSeenPos;
        private bool hadSeen;
        private float nextBarkAt;
        private float barkClearAt;
        private Text barkLabel;
        private bool wasSuppressed;

        private void Awake()
        {
            identity = GetComponent<NpcIdentity>();
            mover = GetComponent<NpcMover>();
            vision = GetComponent<NpcVision>();
        }

        private void Update()
        {
            var system = SuspicionSystem.Instance;
            if (system == null || !system.IsReady) return;

            var profile = NpcSuspicionProfile.GetOrDefault(identity);
            if (profile.SuppressStageBehaviour)
            {
                // 억제 진입 시 1회만 Stop. 매 프레임 Stop하면 003 InformerBehaviour의 이동을 취소한다.
                if (!wasSuppressed) { mover.Stop(); wasSuppressed = true; }
                return;
            }
            wasSuppressed = false;

            focus = PickFocus(system, out var stage);
            if (focus == null) { mover.Stop(); ClearBarkIfDue(); return; }

            switch (stage)
            {
                case SuspicionStage.Aware:
                    mover.Stop();
                    if (vision != null && vision.IsSeeing(focus))
                    {
                        lastSeenPos = focus.Position;
                        hadSeen = true;
                        mover.FaceTowards(focus.Position);          // 쳐다본다
                    }
                    else if (hadSeen)
                    {
                        mover.FaceTowards(lastSeenPos);             // 지나가면 돌아본다
                    }
                    break;

                case SuspicionStage.Alert:
                    mover.Follow(focus.transform, followDistance);  // 따라다닌다
                    if (Vector3.Distance(transform.position, focus.Position) <= followDistance + 0.5f) TryBark(system);
                    break;

                case SuspicionStage.Certain:
                    mover.Stop();                                   // 신고 흐름은 005
                    break;

                default:
                    mover.Stop();
                    break;
            }
            ClearBarkIfDue();
        }

        private PlayerEntity PickFocus(SuspicionSystem system, out SuspicionStage best)
        {
            PlayerEntity pick = null;
            best = SuspicionStage.Indifferent;
            foreach (var p in PlayerEntity.All)
            {
                var s = system.GetStage(identity, p);
                if (s > best || (s == best && pick == null && s > SuspicionStage.Indifferent))
                {
                    best = s;
                    pick = p;
                }
            }
            return best > SuspicionStage.Indifferent ? pick : null;
        }

        private void TryBark(SuspicionSystem system)
        {
            if (Time.time < nextBarkAt) return;
            var barks = system.Rules.barks;
            if (barks == null || barks.Length == 0) return;
            if (barkLabel == null)
            {
                if (RuntimeHud.Instance == null) return;
                barkLabel = RuntimeHud.Instance.CreateWorldLabel(string.Empty);
                barkLabel.color = Color.white;
                barkLabel.fontSize = 24;
            }
            barkLabel.text = "“" + barks[Random.Range(0, barks.Length)] + "”";
            nextBarkAt = Time.time + barkCooldown;
            barkClearAt = Time.time + Mathf.Min(3f, barkCooldown);
        }

        private void ClearBarkIfDue()
        {
            if (barkLabel == null) return;
            if (Time.time >= barkClearAt && !string.IsNullOrEmpty(barkLabel.text)) barkLabel.text = string.Empty;
        }

        private void LateUpdate()
        {
            if (barkLabel == null) return;
            var cam = Camera.main;
            if (cam == null) { barkLabel.enabled = false; return; }
            Vector3 screen = cam.WorldToScreenPoint(identity.EyePosition + Vector3.up * barkLabelHeight);
            if (screen.z <= 0f || string.IsNullOrEmpty(barkLabel.text)) { barkLabel.enabled = false; return; }
            barkLabel.enabled = true;
            var canvasRt = barkLabel.canvas.transform as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screen, null, out Vector2 local);
            barkLabel.rectTransform.anchoredPosition = local + canvasRt.rect.size * 0.5f;
        }
    }
}

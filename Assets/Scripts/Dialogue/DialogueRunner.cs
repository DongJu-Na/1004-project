using UnityEngine;
using UnityEngine.InputSystem;

namespace Project1028.PlayFoundation
{
    /// <summary>
    /// 플레이어 개체별 대화 진행. 시작 시 잠금, Interact로 한 줄씩 진행, 마지막 줄 후 목적지 전달(FR-009~FR-013).
    /// Interact 액션에 Hold 인터랙션이 붙어 있으므로 WasPressedThisFrame(액추에이션 기반)만 사용한다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerEntity))]
    public sealed class DialogueRunner : MonoBehaviour
    {
        private PlayerEntity owner;
        private PlayerInput playerInput;
        private InputAction interactAction;

        private DialogueData data;
        private int lineIndex = -1;
        private int startedFrame = -1;

        public bool IsActive => data != null;
        public NpcIdentity CurrentNpc { get; private set; }
        public int CurrentLineIndex => lineIndex;
        /// <summary>대화가 끝난 프레임. 같은 프레임의 Interact 입력으로 즉시 재시작되는 것을 막는 데 쓴다.</summary>
        public int LastEndedFrame { get; private set; } = -1;
        /// <summary>일시 정지 중에는 Interact로 진행하지 않는다 (007 C 유형 선택 대기).</summary>
        public bool IsPaused { get; private set; }

        private void Awake()
        {
            owner = GetComponent<PlayerEntity>();
            playerInput = GetComponent<PlayerInput>();
        }

        private void OnEnable()
        {
            interactAction = playerInput != null && playerInput.actions != null ? playerInput.actions.FindAction("Interact", false) : null;
        }

        private void Update()
        {
            if (!IsActive || interactAction == null || IsPaused) return;
            if (Time.frameCount == startedFrame) return;
            if (interactAction.WasPressedThisFrame())
            {
                Advance();
            }
        }

        public bool TryBegin(NpcIdentity npc)
        {
            if (npc == null || IsActive) return false;

            if (!DialogueLoader.TryLoad(npc.NpcId, out var loaded, out var result))
            {
                foreach (var msg in result.Messages) RuntimeHud.Instance?.Warn($"[{npc.NpcId}] {msg}");
                return false;
            }
            foreach (var msg in result.Messages) RuntimeHud.Instance?.Warn($"[{npc.NpcId}] {msg}");

            data = loaded;
            CurrentNpc = npc;
            lineIndex = 0;
            startedFrame = Time.frameCount;
            IsPaused = false;
            owner.Lock(this);
            ShowCurrentLine();
            DialogueEvents.RaiseStarted(npc, owner);
            return true;
        }

        /// <summary>파일 대신 인라인 대사로 시작한다 (007 인카운터). 목적지 없음.</summary>
        public bool TryBeginInline(NpcIdentity speaker, string[] lines)
        {
            if (speaker == null || IsActive) return false;
            if (lines == null || lines.Length == 0) { RuntimeHud.Instance?.Warn($"[{speaker.NpcId}] 인라인 대사가 없습니다."); return false; }
            if (lines.Length < DialogueValidator.MinLines || lines.Length > DialogueValidator.MaxLines)
                RuntimeHud.Instance?.Warn($"[{speaker.NpcId}] §6.1 3~5줄 규격 위반: {lines.Length}줄");
            data = new DialogueData { npcId = speaker.NpcId, lines = lines, destination = null };
            CurrentNpc = speaker;
            lineIndex = 0;
            startedFrame = Time.frameCount;
            IsPaused = false;
            owner.Lock(this);
            ShowCurrentLine();
            DialogueEvents.RaiseStarted(speaker, owner);
            return true;
        }

        public void Pause() { if (IsActive) IsPaused = true; }
        public void Resume() { IsPaused = false; }

        /// <summary>이벤트 없이 강제 종료 (인카운터 중단). 목적지 전달 없음.</summary>
        public void Abort()
        {
            if (!IsActive) return;
            RuntimeHud.Instance?.HideDialogue(owner);
            data = null; lineIndex = -1; CurrentNpc = null; IsPaused = false;
            LastEndedFrame = Time.frameCount;
            owner.Unlock(this);
        }

        public void Advance()
        {
            if (!IsActive) return;

            if (lineIndex >= data.lines.Length - 1)
            {
                End();
                return;
            }

            lineIndex++;
            ShowCurrentLine();
            DialogueEvents.RaiseLineAdvanced(CurrentNpc, owner, lineIndex);
        }

        private void ShowCurrentLine()
        {
            RuntimeHud.Instance?.ShowDialogueLine(owner, CurrentNpc.DisplayName, data.lines[lineIndex], lineIndex, data.lines.Length);
        }

        private void End()
        {
            var npc = CurrentNpc;
            var finished = data;

            RuntimeHud.Instance?.HideDialogue(owner);
            data = null;
            lineIndex = -1;
            CurrentNpc = null;
            IsPaused = false;
            LastEndedFrame = Time.frameCount;
            owner.Unlock(this);

            DeliverDestination(npc, finished);
            DialogueEvents.RaiseEnded(npc, owner);
        }

        private void DeliverDestination(NpcIdentity npc, DialogueData finished)
        {
            if (finished?.destination == null) return;
            if (owner.HasDestinationFrom(npc.NpcId)) return; // 마커는 하나만 유지 (FR-013)

            var d = finished.destination;
            var destination = new Destination(d.name, new Vector3(d.x, d.y, d.z), npc.NpcId, owner);
            DestinationMarker.Spawn(destination);
            owner.AddDestination(destination);
            DialogueEvents.RaiseDestinationReceived(owner, destination);
        }
    }
}

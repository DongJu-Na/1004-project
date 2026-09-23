using UnityEngine;
using UnityEngine.InputSystem;
using Project1028.PlayFoundation;

namespace Project1028.Subdue
{
    /// <summary>§4.2 손 상태. 물건(한 손/두 손)·기절자(두 손, 느림, 달리기 불가)를 들고 내려놓는다. 개체 단위 (원칙 IV).</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerEntity))]
    public sealed class PlayerHands : MonoBehaviour
    {
        [SerializeField] private Vector3 objectLocalOffset = new Vector3(0.45f, 0.2f, 0.5f);
        [SerializeField] private Vector3 bodyLocalOffset = new Vector3(0f, 0.9f, 0.2f);
        [SerializeField] private float dropDistance = 1f;

        private PlayerEntity owner;
        private ThirdPersonMotor motor;
        private PlayerInput input;

        public HandState State { get; private set; } = HandState.Empty;
        public HeldKind HeldKind { get; private set; } = HeldKind.None;
        public CarriableObject HeldObject { get; private set; }
        public UnconsciousState HeldBody { get; private set; }

        private void Awake()
        {
            owner = GetComponent<PlayerEntity>();
            motor = GetComponent<ThirdPersonMotor>();
            input = GetComponent<PlayerInput>();
        }

        private void Update()
        {
            // 테스트 콘솔 매핑: G = 내려놓기 (정식 액션 바인딩은 후속). 입력 있는 개체만.
            if (input == null || HeldKind == HeldKind.None) return;
            var kb = Keyboard.current;
            if (kb != null && kb.gKey.wasPressedThisFrame && !owner.IsLocked) Drop();
        }

        public bool TryPickUp(CarriableObject obj)
        {
            if (obj == null || obj.IsHeld || !HandRules.CanPickUpObject(State)) { Say("들 수 없다: 손이 비어 있어야 함"); return false; }
            HeldObject = obj;
            HeldKind = HeldKind.Object;
            State = obj.IsTwoHanded ? HandState.TwoHands : HandState.OneHand;
            obj.IsHeld = true;
            SetCollidersEnabled(obj.gameObject, false);
            obj.transform.SetParent(transform, false);
            obj.transform.localPosition = objectLocalOffset;
            obj.transform.localRotation = Quaternion.identity;
            Sync();
            SubdueEvents.RaisePickedUp(owner, HeldKind.Object);
            return true;
        }

        public bool TryPickUpBody(UnconsciousState body)
        {
            if (body == null || body.IsCarried || !HandRules.CanPickUpBody(State)) { Say("들 수 없다: 빈손이어야 함 (기절자는 두 손)"); return false; }
            HeldBody = body;
            HeldKind = HeldKind.UnconsciousNpc;
            State = HandState.TwoHands;
            body.SetCarried(owner);
            SetCollidersEnabled(body.gameObject, false);
            body.transform.SetParent(transform, false);
            body.transform.localPosition = bodyLocalOffset;
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Sync();
            SubdueEvents.RaisePickedUp(owner, HeldKind.UnconsciousNpc);
            return true;
        }

        public bool Drop()
        {
            if (HeldKind == HeldKind.None) return false;
            Vector3 dropPos = transform.position + transform.forward * dropDistance;
            var kind = HeldKind;

            if (HeldObject != null)
            {
                HeldObject.transform.SetParent(null, true);
                HeldObject.transform.position = new Vector3(dropPos.x, HeldObject.transform.localScale.y * 0.5f, dropPos.z);
                HeldObject.transform.rotation = Quaternion.identity;
                SetCollidersEnabled(HeldObject.gameObject, true);
                HeldObject.IsHeld = false;
                HeldObject = null;
            }
            if (HeldBody != null)
            {
                HeldBody.transform.SetParent(null, true);
                HeldBody.transform.position = new Vector3(dropPos.x, 0.5f, dropPos.z); // 눕힌 캡슐 반경
                HeldBody.transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
                SetCollidersEnabled(HeldBody.gameObject, true);
                HeldBody.SetCarried(null);
                HeldBody = null;
            }

            HeldKind = HeldKind.None;
            State = HandState.Empty;
            Sync();
            SubdueEvents.RaiseDropped(owner, kind);
            return true;
        }

        /// <summary>기절자가 깨어나거나 파괴될 때 외부에서 손을 비운다.</summary>
        internal void ForceReleaseBody(UnconsciousState body)
        {
            if (HeldBody != body) return;
            Drop();
        }

        private void Sync()
        {
            owner.HandsBusy = State == HandState.TwoHands;
            if (motor != null)
            {
                var rules = SubdueSystem.Instance != null && SubdueSystem.Instance.IsReady ? SubdueSystem.Instance.Rules : null;
                bool carrying = HeldKind == HeldKind.UnconsciousNpc;
                motor.SpeedMultiplier = carrying && rules != null ? rules.carry.speedMultiplier : 1f;
                motor.SprintAllowed = !carrying;
            }
        }

        private static void SetCollidersEnabled(GameObject go, bool enabled)
        {
            foreach (var c in go.GetComponentsInChildren<Collider>(true)) c.enabled = enabled;
        }

        private void Say(string msg) => RuntimeHud.Instance?.Warn($"{owner.Id}: {msg}");
    }
}

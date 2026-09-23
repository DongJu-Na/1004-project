using UnityEngine;
using UnityEngine.InputSystem;

namespace Project1028.PlayFoundation
{
    /// <summary>
    /// 캐릭터 중심 궤도 카메라. Look 입력으로 yaw 무제한·pitch 제한(FR-004), 장애물에 막히면 캐릭터 쪽으로 당김(FR-005).
    /// 소유 PlayerEntity가 잠겨 있으면 회전 입력을 무시한다(FR-012). PlayerInput이 없으면 입력만 건너뛴다(US4).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class OrbitCamera : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform target;
        [SerializeField] private PlayerEntity owner;
        [SerializeField] private PlayerInput playerInput;

        [Header("Orbit")]
        [SerializeField, Min(0.5f)] private float distance = 4.5f;
        [SerializeField] private float pivotHeight = 0.5f;
        [SerializeField] private float yawSpeed = 0.15f;   // 도/픽셀
        [SerializeField] private float pitchSpeed = 0.12f; // 도/픽셀
        [SerializeField] private float pitchMin = -30f;
        [SerializeField] private float pitchMax = 70f;
        [SerializeField] private float startYaw = 0f;
        [SerializeField] private float startPitch = 15f;

        [Header("Collision")]
        [SerializeField] private LayerMask collisionMask = ~0;
        [SerializeField, Min(0.05f)] private float collisionRadius = 0.25f;
        [SerializeField, Min(0.1f)] private float minDistance = 0.6f;
        [SerializeField, Min(0f)] private float distanceSmoothing = 20f;

        [Header("Cursor")]
        [SerializeField] private bool lockCursorOnStart = true;

        private InputAction lookAction;
        private float yaw;
        private float pitch;
        private float currentDistance;

        public float Yaw => yaw;
        public float Pitch => pitch;

        /// <summary>궤도 대상과 거리를 바꾼다 (006 차량 탑승/하차).</summary>
        public void SetTarget(Transform newTarget, float newDistance)
        {
            target = newTarget;
            distance = Mathf.Max(0.5f, newDistance);
            currentDistance = distance;
        }

        public void Configure(Transform newTarget, PlayerEntity newOwner, PlayerInput input)
        {
            target = newTarget;
            owner = newOwner;
            playerInput = input;
        }

        private void Awake()
        {
            yaw = startYaw;
            pitch = startPitch;
            currentDistance = distance;
        }

        private void OnEnable()
        {
            if (playerInput == null && owner != null) playerInput = owner.GetComponent<PlayerInput>();
            lookAction = playerInput != null && playerInput.actions != null ? playerInput.actions.FindAction("Look", false) : null;
        }

        private void Start()
        {
            if (lockCursorOnStart && lookAction != null)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void Update()
        {
            // Esc로 커서 해제, 게임 뷰 클릭으로 재잠금 (에디터 검증 편의).
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (lookAction != null && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            bool locked = owner != null && owner.IsLocked;
            if (!locked && lookAction != null && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 look = lookAction.ReadValue<Vector2>();
                yaw += look.x * yawSpeed;
                pitch -= look.y * pitchSpeed;
                pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
                if (yaw > 360f || yaw < -360f) yaw %= 360f;
            }

            Vector3 pivot = target.position + Vector3.up * pivotHeight;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 back = rotation * Vector3.back;

            float desired = distance;
            if (Physics.SphereCast(pivot, collisionRadius, back, out RaycastHit hit, distance, collisionMask, QueryTriggerInteraction.Ignore))
            {
                if (!IsOwnCollider(hit.collider))
                {
                    desired = Mathf.Max(minDistance, hit.distance - collisionRadius * 0.5f);
                }
            }

            // 당길 때는 즉시, 되돌아갈 때는 부드럽게.
            currentDistance = desired < currentDistance
                ? desired
                : Mathf.Lerp(currentDistance, desired, 1f - Mathf.Exp(-distanceSmoothing * Time.deltaTime));

            transform.position = pivot + back * currentDistance;
            transform.rotation = rotation;
        }

        private bool IsOwnCollider(Collider collider)
        {
            return target != null && collider != null && collider.transform.IsChildOf(target);
        }
    }
}

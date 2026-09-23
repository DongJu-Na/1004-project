using UnityEngine;
using UnityEngine.InputSystem;

namespace Project1028.PlayFoundation
{
/// <summary>
/// 3인칭 도보 이동. 기존 구현(FR-007)을 유지하며 PlayFoundation 어셈블리로 편입했다.
/// Attack 액션은 읽지 않는다 (헌장 원칙 V). 이동 방향은 PlayerEntity.Camera가 있으면 그 카메라 기준.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMotor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private PlayerInput playerInput;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float walkSpeed = 4f;
    [SerializeField, Min(0f)] private float sprintSpeed = 6.5f;
    [SerializeField, Min(0f)] private float acceleration = 20f;
    [SerializeField, Min(0f)] private float deceleration = 25f;
    [SerializeField, Range(0f, 1f)] private float airControl = 0.35f;
    [SerializeField, Min(0f)] private float rotationSharpness = 15f;

    [Header("Jumping")]
    [SerializeField, Min(0f)] private float jumpHeight = 1.3f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float groundedVerticalSpeed = -2f;

    private CharacterController characterController;
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction jumpAction;
    private Vector3 planarVelocity;
    private float verticalVelocity;
    private bool movementEnabled = true;
    private PlayerEntity owner;

    public bool IsGrounded => characterController != null && characterController.isGrounded;
    /// <summary>운반 등으로 이동 속도를 낮출 때 곱하는 배율 (기본 1).</summary>
    public float SpeedMultiplier { get; set; } = 1f;
    /// <summary>false면 달리기 입력을 무시한다 (기절자 운반 중).</summary>
    public bool SprintAllowed { get; set; } = true;

    public bool IsSprinting => movementEnabled && SprintAllowed && sprintAction != null && sprintAction.IsPressed() && HasMoveInput;
    public bool IsMoving => planarVelocity.sqrMagnitude > 0.01f;
    public float Speed => planarVelocity.magnitude;
    public Vector3 Velocity => planarVelocity + Vector3.up * verticalVelocity;
    public bool MovementEnabled => movementEnabled;

    /// <summary>Idle: 거의 정지, Sprinting: 달리기 입력 중 이동, 그 외 Walking.</summary>
    public MovementState CurrentState
    {
        get
        {
            if (Speed < 0.05f) return MovementState.Idle;
            return IsSprinting ? MovementState.Sprinting : MovementState.Walking;
        }
    }

    private bool HasMoveInput => moveAction != null && moveAction.ReadValue<Vector2>().sqrMagnitude > 0.001f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput ??= GetComponent<PlayerInput>();
        owner = GetComponent<PlayerEntity>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        CacheActions();
    }

    private void OnDisable()
    {
        moveAction = null;
        sprintAction = null;
        jumpAction = null;
        planarVelocity = Vector3.zero;
    }

    private void Update()
    {
        if (characterController == null || !characterController.enabled)
        {
            return; // 차량 탑승 중 등 컨트롤러 비활성
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        UpdateVerticalVelocity();
        UpdatePlanarVelocity();
        characterController.Move(Velocity * Time.deltaTime);
    }

    /// <summary>Temporarily prevents or restores player-controlled movement.</summary>
    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
        if (!enabled)
        {
            planarVelocity = Vector3.zero;
        }
    }

    private void CacheActions()
    {
        if (playerInput == null || playerInput.actions == null)
        {
            return;
        }

        moveAction = playerInput.actions.FindAction("Move", false);
        sprintAction = playerInput.actions.FindAction("Sprint", false);
        jumpAction = playerInput.actions.FindAction("Jump", false);
    }

    private void UpdateVerticalVelocity()
    {
        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0f)
            {
                verticalVelocity = groundedVerticalSpeed;
            }

            if (movementEnabled && jumpAction != null && jumpAction.WasPressedThisFrame())
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private void UpdatePlanarVelocity()
    {
        Vector2 input = movementEnabled && moveAction != null
            ? Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f)
            : Vector2.zero;

        Vector3 direction = GetCameraRelativeDirection(input);
        float targetSpeed = input.sqrMagnitude > 0f
            ? (SprintAllowed && sprintAction != null && sprintAction.IsPressed() ? sprintSpeed : walkSpeed) * Mathf.Max(0f, SpeedMultiplier)
            : 0f;
        Vector3 targetVelocity = direction * targetSpeed;
        float rate = targetSpeed > planarVelocity.magnitude ? acceleration : deceleration;

        if (!characterController.isGrounded)
        {
            rate *= airControl;
        }

        planarVelocity = Vector3.MoveTowards(planarVelocity, targetVelocity, rate * Time.deltaTime);

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                1f - Mathf.Exp(-rotationSharpness * Time.deltaTime));
        }
    }

    private Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        if (input.sqrMagnitude <= 0f)
        {
            return Vector3.zero;
        }

        Transform reference = cameraTransform != null ? cameraTransform : transform;
        if (owner != null && owner.Camera != null)
        {
            reference = owner.Camera.transform;
        }
        Vector3 forward = Vector3.ProjectOnPlane(reference.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(reference.right, Vector3.up).normalized;
        return Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
    }
}
}

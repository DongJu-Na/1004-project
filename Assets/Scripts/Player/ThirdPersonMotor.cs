using UnityEngine;
using UnityEngine.InputSystem;

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

    public bool IsGrounded => characterController != null && characterController.isGrounded;
    public bool IsSprinting => movementEnabled && sprintAction != null && sprintAction.IsPressed() && HasMoveInput;
    public bool IsMoving => planarVelocity.sqrMagnitude > 0.01f;
    public float Speed => planarVelocity.magnitude;
    public Vector3 Velocity => planarVelocity + Vector3.up * verticalVelocity;

    private bool HasMoveInput => moveAction != null && moveAction.ReadValue<Vector2>().sqrMagnitude > 0.001f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput ??= GetComponent<PlayerInput>();

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
        if (characterController == null)
        {
            return;
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
            ? (sprintAction != null && sprintAction.IsPressed() ? sprintSpeed : walkSpeed)
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
        Vector3 forward = Vector3.ProjectOnPlane(reference.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(reference.right, Vector3.up).normalized;
        return Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
    }
}
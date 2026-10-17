using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Reference")]
    [SerializeField]
    private Transform movementReference;

    private PlayerStateMachine stateMachine;
    private PlayerDefinition playerDefinition;
    private CharacterController characterController;

    private Vector2 moveInput;
    private float verticalVelocity;
    private float currentSpeedMultiplier = 1f;
    private bool isMovementOverridden;

    private bool isChargeMoving;
    private Vector3 chargeDirection;
    private float chargeSpeed;
    private float chargeRemainingDistance;
    private float chargeRemainingTime;

    public float BaseMoveSpeed => playerDefinition != null ? playerDefinition.MoveSpeed : 0f;

    public float CurrentMoveSpeed => BaseMoveSpeed * currentSpeedMultiplier;

    public bool IsMoving { get; private set; }

    public bool IsGrounded => characterController != null && characterController.isGrounded;

    public CharacterController CharacterController => characterController;

    public bool IsChargeMoving => isChargeMoving;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        stateMachine = GetComponent<PlayerStateMachine>();

        Player player = GetComponent<Player>();
        playerDefinition = player.Definition;

        if (playerDefinition == null)
            Debug.LogError($"{name}: No PlayerDefinition assigned.", this);
    }

    private void Start()
    {
        if (movementReference == null)
            Debug.LogError($"{name}: No movement reference assigned.", this);
    }

    private void Update()
    {
        if (playerDefinition == null || !characterController.enabled)
            return;

        if (!stateMachine.CanMove)
        {
            CancelChargeMovement();
            IsMoving = false;
            return;
        }

        if (isChargeMoving)
        {
            UpdateChargeMovement();
            return;
        }

        if (isMovementOverridden)
        {
            IsMoving = false;
            return;
        }

        UpdateNormalMovement();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void UpdateNormalMovement()
    {
        Vector3 movement = GetMovementDirection();

        IsMoving = movement.sqrMagnitude > 0.01f;
        RotateTowardsMovement(movement);

        Vector3 horizontalVelocity = movement * CurrentMoveSpeed;
        Vector3 verticalMovement = CalculateGravityDisplacement();

        characterController.Move((horizontalVelocity * Time.deltaTime) + verticalMovement);
    }

    private void UpdateChargeMovement()
    {
        float dt = Time.deltaTime;

        if (dt <= 0f)
            return;

        float distanceThisFrame = Mathf.Min(chargeSpeed * dt, chargeRemainingDistance);

        chargeRemainingTime -= dt;
        chargeRemainingDistance -= distanceThisFrame;

        Vector3 horizontalMovement = chargeDirection * distanceThisFrame;

        Vector3 verticalMovement = CalculateGravityDisplacement();

        CollisionFlags flags = characterController.Move(horizontalMovement + verticalMovement);

        IsMoving = distanceThisFrame > 0.001f;

        // Stop early if a wall or other side obstacle blocks the charge.
        if (
            (flags & CollisionFlags.Sides) != 0
            || chargeRemainingDistance <= 0.001f
            || chargeRemainingTime <= 0f
        )
        {
            CancelChargeMovement();
        }
    }

    public bool BeginChargeMovement(Vector3 direction, float distance, float duration)
    {
        if (
            isChargeMoving
            || !characterController.enabled
            || !stateMachine.CanMove
            || distance <= 0f
            || duration <= 0f
        )
        {
            return false;
        }

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return false;

        chargeDirection = direction.normalized;
        chargeRemainingDistance = distance;
        chargeRemainingTime = duration;
        chargeSpeed = distance / duration;
        isChargeMoving = true;
        isMovementOverridden = false;

        return true;
    }

    public void CancelChargeMovement()
    {
        isChargeMoving = false;
        chargeRemainingDistance = 0f;
        chargeRemainingTime = 0f;
        chargeSpeed = 0f;
        IsMoving = false;
    }

    public void SetMovementOverridden(bool overridden)
    {
        isMovementOverridden = overridden;

        if (overridden)
        {
            CancelChargeMovement();
            verticalVelocity = 0f;
            IsMoving = false;
        }
    }

    private Vector3 CalculateGravityDisplacement()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += playerDefinition.Gravity * Time.deltaTime;

        return Vector3.up * verticalVelocity * Time.deltaTime;
    }

    private Vector3 GetMovementDirection()
    {
        if (movementReference == null)
            return Vector3.zero;

        Vector3 forward = movementReference.forward;
        Vector3 right = movementReference.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 movement = right * moveInput.x + forward * moveInput.y;

        if (movement.sqrMagnitude > 1f)
            movement.Normalize();

        return movement;
    }

    private void RotateTowardsMovement(Vector3 movement)
    {
        if (movement.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(movement);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            playerDefinition.RotationSpeed * Time.deltaTime
        );
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeedMultiplier = Mathf.Max(0f, multiplier);
    }

    public void SetMovementReference(Transform reference)
    {
        movementReference = reference;

        if (movementReference == null)
        {
            Debug.LogError($"{name}: Failed to assign the camera movement reference.", this);
        }
    }

    public void ResetSpeedMultiplier()
    {
        currentSpeedMultiplier = 1f;
    }

    private void OnDisable()
    {
        CancelChargeMovement();
    }
}

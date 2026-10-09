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

    public float BaseMoveSpeed => playerDefinition.MoveSpeed;
    public float CurrentMoveSpeed => playerDefinition.MoveSpeed * currentSpeedMultiplier;
    public bool IsMoving { get; private set; }
    public bool IsGrounded => characterController.isGrounded;
    public CharacterController CharacterController => characterController;

    public void SetMovementOverridden(bool overridden)
    {
        isMovementOverridden = overridden;
        if (overridden)
        {
            verticalVelocity = 0f;
            IsMoving = false;
        }
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        stateMachine = GetComponent<PlayerStateMachine>();
        Player player = GetComponent<Player>();
        playerDefinition = player.Definition;
        if (playerDefinition == null)
        {
            Debug.LogError("PlayerMovement has no PlayerDefinition assigned.");
        }
        if (movementReference == null)
        {
            Debug.LogError("PlayerMovement has no movement reference assigned.");
        }
    }

    private void Update()
    {
        Move();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void Move()
    {
        if (!stateMachine.CanMove || isMovementOverridden)
        {
            IsMoving = false;
            return;
        }
        Vector3 movement = GetMovementDirection();
        IsMoving = movement.sqrMagnitude > 0.01f;
        RotateTowardsMovement(movement);
        characterController.Move(movement * CurrentMoveSpeed * Time.deltaTime);
        ApplyGravity();
    }

    private Vector3 GetMovementDirection()
    {
        if (movementReference == null)
        {
            return Vector3.zero;
        }
        Vector3 forward = movementReference.forward;
        Vector3 right = movementReference.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        Vector3 movement = right * moveInput.x + forward * moveInput.y;
        if (movement.sqrMagnitude > 1f)
        {
            movement.Normalize();
        }
        return movement;
    }

    private void RotateTowardsMovement(Vector3 movement)
    {
        if (movement.sqrMagnitude < 0.01f)
        {
            return;
        }
        Quaternion targetRotation = Quaternion.LookRotation(movement);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            playerDefinition.RotationSpeed * Time.deltaTime
        );
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        verticalVelocity += playerDefinition.Gravity * Time.deltaTime;
        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeedMultiplier = Mathf.Max(0f, multiplier);
    }

    public void ResetSpeedMultiplier()
    {
        currentSpeedMultiplier = 1f;
    }
}

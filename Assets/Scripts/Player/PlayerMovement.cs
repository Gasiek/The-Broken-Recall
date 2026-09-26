using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private PlayerStateMachine stateMachine;
    private PlayerDefinition playerDefinition;
    private CharacterController characterController;

    private Vector2 moveInput;
    private float verticalVelocity;
    private float currentSpeedMultiplier = 1f;

    public float CurrentMoveSpeed => playerDefinition.MoveSpeed * currentSpeedMultiplier;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        stateMachine = GetComponent<PlayerStateMachine>();
        Player player = GetComponent<Player>();
        playerDefinition = player.Definition;

        if (playerDefinition == null)
            Debug.LogError("PlayerMovement has no PlayerDefinition assigned.");
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
        if (!stateMachine.CanMove)
            return;

        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);

        if (movement.sqrMagnitude > 1f)
            movement.Normalize();

        RotateTowardsMovement(movement);

        characterController.Move(movement * CurrentMoveSpeed * Time.deltaTime);

        ApplyGravity();
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

    private void ApplyGravity()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += playerDefinition.Gravity * Time.deltaTime;

        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeedMultiplier = multiplier;
    }

    public void ResetSpeedMultiplier()
    {
        currentSpeedMultiplier = 1f;
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float baseMoveSpeed = 5f;

    [SerializeField]
    private float rotationSpeed = 15f;

    [SerializeField]
    private float gravity = -20f;
    private float currentSpeedMultiplier = 1f;

    public float CurrentMoveSpeed => baseMoveSpeed * currentSpeedMultiplier;

    private CharacterController characterController;

    private Vector2 moveInput;
    private float verticalVelocity;

    public PlayerState State { get; private set; } = PlayerState.Normal;

    public bool CanMove => State != PlayerState.Dashing;
    public bool CanAct => State == PlayerState.Normal;
    public bool CanDash => State == PlayerState.Normal;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        if (!CanMove)
        {
            return;
        }

        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);

        if (movement.sqrMagnitude > 1f)
        {
            movement.Normalize();
        }

        RotateTowardsMovement(movement);

        characterController.Move(movement * CurrentMoveSpeed * Time.deltaTime);

        ApplyGravity();
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
            rotationSpeed * Time.deltaTime
        );
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    public void StartActing()
    {
        if (!CanAct)
        {
            return;
        }

        State = PlayerState.Acting;
    }

    public void FinishActing()
    {
        if (State != PlayerState.Acting)
        {
            return;
        }

        State = PlayerState.Normal;
    }

    public void StartDashing()
    {
        if (!CanDash)
        {
            return;
        }

        State = PlayerState.Dashing;
    }

    public void FinishDashing()
    {
        if (State != PlayerState.Dashing)
        {
            return;
        }

        State = PlayerState.Normal;
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

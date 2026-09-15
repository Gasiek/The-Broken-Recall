using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private DashController dashController;

    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private float rotationSpeed = 15f;

    [SerializeField]
    private float gravity = -20f;

    private CharacterController characterController;

    private Vector2 moveInput;
    private float verticalVelocity;

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
        if (dashController.IsDashing)
        {
            return;
        }

        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);

        if (movement.sqrMagnitude > 1f)
        {
            movement.Normalize();
        }

        RotateTowardsMovement(movement);

        characterController.Move(movement * moveSpeed * Time.deltaTime);

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
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
}

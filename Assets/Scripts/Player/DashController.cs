using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class DashController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.5f;

    private CharacterController characterController;

    private float dashTimer;
    private float cooldownTimer;
    private Vector3 dashDirection;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        UpdateCooldown();

        if (playerController.State == PlayerState.Dashing)
        {
            UpdateDash();
        }
    }

    public void Dash()
    {
        if (!playerController.CanDash || cooldownTimer > 0f)
        {
            return;
        }

        dashDirection = transform.forward;
        dashTimer = dashDuration;
        cooldownTimer = dashCooldown;

        playerController.StartDashing();
    }

    private void UpdateDash()
    {
        float dashSpeed = dashDistance / dashDuration;

        characterController.Move(
            dashDirection * dashSpeed * Time.deltaTime
        );

        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
        {
            EndDash();
        }
    }

    private void UpdateCooldown()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    private void EndDash()
    {
        playerController.FinishDashing();
    }
}
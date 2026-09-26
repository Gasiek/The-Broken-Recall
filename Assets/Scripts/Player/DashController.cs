using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(Player))]
public class DashController : MonoBehaviour
{
    private PlayerStateMachine stateMachine;

    private PlayerDefinition playerDefinition;

    private CharacterController characterController;

    private float dashTimer;
    private float cooldownTimer;
    private Vector3 dashDirection;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        stateMachine = GetComponent<PlayerStateMachine>();
        Player player = GetComponent<Player>();
        playerDefinition = player.Definition;

        if (playerDefinition == null)
            Debug.LogError("DashController has no PlayerDefinition assigned.");
    }

    private void Update()
    {
        UpdateCooldown();

        if (stateMachine.CurrentState == PlayerState.Dashing)
        {
            UpdateDash();
        }
    }

    public void Dash()
    {
        if (!stateMachine.CanDash || cooldownTimer > 0f)
        {
            return;
        }

        dashDirection = transform.forward;

        dashTimer = playerDefinition.DashDuration;
        cooldownTimer = playerDefinition.DashCooldown;

        stateMachine.StartDashing();
    }

    private void UpdateDash()
    {
        float dashSpeed = playerDefinition.DashDistance / playerDefinition.DashDuration;

        characterController.Move(dashDirection * dashSpeed * Time.deltaTime);

        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
        {
            EndDash();
        }
    }

    private void UpdateCooldown()
    {
        if (cooldownTimer <= 0f)
        {
            return;
        }

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer < 0f)
        {
            cooldownTimer = 0f;
        }
    }

    private void EndDash()
    {
        stateMachine.FinishDashing();
    }
}

using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(ActionController))]
public class PlayerAnimation : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int BasicHitHash = Animator.StringToHash("BasicHit");
    private static readonly int BlockingHash = Animator.StringToHash("Blocking");

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private float dampTime = 0.15f;

    private PlayerMovement playerMovement;
    private ActionController actionController;
    private ActionDefinition lastAction;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        actionController = GetComponent<ActionController>();
    }

    private void Update()
    {
        if (animator == null)
            return;

        float targetSpeed = playerMovement.IsMoving ? 1f : 0f;
        animator.SetFloat(SpeedHash, targetSpeed, dampTime, Time.deltaTime);

        ActionDefinition currentAction = actionController.CurrentAction;
        if (currentAction != lastAction)
        {
            lastAction = currentAction;

            switch (currentAction)
            {
                case BasicHitAction:
                    animator.SetTrigger(BasicHitHash);
                    break;
                case BlockAction:
                    animator.SetTrigger(BlockingHash);
                    break;
                default:
                    ResetAnimations();
                    break;
            }
        }
    }

    private void OnDisable()
    {
        lastAction = null;

        if (animator != null)
        {
            animator.SetFloat(SpeedHash, 0f);
            ResetAnimations();
        }
    }

    public void ResetAnimations()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(BasicHitHash);
        animator.ResetTrigger(BlockingHash);
    }
}

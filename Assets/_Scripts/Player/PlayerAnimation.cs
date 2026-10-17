using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(ActionController))]
public class PlayerAnimation : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int BasicHitHash = Animator.StringToHash("BasicHit");
    private static readonly int BlockingHash = Animator.StringToHash("Block");
    private static readonly int TornadoHash = Animator.StringToHash("Tornado");
    private static readonly int JumpSlamHash = Animator.StringToHash("JumpSlam");
    private static readonly int SwordChargeHash = Animator.StringToHash("SwordCharge");
    private static readonly int HealHash = Animator.StringToHash("Heal");
    private static readonly int ElementalChargeHash = Animator.StringToHash("ElementalCharge");

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
                case TornadoAction:
                    animator.SetTrigger(TornadoHash);
                    break;
                case JumpSlamAction:
                    animator.SetTrigger(JumpSlamHash);
                    break;
                case SwordChargeAction:
                    animator.SetTrigger(SwordChargeHash);
                    break;
                case HealAction:
                    animator.SetTrigger(HealHash);
                    break;
                case LightningChargeAction:
                    animator.SetTrigger(ElementalChargeHash);
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
        animator.ResetTrigger(TornadoHash);
        animator.ResetTrigger(JumpSlamHash);
        animator.ResetTrigger(SwordChargeHash);
        animator.ResetTrigger(HealHash);
        animator.ResetTrigger(ElementalChargeHash);
    }
}

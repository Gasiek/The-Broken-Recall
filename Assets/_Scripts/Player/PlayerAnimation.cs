using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(ActionController))]
[RequireComponent(typeof(Health))]
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
    private static readonly int KickHash = Animator.StringToHash("Kick");
    private static readonly int DyingHash = Animator.StringToHash("Dying");
    private static readonly int StunHash = Animator.StringToHash("Stun");

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private float dampTime = 0.15f;

    private PlayerMovement playerMovement;
    private ActionController actionController;
    private Health health;
    private ActionDefinition lastAction;
    private bool isDead;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        actionController = GetComponent<ActionController>();
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.Died += HandleDeath;
        }
    }

    private void Update()
    {
        if (animator == null || isDead)
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
                case PushAction:
                    animator.SetTrigger(KickHash);
                    break;
                case StunAction:
                    animator.SetTrigger(StunHash);
                    break;
                default:
                    ResetAnimations();
                    break;
            }
        }
    }

    private void HandleDeath()
    {
        isDead = true;

        if (animator == null)
            return;

        animator.SetFloat(SpeedHash, 0f);
        ResetAnimations();
        animator.SetTrigger(DyingHash);
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Died -= HandleDeath;
        }

        isDead = false;
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
        animator.ResetTrigger(KickHash);
        animator.ResetTrigger(DyingHash);
        animator.ResetTrigger(StunHash);
    }
}

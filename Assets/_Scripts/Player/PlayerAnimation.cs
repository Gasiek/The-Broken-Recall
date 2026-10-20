using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(ActionController))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(DashController))]
public class PlayerAnimation : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int BlockingHash = Animator.StringToHash("Block");
    private static readonly int TornadoHash = Animator.StringToHash("Tornado");
    private static readonly int JumpSlamHash = Animator.StringToHash("JumpSlam");
    private static readonly int SwordChargeHash = Animator.StringToHash("SwordCharge");
    private static readonly int HealHash = Animator.StringToHash("Heal");
    private static readonly int ElementalChargeHash = Animator.StringToHash("ElementalCharge");
    private static readonly int KickHash = Animator.StringToHash("Kick");
    private static readonly int DyingHash = Animator.StringToHash("Dying");
    private static readonly int StunHash = Animator.StringToHash("Stun");
    private static readonly int ComboStepHash = Animator.StringToHash("BasicHitCombo");
    private static readonly int RollHash = Animator.StringToHash("Roll");

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private float dampTime = 0.15f;

    [Header("Combo Settings")]
    [Tooltip("Maximum delay in seconds between consecutive BasicHits to continue the combo.")]
    [SerializeField, Min(0.1f)]
    private float comboResetTime = 2f;

    private PlayerMovement playerMovement;
    private ActionController actionController;
    private Health health;
    private DashController dashController;
    private ActionDefinition lastAction;
    private bool isDead;
    private int currentComboStep;
    private float lastBasicHitTime = -999f;

    public int CurrentComboStep => currentComboStep;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        actionController = GetComponent<ActionController>();
        health = GetComponent<Health>();
        dashController = GetComponent<DashController>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.Died += HandleDeath;
        }

        if (dashController != null)
        {
            dashController.DashStarted += HandleDashStarted;
        }
    }

    private void Update()
    {
        if (animator == null || isDead)
            return;

        if (currentComboStep > 0 && Time.time - lastBasicHitTime > comboResetTime)
        {
            ResetCombo();
        }

        float targetSpeed = playerMovement.IsMoving ? 1f : 0f;
        animator.SetFloat(SpeedHash, targetSpeed, dampTime, Time.deltaTime);

        ActionDefinition currentAction = actionController.CurrentAction;
        if (currentAction != lastAction)
        {
            lastAction = currentAction;

            switch (currentAction)
            {
                case BasicHitAction:
                    if (Time.time - lastBasicHitTime <= comboResetTime)
                    {
                        currentComboStep = (currentComboStep % 3) + 1;
                    }
                    else
                    {
                        currentComboStep = 1;
                    }

                    lastBasicHitTime = Time.time;

                    animator.SetInteger(ComboStepHash, currentComboStep);
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

    private void HandleDashStarted()
    {
        if (animator == null || isDead)
            return;

        animator.SetTrigger(RollHash);
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Died -= HandleDeath;
        }

        if (dashController != null)
        {
            dashController.DashStarted -= HandleDashStarted;
        }

        isDead = false;
        lastAction = null;
        ResetCombo();
        animator.SetFloat(SpeedHash, 0f);
        ResetAnimations();
    }

    public void ResetCombo()
    {
        currentComboStep = 0;
        animator.SetInteger(ComboStepHash, 0);
    }

    public void ResetAnimations()
    {

        if (currentComboStep > 0 && Time.time - lastBasicHitTime > comboResetTime)
        {
            ResetCombo();
        }
        animator.ResetTrigger(BlockingHash);
        animator.ResetTrigger(TornadoHash);
        animator.ResetTrigger(JumpSlamHash);
        animator.ResetTrigger(SwordChargeHash);
        animator.ResetTrigger(HealHash);
        animator.ResetTrigger(ElementalChargeHash);
        animator.ResetTrigger(KickHash);
        animator.ResetTrigger(DyingHash);
        animator.ResetTrigger(StunHash);
        animator.ResetTrigger(RollHash);
    }
}

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
    private static readonly int BasicHitHash = Animator.StringToHash("BasicHit");

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private float dampTime = 0.15f;

    [Header("Combo Settings")]
    [Tooltip("Maximum delay in seconds between consecutive BasicHits to continue the combo.")]
    [SerializeField, Min(0.1f)]
    private float comboResetTime = 2f;
    private int currentComboStep;
    private float lastBasicHitFinishedTime = -999f;
    private bool canContinueCombo;

    private PlayerMovement playerMovement;
    private ActionController actionController;
    private Health health;
    private DashController dashController;

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
            health.Died += HandleDeath;

        if (dashController != null)
            dashController.DashStarted += HandleDashStarted;

        if (actionController != null)
            actionController.ActionStarted += HandleActionStarted;
    }

    private void Update()
    {
        if (animator == null || health.IsDead)
            return;

        if (canContinueCombo && Time.time - lastBasicHitFinishedTime > comboResetTime)
        {
            ResetCombo();
            canContinueCombo = false;
        }

        float targetSpeed = playerMovement.IsMoving ? 1f : 0f;
        animator.SetFloat(SpeedHash, targetSpeed, dampTime, Time.deltaTime);
    }

    private void HandleActionStarted()
    {
        if (animator == null || health.IsDead)
            return;

        ActionDefinition action = actionController.CurrentAction;

        if (action == null)
        {
            ResetAnimations();
            return;
        }

        switch (action.AnimationType)
        {
            case ActionAnimationType.BasicHitCombo:
                AdvanceCombo();
                break;

            case ActionAnimationType.Block:
                animator.SetTrigger(BlockingHash);
                break;

            case ActionAnimationType.Tornado:
                animator.SetTrigger(TornadoHash);
                break;

            case ActionAnimationType.JumpSlam:
                animator.SetTrigger(JumpSlamHash);
                break;

            case ActionAnimationType.SwordCharge:
                animator.SetTrigger(SwordChargeHash);
                break;

            case ActionAnimationType.Heal:
                animator.SetTrigger(HealHash);
                break;

            case ActionAnimationType.LightningSword:
                animator.SetTrigger(ElementalChargeHash);
                break;

            case ActionAnimationType.Kick:
                animator.SetTrigger(KickHash);
                break;

            case ActionAnimationType.Stun:
                animator.SetTrigger(StunHash);
                break;

            case ActionAnimationType.None:
            default:
                ResetAnimations();
                break;
        }
    }

    private void AdvanceCombo()
    {
        if (canContinueCombo && Time.time - lastBasicHitFinishedTime <= comboResetTime)
        {
            currentComboStep = (currentComboStep % 3) + 1;
        }
        else
        {
            currentComboStep = 1;
        }

        canContinueCombo = false;
        animator.SetInteger(ComboStepHash, currentComboStep);
        animator.SetTrigger(BasicHitHash);
    }

    public void BasicHitAnimationFinished()
    {
        lastBasicHitFinishedTime = Time.time;
        canContinueCombo = true;
    }

    private void HandleDeath()
    {
        if (animator == null)
            return;

        animator.SetFloat(SpeedHash, 0f);
        ResetAnimations();
        animator.SetTrigger(DyingHash);
    }

    private void HandleDashStarted()
    {
        if (animator == null || health.IsDead)
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

        if (actionController != null)
            actionController.ActionStarted -= HandleActionStarted;

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
        if (animator == null)
            return;

        animator.ResetTrigger(BlockingHash);
        animator.ResetTrigger(TornadoHash);
        animator.ResetTrigger(JumpSlamHash);
        animator.ResetTrigger(SwordChargeHash);
        animator.ResetTrigger(HealHash);
        animator.ResetTrigger(ElementalChargeHash);
        animator.ResetTrigger(KickHash);
        animator.ResetTrigger(DyingHash);
        animator.ResetTrigger(StunHash);
        animator.ResetTrigger(BasicHitHash);
    }
}

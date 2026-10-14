using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int DieHash = Animator.StringToHash("Die");
    private static readonly int StunnedHash = Animator.StringToHash("Stunned");

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private float speedSmoothTime = 0.15f;

    private void Awake()
    {
        if (animator == null)
            Debug.LogError($"{name}: No Animator found.", this);
    }

    public void SetLocomotion(float speed)
    {
        if (animator == null)
            return;

        animator.SetFloat(SpeedHash, speed, speedSmoothTime, Time.deltaTime);
    }

    public void PlayAttack(string triggerName)
    {
        if (animator == null || string.IsNullOrEmpty(triggerName))
            return;

        animator.SetTrigger(triggerName);
    }

    public void SetStunned(bool isStunned)
    {
        if (animator == null)
            return;

        animator.SetBool(StunnedHash, isStunned);
    }

    public void PlayDie()
    {
        if (animator == null)
            return;

        // Death must not remain blocked by the stun condition.
        animator.SetBool(StunnedHash, false);
        animator.ResetTrigger(DieHash);
        animator.SetTrigger(DieHash);
    }
}

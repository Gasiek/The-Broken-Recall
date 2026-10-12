using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private static readonly int GetHitHash = Animator.StringToHash("GetHit");

    private static readonly int DieHash = Animator.StringToHash("Die");

    [SerializeField]
    private Animator animator;
    private float speedSmoothTime = 0.15f;

    public Animator Animator => animator;

    private void Awake()
    {
        if (animator == null)
        {
            Debug.LogError($"{name}: No child Animator found.", this);
        }
    }

    public void SetLocomotion(float speed)
    {
        if (animator != null)
        {
            animator.SetFloat(SpeedHash, speed, speedSmoothTime, Time.deltaTime);
        }
    }

    public void PlayAttack()
    {
        if (animator != null)
            animator.SetTrigger(AttackHash);
    }

    public void PlayGetHit()
    {
        if (animator != null)
            animator.SetTrigger(GetHitHash);
    }

    public void PlayDie()
    {
        if (animator != null)
            animator.SetTrigger(DieHash);
    }
}

using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int DieHash = Animator.StringToHash("Die");

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private float speedSmoothTime = 0.15f;

    private void Awake()
    {
        if (animator == null)
        {
            Debug.LogError($"{name}: No Animator found.", this);
        }
    }

    public void SetLocomotion(float speed)
    {
        animator.SetFloat(SpeedHash, speed, speedSmoothTime, Time.deltaTime);
    }

    public void PlayAttack(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    public void PlayDie()
    {
        animator.SetTrigger(DieHash);
    }
}

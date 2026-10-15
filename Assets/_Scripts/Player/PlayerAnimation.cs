using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimation : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private float dampTime = 0.15f;

    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (animator == null)
            return;

        float targetSpeed = playerMovement.IsMoving ? 1f : 0f;
        animator.SetFloat(SpeedHash, targetSpeed, dampTime, Time.deltaTime);
    }

    private void OnDisable()
    {
        if (animator != null)
        {
            animator.SetFloat(SpeedHash, 0f);
        }
    }
}

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

        switch (actionController.CurrentAction)
        {
            case BasicHitAction basicHitAction:
                animator.SetBool(BasicHitHash, true);
                break;
            case BlockAction blockingAction:
                animator.SetBool(BlockingHash, true);
                break;
            default:
                RestartActions();
                break;
        }
    }

    private void OnDisable()
    {
        if (animator != null)
        {
            animator.SetFloat(SpeedHash, 0f);
            RestartActions();
        }
    }

    public void RestartActions(){
        animator.SetBool(BasicHitHash, false);
        animator.SetBool(BlockingHash, false);
    }
}

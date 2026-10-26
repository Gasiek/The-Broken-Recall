using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    [SerializeField]
    private ActionController actionController;

    [SerializeField]
    private PlayerAnimation playerAnimation;

    private void Awake()
    {
        if (actionController == null)
            Debug.LogError($"{name}: No ActionController found.", this);
        if (playerAnimation == null)
            Debug.LogError($"{name}: No PlayerAnimation found.", this);
    }

    // Add this event to the animation frame where the
    // action's gameplay effect should occur.
    public void ActionImpact()
    {
        if (actionController == null)
            return;

        actionController.HandleAnimationImpact();
    }

    // Add this event near the end of an action clip.
    public void ActionFinished()
    {
        if (actionController == null)
            return;

        actionController.HandleAnimationFinished();
    }

    public void BasicHitAnimationFinished()
    {
        playerAnimation?.BasicHitAnimationFinished();
    }

    public void HealSittingStarted()
    {
        actionController?.HandleHealSittingStarted();
    }

    // Optional: add this to the end of the death clip.
    public void DeathFinished()
    {
        // Forward to your player death/respawn system
        // if it needs to know when the death animation ends.
    }
}

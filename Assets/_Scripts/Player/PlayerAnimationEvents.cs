using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    [SerializeField]
    private ActionController actionController;

    [SerializeField]
    private PlayerAnimation playerAnimation;

    [SerializeField]
    private ParticleSystem swordSlashVFX;

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

    public void PlayAttackVFX()
    {
        if (actionController == null)
            return;

        if (actionController.CurrentAction is not AttackActionDefinition attack)
            return;

        AttackVFXDefinition vfx = attack.ImpactVFX;

        if (vfx == null || vfx.Prefab == null)
            return;

        Transform player = transform;

        Quaternion localRotation = Quaternion.Euler(vfx.LocalRotation);
        Vector3 scale = Vector3.Scale(vfx.Prefab.transform.localScale, vfx.Scale);

        VfxSpawner.SpawnAttached(vfx.Prefab, player, vfx.LocalPosition, localRotation, scale);
    }
}

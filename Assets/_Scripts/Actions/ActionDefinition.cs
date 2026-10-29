using UnityEngine;

public enum ActionAnimationType
{
    None,
    BasicHitCombo,
    Block,
    Tornado,
    JumpSlam,
    SwordCharge,
    Heal,
    LightningSword,
    Kick,
    Stun,
}

public abstract class ActionDefinition : ScriptableObject
{
    [SerializeField]
    private string displayName;

    [Header("Animation")]
    [SerializeField]
    private ActionAnimationType animationType = ActionAnimationType.None;

    [SerializeField]
    private Sprite icon;

    [Header("Action Audio")]
    [SerializeField]
    private AudioClip[] actionSounds;

    [SerializeField, Range(0f, 1f)]
    private float actionVolume = 1f;

    [Header("Reshuffle Cooldown")]
    [SerializeField, Min(0)]
    private int reshuffleCooldown = 0;

    public string DisplayName => displayName;
    public ActionAnimationType AnimationType => animationType;
    public Sprite Icon => icon;
    public int ReshuffleCooldown => reshuffleCooldown;
    public AudioClip GetRandomActionSound()
    {
        if (actionSounds == null || actionSounds.Length == 0)
            return null;

        // Avoid selecting null entries.
        for (int i = 0; i < 10; i++)
        {
            AudioClip clip = actionSounds[Random.Range(0, actionSounds.Length)];
            if (clip != null)
                return clip;
        }

        return null;
    }

    public float ActionVolume => actionVolume;

    // Called when the action begins.
    public virtual void OnStarted(ActionContext context) { }

    // Called by an animation event at the appropriate frame.
    public virtual void OnAnimationImpact(ActionContext context) { }

    // Called when the action animation reaches its finish event.
    public virtual void OnAnimationFinished(ActionContext context) { }

    // Called if the action is interrupted or cancelled.
    public virtual void OnCancelled(ActionContext context) { }

    public virtual void OnHealSittingStarted(ActionContext context) { }

    public virtual string GetDescription() => string.Empty;
}

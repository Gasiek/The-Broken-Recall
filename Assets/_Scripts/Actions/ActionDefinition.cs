using System.Collections;
using UnityEngine;

public abstract class ActionDefinition : ScriptableObject
{
    [SerializeField]
    private string id;

    [SerializeField]
    private string displayName;

    [SerializeField]
    private float duration = 0.5f;

    [SerializeField]
    private Sprite icon;

    [Header("Reshuffle Cooldown")]
    [SerializeField, Min(0)]
    private int reshuffleCooldown = 0;

    public string Id => id;
    public string DisplayName => displayName;
    public float Duration => duration;
    public Sprite Icon => icon;
    public int ReshuffleCooldown => reshuffleCooldown;

    // Called when the action begins.
    public virtual void OnStarted(ActionContext context) { }

    // Called by an animation event at the appropriate frame.
    public virtual void OnAnimationImpact(ActionContext context) { }

    // Called when the action animation reaches its finish event.
    public virtual void OnAnimationFinished(ActionContext context) { }

    // Called if the action is interrupted or cancelled.
    public virtual void OnCancelled(ActionContext context) { }

    public virtual string GetDescription() => string.Empty;
}

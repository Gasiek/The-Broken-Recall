using UnityEngine;

[CreateAssetMenu(fileName = "ApplyEffectAction", menuName = "RPG/Actions/Apply Self Effect")]
public class ApplyEffectAction : ActionDefinition
{
    [Header("Effect")]
    [SerializeField]
    private EffectDefinition effectDefinition;

    public override void OnStarted(ActionContext context)
    {
        if (context?.Player == null)
            return;

        if (context.ActionController == null)
        {
            Debug.LogWarning($"{name}: ActionController is missing from the context.", this);
            return;
        }

        if (effectDefinition == null)
        {
            Debug.LogWarning($"{name}: No EffectDefinition assigned.", this);
            context.ActionController.CompleteAction(context);
            return;
        }

        CharacterEffectManager effectManager =
            context.Player.GetComponent<CharacterEffectManager>();

        if (effectManager == null)
        {
            Debug.LogWarning(
                $"{context.Player.name} has no CharacterEffectManager.",
                context.Player
            );

            context.ActionController.CompleteAction(context);
            return;
        }

        effectManager.ApplyEffect(effectDefinition);

        // This action applies its effect immediately and doesn't
        // need to wait for the effect itself to expire.
        context.ActionController.CompleteAction(context);
    }

    public override void OnAnimationImpact(ActionContext context)
    {
        // The effect is applied when the action starts.
    }

    public override string GetDescription()
    {
        if (effectDefinition == null)
            return "Applies an effect.";

        return $"Applies {effectDefinition.DisplayName} for "
            + $"{effectDefinition.Duration:0.#} seconds.";
    }
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PushAction", menuName = "RPG/Actions/Attacks/Push")]
public class PushAction : AttackActionDefinition
{
    [Header("Push")]
    [SerializeField]
    private float knockbackDistance = 2f;

    public override void OnAnimationImpact(ActionContext context)
    {
        if (context?.Player == null)
            return;

        // Shared damage and effect application.
        PerformForwardHit(context);

        // Push is unique to this action.
        Collider[] hits = ActionHitDetector.DetectForward(
            context.Player,
            HitboxForwardOffset,
            HitboxRadius,
            TargetLayer,
            QueryTriggerInteraction.Ignore
        );

        Vector3 pushDirection = Vector3
            .ProjectOnPlane(context.Player.transform.forward, Vector3.up)
            .normalized;

        HashSet<IPushable> pushedTargets = new HashSet<IPushable>();

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            IPushable pushable = hit.GetComponent<IPushable>();

            if (pushable != null && pushedTargets.Add(pushable))
                pushable.Push(pushDirection, knockbackDistance);
        }
    }

    public override string GetDescription()
    {
        return $"Attacks forward and pushes enemies back " + $"{knockbackDistance:0.#}m.";
    }
}

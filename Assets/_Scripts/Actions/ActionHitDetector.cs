using UnityEngine;

public static class ActionHitDetector
{
    /// <summary>
    /// Detects all colliders inside a sphere positioned in front of the player.
    /// The sphere's height is based on Player.AimPosition.
    /// </summary>
    public static Collider[] DetectForward(
        Player player,
        float forwardOffset,
        float radius,
        LayerMask targetLayer,
        QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore
    )
    {
        if (player == null)
            return new Collider[0];

        Vector3 forward = Vector3.ProjectOnPlane(player.transform.forward, Vector3.up).normalized;

        if (forward.sqrMagnitude < 0.0001f)
            forward = player.transform.forward;

        Vector3 center = player.AimPosition + forward * forwardOffset;

        return Physics.OverlapSphere(center, radius, targetLayer, triggerInteraction);
    }

    /// <summary>
    /// Detects all colliders inside a sphere centered on the supplied world position.
    /// </summary>
    public static Collider[] DetectRadial(
        Vector3 center,
        float radius,
        LayerMask targetLayer,
        QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore
    )
    {
        return Physics.OverlapSphere(center, radius, targetLayer, triggerInteraction);
    }
}

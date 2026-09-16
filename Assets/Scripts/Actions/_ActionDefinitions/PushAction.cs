using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PushAction", menuName = "RPG/Actions/Push")]
public class PushAction : ActionDefinition
{
    [SerializeField]
    private int damage = 10;

    [SerializeField]
    private float attackRange = 1.5f;

    [Tooltip("The radius of the sphere cast for the attack.")]
    [SerializeField]
    private float attackRadius = 0.35f;

    [SerializeField]
    private float knockbackDistance = 2f;

    [SerializeField]
    private LayerMask targetLayer;

    public override IEnumerator Execute(ActionContext context)
    {
        context.Visuals.Push();

        yield return new WaitForSeconds(Duration * 0.2f);

        DealDamageAndPush(context);

        yield return new WaitForSeconds(Duration * 0.5f);
    }

    private void DealDamageAndPush(ActionContext context)
    {
        Vector3 origin = context.Player.transform.position;
        Vector3 direction = context.Player.transform.forward;

        if (
            !Physics.SphereCast(
                origin,
                attackRadius,
                direction,
                out RaycastHit hit,
                attackRange,
                targetLayer
            )
        )
        {
            return;
        }

        IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        IPushable pushable = hit.collider.GetComponentInParent<IPushable>();

        if (pushable != null)
        {
            pushable.Push(direction, knockbackDistance);
        }
    }
}

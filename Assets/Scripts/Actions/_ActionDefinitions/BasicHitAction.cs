using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "BasicHitAction",
    menuName = "RPG/Actions/Basic Hit"
)]
public class BasicHitAction : ActionDefinition
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange = 1.5f;
    [Tooltip("The radius of the sphere cast for the attack.")]
    [SerializeField] private float attackRadius = 0.35f;
    [SerializeField] private LayerMask targetLayer;

    public override IEnumerator Execute(ActionContext context)
    {
        context.Visuals.BasicHit();

        yield return new WaitForSeconds(Duration * 0.5f);

        DealDamage(context);

        yield return new WaitForSeconds(Duration * 0.5f);
    }

    private void DealDamage(ActionContext context)
    {
        Vector3 origin = context.Player.transform.position;
        Vector3 direction = context.Player.transform.forward;
        if (!Physics.SphereCast(
            origin,
            attackRadius,
            direction,
            out RaycastHit hit,
            attackRange,
            targetLayer))
    {
        return;
    }

        IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();

        if (damageable == null)
        {
            return;
        }

        damageable.TakeDamage(damage);
    }
}
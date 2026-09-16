using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "TornadoAction",
    menuName = "RPG/Actions/Tornado"
)]
public class TornadoAction : ActionDefinition
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float radius = 1.5f;
    [SerializeField] private LayerMask targetLayer;

    public override IEnumerator Execute(ActionContext context)
    {
        context.Visuals.Tornado();

        yield return new WaitForSeconds(Duration * 0.5f);

        DealDamage(context);

        yield return new WaitForSeconds(Duration * 0.5f);
    }

    private void DealDamage(ActionContext context)
    {
        Vector3 center = context.Player.transform.position;

        Collider[] hits = Physics.OverlapSphere(
            center,
            radius,
            targetLayer
        );

        foreach (Collider hit in hits)
        {
            IDamageable damageable =
                hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
            {
                continue;
            }

            damageable.TakeDamage(damage);
        }
    }
}
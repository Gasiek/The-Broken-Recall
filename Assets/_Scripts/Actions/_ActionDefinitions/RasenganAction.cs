using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Rasengan", menuName = "RPG/Actions/Rasengan")]
public class RasenganAction : ActionDefinition
{
    [SerializeField]
    private int damage = 30;

    [SerializeField]
    private float attackRange = 1.5f;

    [Tooltip("The radius of the sphere cast for the attack.")]
    [SerializeField]
    private float attackRadius = 0.35f;

    [SerializeField]
    private float knockbackDistance = 4f;

    [SerializeField]
    private LayerMask targetLayer;

    [Tooltip("Rasengan prefab spawned in the player's left hand for the duration of the attack.")]
    [SerializeField]
    private GameObject rasenganPrefab;

    [Tooltip("Scale at spawn, as a fraction of the prefab's scale. Grows to full size by the time of the hit.")]
    [SerializeField, Range(0f, 1f)]
    private float startScaleFactor = 0.1f;

    public override IEnumerator Execute(ActionContext context)
    {
        // ScriptableObject is a shared asset - keep the spawned instance local,
        // never in a field, so multiple users/executions don't overwrite each other.
        GameObject rasengan = SpawnRasengan(context);

        yield return GrowRasengan(rasengan, Duration * 0.5f);

        DealDamageAndPush(context);

        yield return new WaitForSeconds(Duration * 0.5f);

        DestroyRasengan(rasengan);
    }

    private void DealDamageAndPush(ActionContext context)
    {
        Vector3 origin = context.Player.position;
        Vector3 direction = context.Player.forward;

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
            CombatResolver.ResolveAttackHit(
                context.Player.gameObject,
                context.WeaponBuffs,
                damageable,
                damage,
                hit.point
            );
        }

        IPushable pushable = hit.collider.GetComponentInParent<IPushable>();

        if (pushable != null)
        {
            pushable.Push(direction, knockbackDistance);
        }
    }

    private GameObject SpawnRasengan(ActionContext context)
    {
        if (rasenganPrefab == null)
        {
            Debug.LogWarning($"[{nameof(RasenganAction)}] No Rasengan prefab assigned.", this);
        }

        Player player = context.Player.GetComponent<Player>();
        GameObject instance = Instantiate(rasenganPrefab, player.LeftHand, false);

        Destroy(instance, Duration);

        return instance;
    }

    private IEnumerator GrowRasengan(GameObject rasengan, float growDuration)
    {
        if (rasengan == null)
        {
            yield return new WaitForSeconds(growDuration);
            yield break;
        }

        Transform rasenganTransform = rasengan.transform;
        Vector3 targetScale = rasenganTransform.localScale;
        Vector3 startScale = targetScale * startScaleFactor;

        float elapsed = 0f;

        while (elapsed < growDuration)
        {
            if (rasengan == null)
                yield break;

            float t = elapsed / growDuration;
            rasenganTransform.localScale = Vector3.Lerp(startScale, targetScale, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rasenganTransform.localScale = targetScale;
    }

    private void DestroyRasengan(GameObject rasengan)
    {
        if (rasengan != null)
        {
            Destroy(rasengan);
        }
    }

    public override string GetDescription() =>
        $"Powerful ninjutsu, dealing {damage} damage and pushing the target back {knockbackDistance:0.#}m dattebayo.";
}

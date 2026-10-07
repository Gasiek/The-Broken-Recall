using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private LayerMask collisionMask;

    [SerializeField]
    private float collisionRadius = 0.1f;

    private Vector3 direction;

    private float speed;
    private int damage;
    private float lifetime;

    private float lifetimeTimer;

    public void Initialize(Vector3 direction, float speed, int damage, float lifetime)
    {
        this.direction = direction.normalized;
        this.speed = speed;
        this.damage = damage;
        this.lifetime = lifetime;

        lifetimeTimer = 0f;
    }

    private void Update()
    {
        lifetimeTimer += Time.deltaTime;

        if (lifetimeTimer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        float distance = speed * Time.deltaTime;

        if (
            Physics.SphereCast(
                transform.position,
                collisionRadius,
                direction,
                out RaycastHit hit,
                distance,
                collisionMask,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            HandleHit(hit);
            return;
        }

        transform.position += direction * distance;
    }

    private void HandleHit(RaycastHit hit)
    {
        IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}

using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private LayerMask collisionMask;

    [SerializeField]
    private float collisionRadius = 0.1f;

    [SerializeField]
    private float gizmoDirectionLength = 1f;

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
        IDamageable damageable = hit.collider.GetComponent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        // Preview the collision sphere.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collisionRadius);

        // Preview the movement direction.
        Vector3 drawDirection = Application.isPlaying ? direction : transform.forward;

        if (drawDirection.sqrMagnitude > 0.001f)
        {
            drawDirection.Normalize();

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(
                transform.position,
                transform.position + drawDirection * gizmoDirectionLength
            );
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(Health))]
public class HitVisuals : MonoBehaviour
{
    [Header("Hit VFX")]
    [SerializeField]
    private ParticleSystem hitParticlePrefab;

    [SerializeField]
    private Vector3 spawnOffset = new Vector3(0f, 1f, 0f);

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.DamageReceived += OnDamageReceived;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.DamageReceived -= OnDamageReceived;
        }
    }

    private void OnDamageReceived(int damage)
    {
        if (hitParticlePrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = transform.position + spawnOffset;
        ParticleSystem vfx = Instantiate(hitParticlePrefab, spawnPosition, Quaternion.identity);
        
        float duration = vfx.main.duration + vfx.main.startLifetime.constantMax;
        Destroy(vfx.gameObject, duration);
    }
}

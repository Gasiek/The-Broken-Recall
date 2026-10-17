using UnityEngine;

[RequireComponent(typeof(Health))]
public class PersistentEnemy : PersistentIdComponent
{
    private Health health;
    private bool subscribed;
    private bool initialized;
    private bool alreadyDeadInSession;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void Start()
    {
        // The spawner assigns runtime IDs immediately after Instantiate,
        // before Start runs.
        GameSession session = GameSession.Instance;

        if (session != null && session.IsEnemyDead(StableId))
        {
            alreadyDeadInSession = true;
            Destroy(gameObject);
            return;
        }

        initialized = true;

        SubscribeToDeath();

        // Covers enemies whose health was depleted before Start.
        if (health != null && health.IsDead)
            OnDied();
    }

    private void OnEnable()
    {
        // Start performs the first subscription after checking persistence.
        // After initialization, support disable/re-enable safely.
        if (initialized && !alreadyDeadInSession)
            SubscribeToDeath();
    }

    private void OnDisable()
    {
        UnsubscribeFromDeath();
    }

    private void SubscribeToDeath()
    {
        if (subscribed || health == null || alreadyDeadInSession)
            return;

        health.Died += OnDied;
        subscribed = true;
    }

    private void UnsubscribeFromDeath()
    {
        if (!subscribed)
            return;

        if (health != null)
            health.Died -= OnDied;

        subscribed = false;
    }

    private void OnDied()
    {
        if (string.IsNullOrWhiteSpace(StableId))
        {
            Debug.LogError(
                "[PersistentEnemy] Stable ID is missing; enemy death cannot be persisted.",
                this
            );
            return;
        }

        GameSession.Instance?.MarkEnemyDead(StableId);
    }

    private void OnDestroy()
    {
        UnsubscribeFromDeath();
    }
}

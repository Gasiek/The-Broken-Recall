using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class EnemyHealthUI : MonoBehaviour
{
    private const string EnemyHealthBarElementName = "EnemyHealthBar";
    private const string EnemyHealthFillElementName = "EnemyHealthFill";

    [Header("Targeting")]
    [Tooltip("Maximum distance from the player for an enemy health bar to appear.")]
    [SerializeField, Min(0.1f)]
    private float maxDistance = 5f;

    [Tooltip("Maximum angle from the player's forward direction.")]
    [SerializeField, Range(10f, 360f)]
    private float viewAngle = 140f;

    [SerializeField]
    private LayerMask enemyLayer;

    [Header("Visibility")]
    [Tooltip("How long the bar remains visible after the enemy leaves view.")]
    [SerializeField, Min(0f)]
    private float visibleDuration = 5f;

    [SerializeField, Min(0.02f)]
    private float scanInterval = 0.1f;

    private PanelRenderer panelRenderer;
    private VisualElement enemyHealthBar;
    private VisualElement enemyHealthFill;

    private GameSession session;
    private Player currentPlayer;
    private Transform playerTransform;
    private Health currentTargetHealth;

    private float visibleUntil;
    private float nextScanTime;

    private readonly Collider[] scanResults = new Collider[16];

    public Health CurrentTargetHealth => currentTargetHealth;

    private void Awake()
    {
        panelRenderer = GetComponent<PanelRenderer>();
    }

    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReload);

        session = GameSession.Instance;

        if (session == null)
        {
            Debug.LogError(
                "[EnemyHealthUI] GameSession.Instance is null. "
                    + "Make sure GameSession is created before this component.",
                this
            );
            return;
        }

        session.PlayerSpawned += OnPlayerSpawned;

        // Also handle the case where the player spawned before this UI enabled.
        if (session.CurrentPlayer != null)
            BindPlayer(session.CurrentPlayer);

        nextScanTime = 0f;
    }

    private void OnDisable()
    {
        if (panelRenderer != null)
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        if (session != null)
            session.PlayerSpawned -= OnPlayerSpawned;

        UnbindPlayer();

        enemyHealthBar = null;
        enemyHealthFill = null;
    }

    private void Update()
    {
        // The player may have been destroyed during a scene transition.
        if (playerTransform == null)
        {
            if (currentTargetHealth != null)
                SetTarget(null);

            return;
        }

        if (Time.time >= nextScanTime)
        {
            nextScanTime = Time.time + scanInterval;

            Health foundTarget = FindTargetEnemy();

            if (foundTarget != null)
            {
                // Refresh the visibility timer while a target remains in view.
                visibleUntil = Time.time + visibleDuration;

                if (foundTarget != currentTargetHealth)
                    SetTarget(foundTarget);
                else
                    UpdateVisibility();
            }
        }

        if (
            currentTargetHealth != null
            && (currentTargetHealth.IsDead || Time.time >= visibleUntil)
        )
        {
            SetTarget(null);
        }
    }

    private void OnPlayerSpawned(Player player)
    {
        BindPlayer(player);
    }

    private void BindPlayer(Player player)
    {
        if (currentPlayer == player)
            return;

        UnbindPlayer();

        currentPlayer = player;
        playerTransform = player != null ? player.transform : null;

        nextScanTime = 0f;
        visibleUntil = 0f;
    }

    private void UnbindPlayer()
    {
        SetTarget(null);

        currentPlayer = null;
        playerTransform = null;
        visibleUntil = 0f;
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        enemyHealthBar = root.Q<VisualElement>(EnemyHealthBarElementName);
        enemyHealthFill = root.Q<VisualElement>(EnemyHealthFillElementName);

        if (enemyHealthBar == null || enemyHealthFill == null)
        {
            Debug.LogError(
                "[EnemyHealthUI] Could not find 'EnemyHealthBar' or "
                    + "'EnemyHealthFill' in the UI document.",
                this
            );
            return;
        }

        Refresh();
        UpdateVisibility();
    }

    private Health FindTargetEnemy()
    {
        if (playerTransform == null)
            return null;

        int count = Physics.OverlapSphereNonAlloc(
            playerTransform.position,
            maxDistance,
            scanResults,
            enemyLayer
        );

        Health bestTarget = null;
        float closestDistanceSquared = float.MaxValue;

        Vector3 playerPosition = playerTransform.position;
        Vector3 forward = playerTransform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            return null;

        forward.Normalize();

        float minDot = Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad);

        for (int i = 0; i < count; i++)
        {
            Collider col = scanResults[i];

            if (col == null)
                continue;

            // Supports colliders placed on child objects.
            Health health = col.GetComponentInParent<Health>();

            if (health == null || health.IsDead)
                continue;

            Vector3 direction = health.transform.position - playerPosition;
            direction.y = 0f;

            float distanceSquared = direction.sqrMagnitude;

            if (distanceSquared < 0.001f)
                continue;

            direction.Normalize();

            if (Vector3.Dot(forward, direction) < minDot)
                continue;

            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                bestTarget = health;
            }
        }

        return bestTarget;
    }

    private void SetTarget(Health newTarget)
    {
        if (currentTargetHealth == newTarget)
        {
            Refresh();
            UpdateVisibility();
            return;
        }

        UnsubscribeFromTarget();

        currentTargetHealth = newTarget;

        if (currentTargetHealth != null)
        {
            currentTargetHealth.HealthChanged += OnTargetHealthChanged;
            currentTargetHealth.Died += OnTargetDied;
        }
        else
        {
            visibleUntil = 0f;
        }

        Refresh();
        UpdateVisibility();
    }

    private void UnsubscribeFromTarget()
    {
        if (currentTargetHealth == null)
            return;

        currentTargetHealth.HealthChanged -= OnTargetHealthChanged;
        currentTargetHealth.Died -= OnTargetDied;
    }

    private void OnTargetHealthChanged()
    {
        Refresh();
        UpdateVisibility();
    }

    private void OnTargetDied()
    {
        SetTarget(null);
    }

    private void Refresh()
    {
        if (enemyHealthFill == null)
            return;

        float ratio = 0f;

        if (currentTargetHealth != null && currentTargetHealth.MaxHealth > 0)
        {
            ratio = Mathf.Clamp01(
                (float)currentTargetHealth.CurrentHealth / currentTargetHealth.MaxHealth
            );
        }

        enemyHealthFill.style.width = Length.Percent(ratio * 100f);
    }

    private void UpdateVisibility()
    {
        if (enemyHealthBar == null)
            return;

        bool visible =
            playerTransform != null
            && currentTargetHealth != null
            && !currentTargetHealth.IsDead
            && Time.time < visibleUntil;

        enemyHealthBar.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}

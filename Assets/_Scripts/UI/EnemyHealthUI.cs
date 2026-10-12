using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class EnemyHealthUI : MonoBehaviour
{
    private const string EnemyHealthBarElementName = "EnemyHealthBar";
    private const string EnemyHealthFillElementName = "EnemyHealthFill";

    [Header("Targeting")]
    [SerializeField]
    private Transform playerTransform;

    [Tooltip("Maximum distance from player to enemy for the health bar to appear.")]
    [SerializeField, Min(0.1f)]
    private float maxDistance = 5f;

    [Tooltip("Maximum angle between player forward direction and enemy direction for the health bar to appear.")]
    [SerializeField, Range(10f, 360f)]
    private float viewAngle = 140f;

    [SerializeField]
    private LayerMask enemyLayer;

    [Header("Visibility")]
    [SerializeField, Min(0f)]
    private float visibleDuration = 5f;

    [SerializeField, Min(0.02f)]
    private float scanInterval = 0.1f;

    private PanelRenderer panelRenderer;
    private VisualElement enemyHealthBar;
    private VisualElement enemyHealthFill;

    private Health currentTargetHealth;
    private float visibleUntil;
    private float nextScanTime;

    private Collider[] scanResults = new Collider[16];

    public Health CurrentTargetHealth => currentTargetHealth;

    private void Awake()
    {
        panelRenderer = GetComponent<PanelRenderer>();
    }

    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        if (panelRenderer != null)
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        UnsubscribeFromTarget();

        enemyHealthBar = null;
        enemyHealthFill = null;
        currentTargetHealth = null;
    }

    private void Update()
    {
        if (playerTransform == null)
            return;

        if (Time.time >= nextScanTime)
        {
            nextScanTime = Time.time + scanInterval;

            Health target = FindTargetEnemy();

            if (target != null)
            {
                SetTarget(target);
                visibleUntil = Time.time + visibleDuration;
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

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        enemyHealthBar = root.Q<VisualElement>(EnemyHealthBarElementName);
        enemyHealthFill = root.Q<VisualElement>(EnemyHealthFillElementName);

        if (enemyHealthBar == null || enemyHealthFill == null)
        {
            Debug.LogError(
                "EnemyHealthUI could not find its health bar elements in the UXML.",
                this
            );
            return;
        }

        Refresh();
        UpdateVisibility();
    }

    private Health FindTargetEnemy()
    {
        int count = Physics.OverlapSphereNonAlloc(
            playerTransform.position,
            maxDistance,
            scanResults,
            enemyLayer
        );

        Health bestTarget = null;
        float closestDistance = float.MaxValue;

        Vector3 playerPosition = playerTransform.position;
        Vector3 forward = playerTransform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            return null;

        forward.Normalize();

        for (int i = 0; i < count; i++)
        {
            Collider col = scanResults[i];

            if (col == null)
                continue;

            // Assumes the collider and Health are on the same GameObject.
            Health health = col.GetComponent<Health>();

            if (health == null || health.IsDead)
                continue;

            Vector3 direction = health.transform.position - playerPosition;
            direction.y = 0f;

            float distance = direction.magnitude;

            if (distance < 0.001f)
                continue;

            if (Vector3.Angle(forward, direction) > viewAngle * 0.5f)
                continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
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
            currentTargetHealth != null && !currentTargetHealth.IsDead && Time.time < visibleUntil;

        enemyHealthBar.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}

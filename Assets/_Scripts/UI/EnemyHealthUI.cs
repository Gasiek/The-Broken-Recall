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
    [SerializeField]
    private float maxDistance = 5f;

    [Tooltip("Total viewing cone angle in degrees in front of the player (e.g. 140 means +/- 70 degrees).")]
    [SerializeField, Range(10f, 360f)]
    private float viewAngle = 140f;

    [Tooltip("Layer mask used to find enemy colliders.")]
    [SerializeField]
    private LayerMask enemyLayer;

    private PanelRenderer panelRenderer;
    private VisualElement enemyHealthBar;
    private VisualElement enemyHealthFill;

    private Health currentTargetHealth;

    public Health CurrentTargetHealth => currentTargetHealth;

    private void Awake()
    {
        ResolvePlayer();
        ResolveEnemyLayer();
    }

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        if (panelRenderer != null)
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        SetTarget(null);

        enemyHealthBar = null;
        enemyHealthFill = null;
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            ResolvePlayer();
            if (playerTransform == null)
                return;
        }

        Health bestTarget = FindTargetEnemy();

        if (bestTarget != currentTargetHealth)
        {
            SetTarget(bestTarget);
        }
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        enemyHealthBar = root.Q<VisualElement>(EnemyHealthBarElementName);
        enemyHealthFill = root.Q<VisualElement>(EnemyHealthFillElementName);

        if (enemyHealthBar == null)
        {
            Debug.LogError($"EnemyHealthUI could not find '{EnemyHealthBarElementName}' in HUD UXML.", this);
        }

        if (enemyHealthFill == null)
        {
            Debug.LogError($"EnemyHealthUI could not find '{EnemyHealthFillElementName}' in HUD UXML.", this);
        }

        Refresh();
        UpdateVisibility();
    }

    private Health FindTargetEnemy()
    {
        Collider[] colliders = Physics.OverlapSphere(playerTransform.position, maxDistance, enemyLayer);
        Health bestCandidate = null;
        float closestDistance = float.MaxValue;

        Vector3 playerPos = playerTransform.position;
        Vector3 playerForward = playerTransform.forward;
        playerForward.y = 0f;
        playerForward.Normalize();

        float halfAngle = viewAngle * 0.5f;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider col = colliders[i];
            if (col == null)
                continue;

            // Must have Health and not be dead
            Health health = col.GetComponentInParent<Health>();
            if (health == null || health.IsDead || health.transform == playerTransform)
                continue;

            // Must be an enemy
            EnemyController enemy = col.GetComponentInParent<EnemyController>();
            if (enemy == null)
                continue;

            Vector3 toEnemy = health.transform.position - playerPos;
            toEnemy.y = 0f;
            float distance = toEnemy.magnitude;

            if (distance > maxDistance || distance < 0.001f)
                continue;

            float angle = Vector3.Angle(playerForward, toEnemy);
            if (angle > halfAngle)
                continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestCandidate = health;
            }
        }

        return bestCandidate;
    }

    private void SetTarget(Health newTarget)
    {
        if (currentTargetHealth == newTarget)
            return;

        if (currentTargetHealth != null)
        {
            currentTargetHealth.HealthChanged -= OnTargetHealthChanged;
            currentTargetHealth.Died -= OnTargetDied;
        }

        currentTargetHealth = newTarget;

        if (currentTargetHealth != null)
        {
            currentTargetHealth.HealthChanged += OnTargetHealthChanged;
            currentTargetHealth.Died += OnTargetDied;
        }

        Refresh();
        UpdateVisibility();
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

        if (currentTargetHealth == null || currentTargetHealth.MaxHealth <= 0)
        {
            enemyHealthFill.style.width = Length.Percent(0f);
            return;
        }

        float ratio = Mathf.Clamp01((float)currentTargetHealth.CurrentHealth / currentTargetHealth.MaxHealth);
        enemyHealthFill.style.width = Length.Percent(ratio * 100f);
    }

    private void UpdateVisibility()
    {
        if (enemyHealthBar == null)
            return;

        bool isVisible = currentTargetHealth != null && !currentTargetHealth.IsDead;
        enemyHealthBar.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void ResolvePlayer()
    {
        if (playerTransform != null)
            return;

        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void ResolveEnemyLayer()
    {
        if (enemyLayer.value == 0)
        {
            int layer = LayerMask.NameToLayer("Enemy");
            if (layer >= 0)
            {
                enemyLayer = 1 << layer;
            }
            else
            {
                enemyLayer = ~0;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null)
            return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawWireSphere(playerTransform.position, maxDistance);

        Vector3 forward = playerTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 leftDir = Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * forward;
        Vector3 rightDir = Quaternion.Euler(0f, viewAngle * 0.5f, 0f) * forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(playerTransform.position, playerTransform.position + leftDir * maxDistance);
        Gizmos.DrawLine(playerTransform.position, playerTransform.position + rightDir * maxDistance);
    }
}

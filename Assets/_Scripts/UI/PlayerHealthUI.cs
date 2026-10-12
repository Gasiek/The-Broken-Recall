using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class PlayerHealthUI : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField]
    private Health health;

    private PanelRenderer panelRenderer;
    private VisualElement healthFill;

    private void Awake()
    {
        if (health == null)
            Debug.LogError("PlayerHealthUI has no Health assigned.", this);
    }

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);

        if (health != null)
            health.HealthChanged += Refresh;
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        if (health != null)
            health.HealthChanged -= Refresh;

        healthFill = null;
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        healthFill = root.Q<VisualElement>("HealthFill");

        if (healthFill == null)
            Debug.LogError("PlayerHealthUI could not find 'HealthFill' in the HUD UXML.", this);

        Refresh();
    }

    private void Refresh()
    {
        if (health == null || healthFill == null)
            return;

        float ratio =
            health.MaxHealth > 0
                ? Mathf.Clamp01((float)health.CurrentHealth / health.MaxHealth)
                : 0f;

        healthFill.style.width = Length.Percent(ratio * 100f);
    }
}

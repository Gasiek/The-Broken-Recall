using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class PlayerHealthUI : PlayerHUDBase
{
    private PanelRenderer panelRenderer;
    private VisualElement healthFill;
    private Health health;

    protected override void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();

        if (panelRenderer == null)
        {
            Debug.LogError("[PlayerHealthUI] PanelRenderer is missing.", this);
            return;
        }

        panelRenderer.RegisterUIReloadCallback(OnUIReload);

        base.OnEnable();
    }

    protected override void OnDisable()
    {
        if (panelRenderer != null)
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        base.OnDisable();

        healthFill = null;
    }

    protected override void OnPlayerBound(Player player)
    {
        UnsubscribeFromHealth();

        if (player == null)
            return;

        health = player.Health;

        if (health == null)
        {
            Debug.LogWarning(
                "[PlayerHealthUI] The bound player has no Health component reference.",
                player
            );
            Refresh();
            return;
        }

        health.HealthChanged += Refresh;
        Refresh();
    }

    protected override void OnPlayerUnbound(Player player)
    {
        UnsubscribeFromHealth();
    }

    private void UnsubscribeFromHealth()
    {
        if (health != null)
            health.HealthChanged -= Refresh;

        health = null;
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        healthFill = root != null ? root.Q<VisualElement>("HealthFill") : null;

        if (healthFill == null)
        {
            Debug.LogError("[PlayerHealthUI] Could not find 'HealthFill' in the HUD UXML.", this);
            return;
        }

        Refresh();
    }

    private void Refresh()
    {
        if (healthFill == null)
            return;

        if (health == null)
        {
            healthFill.style.width = Length.Percent(0f);
            return;
        }

        float ratio =
            health.MaxHealth > 0
                ? Mathf.Clamp01((float)health.CurrentHealth / health.MaxHealth)
                : 0f;

        healthFill.style.width = Length.Percent(ratio * 100f);
    }
}

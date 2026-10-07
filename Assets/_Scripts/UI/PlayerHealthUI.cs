using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField]
    private Health health;

    [SerializeField]
    private Image fillImage;

    private void Awake()
    {
        if (health == null)
            Debug.LogError("PlayerHealthUI has no Health assigned.");

        if (fillImage == null)
            Debug.LogError("PlayerHealthUI has no fill Image assigned.");
    }

    private void OnEnable()
    {
        if (health != null)
            health.HealthChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (health != null)
            health.HealthChanged -= Refresh;
    }

    private void Refresh()
    {
        if (health == null || fillImage == null)
            return;

        if (health.MaxHealth <= 0)
        {
            fillImage.fillAmount = 0f;
            return;
        }

        fillImage.fillAmount = (float)health.CurrentHealth / health.MaxHealth;
    }
}

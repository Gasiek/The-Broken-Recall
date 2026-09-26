using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    private int maxHealth;
    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvincible { get; private set; }

    public event Action HealthChanged;
    public event Action Died;

    public void Initialize(int maxHealth)
    {
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;
        IsInvincible = false;

        HealthChanged?.Invoke();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;

        if (currentHealth <= 0)
            return;

        if (IsInvincible)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log(
            $"{gameObject.name} took {amount} damage. " + $"Health: {currentHealth}/{maxHealth}"
        );

        HealthChanged?.Invoke();

        if (currentHealth <= 0)
            Die();
    }

    public void StartInvincibility()
    {
        IsInvincible = true;
    }

    public void StopInvincibility()
    {
        IsInvincible = false;
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died.");
        Died?.Invoke();
    }
}

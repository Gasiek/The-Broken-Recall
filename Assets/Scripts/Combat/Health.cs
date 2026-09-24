using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    private int maxHealth;
    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public event Action Died;

    public void Initialize(int maxHealth)
    {
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (currentHealth <= 0)
        {
            return;
        }

        currentHealth -= amount;

        Debug.Log(
            $"{gameObject.name} took {amount} damage. " + $"Health: {currentHealth}/{maxHealth}"
        );

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        currentHealth = 0;

        Debug.Log($"{gameObject.name} died.");

        Died?.Invoke();
    }
}

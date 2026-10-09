using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    private int maxHealth;
    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvincible { get; private set; }
    public bool IsDead => currentHealth <= 0;

    public event Action HealthChanged;
    public event Action Died;

    public event Action<int> DamageReceived;
    public event Action DamageBlocked;
    public event Action<int> HealthRestored;

    public void Initialize(int newMaxHealth)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);
        currentHealth = maxHealth;
        IsInvincible = false;

        HealthChanged?.Invoke();
    }

    /// <summary>
    /// Attempts to apply damage and returns the actual damage dealt.
    /// Returns zero if damage is invalid, blocked, or the target is dead.
    /// </summary>
    public int TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead)
            return 0;

        if (IsInvincible)
        {
            DamageBlocked?.Invoke();
            return 0;
        }

        int previousHealth = currentHealth;

        currentHealth = Mathf.Max(0, currentHealth - amount);

        int actualDamage = previousHealth - currentHealth;

        if (actualDamage <= 0)
            return 0;

        Debug.Log(
            $"{gameObject.name} took {actualDamage} damage. "
                + $"Health: {currentHealth}/{maxHealth}"
        );

        DamageReceived?.Invoke(actualDamage);
        HealthChanged?.Invoke();

        if (IsDead)
            Die();

        return actualDamage;
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead)
            return;

        int previousHealth = currentHealth;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        int actualHealing = currentHealth - previousHealth;

        if (actualHealing <= 0)
            return;

        Debug.Log(
            $"{gameObject.name} healed for {actualHealing}. "
                + $"Health: {currentHealth}/{maxHealth}"
        );

        HealthRestored?.Invoke(actualHealing);
        HealthChanged?.Invoke();
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
        Died?.Invoke();
        Debug.Log($"{gameObject.name} died.");
    }
}

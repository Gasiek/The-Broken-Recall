using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    private int maxHealth;
    private int currentHealth;
    private bool isInitialized;
    private bool deathEventRaised;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvincible { get; private set; }
    public bool IsDead => isInitialized && currentHealth <= 0;

    public event Action HealthChanged;
    public event Action Died;
    public event Action<int> DamageReceived;
    public event Action DamageBlocked;
    public event Action<int> HealthRestored;

    public void Initialize(int newMaxHealth)
    {
        if (isInitialized)
        {
            Debug.LogWarning(
                "[Health] Initialize was called more than once. "
                    + "Use RestoreHealth to set health after initialization.",
                this
            );
            return;
        }

        maxHealth = Mathf.Max(1, newMaxHealth);
        currentHealth = maxHealth;
        IsInvincible = false;
        isInitialized = true;
        deathEventRaised = false;

        HealthChanged?.Invoke();
    }

    public void RestoreHealth(int savedHealth)
    {
        if (!isInitialized)
        {
            Debug.LogError("[Health] Initialize health before restoring it.", this);
            return;
        }

        currentHealth = Mathf.Clamp(savedHealth, 0, maxHealth);

        if (currentHealth > 0)
            deathEventRaised = false;

        HealthChanged?.Invoke();

        if (currentHealth <= 0)
            Die();
    }

    public int TakeDamage(int amount)
    {
        if (!isInitialized || IsDead || amount <= 0)
            return 0;

        if (IsInvincible)
        {
            DamageBlocked?.Invoke();
            return 0;
        }

        int actualDamage = Mathf.Min(amount, currentHealth);
        currentHealth -= actualDamage;

        DamageReceived?.Invoke(actualDamage);
        HealthChanged?.Invoke();

        if (currentHealth <= 0)
            Die();

        return actualDamage;
    }

    public void Heal(int amount)
    {
        if (!isInitialized || IsDead || amount <= 0)
            return;

        int previousHealth = currentHealth;
        int missingHealth = maxHealth - currentHealth;

        currentHealth = amount >= missingHealth ? maxHealth : currentHealth + amount;

        int healedAmount = currentHealth - previousHealth;

        if (healedAmount <= 0)
            return;

        HealthRestored?.Invoke(healedAmount);
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
        if (!isInitialized || currentHealth > 0 || deathEventRaised)
            return;

        deathEventRaised = true;

        Died?.Invoke();
        Debug.Log($"{gameObject.name} has died.", this);
    }
}

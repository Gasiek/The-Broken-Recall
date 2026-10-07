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

    [Header("Audio")]
    [SerializeField]
    private AudioClip hitSound;

    [SerializeField]
    [Range(0f, 1f)]
    private float hitSoundVolume = 1f;

    [SerializeField]
    private AudioClip blockSound;

    [SerializeField]
    [Range(0f, 1f)]
    private float blockSoundVolume = 1f;

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;

        if (currentHealth <= 0)
            return;

        if (IsInvincible)
        {
            if (blockSound != null)
            {
                AudioSource.PlayClipAtPoint(blockSound, transform.position, blockSoundVolume);
            }
            return;
        }

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log(
            $"{gameObject.name} took {amount} damage. " + $"Health: {currentHealth}/{maxHealth}"
        );

        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, hitSoundVolume);
        }

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

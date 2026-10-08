using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health))]
public class StatusEffectManager : MonoBehaviour
{
    private class ActiveEffect
    {
        public StatusEffectDefinition Definition;
        public Coroutine Coroutine;
        public ParticleSystem VfxInstance;
        public float RemainingTime;
    }

    private Health health;
    private NavMeshAgent navAgent;
    private PlayerMovement playerMovement;

    private readonly Dictionary<string, ActiveEffect> activeEffects = new Dictionary<string, ActiveEffect>();
    private float defaultNavSpeed;
    private bool hasRecordedDefaultNavSpeed;

    private void Awake()
    {
        health = GetComponent<Health>();
        navAgent = GetComponent<NavMeshAgent>();
        playerMovement = GetComponent<PlayerMovement>();

        if (navAgent != null)
        {
            defaultNavSpeed = navAgent.speed;
            hasRecordedDefaultNavSpeed = true;
        }

        health.Died += HandleDeath;
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.Died -= HandleDeath;
        }
        ClearAllEffects();
    }

    public void ApplyStatus(StatusEffectDefinition definition)
    {
        if (definition == null || health == null || health.CurrentHealth <= 0)
        {
            return;
        }

        string effectId = string.IsNullOrEmpty(definition.Id) ? definition.name : definition.Id;

        // If effect already active, refresh duration
        if (activeEffects.TryGetValue(effectId, out ActiveEffect existing))
        {
            existing.RemainingTime = definition.Duration;
            return;
        }

        // Play apply audio
        if (definition.ApplySound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(definition.ApplySound, transform.position, definition.SoundVolume);
        }

        // Spawn VFX
        ParticleSystem vfxInstance = null;
        if (definition.VfxPrefab != null)
        {
            vfxInstance = Instantiate(definition.VfxPrefab, transform);
            vfxInstance.transform.localPosition = Vector3.up * 0.8f;
            vfxInstance.Play();
        }

        ActiveEffect effect = new ActiveEffect
        {
            Definition = definition,
            VfxInstance = vfxInstance,
            RemainingTime = definition.Duration
        };

        effect.Coroutine = StartCoroutine(ProcessEffectRoutine(effect, effectId));
        activeEffects.Add(effectId, effect);

        UpdateSpeedModifiers();
    }

    private IEnumerator ProcessEffectRoutine(ActiveEffect effect, string effectId)
    {
        StatusEffectDefinition def = effect.Definition;
        float tickTimer = 0f;

        while (effect.RemainingTime > 0f)
        {
            if (health == null || health.CurrentHealth <= 0)
            {
                break;
            }

            // Damage Over Time tick
            if (def.EffectType == StatusEffectType.Burn || def.EffectType == StatusEffectType.Poison)
            {
                tickTimer += Time.deltaTime;
                if (tickTimer >= def.TickInterval)
                {
                    tickTimer -= def.TickInterval;
                    health.TakeDamage(def.DamagePerTick);

                    if (def.TickSound != null && AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX(def.TickSound, transform.position, def.SoundVolume);
                    }
                }
            }

            effect.RemainingTime -= Time.deltaTime;
            yield return null;
        }

        RemoveEffect(effectId);
    }

    private void RemoveEffect(string effectId)
    {
        if (!activeEffects.TryGetValue(effectId, out ActiveEffect effect))
        {
            return;
        }

        if (effect.Coroutine != null)
        {
            StopCoroutine(effect.Coroutine);
        }

        if (effect.VfxInstance != null)
        {
            Destroy(effect.VfxInstance.gameObject);
        }

        activeEffects.Remove(effectId);
        UpdateSpeedModifiers();
    }

    private void UpdateSpeedModifiers()
    {
        float lowestSpeedMultiplier = 1.0f;

        foreach (var pair in activeEffects)
        {
            StatusEffectDefinition def = pair.Value.Definition;
            if (def.EffectType == StatusEffectType.Slow || def.EffectType == StatusEffectType.Freeze || def.EffectType == StatusEffectType.Stun)
            {
                if (def.SpeedMultiplier < lowestSpeedMultiplier)
                {
                    lowestSpeedMultiplier = def.SpeedMultiplier;
                }
            }
        }

        if (navAgent != null && hasRecordedDefaultNavSpeed)
        {
            navAgent.speed = defaultNavSpeed * lowestSpeedMultiplier;
        }

        if (playerMovement != null)
        {
            if (Mathf.Approximately(lowestSpeedMultiplier, 1.0f))
            {
                playerMovement.ResetSpeedMultiplier();
            }
            else
            {
                playerMovement.SetSpeedMultiplier(lowestSpeedMultiplier);
            }
        }
    }

    private void HandleDeath()
    {
        ClearAllEffects();
    }

    public void ClearAllEffects()
    {
        foreach (var pair in activeEffects)
        {
            if (pair.Value.Coroutine != null)
            {
                StopCoroutine(pair.Value.Coroutine);
            }
            if (pair.Value.VfxInstance != null)
            {
                Destroy(pair.Value.VfxInstance.gameObject);
            }
        }
        activeEffects.Clear();
        UpdateSpeedModifiers();
    }
}

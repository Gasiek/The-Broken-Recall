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

    private const float MinimumTickInterval = 0.05f;

    private Health health;
    private NavMeshAgent navAgent;
    private PlayerMovement playerMovement;
    private AudioManager audioManager;

    private readonly Dictionary<string, ActiveEffect> activeEffects =
        new Dictionary<string, ActiveEffect>();

    private float defaultNavSpeed;
    private bool hasRecordedDefaultNavSpeed;

    private void Awake()
    {
        health = GetComponent<Health>();
        health.Died += HandleDeath;

        navAgent = GetComponent<NavMeshAgent>();
        playerMovement = GetComponent<PlayerMovement>();

        if (navAgent != null)
        {
            defaultNavSpeed = navAgent.speed;
            hasRecordedDefaultNavSpeed = true;
        }
    }

    private void Start()
    {
        audioManager = AudioManager.Instance;
    }

    private void OnDisable()
    {
        ClearAllEffects();
    }

    private void OnDestroy()
    {
        health.Died -= HandleDeath;
    }

    public void ApplyStatus(StatusEffectDefinition definition)
    {
        if (definition == null || health.IsDead || !enabled)
            return;

        if (definition.Duration <= 0f)
            return;

        string effectId = string.IsNullOrWhiteSpace(definition.Id)
            ? definition.name
            : definition.Id;

        // Refresh an existing effect without restarting its coroutine.
        if (activeEffects.TryGetValue(effectId, out ActiveEffect existing))
        {
            existing.RemainingTime = definition.Duration;
            return;
        }

        PlaySound(definition.ApplySound, definition.SoundVolume);

        ParticleSystem vfxInstance = VfxSpawner.SpawnAttached(
            definition.VfxPrefab,
            transform,
            Vector3.up * 0.8f
        );

        ActiveEffect effect = new ActiveEffect
        {
            Definition = definition,
            VfxInstance = vfxInstance,
            RemainingTime = definition.Duration,
        };

        activeEffects.Add(effectId, effect);

        effect.Coroutine = StartCoroutine(ProcessEffectRoutine(effect, effectId));

        UpdateSpeedModifiers();
    }

    private IEnumerator ProcessEffectRoutine(ActiveEffect effect, string effectId)
    {
        StatusEffectDefinition definition = effect.Definition;

        float tickInterval = Mathf.Max(MinimumTickInterval, definition.TickInterval);

        float tickTimer = 0f;

        while (effect.RemainingTime > 0f)
        {
            if (health.IsDead)
                break;

            if (IsDamageOverTime(definition.EffectType))
            {
                tickTimer += Time.deltaTime;

                if (tickTimer >= tickInterval)
                {
                    tickTimer -= tickInterval;

                    if (definition.DamagePerTick > 0)
                    {
                        health.TakeDamage(definition.DamagePerTick);

                        // Damage may have killed the target.
                        if (health.IsDead)
                            break;

                        PlaySound(definition.TickSound, definition.SoundVolume);
                    }
                }
            }

            effect.RemainingTime -= Time.deltaTime;

            yield return null;
        }

        RemoveEffect(effectId);
    }

    private static bool IsDamageOverTime(StatusEffectType effectType)
    {
        return effectType == StatusEffectType.Burn || effectType == StatusEffectType.Poison;
    }

    private static bool IsMovementEffect(StatusEffectType effectType)
    {
        return effectType == StatusEffectType.Slow
            || effectType == StatusEffectType.Freeze
            || effectType == StatusEffectType.Stun;
    }

    private void RemoveEffect(string effectId)
    {
        if (!activeEffects.TryGetValue(effectId, out ActiveEffect effect))
            return;

        if (effect.Coroutine != null)
        {
            StopCoroutine(effect.Coroutine);
            effect.Coroutine = null;
        }

        if (effect.VfxInstance != null)
        {
            VfxSpawner.StopAndDestroy(effect.VfxInstance);
            effect.VfxInstance = null;
        }

        activeEffects.Remove(effectId);

        UpdateSpeedModifiers();
    }

    private void UpdateSpeedModifiers()
    {
        float lowestSpeedMultiplier = 1f;

        foreach (ActiveEffect effect in activeEffects.Values)
        {
            StatusEffectDefinition definition = effect.Definition;

            if (IsMovementEffect(definition.EffectType))
            {
                lowestSpeedMultiplier = Mathf.Min(
                    lowestSpeedMultiplier,
                    definition.SpeedMultiplier
                );
            }
        }

        if (navAgent != null && hasRecordedDefaultNavSpeed)
        {
            navAgent.speed = defaultNavSpeed * lowestSpeedMultiplier;
        }

        if (playerMovement != null)
        {
            playerMovement.SetSpeedMultiplier(lowestSpeedMultiplier);
        }
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip == null || audioManager == null)
            return;

        audioManager.PlayWorldSFX(clip, transform.position, volume);
    }

    private void HandleDeath()
    {
        ClearAllEffects();
    }

    public void ClearAllEffects()
    {
        foreach (ActiveEffect effect in activeEffects.Values)
        {
            if (effect.Coroutine != null)
                StopCoroutine(effect.Coroutine);

            if (effect.VfxInstance != null)
                VfxSpawner.StopAndDestroy(effect.VfxInstance);
        }

        activeEffects.Clear();

        UpdateSpeedModifiers();
    }
}

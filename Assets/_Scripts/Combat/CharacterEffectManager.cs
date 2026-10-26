using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(EffectVisualController))]
public class CharacterEffectManager : MonoBehaviour
{
    private sealed class ActiveEffect
    {
        public EffectDefinition Definition;
        public float RemainingTime;
    }

    private readonly Dictionary<string, ActiveEffect> activeEffects =
        new Dictionary<string, ActiveEffect>();

    private Health health;
    private NavMeshAgent navAgent;
    private PlayerMovement playerMovement;
    private EffectVisualController visualController;

    private float defaultNavSpeed;
    private bool hasRecordedDefaultNavSpeed;

    private float stunRemainingTime;
    private EffectDefinition activeStunDefinition;

    public bool IsStunned { get; private set; }
    public float StunRemainingTime => stunRemainingTime;

    public event Action<bool> StunStateChanged;

    public int ActiveEffectCount => activeEffects.Count;

    public float EffectSpeedMultiplier
    {
        get
        {
            float multiplier = 1f;

            foreach (ActiveEffect effect in activeEffects.Values)
            {
                if (effect.Definition != null && effect.Definition.Type == EffectType.StatModifier)
                {
                    multiplier *= effect.Definition.SpeedMultiplier;
                }
            }

            return multiplier;
        }
    }

    public float EffectDamageMultiplier
    {
        get
        {
            float multiplier = 1f;

            foreach (ActiveEffect effect in activeEffects.Values)
            {
                if (effect.Definition != null && effect.Definition.Type == EffectType.StatModifier)
                {
                    multiplier *= effect.Definition.DamageMultiplier;
                }
            }

            return multiplier;
        }
    }

    public int EffectBonusDamage
    {
        get
        {
            int bonus = 0;

            foreach (ActiveEffect effect in activeEffects.Values)
            {
                if (effect.Definition != null && effect.Definition.Type == EffectType.StatModifier)
                {
                    bonus += effect.Definition.BonusDamage;
                }
            }

            return bonus;
        }
    }

    private void Awake()
    {
        health = GetComponent<Health>();
        navAgent = GetComponent<NavMeshAgent>();
        playerMovement = GetComponent<PlayerMovement>();
        visualController = GetComponent<EffectVisualController>();

        health.Died += HandleDeath;

        if (navAgent != null)
        {
            defaultNavSpeed = navAgent.speed;
            hasRecordedDefaultNavSpeed = true;
        }

        UpdateMovementModifiers();
    }

    private void Update()
    {
        UpdateEffectTimers();
        UpdateStunTimer();
    }

    private void OnDisable()
    {
        ClearAllEffects();
    }

    private void OnDestroy()
    {
        if (health != null)
            health.Died -= HandleDeath;
    }

    public bool ApplyEffect(EffectDefinition definition)
    {
        if (definition == null || !enabled || health == null || health.IsDead)
        {
            return false;
        }

        if (definition.Type == EffectType.Stun)
        {
            if (definition.Duration <= 0f)
                return false;

            activeStunDefinition = definition;
            ApplyStun(definition.Duration);

            if (IsStunned)
            {
                visualController.ShowEffect(GetEffectKey(definition), definition);
            }

            return IsStunned;
        }

        if (definition.Duration <= 0f)
            return false;

        string effectKey = GetEffectKey(definition);

        if (activeEffects.TryGetValue(effectKey, out ActiveEffect existing))
        {
            existing.Definition = definition;
            existing.RemainingTime = definition.Duration;
        }
        else
        {
            activeEffects.Add(
                effectKey,
                new ActiveEffect { Definition = definition, RemainingTime = definition.Duration }
            );
        }

        // ShowEffect must not duplicate visuals when an effect is refreshed.
        visualController.ShowEffect(effectKey, definition);

        UpdateMovementModifiers();
        return true;
    }

    public bool HasEffect(string effectKey)
    {
        return !string.IsNullOrEmpty(effectKey) && activeEffects.ContainsKey(effectKey);
    }

    public bool RemoveEffect(string effectKey)
    {
        if (string.IsNullOrEmpty(effectKey) || !activeEffects.Remove(effectKey))
        {
            return false;
        }

        visualController.RemoveEffect(effectKey);
        UpdateMovementModifiers();

        return true;
    }

    public int CalculateDamage(int baseDamage)
    {
        float damage = (Mathf.Max(0, baseDamage) + EffectBonusDamage) * EffectDamageMultiplier;

        float nextHitMultiplier = 1f;
        int nextHitBonusDamage = 0;

        foreach (ActiveEffect activeEffect in activeEffects.Values)
        {
            EffectDefinition definition = activeEffect.Definition;

            if (definition == null || definition.Type != EffectType.NextHitBonus)
            {
                continue;
            }

            nextHitBonusDamage += definition.BonusDamage;
            nextHitMultiplier *= definition.DamageMultiplier;
        }

        damage = damage * nextHitMultiplier + nextHitBonusDamage;

        return Mathf.Max(0, Mathf.RoundToInt(damage));
    }

    public void ConsumeNextHitEffects()
    {
        List<string> effectKeysToRemove = new List<string>();

        foreach (KeyValuePair<string, ActiveEffect> pair in activeEffects)
        {
            EffectDefinition definition = pair.Value.Definition;

            if (definition != null && definition.Type == EffectType.NextHitBonus)
            {
                effectKeysToRemove.Add(pair.Key);
            }
        }

        foreach (string effectKey in effectKeysToRemove)
            RemoveEffect(effectKey);
    }

    public void ApplyStun(float duration)
    {
        if (!enabled || health == null || health.IsDead || duration <= 0f)
        {
            return;
        }

        stunRemainingTime = Mathf.Max(stunRemainingTime, duration);

        SetStunned(true);
        UpdateMovementModifiers();
    }

    public void ClearStun()
    {
        stunRemainingTime = 0f;

        if (activeStunDefinition != null)
        {
            visualController.RemoveEffect(GetEffectKey(activeStunDefinition));
        }

        activeStunDefinition = null;

        SetStunned(false);
        UpdateMovementModifiers();
    }

    private void UpdateEffectTimers()
    {
        if (activeEffects.Count == 0)
            return;

        List<string> expired = null;

        foreach (KeyValuePair<string, ActiveEffect> pair in activeEffects)
        {
            pair.Value.RemainingTime -= Time.deltaTime;

            if (pair.Value.RemainingTime <= 0f)
            {
                if (expired == null)
                    expired = new List<string>();

                expired.Add(pair.Key);
            }
        }

        if (expired == null)
            return;

        foreach (string key in expired)
        {
            activeEffects.Remove(key);
            visualController.RemoveEffect(key);
        }

        UpdateMovementModifiers();
    }

    private void UpdateStunTimer()
    {
        if (!IsStunned)
            return;

        stunRemainingTime -= Time.deltaTime;

        if (stunRemainingTime <= 0f)
            ClearStun();
    }

    private void SetStunned(bool stunned)
    {
        if (IsStunned == stunned)
            return;

        IsStunned = stunned;
        StunStateChanged?.Invoke(IsStunned);
    }

    private void UpdateMovementModifiers()
    {
        if (navAgent != null && hasRecordedDefaultNavSpeed)
        {
            navAgent.speed = defaultNavSpeed * EffectSpeedMultiplier;

            if (navAgent.isActiveAndEnabled && navAgent.isOnNavMesh)
                navAgent.isStopped = IsStunned;
        }

        if (playerMovement != null)
        {
            playerMovement.SetEffectSpeedMultiplier(IsStunned ? 0f : EffectSpeedMultiplier);
        }
    }

    private static string GetEffectKey(EffectDefinition definition)
    {
        return definition != null ? definition.name : string.Empty;
    }

    private void HandleDeath()
    {
        ClearAllEffects();
    }

    public void ClearAllEffects()
    {
        activeEffects.Clear();

        stunRemainingTime = 0f;
        activeStunDefinition = null;

        if (visualController != null)
            visualController.ClearAllVisuals();

        SetStunned(false);
        UpdateMovementModifiers();
    }

    public CharacterEffectSaveData CaptureState()
    {
        CharacterEffectSaveData data = new CharacterEffectSaveData
        {
            StunRemainingTime = Mathf.Max(0f, stunRemainingTime),
            StunDefinition = activeStunDefinition,
        };

        foreach (ActiveEffect effect in activeEffects.Values)
        {
            if (effect.Definition == null || effect.RemainingTime <= 0f)
                continue;

            data.Effects.Add(
                new SavedEffect
                {
                    Definition = effect.Definition,
                    RemainingTime = effect.RemainingTime,
                }
            );
        }

        return data;
    }

    public void RestoreState(CharacterEffectSaveData data)
    {
        if (data == null || health == null || health.IsDead)
            return;

        ClearAllEffects();

        if (data.Effects != null)
        {
            foreach (SavedEffect saved in data.Effects)
            {
                if (saved == null || saved.Definition == null || saved.RemainingTime <= 0f)
                {
                    continue;
                }

                if (!ApplyEffect(saved.Definition))
                    continue;

                string key = GetEffectKey(saved.Definition);

                if (activeEffects.TryGetValue(key, out ActiveEffect effect))
                    effect.RemainingTime = saved.RemainingTime;
            }
        }

        if (data.StunRemainingTime > 0f)
        {
            if (data.StunDefinition != null && data.StunDefinition.Type == EffectType.Stun)
            {
                ApplyEffect(data.StunDefinition);

                if (IsStunned)
                    stunRemainingTime = data.StunRemainingTime;
            }
            else
            {
                // Backward-compatible fallback for saves without a stun definition.
                ApplyStun(data.StunRemainingTime);
            }
        }

        UpdateMovementModifiers();
    }
}

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerActionVisuals))]
public class WeaponBuffManager : MonoBehaviour
{
    private PlayerActionVisuals visuals;
    private Health playerHealth;

    private WeaponBuffDefinition activeBuff;
    private ParticleSystem currentSwordVfx;
    private Coroutine durationCoroutine;

    // Fallback/custom fields for direct invocation
    private bool hasCustomBuff;
    private int customBonusDamage;
    private ParticleSystem customOnHitVfx;
    private AudioClip customOnHitSound;
    private float customVolume;

    public bool HasActiveBuff => activeBuff != null || hasCustomBuff;

    private void Awake()
    {
        visuals = GetComponent<PlayerActionVisuals>();
        playerHealth = GetComponent<Health>();
    }

    public void ApplyBuff(WeaponBuffDefinition definition)
    {
        if (definition == null)
        {
            return;
        }

        ClearActiveBuff();

        activeBuff = definition;
        hasCustomBuff = false;

        // Play apply sound
        if (definition.ApplySound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(definition.ApplySound, definition.SoundVolume);
        }

        // Attach sword VFX
        AttachSwordVfx(definition.SwordVfxPrefab);

        if (definition.Mode == WeaponBuffMode.Duration && definition.Duration > 0f)
        {
            durationCoroutine = StartCoroutine(DurationRoutine(definition.Duration));
        }
    }

    public void ApplyCustomBuff(
        int bonusDamage,
        ParticleSystem swordVfxPrefab,
        ParticleSystem onHitVfxPrefab,
        AudioClip dischargeSound,
        float soundVolume
    )
    {
        ClearActiveBuff();

        hasCustomBuff = true;
        customBonusDamage = bonusDamage;
        customOnHitVfx = onHitVfxPrefab;
        customOnHitSound = dischargeSound;
        customVolume = soundVolume;

        AttachSwordVfx(swordVfxPrefab);
    }

    private void AttachSwordVfx(ParticleSystem prefab)
    {
        if (prefab == null)
        {
            return;
        }

        Transform swordTransform = visuals != null && visuals.Sword != null
            ? visuals.Sword
            : transform;

        currentSwordVfx = Instantiate(prefab, swordTransform);
        currentSwordVfx.transform.localPosition = Vector3.zero;
        currentSwordVfx.transform.localRotation = Quaternion.identity;
        currentSwordVfx.Play();
    }

    public int ProcessHit(
        IDamageable targetDamageable,
        int baseDamage,
        Vector3 hitPoint,
        GameObject attacker
    )
    {
        if (!HasActiveBuff)
        {
            targetDamageable.TakeDamage(baseDamage);
            return baseDamage;
        }

        int totalDamage = baseDamage;

        if (activeBuff != null)
        {
            totalDamage = Mathf.RoundToInt((baseDamage + activeBuff.FlatBonusDamage) * activeBuff.DamageMultiplier);

            // On-hit VFX
            if (activeBuff.OnHitVfxPrefab != null)
            {
                ParticleSystem hitVfx = Instantiate(activeBuff.OnHitVfxPrefab, hitPoint, Quaternion.identity);
                hitVfx.Play();
                Destroy(hitVfx.gameObject, 2.0f);
            }

            // On-hit SFX
            if (activeBuff.OnHitSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(activeBuff.OnHitSound, hitPoint, activeBuff.SoundVolume);
            }

            // Lifesteal
            if (activeBuff.LifestealPercentage > 0f && playerHealth != null)
            {
                int healAmount = Mathf.Max(1, Mathf.RoundToInt(totalDamage * activeBuff.LifestealPercentage));
                playerHealth.Heal(healAmount);
            }

            // Status Effect on victim
            if (activeBuff.StatusEffectOnHit != null)
            {
                Component targetComponent = targetDamageable as Component;
                if (targetComponent != null)
                {
                    StatusEffectManager statusMgr = targetComponent.GetComponentInParent<StatusEffectManager>();
                    if (statusMgr == null)
                    {
                        statusMgr = targetComponent.gameObject.AddComponent<StatusEffectManager>();
                    }
                    statusMgr.ApplyStatus(activeBuff.StatusEffectOnHit);
                }
            }

            if (activeBuff.Mode == WeaponBuffMode.SingleHit)
            {
                ClearActiveBuff();
            }
        }
        else if (hasCustomBuff)
        {
            totalDamage = baseDamage + customBonusDamage;

            if (customOnHitVfx != null)
            {
                ParticleSystem hitVfx = Instantiate(customOnHitVfx, hitPoint, Quaternion.identity);
                hitVfx.Play();
                Destroy(hitVfx.gameObject, 2.0f);
            }

            if (customOnHitSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(customOnHitSound, hitPoint, customVolume);
            }

            ClearActiveBuff();
        }

        targetDamageable.TakeDamage(totalDamage);
        return totalDamage;
    }

    private IEnumerator DurationRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        ClearActiveBuff();
    }

    public void ClearActiveBuff()
    {
        if (durationCoroutine != null)
        {
            StopCoroutine(durationCoroutine);
            durationCoroutine = null;
        }

        if (currentSwordVfx != null)
        {
            currentSwordVfx.Stop();
            Destroy(currentSwordVfx.gameObject, 0.5f);
            currentSwordVfx = null;
        }

        activeBuff = null;
        hasCustomBuff = false;
        customBonusDamage = 0;
        customOnHitVfx = null;
        customOnHitSound = null;
    }
}

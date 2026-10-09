using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerActionVisuals))]
public class WeaponBuffManager : MonoBehaviour
{
    private PlayerActionVisuals visuals;
    private Health playerHealth;
    private AudioManager audioManager;
    private WeaponBuffDefinition activeBuff;
    private ParticleSystem currentSwordVfx;
    private Coroutine durationCoroutine;
    private bool hasCustomBuff;
    private int customBonusDamage;
    private ParticleSystem customOnHitVfx;
    private AudioClip customOnHitSound;
    private float customVolume = 1f;
    public bool HasActiveBuff => activeBuff != null || hasCustomBuff;

    private void Awake()
    {
        visuals = GetComponent<PlayerActionVisuals>();
        playerHealth = GetComponent<Health>();
    }

    private void Start()
    {
        audioManager = AudioManager.Instance;
    }

    public void ApplyBuff(WeaponBuffDefinition definition)
    {
        if (definition == null)
            return;
        ClearActiveBuff();
        activeBuff = definition;
        hasCustomBuff = false;
        if (definition.ApplySound != null && audioManager != null)
        {
            audioManager.PlayWorldSFX(
                definition.ApplySound,
                transform.position,
                definition.SoundVolume
            );
        }
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
        customVolume = Mathf.Clamp01(soundVolume);
        AttachSwordVfx(swordVfxPrefab);
    }

    public int CalculateDamage(int baseDamage)
    {
        if (activeBuff != null)
        {
            return Mathf.Max(
                0,
                Mathf.RoundToInt(
                    (baseDamage + activeBuff.FlatBonusDamage) * activeBuff.DamageMultiplier
                )
            );
        }
        if (hasCustomBuff)
            return Mathf.Max(0, baseDamage + customBonusDamage);
        return Mathf.Max(0, baseDamage);
    }

    public void ProcessSuccessfulHit(
        IDamageable targetDamageable,
        Vector3 hitPoint,
        int actualDamage
    )
    {
        if (!HasActiveBuff || actualDamage <= 0)
            return;
        if (activeBuff != null)
        {
            ProcessDefinitionBuffHit(targetDamageable, hitPoint, actualDamage);
            if (activeBuff != null && activeBuff.Mode == WeaponBuffMode.SingleHit)
            {
                ClearActiveBuff();
            }
        }
        else if (hasCustomBuff)
        {
            PlayHitVfx(customOnHitVfx, hitPoint);
            PlayHitSound(customOnHitSound, hitPoint, customVolume);
            ClearActiveBuff();
        }
    }

    private void ProcessDefinitionBuffHit(
        IDamageable targetDamageable,
        Vector3 hitPoint,
        int actualDamage
    )
    {
        if (activeBuff == null)
            return;
        PlayHitVfx(activeBuff.OnHitVfxPrefab, hitPoint);
        PlayHitSound(activeBuff.OnHitSound, hitPoint, activeBuff.SoundVolume);
        if (activeBuff.LifestealPercentage > 0f && playerHealth != null)
        {
            int healAmount = Mathf.RoundToInt(actualDamage * activeBuff.LifestealPercentage);
            if (healAmount > 0)
                playerHealth.Heal(healAmount);
        }
        if (activeBuff.StatusEffectOnHit != null && targetDamageable is Component targetComponent)
        {
            StatusEffectManager statusManager = targetComponent.GetComponent<StatusEffectManager>();
            if (statusManager != null)
            {
                statusManager.ApplyStatus(activeBuff.StatusEffectOnHit);
            }
        }
    }

    private void PlayHitVfx(ParticleSystem prefab, Vector3 hitPoint)
    {
        if (prefab == null)
            return;
        ParticleSystem vfx = Instantiate(prefab, hitPoint, Quaternion.identity);
        vfx.Play();
        Destroy(vfx.gameObject, 2f);
    }

    private void PlayHitSound(AudioClip clip, Vector3 hitPoint, float volume)
    {
        if (clip == null || audioManager == null)
            return;
        audioManager.PlayWorldSFX(clip, hitPoint, volume);
    }

    private void AttachSwordVfx(ParticleSystem prefab)
    {
        if (prefab == null)
            return;
        Transform parent = visuals != null && visuals.Sword != null ? visuals.Sword : transform;
        currentSwordVfx = Instantiate(prefab, parent);
        currentSwordVfx.transform.localPosition = Vector3.zero;
        currentSwordVfx.transform.localRotation = Quaternion.identity;
        currentSwordVfx.Play();
    }

    private IEnumerator DurationRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        durationCoroutine = null;
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
        customVolume = 1f;
    }
}

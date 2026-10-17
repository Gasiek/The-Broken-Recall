using UnityEngine;

[RequireComponent(typeof(PlayerActionVisuals))]
public class WeaponBuffManager : MonoBehaviour
{
    private PlayerActionVisuals visuals;
    private Health playerHealth;
    private AudioManager audioManager;

    private WeaponBuffDefinition activeBuff;
    private ParticleSystem currentSwordVfx;

    private bool hasCustomBuff;
    private int customBonusDamage;
    private ParticleSystem customSwordVfx;
    private ParticleSystem customOnHitVfx;
    private AudioClip customOnHitSound;
    private float customVolume = 1f;

    private float remainingDuration;
    private bool buffTimerRunning;

    public bool HasActiveBuff => activeBuff != null || hasCustomBuff;
    public float RemainingDuration => remainingDuration;

    private void Awake()
    {
        visuals = GetComponent<PlayerActionVisuals>();
        playerHealth = GetComponent<Health>();
    }

    private void Start()
    {
        audioManager = AudioManager.Instance;
    }

    private void Update()
    {
        if (!buffTimerRunning || !HasActiveBuff)
            return;

        remainingDuration -= Time.deltaTime;

        if (remainingDuration <= 0f)
        {
            remainingDuration = 0f;
            ClearActiveBuff();
        }
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
            remainingDuration = definition.Duration;
            buffTimerRunning = true;
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
        customSwordVfx = swordVfxPrefab;
        customOnHitVfx = onHitVfxPrefab;
        customOnHitSound = dischargeSound;
        customVolume = Mathf.Clamp01(soundVolume);

        // Custom buffs have no timed duration in the existing API.
        remainingDuration = 0f;
        buffTimerRunning = false;

        AttachSwordVfx(customSwordVfx);
    }

    public void PauseBuffTimer()
    {
        buffTimerRunning = false;
    }

    public void ResumeBuffTimer()
    {
        if (
            activeBuff != null
            && activeBuff.Mode == WeaponBuffMode.Duration
            && remainingDuration > 0f
        )
        {
            buffTimerRunning = true;
        }
    }

    public WeaponBuffSaveData CaptureState()
    {
        return new WeaponBuffSaveData
        {
            Definition = activeBuff,
            HasCustomBuff = hasCustomBuff,
            CustomBonusDamage = customBonusDamage,
            CustomSwordVfx = customSwordVfx,
            CustomOnHitVfx = customOnHitVfx,
            CustomOnHitSound = customOnHitSound,
            CustomVolume = customVolume,
            RemainingDuration = remainingDuration,
        };
    }

    public void RestoreState(WeaponBuffSaveData data)
    {
        ClearActiveBuff();

        if (data == null)
            return;

        activeBuff = data.Definition;
        hasCustomBuff = data.HasCustomBuff;
        customBonusDamage = data.CustomBonusDamage;
        customSwordVfx = data.CustomSwordVfx;
        customOnHitVfx = data.CustomOnHitVfx;
        customOnHitSound = data.CustomOnHitSound;
        customVolume = Mathf.Clamp01(data.CustomVolume);
        remainingDuration = Mathf.Max(0f, data.RemainingDuration);

        if (hasCustomBuff)
        {
            // Restore the custom buff's own sword VFX.
            buffTimerRunning = false;
            AttachSwordVfx(customSwordVfx);
        }
        else
        {
            if (activeBuff != null)
                AttachSwordVfx(activeBuff.SwordVfxPrefab);

            buffTimerRunning =
                activeBuff != null
                && activeBuff.Mode == WeaponBuffMode.Duration
                && remainingDuration > 0f;
        }
    }

    public int CalculateDamage(int baseDamage)
    {
        if (hasCustomBuff)
            return Mathf.Max(0, baseDamage + customBonusDamage);

        if (activeBuff == null)
            return baseDamage;

        float modifiedDamage =
            (baseDamage + activeBuff.FlatBonusDamage) * activeBuff.DamageMultiplier;

        return Mathf.Max(0, Mathf.RoundToInt(modifiedDamage));
    }

    public void ProcessSuccessfulHit(
        IDamageable targetDamageable,
        Vector3 hitPoint,
        int actualDamage
    )
    {
        if (hasCustomBuff)
        {
            PlayHitVfx(customOnHitVfx, hitPoint);
            PlayHitSound(customOnHitSound, hitPoint, customVolume);

            // The existing caller controls consumption of custom buffs.
            return;
        }

        if (activeBuff == null)
            return;

        ProcessDefinitionBuffHit(targetDamageable, hitPoint, actualDamage);
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

        if (activeBuff.LifestealPercentage > 0f && playerHealth != null && actualDamage > 0)
        {
            int healAmount = Mathf.RoundToInt(actualDamage * activeBuff.LifestealPercentage);

            if (healAmount > 0)
                playerHealth.Heal(healAmount);
        }

        // Keep status effects in the existing hit-processing pipeline.
    }

    private void PlayHitVfx(ParticleSystem prefab, Vector3 position)
    {
        if (prefab == null)
            return;

        VfxSpawner.SpawnOneShot(prefab, position, Quaternion.identity);
    }

    private void PlayHitSound(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null || audioManager == null)
            return;

        audioManager.PlayWorldSFX(clip, position, volume);
    }

    private void AttachSwordVfx(ParticleSystem prefab)
    {
        if (currentSwordVfx != null)
        {
            VfxSpawner.StopAndDestroy(currentSwordVfx, 0.5f);
            currentSwordVfx = null;
        }

        if (prefab == null || visuals == null)
            return;

        Transform parent = visuals.Sword != null ? visuals.Sword : transform;

        currentSwordVfx = VfxSpawner.SpawnAttached(prefab, parent, Vector3.zero);
    }

    public void ClearActiveBuff()
    {
        if (currentSwordVfx != null)
        {
            VfxSpawner.StopAndDestroy(currentSwordVfx, 0.5f);
            currentSwordVfx = null;
        }

        activeBuff = null;
        hasCustomBuff = false;
        customBonusDamage = 0;
        customSwordVfx = null;
        customOnHitVfx = null;
        customOnHitSound = null;
        customVolume = 1f;

        remainingDuration = 0f;
        buffTimerRunning = false;
    }
}

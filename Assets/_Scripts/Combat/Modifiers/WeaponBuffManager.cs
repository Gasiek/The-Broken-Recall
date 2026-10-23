using UnityEngine;

[RequireComponent(typeof(Player))]
public class WeaponBuffManager : MonoBehaviour
{
    [Header("Weapon Vfx Offset")]
    [SerializeField]
    private Vector3 vfxRotationOffset = new Vector3(90f, 0f, 0f);

    [SerializeField]
    private Vector3 vfxPositionOffset = Vector3.zero;

    [SerializeField]
    private Vector3 vfxScale = Vector3.one;

    [SerializeField]
    private Transform weapon;

    private Player player;
    private Health playerHealth;
    private AudioManager audioManager;

    private WeaponBuffDefinition activeBuff;
    private ParticleSystem currentSwordVfx;
    private LightningVfx currentSwordLightningVfx;

    private bool hasCustomBuff;
    private int customBonusDamage;
    private LightningVfx customSwordLightningVfx;
    private ParticleSystem customOnHitVfx;
    private AudioClip customOnHitSound;
    private float customVolume = 1f;

    private float remainingDuration;
    private bool buffTimerRunning;

    public bool HasActiveBuff => activeBuff != null || hasCustomBuff;
    public float RemainingDuration => remainingDuration;

    private void Awake()
    {
        player = GetComponent<Player>();
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
        LightningVfx swordLightningVfxPrefab,
        ParticleSystem onHitVfxPrefab,
        AudioClip dischargeSound,
        float soundVolume
    )
    {
        ClearActiveBuff();

        hasCustomBuff = true;
        customBonusDamage = bonusDamage;
        customSwordLightningVfx = swordLightningVfxPrefab;
        customOnHitVfx = onHitVfxPrefab;
        customOnHitSound = dischargeSound;
        customVolume = Mathf.Clamp01(soundVolume);

        remainingDuration = 0f;
        buffTimerRunning = false;

        AttachSwordLightningVfx(swordLightningVfxPrefab);
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
            CustomSwordLightningVfx = customSwordLightningVfx,
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
        customSwordLightningVfx = data.CustomSwordLightningVfx;
        customOnHitVfx = data.CustomOnHitVfx;
        customOnHitSound = data.CustomOnHitSound;
        customVolume = Mathf.Clamp01(data.CustomVolume);
        remainingDuration = Mathf.Max(0f, data.RemainingDuration);

        if (hasCustomBuff)
        {
            buffTimerRunning = false;
            AttachSwordLightningVfx(customSwordLightningVfx);
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
        if (!HasActiveBuff || actualDamage <= 0)
            return;

        if (activeBuff != null)
        {
            ProcessDefinitionBuffHit(targetDamageable, hitPoint, actualDamage);

            if (activeBuff != null && activeBuff.Mode == WeaponBuffMode.SingleHit)
                ClearActiveBuff();
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

        if (activeBuff.LifestealPercentage > 0f && playerHealth != null && actualDamage > 0)
        {
            int healAmount = Mathf.RoundToInt(actualDamage * activeBuff.LifestealPercentage);

            if (healAmount > 0)
                playerHealth.Heal(healAmount);
        }

        if (activeBuff.StatusEffectOnHit != null && targetDamageable is Component targetComponent)
        {
            StatusEffectManager statusManager = targetComponent.GetComponent<StatusEffectManager>();

            if (statusManager != null)
                statusManager.ApplyStatus(activeBuff.StatusEffectOnHit);
        }
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

        if (prefab == null || player == null || weapon == null)
            return;

        currentSwordVfx = VfxSpawner.SpawnAttached(
            prefab,
            weapon,
            vfxPositionOffset,
            Quaternion.Euler(vfxRotationOffset),
            vfxScale
        );
    }

    private void AttachSwordLightningVfx(LightningVfx vfxPrefab)
    {
        if (currentSwordLightningVfx != null)
        {
            Destroy(currentSwordLightningVfx.gameObject);
            currentSwordLightningVfx = null;
        }

        if (vfxPrefab == null || player == null || weapon == null)
            return;

        currentSwordLightningVfx = Instantiate(vfxPrefab, weapon);

        Transform lightningTransform = currentSwordLightningVfx.transform;
        lightningTransform.localPosition = vfxPositionOffset;
        lightningTransform.localRotation = Quaternion.Euler(vfxRotationOffset);
        lightningTransform.localScale = vfxScale;
    }

    public void ClearActiveBuff()
    {
        if (currentSwordVfx != null)
        {
            VfxSpawner.StopAndDestroy(currentSwordVfx, 0.5f);
            currentSwordVfx = null;
        }

        if (currentSwordLightningVfx != null)
        {
            Destroy(currentSwordLightningVfx.gameObject);
            currentSwordLightningVfx = null;
        }

        activeBuff = null;
        hasCustomBuff = false;
        customBonusDamage = 0;
        customSwordLightningVfx = null;
        customOnHitVfx = null;
        customOnHitSound = null;
        customVolume = 1f;

        remainingDuration = 0f;
        buffTimerRunning = false;
    }
}

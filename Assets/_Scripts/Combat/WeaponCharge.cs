using UnityEngine;

/// <summary>
/// Backward-compatibility wrapper that delegates to WeaponBuffManager.
/// </summary>
public class WeaponCharge : MonoBehaviour
{
    private WeaponBuffManager buffManager;

    private void Awake()
    {
        EnsureBuffManager();
    }

    private void EnsureBuffManager()
    {
        if (buffManager == null)
        {
            buffManager = GetComponent<WeaponBuffManager>();
            if (buffManager == null)
            {
                buffManager = gameObject.AddComponent<WeaponBuffManager>();
            }
        }
    }

    public bool IsCharged
    {
        get
        {
            EnsureBuffManager();
            return buffManager != null && buffManager.HasActiveBuff;
        }
    }

    public void ApplyCharge(
        int bonusDamage,
        ParticleSystem swordVfxPrefab,
        ParticleSystem hitVfxPrefab = null,
        AudioClip onDischargeSound = null,
        float soundVolume = 0.8f
    )
    {
        EnsureBuffManager();
        buffManager.ApplyCustomBuff(
            bonusDamage,
            swordVfxPrefab,
            hitVfxPrefab,
            onDischargeSound,
            soundVolume
        );
    }
}

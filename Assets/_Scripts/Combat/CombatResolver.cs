using UnityEngine;

public static class CombatResolver
{
    /// <summary>
    /// Resolves an attack hit from the player against a damageable target.
    /// Automatically applies active weapon buffs, damage bonuses, lifesteal,
    /// on-hit VFX/SFX, and status effects without attack actions needing any custom code.
    /// </summary>
    public static int ResolvePlayerAttackHit(
        GameObject attacker,
        IDamageable targetDamageable,
        int baseDamage,
        Vector3 hitPoint
    )
    {
        if (targetDamageable == null)
        {
            return 0;
        }

        if (attacker != null && attacker.TryGetComponent(out WeaponBuffManager buffManager) && buffManager.HasActiveBuff)
        {
            return buffManager.ProcessHit(targetDamageable, baseDamage, hitPoint, attacker);
        }

        targetDamageable.TakeDamage(baseDamage);
        return baseDamage;
    }
}

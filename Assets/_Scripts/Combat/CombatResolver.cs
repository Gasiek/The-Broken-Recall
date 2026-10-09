using UnityEngine;

public static class CombatResolver
{
    public static int ResolveAttackHit(
        GameObject attacker,
        WeaponBuffManager buffManager,
        IDamageable targetDamageable,
        int baseDamage,
        Vector3 hitPoint
    )
    {
        if (attacker == null || targetDamageable == null)
            return 0;

        bool hadActiveBuff = buffManager != null && buffManager.HasActiveBuff;

        int damageToApply = hadActiveBuff
            ? buffManager.CalculateDamage(baseDamage)
            : Mathf.Max(0, baseDamage);

        int actualDamage = targetDamageable.TakeDamage(damageToApply);

        if (actualDamage > 0 && hadActiveBuff)
        {
            buffManager.ProcessSuccessfulHit(targetDamageable, hitPoint, actualDamage);
        }

        return actualDamage;
    }
}

using UnityEngine;

public static class CombatResolver
{
    public static int ResolveAttackHit(
        GameObject attacker,
        IDamageable targetDamageable,
        int baseDamage,
        Vector3 hitPoint
    )
    {
        if (attacker == null || targetDamageable == null)
            return 0;

        CharacterEffectManager attackerEffects = attacker.GetComponent<CharacterEffectManager>();

        int damageToApply =
            attackerEffects != null
                ? attackerEffects.CalculateDamage(baseDamage)
                : Mathf.Max(0, baseDamage);

        int actualDamage = targetDamageable.TakeDamage(damageToApply);

        // Next-hit effects are consumed only when the hit deals damage.
        if (actualDamage > 0 && attackerEffects != null)
            attackerEffects.ConsumeNextHitEffects();

        return actualDamage;
    }
}

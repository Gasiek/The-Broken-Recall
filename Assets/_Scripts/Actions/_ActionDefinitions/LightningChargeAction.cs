using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "LightningChargeAction", menuName = "RPG/Actions/Lightning Charge")]
public class LightningChargeAction : ActionDefinition
{
    [Header("Charge Combat")]
    [SerializeField]
    private int bonusDamage = 10;

    [Header("Visual Effects")]
    [Tooltip("LineRenderer lightning attached to the weapon while charged.")]
    [SerializeField]
    private Lightning swordLightningPrefab;

    [Tooltip("Burst of lightning sparks spawned at the hit point when consuming the charge.")]
    [SerializeField]
    private ParticleSystem onHitDischargeVfxPrefab;

    [Header("Audio")]
    [SerializeField]
    private AudioClip chargeSound;

    [SerializeField]
    private AudioClip dischargeSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float soundVolume = 0.8f;

    public override IEnumerator Execute(ActionContext context)
    {
        if (context == null || context.Player == null)
        {
            yield break;
        }

        // 1. Play charge sound at the player's position
        if (chargeSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWorldSFX(
                chargeSound,
                context.Player.transform.position,
                soundVolume
            );
        }

        // 2. Apply lightning charge to player's weapon
        if (context.WeaponBuffs != null)
        {
            context.WeaponBuffs.ApplyCustomBuff(
                bonusDamage,
                swordLightningPrefab,
                onHitDischargeVfxPrefab,
                dischargeSound,
                soundVolume
            );
        }

        yield return new WaitForSeconds(Duration);
    }

    public override string GetDescription() =>
        $"Charges the weapon with lightning, adding +{bonusDamage} bonus damage to the next attack.";
}

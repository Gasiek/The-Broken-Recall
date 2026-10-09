using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "LightningChargeAction", menuName = "RPG/Actions/Lightning Charge")]
public class LightningChargeAction : ActionDefinition
{
    [Header("Charge Combat")]
    [SerializeField]
    private int bonusDamage = 10;

    [Header("Visual Effects")]
    [Tooltip("Looping lightning particles attached to the sword while charged.")]
    [SerializeField]
    private ParticleSystem swordLightningVfxPrefab;

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

        // 1. Play sword charge animation
        if (context.Visuals != null)
        {
            context.Visuals.ChargeSword(Duration);
        }

        // 2. Play charge sound at the player's position
        if (chargeSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWorldSFX(
                chargeSound,
                context.Player.transform.position,
                soundVolume
            );
        }

        // 3. Apply lightning charge to player's weapon
        WeaponBuffManager buffManager = context.Player.GetComponent<WeaponBuffManager>();
        if (buffManager == null)
        {
            buffManager = context.Player.gameObject.AddComponent<WeaponBuffManager>();
        }

        buffManager.ApplyCustomBuff(
            bonusDamage,
            swordLightningVfxPrefab,
            onHitDischargeVfxPrefab,
            dischargeSound,
            soundVolume
        );

        yield return new WaitForSeconds(Duration);
    }
}

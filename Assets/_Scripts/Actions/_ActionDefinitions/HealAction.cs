using UnityEngine;

[CreateAssetMenu(fileName = "HealAction", menuName = "RPG/Actions/Heal")]
public class HealAction : ActionDefinition
{
    [Header("Healing")]
    [SerializeField]
    private int healAmount = 25;

    [Header("Visual Effects")]
    [SerializeField]
    private ParticleSystem healVfxPrefab;

    [SerializeField]
    private Vector3 vfxOffset = Vector3.zero;

    public override void OnAnimationImpact(ActionContext context)
    {
        if (context == null)
            return;

        if (context.Health != null)
            context.Health.Heal(healAmount);

        if (healVfxPrefab != null && context.Player != null)
        {
            Vector3 spawnPosition = context.Player.transform.position + vfxOffset;

            VfxSpawner.SpawnOneShot(
                healVfxPrefab,
                spawnPosition,
                context.Player.transform.rotation,
                context.Player.transform
            );
        }
    }

    public override string GetDescription() => $"Restores {healAmount} health instantly.";
}

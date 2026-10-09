using System.Collections;
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

    public override IEnumerator Execute(ActionContext context)
    {
        if (context == null)
            yield break;

        if (context.Health != null)
        {
            context.Health.Heal(healAmount);
        }

        if (healVfxPrefab != null && context.Player != null)
        {
            Vector3 spawnPosition = context.Player.position + vfxOffset;

            VfxSpawner.SpawnOneShot(
                healVfxPrefab,
                spawnPosition,
                context.Player.rotation,
                context.Player
            );
        }

        yield return new WaitForSeconds(Duration);
    }
}

using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ApplyWeaponBuffAction",
    menuName = "RPG/Actions/Apply Weapon Buff"
)]
public class ApplyWeaponBuffAction : ActionDefinition
{
    [Header("Weapon Buff Configuration")]
    [SerializeField]
    private WeaponBuffDefinition buffDefinition;

    public WeaponBuffDefinition BuffDefinition => buffDefinition;

    public override IEnumerator Execute(ActionContext context)
    {
        if (context == null || context.Player == null)
        {
            yield break;
        }

        // 1. Play weapon charge animation
        if (context.Visuals != null)
        {
            context.Visuals.ChargeSword(Duration);
        }

        // 2. Apply buff to player's weapon manager
        if (buffDefinition != null)
        {
            WeaponBuffManager buffManager = context.Player.GetComponent<WeaponBuffManager>();
            if (buffManager == null)
            {
                buffManager = context.Player.gameObject.AddComponent<WeaponBuffManager>();
            }

            buffManager.ApplyBuff(buffDefinition);
        }

        yield return new WaitForSeconds(Duration);
    }
}

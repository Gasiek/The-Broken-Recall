using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "HealAction", menuName = "RPG/Actions/Heal")]
public class HealAction : ActionDefinition
{
    [SerializeField]
    private int healAmount = 25;

    public override IEnumerator Execute(ActionContext context)
    {
        if (context.Health != null)
        {
            context.Health.Heal(healAmount);
        }

        yield return new WaitForSeconds(Duration);
    }
}

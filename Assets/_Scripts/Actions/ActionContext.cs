using UnityEngine;

public class ActionContext
{
    public Transform Player { get; }
    public PlayerMovement Movement { get; }
    public Health Health { get; }
    public ActionDefinition Action { get; }
    public WeaponBuffManager WeaponBuffs { get; }

    public ActionContext(
        Transform player,
        PlayerMovement movement,
        Health health,
        ActionDefinition action,
        WeaponBuffManager weaponBuffs
    )
    {
        Player = player;
        Movement = movement;
        Health = health;
        Action = action;
        WeaponBuffs = weaponBuffs;
    }
}

using UnityEngine;

public class ActionContext
{
    public Player Player { get; }
    public PlayerMovement Movement { get; }
    public Health Health { get; }
    public ActionDefinition Action { get; }
    public WeaponBuffManager WeaponBuffs { get; }

    public ActionContext(
        Player player,
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

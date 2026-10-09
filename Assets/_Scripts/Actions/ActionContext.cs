using UnityEngine;

public class ActionContext
{
    public Transform Player { get; }
    public PlayerMovement Movement { get; }
    public Health Health { get; }
    public PlayerActionVisuals Visuals { get; }
    public ActionDefinition Action { get; }
    public WeaponBuffManager WeaponBuffs { get; }

    public ActionContext(
        Transform player,
        PlayerMovement movement,
        Health health,
        PlayerActionVisuals visuals,
        ActionDefinition action,
        WeaponBuffManager weaponBuffs
    )
    {
        Player = player;
        Movement = movement;
        Health = health;
        Visuals = visuals;
        Action = action;
        WeaponBuffs = weaponBuffs;
    }
}

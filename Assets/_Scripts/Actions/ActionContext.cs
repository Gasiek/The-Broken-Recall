using UnityEngine;

public class ActionContext
{
    public Transform Player { get; }
    public PlayerMovement Movement { get; }
    public Health Health { get; }
    public PlayerActionVisuals Visuals { get; }
    public ActionDefinition Action { get; }

    public ActionContext(
        Transform player,
        PlayerMovement movement,
        Health health,
        PlayerActionVisuals visuals,
        ActionDefinition action
    )
    {
        Player = player;
        Movement = movement;
        Health = health;
        Visuals = visuals;
        Action = action;
    }
}

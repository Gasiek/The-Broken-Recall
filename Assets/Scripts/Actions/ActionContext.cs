using UnityEngine;

public class ActionContext
{
    public Transform Player { get; }
    public PlayerMovement Movement { get; }
    public PlayerActionVisuals Visuals { get; }
    public ActionDefinition Action { get; }

    public ActionContext(
        Transform player,
        PlayerMovement movement,
        PlayerActionVisuals visuals,
        ActionDefinition action
    )
    {
        Player = player;
        Movement = movement;
        Visuals = visuals;
        Action = action;
    }
}

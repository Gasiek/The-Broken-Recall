using UnityEngine;

public class ActionContext
{
    public Player Player { get; }
    public PlayerMovement Movement { get; }
    public Health Health { get; }
    public ActionDefinition Action { get; }
    public ActionController ActionController { get; }

    public ActionContext(
        Player player,
        PlayerMovement movement,
        Health health,
        ActionDefinition action,
        ActionController actionController
    )
    {
        Player = player;
        Movement = movement;
        Health = health;
        Action = action;
        ActionController = actionController;
    }
}

using UnityEngine;

public class ActionContext
{
    public Player Player { get; }
    public PlayerMovement Movement { get; }
    public Health Health { get; }
    public ActionDefinition Action { get; }
    public ActionController ActionController { get; }
    public ActionRoutineRunner RoutineRunner { get; }
    public Coroutine ActionRoutine { get; set; }

    public ActionContext(
        Player player,
        PlayerMovement movement,
        Health health,
        ActionDefinition action,
        ActionController actionController,
        ActionRoutineRunner routineRunner
    )
    {
        Player = player;
        Movement = movement;
        Health = health;
        Action = action;
        ActionController = actionController;
        RoutineRunner = routineRunner;
    }
}

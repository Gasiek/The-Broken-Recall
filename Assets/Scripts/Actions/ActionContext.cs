public class ActionContext
{
    public PlayerController Player { get; }
    public PlayerActionVisuals Visuals { get; }
    public ActionDefinition Action { get; }

    public ActionContext(
        PlayerController player,
        PlayerActionVisuals visuals,
        ActionDefinition action)
    {
        Player = player;
        Visuals = visuals;
        Action = action;
    }
}
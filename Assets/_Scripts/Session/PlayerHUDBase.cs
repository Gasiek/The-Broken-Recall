using UnityEngine;

public abstract class PlayerHUDBase : MonoBehaviour
{
    protected Player CurrentPlayer { get; private set; }

    private GameSession subscribedSession;

    protected virtual void OnEnable()
    {
        TrySubscribeToSession();
    }

    protected virtual void OnDisable()
    {
        UnsubscribeFromSession();
        BindPlayer(null);
    }

    private void TrySubscribeToSession()
    {
        GameSession session = GameSession.Instance;

        if (session == null)
            return;

        if (subscribedSession != session)
        {
            UnsubscribeFromSession();

            subscribedSession = session;
            subscribedSession.PlayerSpawned += BindPlayer;
            subscribedSession.PlayerDespawning += UnbindPlayer;
        }

        // Also handles HUDs enabled after the player has spawned.
        BindPlayer(session.CurrentPlayer);
    }

    private void UnsubscribeFromSession()
    {
        if (subscribedSession == null)
            return;

        subscribedSession.PlayerSpawned -= BindPlayer;
        subscribedSession.PlayerDespawning -= UnbindPlayer;
        subscribedSession = null;
    }

    private void BindPlayer(Player player)
    {
        if (CurrentPlayer == player)
            return;

        Player previousPlayer = CurrentPlayer;
        CurrentPlayer = null;

        if (previousPlayer != null)
            OnPlayerUnbound(previousPlayer);

        if (player == null)
            return;

        CurrentPlayer = player;
        OnPlayerBound(player);
    }

    private void UnbindPlayer(Player player)
    {
        if (CurrentPlayer == player)
            BindPlayer(null);
    }

    protected abstract void OnPlayerBound(Player player);
    protected abstract void OnPlayerUnbound(Player player);
}

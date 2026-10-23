
using UnityEngine;

public class PersistentEncounter : PersistentIdComponent
{
    public bool IsCompleted
    {
        get
        {
            if (string.IsNullOrWhiteSpace(StableId))
                return false;

            GameSession session = GameSession.Instance;

            return session != null &&
                   session.IsEncounterCompleted(StableId);
        }
    }

    public bool CanStartEncounter => !IsCompleted;

    public void MarkCompleted()
    {
        if (string.IsNullOrWhiteSpace(StableId))
        {
            Debug.LogError(
                "[PersistentEncounter] Stable ID is missing. " +
                "Cannot persist encounter completion.",
                this
            );
            return;
        }

        GameSession session = GameSession.Instance;

        if (session == null)
        {
            Debug.LogError(
                "[PersistentEncounter] No GameSession exists. " +
                "Encounter completion could not be recorded.",
                this
            );
            return;
        }

        session.MarkEncounterCompleted(StableId);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class ActionCollection : MonoBehaviour
{
    [SerializeField] private List<ActionDefinition> actions = new();

    public IReadOnlyList<ActionDefinition> Actions => actions;

    public bool Contains(ActionDefinition action)
    {
        return actions.Contains(action);
    }

    public void Add(ActionDefinition action)
    {
        if (action == null)
        {
            Debug.LogWarning("Tried to add a null action.");
            return;
        }

        if (actions.Contains(action))
        {
            Debug.LogWarning($"Action '{action.DisplayName}' is already known.");
            return;
        }

        actions.Add(action);
    }

    public bool Remove(ActionDefinition action)
    {
        if (action == null)
        {
            return false;
        }

        return actions.Remove(action);
    }

    public ActionDefinition GetRandomAction()
    {
        if (actions.Count == 0)
        {
            Debug.LogWarning("Cannot get a random action: collection is empty.");
            return null;
        }

        int index = Random.Range(0, actions.Count);
        return actions[index];
    }
}
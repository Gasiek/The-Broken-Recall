using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionCollection : MonoBehaviour
{
    [SerializeField]
    private List<ActionCardInstance> actions = new();

    public IReadOnlyList<ActionCardInstance> Actions => actions;

    public event Action Changed;

    public bool Contains(ActionCardInstance card)
    {
        return card != null && actions.Contains(card);
    }

    public void Add(ActionDefinition definition)
    {
        if (definition == null)
        {
            Debug.LogWarning("Tried to add a null action definition.");
            return;
        }

        actions.Add(new ActionCardInstance(definition));
        Changed?.Invoke();
    }

    public bool Add(ActionCardInstance card)
    {
        if (card == null || card.Definition == null)
        {
            Debug.LogWarning("Tried to add an invalid action card.");
            return false;
        }

        if (actions.Contains(card))
        {
            Debug.LogWarning("This card instance is already in the collection.");
            return false;
        }

        actions.Add(card);
        Changed?.Invoke();
        return true;
    }

    public bool Remove(ActionCardInstance card)
    {
        if (card == null)
        {
            return false;
        }

        bool removed = actions.Remove(card);

        if (removed)
        {
            Changed?.Invoke();
        }

        return removed;
    }

    public ActionCardInstance GetRandomAction()
    {
        if (actions.Count == 0)
        {
            Debug.LogWarning("Cannot get a random action: collection is empty.");
            return null;
        }

        return actions[UnityEngine.Random.Range(0, actions.Count)];
    }
}

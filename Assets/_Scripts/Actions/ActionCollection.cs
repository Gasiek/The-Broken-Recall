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
            Debug.LogWarning("[ActionCollection] Cannot add a null definition.", this);
            return;
        }

        actions.Add(new ActionCardInstance(definition));
        Changed?.Invoke();
    }

    public bool Add(ActionCardInstance card)
    {
        if (card == null || card.Definition == null)
        {
            Debug.LogWarning("[ActionCollection] Cannot add an invalid card.", this);
            return false;
        }

        if (actions.Contains(card))
        {
            Debug.LogWarning("[ActionCollection] Card is already in the collection.", this);
            return false;
        }

        actions.Add(card);
        Changed?.Invoke();
        return true;
    }

    public bool Remove(ActionCardInstance card)
    {
        bool removed = actions.Remove(card);

        if (removed)
            Changed?.Invoke();

        return removed;
    }

    public void ReplaceAll(IEnumerable<ActionCardInstance> cards)
    {
        actions.Clear();

        if (cards != null)
        {
            foreach (ActionCardInstance card in cards)
            {
                if (card != null && card.Definition != null)
                    actions.Add(card);
            }
        }

        Changed?.Invoke();
    }

    public ActionCardInstance GetRandomAction()
    {
        if (actions.Count == 0)
            return null;

        return actions[UnityEngine.Random.Range(0, actions.Count)];
    }
}

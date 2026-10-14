using System;
using UnityEngine;

[Serializable]
public sealed class ActionCardInstance
{
    [SerializeField]
    private ActionDefinition definition;

    public ActionDefinition Definition => definition;

    public ActionCardInstance(ActionDefinition definition)
    {
        this.definition = definition;
    }
}

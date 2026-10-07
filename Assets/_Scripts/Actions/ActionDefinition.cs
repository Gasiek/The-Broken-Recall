using System.Collections;
using UnityEngine;

public abstract class ActionDefinition : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private Sprite icon;

    public string Id => id;
    public string DisplayName => displayName;
    public float Duration => duration;
    public Sprite Icon => icon;
    public abstract IEnumerator Execute(ActionContext context);
}
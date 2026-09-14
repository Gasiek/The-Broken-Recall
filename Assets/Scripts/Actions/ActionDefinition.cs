using UnityEngine;

[CreateAssetMenu(
    fileName = "Action",
    menuName = "RPG/Actions/Action Definition"
)]
public class ActionDefinition : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
}

using UnityEngine;

[CreateAssetMenu(
    fileName = "SceneDestination",
    menuName = "Game/Scene Destination"
)]
public sealed class SceneDestination : ScriptableObject
{
    [SerializeField]
    private string destinationName;

    [SerializeField]
    private string sceneName;

    public string DestinationName => destinationName;
    public string SceneName => sceneName;
}

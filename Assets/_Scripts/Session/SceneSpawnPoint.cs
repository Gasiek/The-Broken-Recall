using UnityEngine;

public sealed class SceneSpawnPoint : MonoBehaviour
{
    [SerializeField]
    private SceneDestination destination;

    public SceneDestination Destination => destination;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (destination == null)
        {
            Debug.LogWarning("[SceneSpawnPoint] Assign a SceneDestination asset.", this);
        }
    }
#endif
}

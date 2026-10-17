using UnityEngine;

public sealed class SceneTransition : MonoBehaviour
{
    [SerializeField]
    private SceneDestination destination;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered || other.GetComponentInParent<Player>() == null)
            return;

        SceneTransitionManager manager = SceneTransitionManager.Instance;

        if (manager == null)
        {
            Debug.LogError("[SceneTransition] No SceneTransitionManager exists.", this);
            return;
        }

        if (destination == null)
        {
            Debug.LogError("[SceneTransition] No destination asset assigned.", this);
            return;
        }

        if (manager.TransitionTo(destination))
            triggered = true;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (destination == null)
        {
            Debug.LogWarning("[SceneTransition] Assign a SceneDestination asset.", this);
        }
    }
#endif
}

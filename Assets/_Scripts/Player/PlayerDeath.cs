using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerDeath : MonoBehaviour
{
    [Header("Death Settings")]
    [SerializeField, Min(0f)]
    private float restartDelay = 3f;

    private Health health;
    private PlayerStateMachine stateMachine;
    private Coroutine deathRoutine;

    private void Awake()
    {
        health = GetComponent<Health>();
        stateMachine = GetComponent<PlayerStateMachine>();
    }

    private void OnEnable()
    {
        health.Died += HandleDeath;
    }

    private void OnDisable()
    {
        health.Died -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (deathRoutine != null)
            return;

        stateMachine.Die();
        deathRoutine = StartCoroutine(RestartSceneAfterDelay());
    }

    private IEnumerator RestartSceneAfterDelay()
    {
        yield return new WaitForSecondsRealtime(restartDelay);

        SceneTransitionManager manager = SceneTransitionManager.Instance;

        if (manager == null)
        {
            Debug.LogError("Cannot restart: SceneTransitionManager was not found.", this);
            yield break;
        }

        manager.RestartFromBeginning();
    }
}

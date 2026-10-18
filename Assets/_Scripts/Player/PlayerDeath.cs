using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        yield return new WaitForSeconds(restartDelay);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

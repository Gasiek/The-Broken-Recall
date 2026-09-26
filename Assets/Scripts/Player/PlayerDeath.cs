using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerDeath : MonoBehaviour
{
    private Health health;
    private PlayerStateMachine stateMachine;

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
        stateMachine.Die();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

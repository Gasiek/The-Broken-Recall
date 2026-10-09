using UnityEngine;

public class Bed : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField]
    private Health playerHealth;

    [SerializeField]
    private PlayerStateMachine playerStateMachine;

    [SerializeField]
    private Transform playerTransform;

    [Header("Sleep Duration Settings")]
    [SerializeField]
    private float fadeOutDuration = 0.8f;

    [SerializeField]
    private float sleepDuration = 0.5f;

    [SerializeField]
    private float fadeInDuration = 0.8f;

    [Header("Audio (Optional)")]
    [SerializeField]
    private AudioClip yawnSound;

    [SerializeField, Range(0f, 1f)]
    private float yawnVolume = 1f;

    private ScreenFader screenFader;

    public bool CanInteract =>
        playerHealth != null
        && playerStateMachine != null
        && playerTransform != null
        && screenFader != null
        && AudioManager.Instance != null
        && playerStateMachine.CanSleep;

    private void Awake()
    {
        if (playerHealth == null || playerStateMachine == null || playerTransform == null)
        {
            Debug.LogError(
                "[Bed] Assign PlayerHealth, PlayerStateMachine, "
                    + "and PlayerTransform in the Inspector.",
                this
            );
        }
    }

    private void Start()
    {
        screenFader = ScreenFader.Instance;

        if (screenFader == null)
        {
            Debug.LogError(
                "[Bed] No ScreenFader was found. " + "Make sure one exists in the scene.",
                this
            );
        }
    }

    public void Interact()
    {
        if (!CanInteract)
        {
            Debug.LogWarning(
                "[Bed] Cannot rest. Check the required references, "
                    + "AudioManager, and player state.",
                this
            );

            return;
        }

        playerStateMachine.StartSleeping();

        screenFader.SleepSequence(fadeOutDuration, sleepDuration, fadeInDuration, FinishRest);
    }

    private void FinishRest()
    {
        PlayYawnSound();
        RestoreHealth();

        if (playerStateMachine != null)
        {
            playerStateMachine.FinishSleeping();
        }
    }

    private void PlayYawnSound()
    {
        if (yawnSound == null || AudioManager.Instance == null)
            return;

        AudioManager.Instance.PlayWorldSFX(yawnSound, playerTransform.position, yawnVolume);
    }

    private void RestoreHealth()
    {
        if (playerHealth == null)
            return;

        int missingHealth = playerHealth.MaxHealth - playerHealth.CurrentHealth;

        if (missingHealth <= 0)
            return;

        playerHealth.Heal(missingHealth);

        Debug.Log(
            $"[Bed] Player rested and restored health! "
                + $"({playerHealth.CurrentHealth}/{playerHealth.MaxHealth})",
            this
        );
    }
}

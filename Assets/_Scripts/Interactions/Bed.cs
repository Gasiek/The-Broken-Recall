using UnityEngine;

public class Bed : MonoBehaviour, IInteractable
{
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
    private ActionMemory actionMemory;

    private ScreenFader screenFader;

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

    public bool CanInteract(Player player)
    {
        return player != null
            && player.Health != null
            && player.StateMachine != null
            && player.ActionMemory != null
            && screenFader != null
            && AudioManager.Instance != null
            && player.StateMachine.CanSleep;
    }

    public void Interact(Player player)
    {
        if (!CanInteract(player))
        {
            Debug.LogWarning(
                "[Bed] Cannot rest. Check the required references, "
                    + "AudioManager, ScreenFader, and player state.",
                this
            );

            return;
        }

        player.StateMachine.StartSleeping();
        actionMemory = player.ActionMemory;
        screenFader.SleepSequence(
            fadeOutDuration,
            sleepDuration,
            fadeInDuration,
            () => FinishRest(player)
        );
    }

    private void FinishRest(Player player)
    {
        if (player == null)
        {
            return;
        }

        PlayYawnSound(player);
        RestoreHealth(player);

        actionMemory.ResetCooldownsAndReshuffle();

        if (player.StateMachine != null)
        {
            player.StateMachine.FinishSleeping();
        }
    }

    private void PlayYawnSound(Player player)
    {
        if (yawnSound == null || AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.PlayWorldSFX(yawnSound, player.transform.position, yawnVolume);
    }

    private void RestoreHealth(Player player)
    {
        if (player.Health == null)
        {
            return;
        }

        int missingHealth = player.Health.MaxHealth - player.Health.CurrentHealth;

        if (missingHealth <= 0)
        {
            return;
        }

        player.Health.Heal(missingHealth);

        Debug.Log(
            $"[Bed] Player rested and restored health! "
                + $"({player.Health.CurrentHealth}/{player.Health.MaxHealth})",
            this
        );
    }
}

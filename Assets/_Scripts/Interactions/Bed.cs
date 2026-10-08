using UnityEngine;

public class Bed : MonoBehaviour, IInteractable
{
    private Health playerHealth;

    [Header("Sleep Duration Settings")]
    [SerializeField]
    private float fadeOutDuration = 0.8f;

    [SerializeField]
    private float sleepDuration = 0.5f;

    [SerializeField]
    private float fadeInDuration = 0.8f;

    [Header("Audio (Optional)")]
    [SerializeField]
    private AudioClip restSound;

    [SerializeField]
    [Range(0f, 1f)]
    private float restVolume = 1f;

    private bool isSleeping;

    public bool CanInteract
    {
        get
        {
            EnsurePlayerReference();
            return playerHealth != null && !isSleeping;
        }
    }

    private void Awake()
    {
        EnsurePlayerReference();
    }

    private void EnsurePlayerReference()
    {
        if (playerHealth == null)
        {
            Player player = FindFirstObjectByType<Player>();
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
            }
        }
    }

    public void Interact()
    {
        EnsurePlayerReference();

        if (playerHealth == null || isSleeping)
        {
            return;
        }

        if (ScreenFader.Instance != null)
        {
            isSleeping = true;
            ScreenFader.Instance.SleepSequence(fadeOutDuration, sleepDuration, fadeInDuration, () =>
            {
                PerformRest();
                isSleeping = false;
            });
        }
        else
        {
            // Fallback if ScreenFader is not in scene
            PerformRest();
        }
    }

    private void PerformRest()
    {
        if (playerHealth == null)
            return;

        int missingHealth = playerHealth.MaxHealth - playerHealth.CurrentHealth;
        playerHealth.Heal(missingHealth);

        Debug.Log($"[Bed] Player rested and restored 100% health! ({playerHealth.CurrentHealth}/{playerHealth.MaxHealth})");

        if (restSound != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(restSound, transform.position, restVolume);
            }
            else
            {
                AudioSource.PlayClipAtPoint(restSound, transform.position, restVolume);
            }
        }
    }
}

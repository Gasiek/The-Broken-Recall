using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AudioSource))]
public class CharacterAudio : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField]
    private AudioClip hitSound;

    [SerializeField]
    [Range(0f, 1f)]
    private float hitVolume = 1f;

    [Header("Blocked Damage")]
    [SerializeField]
    private AudioClip blockSound;

    [SerializeField]
    [Range(0f, 1f)]
    private float blockVolume = 1f;

    private Health health;
    private AudioSource audioSource;

    private void Awake()
    {
        health = GetComponent<Health>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if (health == null)
            return;

        health.DamageReceived += OnDamageReceived;
        health.DamageBlocked += OnDamageBlocked;
    }

    private void OnDisable()
    {
        if (health == null)
            return;

        health.DamageReceived -= OnDamageReceived;
        health.DamageBlocked -= OnDamageBlocked;
    }

    private void OnDamageReceived(int amount)
    {
        if (hitSound == null)
            return;

        audioSource.PlayOneShot(hitSound, hitVolume);
    }

    private void OnDamageBlocked()
    {
        if (blockSound == null)
            return;

        audioSource.PlayOneShot(blockSound, blockVolume);
    }
}

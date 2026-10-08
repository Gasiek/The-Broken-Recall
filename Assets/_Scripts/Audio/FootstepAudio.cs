using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(AudioSource))]
public class FootstepAudio : MonoBehaviour
{
    [Header("Footsteps")]
    [SerializeField]
    private AudioClip[] footstepSounds;

    [SerializeField]
    [Range(0f, 1f)]
    private float footstepVolume = 0.5f;

    [SerializeField]
    private float stepInterval = 0.45f;

    [SerializeField]
    private float minimumStepInterval = 0.1f;

    private PlayerMovement movement;
    private AudioSource audioSource;
    private float stepTimer;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!movement.IsMoving || !movement.IsGrounded)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            PlayFootstep();
            stepTimer = GetCurrentStepInterval();
        }
    }

    private float GetCurrentStepInterval()
    {
        if (movement.BaseMoveSpeed <= 0f)
        {
            return stepInterval;
        }

        float speedRatio = movement.CurrentMoveSpeed / movement.BaseMoveSpeed;

        return Mathf.Max(stepInterval / speedRatio, minimumStepInterval);
    }

    private void PlayFootstep()
    {
        if (footstepSounds == null || footstepSounds.Length == 0)
        {
            return;
        }

        AudioClip footstep = footstepSounds[Random.Range(0, footstepSounds.Length)];

        audioSource.PlayOneShot(footstep, footstepVolume);
    }
}

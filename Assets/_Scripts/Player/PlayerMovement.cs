using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private PlayerStateMachine stateMachine;
    private PlayerDefinition playerDefinition;
    private CharacterController characterController;

    private Vector2 moveInput;
    private float verticalVelocity;
    private float currentSpeedMultiplier = 1f;

    [Header("Audio")]
    [SerializeField]
    private AudioClip footstepSound;

    [SerializeField]
    [Range(0f, 1f)]
    private float footstepVolume = 0.5f;

    private AudioSource footstepAudioSource;

    public float CurrentMoveSpeed => playerDefinition.MoveSpeed * currentSpeedMultiplier;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        stateMachine = GetComponent<PlayerStateMachine>();
        Player player = GetComponent<Player>();
        playerDefinition = player.Definition;

        SetupFootstepAudio();

        if (playerDefinition == null)
            Debug.LogError("PlayerMovement has no PlayerDefinition assigned.");
    }

    private void SetupFootstepAudio()
    {
        footstepAudioSource = gameObject.AddComponent<AudioSource>();
        footstepAudioSource.clip = footstepSound;
        footstepAudioSource.volume = footstepVolume;
        footstepAudioSource.loop = true;
        footstepAudioSource.playOnAwake = false;
        footstepAudioSource.spatialBlend = 1f; // 3D sound
    }

    private void Update()
    {
        Move();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void Move()
    {
        if (!stateMachine.CanMove)
        {
            StopFootsteps();
            return;
        }

        Vector3 cameraForward = Camera.main != null ? Camera.main.transform.forward : Vector3.forward;
        Vector3 cameraRight = Camera.main != null ? Camera.main.transform.right : Vector3.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 movement = cameraRight * moveInput.x + cameraForward * moveInput.y;

        if (movement.sqrMagnitude > 1f)
            movement.Normalize();

        RotateTowardsMovement(movement);

        characterController.Move(movement * CurrentMoveSpeed * Time.deltaTime);

        ApplyGravity();

        HandleFootsteps(movement);
    }

    private void HandleFootsteps(Vector3 movement)
    {
        if (footstepSound == null)
            return;

        // Make sure audio source has the right volume/clip if changed in inspector
        footstepAudioSource.volume = footstepVolume;
        if (footstepAudioSource.clip != footstepSound)
            footstepAudioSource.clip = footstepSound;

        // Player is moving on the ground
        bool isWalking = movement.sqrMagnitude > 0.01f && characterController.isGrounded;

        if (isWalking)
        {
            if (!footstepAudioSource.isPlaying)
            {
                footstepAudioSource.Play();
            }
        }
        else
        {
            StopFootsteps();
        }
    }

    private void StopFootsteps()
    {
        if (footstepAudioSource != null && footstepAudioSource.isPlaying)
        {
            footstepAudioSource.Stop();
        }
    }

    private void RotateTowardsMovement(Vector3 movement)
    {
        if (movement.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(movement);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            playerDefinition.RotationSpeed * Time.deltaTime
        );
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += playerDefinition.Gravity * Time.deltaTime;

        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeedMultiplier = multiplier;
    }

    public void ResetSpeedMultiplier()
    {
        currentSpeedMultiplier = 1f;
    }
}

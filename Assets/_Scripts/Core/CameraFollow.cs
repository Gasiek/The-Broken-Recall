using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private Transform target;

    [Header("Distance & Height")]
    [SerializeField]
    private float distance = 8f;

    [SerializeField]
    private float height = 8f;

    [Header("Follow Settings")]
    [SerializeField]
    private float followSpeed = 25f;

    [Header("Mouse Orbit Settings")]
    [SerializeField]
    private float mouseSensitivity = 1.5f;

    [SerializeField]
    private float minPitch = 15f;

    [SerializeField]
    private float maxPitch = 70f;

    [SerializeField]
    private bool lockCursor = true;

    private float currentYaw;
    private float currentPitch = 40f;

    private void Start()
    {
        if (target != null)
        {
            currentYaw = target.eulerAngles.y;
        }

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // Freeze camera movement and rotation when the game is paused
        if (Time.timeScale == 0f || (PauseController.Instance != null && PauseController.Instance.IsPaused))
        {
            return;
        }

        HandleMouseInput();

        // Target point (center of the player)
        Vector3 targetFocusPoint = target.position + Vector3.up * 1.5f;

        // Calculate exact rotation around player
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

        // Strict constant distance sphere orbit:
        // Position is ALWAYS exactly 'distance' away from targetFocusPoint
        Vector3 orbitOffset = rotation * (Vector3.back * distance);
        transform.position = targetFocusPoint + orbitOffset;

        // Camera is ALWAYS 100% oriented directly at player focus point
        transform.rotation = rotation;
    }

    private void HandleMouseInput()
    {
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            currentYaw += mouseDelta.x * mouseSensitivity;
            currentPitch -= mouseDelta.y * mouseSensitivity;
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
        }
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
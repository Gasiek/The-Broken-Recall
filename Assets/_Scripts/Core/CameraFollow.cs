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
    private float height = 1.5f;

    [Header("Orbit Settings")]
    [SerializeField]
    private float mouseSensitivity = 1.5f;

    [SerializeField]
    private float minPitch = 15f;

    [SerializeField]
    private float maxPitch = 70f;

    private float currentYaw;
    private float currentPitch = 40f;

    private void Awake()
    {
        if (target == null)
        {
            Debug.LogError("CameraFollow has no target assigned.");
        }
    }

    private void Start()
    {
        if (target != null)
        {
            currentYaw = target.eulerAngles.y;
        }

        LockCursor();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        UpdateCameraPosition();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        Vector2 lookInput = context.ReadValue<Vector2>();

        currentYaw += lookInput.x * mouseSensitivity;
        currentPitch -= lookInput.y * mouseSensitivity;

        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
    }

    public void OnPause()
    {
        UnlockCursor();
    }

    public void OnResume()
    {
        LockCursor();
    }

    private void UpdateCameraPosition()
    {
        Vector3 targetFocusPoint = target.position + Vector3.up * height;

        Quaternion orbitRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

        Vector3 orbitOffset = orbitRotation * (Vector3.back * distance);

        transform.position = targetFocusPoint + orbitOffset;

        transform.LookAt(targetFocusPoint);
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDestroy()
    {
        UnlockCursor();
    }
}

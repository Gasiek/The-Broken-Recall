using UnityEngine;

public class CameraFollow : MonoBehaviour
{
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
    private Transform target;

    public Transform Target => target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (target != null)
            InitializeOrbit();
    }

    private void InitializeOrbit()
    {
        currentYaw = target.eulerAngles.y;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
    }

    public void Look(Vector2 lookInput)
    {
        if (target == null)
            return;

        currentYaw += lookInput.x * mouseSensitivity;
        currentPitch -= lookInput.y * mouseSensitivity;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        Vector3 targetFocusPoint = target.position + Vector3.up * height;

        Quaternion orbitRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

        Vector3 orbitOffset = orbitRotation * (Vector3.back * distance);

        transform.position = targetFocusPoint + orbitOffset;
        transform.LookAt(targetFocusPoint);
    }
}

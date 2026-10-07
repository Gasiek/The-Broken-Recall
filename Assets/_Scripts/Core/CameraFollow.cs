using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private Transform target;

    [Header("Camera")]
    [SerializeField]
    private Vector3 offset = new Vector3(0f, 10f, -11f);

    [Header("Follow")]
    [SerializeField]
    private float followSpeed = 3f;

    [Header("Look Ahead")]
    [SerializeField]
    private float lookAheadDistance = 1f;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 lookAhead = target.forward * lookAheadDistance;

        Vector3 desiredPosition =
            target.position + lookAhead + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );
    }
}
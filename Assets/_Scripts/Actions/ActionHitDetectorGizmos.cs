using UnityEngine;

public class ActionHitDetectorGizmos : MonoBehaviour
{
    [Header("Forward Detector")]
    [SerializeField]
    private bool showForwardDetector = true;

    [SerializeField]
    private float forwardOffset = 1.6f;

    [SerializeField]
    private float forwardRadius = 0.5f;

    [Header("Radial Detector")]
    [SerializeField]
    private bool showRadialDetector = true;

    [SerializeField]
    private float radialRadius = 3.5f;

    [SerializeField]
    private Vector3 radialOffset = Vector3.zero;

    [Header("Display")]
    [SerializeField]
    private bool onlyWhenSelected = true;

    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void OnDrawGizmos()
    {
        if (onlyWhenSelected)
            return;

        DrawDetectors();
    }

    private void OnDrawGizmosSelected()
    {
        if (!onlyWhenSelected)
            return;

        DrawDetectors();
    }

    private void DrawDetectors()
    {
        if (player == null)
            player = GetComponent<Player>();

        if (player == null)
            return;

        if (showForwardDetector)
        {
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

            if (forward.sqrMagnitude < 0.0001f)
                forward = transform.forward;

            Vector3 center = player.AimPosition + forward * forwardOffset;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, forwardRadius);

            // Line from aim position to the forward detector.
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(player.AimPosition, center);
        }

        if (showRadialDetector)
        {
            Vector3 center = transform.position + radialOffset;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(center, radialRadius);
        }
    }
}

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyWalkVisuals : MonoBehaviour
{
    [Header("Limbs Setup")]
    [Tooltip("Left arm transform. If empty, automatically searches for 'Arm_L'.")]
    [SerializeField]
    private Transform leftArm;

    [Tooltip("Right arm transform. If empty, automatically searches for 'Arm_R'.")]
    [SerializeField]
    private Transform rightArm;

    [Tooltip("Optional left leg. If empty, searches for 'Leg_L'.")]
    [SerializeField]
    private Transform leftLeg;

    [Tooltip("Optional right leg. If empty, searches for 'Leg_R'.")]
    [SerializeField]
    private Transform rightLeg;

    [Tooltip("Optional body/torso for vertical bobbing.")]
    [SerializeField]
    private Transform bodyTorso;

    [Header("Arm Swing")]
    [SerializeField]
    private float armSwingAngle = 35f;

    [SerializeField]
    private Vector3 armSwingAxis = new Vector3(1f, 0f, 0f);

    [Header("Leg Swing")]
    [SerializeField]
    private float legSwingAngle = 20f;

    [SerializeField]
    private Vector3 legSwingAxis = new Vector3(1f, 0f, 0f);

    [Header("Body Bobbing & Tilt")]
    [SerializeField]
    private float bobHeight = 0.06f;

    [SerializeField]
    private float tiltAngle = 3f;

    [Header("Animation Dynamics")]
    [SerializeField]
    private float swingFrequency = 9f;

    [SerializeField]
    private float returnSpeed = 8f;

    private NavMeshAgent agent;
    private Quaternion leftArmIdleRot;
    private Quaternion rightArmIdleRot;
    private Quaternion leftLegIdleRot;
    private Quaternion rightLegIdleRot;
    private Quaternion torsoIdleRot;
    private Vector3 torsoIdlePos;

    private float walkCycle;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        AutoAssignLimbs();
        CacheIdleTransforms();
    }

    private void AutoAssignLimbs()
    {
        if (leftArm == null) leftArm = FindChildRecursive(transform, "Arm_L");
        if (rightArm == null) rightArm = FindChildRecursive(transform, "Arm_R");
        if (leftLeg == null) leftLeg = FindChildRecursive(transform, "Leg_L");
        if (rightLeg == null) rightLeg = FindChildRecursive(transform, "Leg_R");
        if (bodyTorso == null) bodyTorso = FindChildRecursive(transform, "Torso");

        if (leftArm == null || rightArm == null)
        {
            Debug.LogWarning($"[{nameof(EnemyWalkVisuals)}] Missing arm references on '{gameObject.name}' (leftArm: {(leftArm != null ? leftArm.name : "null")}, rightArm: {(rightArm != null ? rightArm.name : "null")})!", this);
        }
    }

    private void CacheIdleTransforms()
    {
        if (leftArm != null) leftArmIdleRot = leftArm.localRotation;
        if (rightArm != null) rightArmIdleRot = rightArm.localRotation;
        if (leftLeg != null) leftLegIdleRot = leftLeg.localRotation;
        if (rightLeg != null) rightLegIdleRot = rightLeg.localRotation;
        if (bodyTorso != null)
        {
            torsoIdleRot = bodyTorso.localRotation;
            torsoIdlePos = bodyTorso.localPosition;
        }
    }

    private void Update()
    {
        float speed = 0f;
        if (agent != null && !agent.isStopped)
        {
            speed = agent.velocity.magnitude;
        }

        bool isMoving = speed > 0.15f;

        if (isMoving)
        {
            float speedFactor = Mathf.Clamp(speed / (agent.speed > 0 ? agent.speed : 1f), 0.5f, 1.8f);
            walkCycle += Time.deltaTime * swingFrequency * speedFactor;

            float sin = Mathf.Sin(walkCycle);
            float cos = Mathf.Cos(walkCycle);

            // 1. Alternate arm swings (opposite phases)
            if (leftArm != null)
            {
                leftArm.localRotation = leftArmIdleRot * Quaternion.AngleAxis(sin * armSwingAngle, armSwingAxis);
            }
            if (rightArm != null)
            {
                rightArm.localRotation = rightArmIdleRot * Quaternion.AngleAxis(-sin * armSwingAngle, armSwingAxis);
            }

            // 2. Alternate leg swings
            if (leftLeg != null)
            {
                leftLeg.localRotation = leftLegIdleRot * Quaternion.AngleAxis(-sin * legSwingAngle, legSwingAxis);
            }
            if (rightLeg != null)
            {
                rightLeg.localRotation = rightLegIdleRot * Quaternion.AngleAxis(sin * legSwingAngle, legSwingAxis);
            }

            // 3. Subtle torso waddle and bob
            if (bodyTorso != null)
            {
                float bob = Mathf.Abs(sin) * bobHeight;
                bodyTorso.localPosition = torsoIdlePos + Vector3.up * bob;
                bodyTorso.localRotation = torsoIdleRot * Quaternion.AngleAxis(cos * tiltAngle, Vector3.forward);
            }
        }
        else
        {
            // Smoothly return to idle pose when standing still
            float lerp = Time.deltaTime * returnSpeed;

            if (leftArm != null)
            {
                leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, leftArmIdleRot, lerp);
            }
            if (rightArm != null)
            {
                rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, rightArmIdleRot, lerp);
            }
            if (leftLeg != null)
            {
                leftLeg.localRotation = Quaternion.Slerp(leftLeg.localRotation, leftLegIdleRot, lerp);
            }
            if (rightLeg != null)
            {
                rightLeg.localRotation = Quaternion.Slerp(rightLeg.localRotation, rightLegIdleRot, lerp);
            }
            if (bodyTorso != null)
            {
                bodyTorso.localPosition = Vector3.Lerp(bodyTorso.localPosition, torsoIdlePos, lerp);
                bodyTorso.localRotation = Quaternion.Slerp(bodyTorso.localRotation, torsoIdleRot, lerp);
            }
        }
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Equals(childName, System.StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }

            Transform found = FindChildRecursive(child, childName);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }
}

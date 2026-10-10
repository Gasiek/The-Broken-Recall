using System;
using DG.Tweening;
using UnityEngine;

public class BossAttackVisuals : MonoBehaviour
{
    [Header("Limbs & Body Setup")]
    [Tooltip("Right arm transform. If assigned, this arm animates the attack. Since the weapon is a child of the arm, it moves along naturally.")]
    [SerializeField]
    private Transform rightArm;

    [Tooltip("Torso / body transform to lean into the swing and slam.")]
    [SerializeField]
    private Transform torso;

    [Tooltip("Head transform to nod/tilt along with the body swing. If empty, searches for 'Head'.")]
    [SerializeField]
    private Transform head;

    [Tooltip("Weapon transform. Used as fallback if rightArm is not assigned.")]
    [SerializeField]
    private Transform weapon;

    [Header("Ground Slam - Arm Dynamics")]
    [SerializeField]
    private bool useWindUp = true;

    [Tooltip("Arm rotation offset during wind-up (raising arm and weapon back/up).")]
    [SerializeField]
    private Vector3 armWindUpRotation = new Vector3(0f, 0f, 90f);

    [Tooltip("Arm rotation offset at the impact moment of the slam (slamming down in front).")]
    [SerializeField]
    private Vector3 armSlamRotation = new Vector3(0f, 0f, -55f);

    [Header("Ground Slam - Torso Dynamics")]
    [SerializeField]
    private bool animateTorso = true;

    [Tooltip("Torso rotation offset during the wind-up phase (arching torso back).")]
    [SerializeField]
    private Vector3 torsoWindUpRotation = new Vector3(0f, 0f, 15f);

    [Tooltip("Torso rotation offset during impact (leaning torso heavily into the ground slam).")]
    [SerializeField]
    private Vector3 torsoSlamRotation = new Vector3(0f, 0f, -20f);

    [Header("Ground Slam - Head Dynamics")]
    [SerializeField]
    private bool animateHead = true;

    [Tooltip("Head rotation offset during wind-up (tilting head back as body arches).")]
    [SerializeField]
    private Vector3 headWindUpRotation = new Vector3(0f, 0f, 10f);

    [Tooltip("Head rotation offset during impact (head snapping forward/down into the strike).")]
    [SerializeField]
    private Vector3 headSlamRotation = new Vector3(0f, 0f, -25f);

    [Header("Timing")]
    [Tooltip("Duration of the downward strike swing in seconds.")]
    [SerializeField]
    private float slamDuration = 0.12f;

    [Tooltip("Duration of the recovery swing back to idle stance.")]
    [SerializeField]
    private float recoveryDuration = 0.35f;

    [Header("Ground Impact VFX & Audio")]
    [SerializeField]
    private ParticleSystem groundImpactVfxPrefab;

    [SerializeField]
    private float impactForwardOffset = 3f;

    [SerializeField]
    private AudioClip impactSound;

    [SerializeField, Range(0f, 1f)]
    private float soundVolume = 1f;

    private Vector3 armIdleEuler;
    private Vector3 torsoIdleEuler;
    private Vector3 headIdleEuler;
    private Vector3 weaponIdleEuler;
    private bool isInitialized;

    public float SlamDuration => slamDuration;
    public float RecoveryDuration => recoveryDuration;

    private Transform MainAttackTarget => rightArm != null ? rightArm : weapon;

    private void Awake()
    {
        InitializeLimbs();
    }

    private void InitializeLimbs()
    {
        if (rightArm == null) rightArm = FindChildRecursive(transform, "Arm_R");
        if (torso == null) torso = FindChildRecursive(transform, "Torso");
        if (head == null) head = FindChildRecursive(transform, "Head");
        if (weapon == null)
        {
            weapon = FindChildRecursive(transform, "Weapon");
            if (weapon == null) weapon = FindChildRecursive(transform, "Mace");
        }

        if (rightArm != null)
        {
            armIdleEuler = rightArm.localEulerAngles;
            isInitialized = true;
        }

        if (torso != null)
        {
            torsoIdleEuler = torso.localEulerAngles;
        }

        if (head != null)
        {
            headIdleEuler = head.localEulerAngles;
        }

        if (weapon != null)
        {
            weaponIdleEuler = weapon.localEulerAngles;
            if (rightArm == null)
            {
                isInitialized = true;
            }
        }

        if (rightArm == null && weapon == null)
        {
            Debug.LogError($"[{nameof(BossAttackVisuals)}] Neither 'rightArm' nor 'weapon' found on '{gameObject.name}'!", this);
        }
    }

    private void OnDisable()
    {
        if (rightArm != null) rightArm.DOKill();
        if (torso != null) torso.DOKill();
        if (head != null) head.DOKill();
        if (weapon != null) weapon.DOKill();
    }

    public void StartWindUp(float duration)
    {
        if (!isInitialized || !useWindUp)
        {
            return;
        }

        Transform target = MainAttackTarget;
        if (target != null)
        {
            target.DOKill();
            Vector3 baseEuler = (target == rightArm) ? armIdleEuler : weaponIdleEuler;
            target
                .DOLocalRotate(baseEuler + armWindUpRotation, duration)
                .SetEase(Ease.OutQuad);
        }

        if (torso != null && animateTorso)
        {
            torso.DOKill();
            torso
                .DOLocalRotate(torsoIdleEuler + torsoWindUpRotation, duration)
                .SetEase(Ease.OutQuad);
        }

        if (head != null && animateHead)
        {
            head.DOKill();
            head
                .DOLocalRotate(headIdleEuler + headWindUpRotation, duration)
                .SetEase(Ease.OutQuad);
        }
    }

    public void PlaySlamAttack()
    {
        if (!isInitialized)
        {
            Debug.LogError($"[{nameof(BossAttackVisuals)}] Cannot execute PlaySlamAttack because visuals are not initialized on '{gameObject.name}'!", this);
            return;
        }

        Transform target = MainAttackTarget;
        if (target != null) target.DOKill();
        if (torso != null) torso.DOKill();
        if (head != null) head.DOKill();

        Sequence slamSeq = DOTween.Sequence();

        // 1. Fast, heavy downward slam
        if (target != null)
        {
            Vector3 baseEuler = (target == rightArm) ? armIdleEuler : weaponIdleEuler;
            slamSeq.Append(
                target
                    .DOLocalRotate(baseEuler + armSlamRotation, slamDuration)
                    .SetEase(Ease.InQuad)
            );
        }

        if (torso != null && animateTorso)
        {
            slamSeq.Join(
                torso
                    .DOLocalRotate(torsoIdleEuler + torsoSlamRotation, slamDuration)
                    .SetEase(Ease.InQuad)
            );
        }

        if (head != null && animateHead)
        {
            slamSeq.Join(
                head
                    .DOLocalRotate(headIdleEuler + headSlamRotation, slamDuration)
                    .SetEase(Ease.InQuad)
            );
        }

        // 2. Trigger particle effect and impact sound on ground impact
        slamSeq.AppendCallback(TriggerGroundImpact);

        // 3. Smooth recovery back to idle stance
        if (target != null)
        {
            Vector3 baseEuler = (target == rightArm) ? armIdleEuler : weaponIdleEuler;
            slamSeq.Append(
                target
                    .DOLocalRotate(baseEuler, recoveryDuration)
                    .SetEase(Ease.OutQuad)
            );
        }

        if (torso != null && animateTorso)
        {
            slamSeq.Join(
                torso
                    .DOLocalRotate(torsoIdleEuler, recoveryDuration)
                    .SetEase(Ease.OutQuad)
            );
        }

        if (head != null && animateHead)
        {
            slamSeq.Join(
                head
                    .DOLocalRotate(headIdleEuler, recoveryDuration)
                    .SetEase(Ease.OutQuad)
            );
        }
    }

    private void TriggerGroundImpact()
    {
        Vector3 impactPosition = transform.position + transform.forward * impactForwardOffset;

        // Raycast down to find ground level
        if (Physics.Raycast(impactPosition + Vector3.up * 3f, Vector3.down, out RaycastHit hit, 6f, ~0, QueryTriggerInteraction.Ignore))
        {
            impactPosition.y = hit.point.y;
        }

        if (groundImpactVfxPrefab != null)
        {
            VfxSpawner.SpawnOneShot(groundImpactVfxPrefab, impactPosition, Quaternion.identity);
        }

        if (impactSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWorldSFX(impactSound, impactPosition, soundVolume);
        }
    }

    public void ResetToIdle()
    {
        if (rightArm != null)
        {
            rightArm.DOKill();
            rightArm.localEulerAngles = armIdleEuler;
        }
        if (torso != null)
        {
            torso.DOKill();
            torso.localEulerAngles = torsoIdleEuler;
        }
        if (head != null)
        {
            head.DOKill();
            head.localEulerAngles = headIdleEuler;
        }
        if (weapon != null)
        {
            weapon.DOKill();
            weapon.localEulerAngles = weaponIdleEuler;
        }
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Equals(childName, StringComparison.OrdinalIgnoreCase))
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

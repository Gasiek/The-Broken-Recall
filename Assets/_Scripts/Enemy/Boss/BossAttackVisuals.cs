using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

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

    [Header("Ground Slam - Timing")]
    [Tooltip("Duration of the downward strike swing in seconds.")]
    [SerializeField]
    private float slamDuration = 0.12f;

    [Tooltip("Duration of the recovery swing back to idle stance.")]
    [SerializeField]
    private float recoveryDuration = 0.35f;

    [Header("Spin Attack (Atak Dookoła) - Timing")]
    [Tooltip("Duration of bending down and extending the weapon outward in seconds.")]
    [SerializeField]
    private float spinCrouchDuration = 0.35f;

    [Tooltip("Duration of the full 360-degree spin rotation in seconds.")]
    [SerializeField]
    private float spinDuration = 0.45f;

    [Tooltip("Duration of recovering back to the normal standing stance.")]
    [SerializeField]
    private float spinRecoveryDuration = 0.35f;

    [Header("Spin Attack - Pose Offsets")]
    [Tooltip("Arm rotation offset when holding the weapon out for the sweep.")]
    [SerializeField]
    private Vector3 armSpinPoseRotation = new Vector3(15f, 0f, -30f);

    [Tooltip("Torso forward tilt / bend angle when crouching down.")]
    [SerializeField]
    private Vector3 torsoSpinCrouchRotation = new Vector3(0f, 0f, -25f);

    [Tooltip("Head tilt offset during crouch.")]
    [SerializeField]
    private Vector3 headSpinCrouchRotation = new Vector3(0f, 0f, -15f);

    [Header("Spin Attack - Effects")]
    [SerializeField]
    private AudioClip spinSound;

    [SerializeField, Range(0f, 1f)]
    private float spinSoundVolume = 1f;

    [SerializeField]
    private ParticleSystem spinVfxPrefab;

    [Header("Ground Impact VFX & Audio")]
    [SerializeField]
    private ParticleSystem groundImpactVfxPrefab;

    [SerializeField]
    private float impactForwardOffset = 3f;

    [SerializeField]
    private AudioClip impactSound;

    [SerializeField, Range(0f, 1f)]
    private float soundVolume = 1f;

    [Header("Debug & Testing")]
    [Tooltip("If enabled, pressing the 'T' key on keyboard during Play Mode will trigger the Spin Attack.")]
    [SerializeField]
    private bool enableDebugHotkey = true;

    private Vector3 armIdleEuler;
    private Vector3 torsoIdleEuler;
    private Vector3 headIdleEuler;
    private Vector3 weaponIdleEuler;
    private NavMeshAgent agent;
    private bool isInitialized;

    public float SlamDuration => slamDuration;
    public float RecoveryDuration => recoveryDuration;
    public float SpinCrouchDuration => spinCrouchDuration;
    public float SpinDuration => spinDuration;
    public float SpinRecoveryDuration => spinRecoveryDuration;
    public float TotalSpinAttackDuration => spinCrouchDuration + spinDuration + spinRecoveryDuration;

    public bool IsPerformingVisual { get; private set; }

    private Transform MainAttackTarget => rightArm != null ? rightArm : weapon;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        InitializeLimbs();
    }

    private void Update()
    {
        if (!enableDebugHotkey) return;

        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            TestSpinAttack();
        }
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
        IsPerformingVisual = false;
        if (rightArm != null) rightArm.DOKill();
        if (torso != null) torso.DOKill();
        if (head != null) head.DOKill();
        if (weapon != null) weapon.DOKill();
        transform.DOKill();
        if (agent != null) agent.updateRotation = true;
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

    /// <summary>
    /// Crouches down with the weapon extended outward in preparation for the 360-degree spin.
    /// </summary>
    public void StartSpinCrouch(float duration = -1f)
    {
        if (!isInitialized) return;

        float animDuration = duration > 0f ? duration : spinCrouchDuration;

        Transform target = MainAttackTarget;
        if (target != null)
        {
            target.DOKill();
            Vector3 baseEuler = (target == rightArm) ? armIdleEuler : weaponIdleEuler;
            target
                .DOLocalRotate(baseEuler + armSpinPoseRotation, animDuration)
                .SetEase(Ease.OutQuad);
        }

        if (torso != null && animateTorso)
        {
            torso.DOKill();
            torso
                .DOLocalRotate(torsoIdleEuler + torsoSpinCrouchRotation, animDuration)
                .SetEase(Ease.OutQuad);
        }

        if (head != null && animateHead)
        {
            head.DOKill();
            head
                .DOLocalRotate(headIdleEuler + headSpinCrouchRotation, animDuration)
                .SetEase(Ease.OutQuad);
        }
    }

    /// <summary>
    /// Performs the rapid 360-degree rotation spin.
    /// </summary>
    public void PlaySpinRotation(Action onComplete = null)
    {
        if (agent != null) agent.updateRotation = false;

        TriggerSpinEffects();

        transform.DOKill();
        Vector3 targetRot = transform.eulerAngles + new Vector3(0f, 360f, 0f);

        transform
            .DORotate(targetRot, spinDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.InOutCubic)
            .OnComplete(() =>
            {
                if (agent != null) agent.updateRotation = true;
                onComplete?.Invoke();
            });
    }

    /// <summary>
    /// Recovers back to the normal upright standing pose.
    /// </summary>
    public void RecoverFromSpin()
    {
        Transform target = MainAttackTarget;
        if (target != null)
        {
            target.DOKill();
            Vector3 baseEuler = (target == rightArm) ? armIdleEuler : weaponIdleEuler;
            target
                .DOLocalRotate(baseEuler, spinRecoveryDuration)
                .SetEase(Ease.OutQuad);
        }

        if (torso != null && animateTorso)
        {
            torso.DOKill();
            torso
                .DOLocalRotate(torsoIdleEuler, spinRecoveryDuration)
                .SetEase(Ease.OutQuad);
        }

        if (head != null && animateHead)
        {
            head.DOKill();
            head
                .DOLocalRotate(headIdleEuler, spinRecoveryDuration)
                .SetEase(Ease.OutQuad);
        }
    }

    /// <summary>
    /// Complete orchestrated sequence: Crouch & extend weapon -> Spin 360 degrees -> Recover back to idle.
    /// </summary>
    public Sequence PlaySpinAttack(Action onSpinStrike = null, Action onComplete = null)
    {
        if (!isInitialized)
        {
            Debug.LogError($"[{nameof(BossAttackVisuals)}] Cannot execute PlaySpinAttack because visuals are not initialized on '{gameObject.name}'!", this);
            return null;
        }

        Transform target = MainAttackTarget;
        if (target != null) target.DOKill();
        if (torso != null) torso.DOKill();
        if (head != null) head.DOKill();
        transform.DOKill();

        Vector3 baseEuler = (target == rightArm) ? armIdleEuler : weaponIdleEuler;

        IsPerformingVisual = true;

        Sequence spinSeq = DOTween.Sequence();

        // Phase 1: Crouch down and extend weapon outward ("ma się schylić z wyciągniętą bronią")
        if (torso != null && animateTorso)
        {
            spinSeq.Join(torso.DOLocalRotate(torsoIdleEuler + torsoSpinCrouchRotation, spinCrouchDuration).SetEase(Ease.OutQuad));
        }
        if (head != null && animateHead)
        {
            spinSeq.Join(head.DOLocalRotate(headIdleEuler + headSpinCrouchRotation, spinCrouchDuration).SetEase(Ease.OutQuad));
        }
        if (target != null)
        {
            spinSeq.Join(target.DOLocalRotate(baseEuler + armSpinPoseRotation, spinCrouchDuration).SetEase(Ease.OutQuad));
        }

        // Phase 2: Full 360-degree spin rotation ("i się obrócić")
        // Torso, head and arm maintain their crouched/extended pose during this entire phase!
        spinSeq.AppendCallback(() =>
        {
            if (agent != null) agent.updateRotation = false;
            TriggerSpinEffects();
        });

        spinSeq.Append(
            transform
                .DORotate(transform.eulerAngles + new Vector3(0f, 360f, 0f), spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutCubic)
        );

        // Strike impact moment occurs during the spin (around 40% into the spin rotation)
        if (onSpinStrike != null)
        {
            spinSeq.InsertCallback(spinCrouchDuration + spinDuration * 0.4f, () => onSpinStrike.Invoke());
        }

        spinSeq.AppendCallback(() =>
        {
            if (agent != null) agent.updateRotation = true;
        });

        // Phase 3: Smooth recovery back to idle stance - strictly AFTER the 360 spin finishes!
        if (torso != null && animateTorso)
        {
            spinSeq.Append(torso.DOLocalRotate(torsoIdleEuler, spinRecoveryDuration).SetEase(Ease.OutQuad));
            if (head != null && animateHead)
            {
                spinSeq.Join(head.DOLocalRotate(headIdleEuler, spinRecoveryDuration).SetEase(Ease.OutQuad));
            }
            if (target != null)
            {
                spinSeq.Join(target.DOLocalRotate(baseEuler, spinRecoveryDuration).SetEase(Ease.OutQuad));
            }
        }
        else if (target != null)
        {
            spinSeq.Append(target.DOLocalRotate(baseEuler, spinRecoveryDuration).SetEase(Ease.OutQuad));
            if (head != null && animateHead)
            {
                spinSeq.Join(head.DOLocalRotate(headIdleEuler, spinRecoveryDuration).SetEase(Ease.OutQuad));
            }
        }
        else if (head != null && animateHead)
        {
            spinSeq.Append(head.DOLocalRotate(headIdleEuler, spinRecoveryDuration).SetEase(Ease.OutQuad));
        }

        spinSeq.OnComplete(() =>
        {
            IsPerformingVisual = false;
            onComplete?.Invoke();
        });

        return spinSeq;
    }

    private void TriggerSpinEffects()
    {
        if (spinVfxPrefab != null)
        {
            Vector3 vfxPos = transform.position + Vector3.up * 1f;
            VfxSpawner.SpawnOneShot(spinVfxPrefab, vfxPos, Quaternion.identity);
        }

        if (spinSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWorldSFX(spinSound, transform.position, spinSoundVolume);
        }
    }

    public void ResetToIdle()
    {
        IsPerformingVisual = false;
        transform.DOKill();
        if (agent != null) agent.updateRotation = true;

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

    [ContextMenu("Test Spin Attack")]
    public void TestSpinAttack()
    {
        if (!isInitialized)
        {
            InitializeLimbs();
        }

        Debug.Log($"[{nameof(BossAttackVisuals)}] Triggering test Spin Attack on '{gameObject.name}'!");
        PlaySpinAttack(
            onSpinStrike: () => Debug.Log($"[{nameof(BossAttackVisuals)}] Spin strike impact point reached!"),
            onComplete: () => Debug.Log($"[{nameof(BossAttackVisuals)}] Spin attack completed!")
        );
    }

    [ContextMenu("Test Ground Slam")]
    public void TestGroundSlam()
    {
        if (!isInitialized)
        {
            InitializeLimbs();
        }

        Debug.Log($"[{nameof(BossAttackVisuals)}] Triggering test Ground Slam on '{gameObject.name}'!");
        StartWindUp(0.4f);
        DOVirtual.DelayedCall(0.4f, () =>
        {
            PlaySlamAttack();
        });
    }

    [ContextMenu("Reset Limbs To Idle")]
    public void ContextMenuResetToIdle()
    {
        ResetToIdle();
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

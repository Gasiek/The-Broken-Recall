using DG.Tweening;
using UnityEngine;

public class EnemyAttackVisuals : MonoBehaviour
{
    [SerializeField]
    private Transform weapon;

    [Header("Basic Attack")]
    [SerializeField]
    private Vector3 basicAttackRotation = new Vector3(0f, 0f, 90f);

    [SerializeField]
    private float basicAttackDuration = 0.2f;

    private Vector3 weaponIdleRotation;

    private void Awake()
    {
        if (weapon == null)
        {
            Debug.LogError($"[{nameof(EnemyAttackVisuals)}] 'weapon' transform is not assigned on '{gameObject.name}'!", this);
            return;
        }

        weaponIdleRotation = weapon.localEulerAngles;
    }

    public void BasicAttack()
    {
        if (weapon == null)
        {
            Debug.LogError($"[{nameof(EnemyAttackVisuals)}] Cannot execute BasicAttack because 'weapon' is null on '{gameObject.name}'!", this);
            return;
        }

        weapon.DOKill();

        weapon
            .DOLocalRotate(
                weaponIdleRotation + basicAttackRotation,
                basicAttackDuration
            )
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo);
    }
}
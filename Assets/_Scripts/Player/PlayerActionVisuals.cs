using DG.Tweening;
using UnityEngine;

public class PlayerActionVisuals : MonoBehaviour
{
    [SerializeField]
    private Transform sword;

    public Transform Sword => sword;

    [Header("Push")]
    [SerializeField]
    private Vector3 pushRotation = new Vector3(90f, 0f, 0f);

    [SerializeField]
    private Vector3 pushOffset = new Vector3(0f, 0f, 0.5f);

    [SerializeField]
    private float pushTransitionDuration = 0.05f;

    [SerializeField]
    private float pushDuration = 0.2f;

    [Header("Basic Hit")]
    [SerializeField]
    private Vector3 basicHitRotation = new Vector3(0f, 0f, 90f);

    [SerializeField]
    private float basicHitSwingDuration = 0.2f;

    [Header("Block")]
    [SerializeField]
    private Vector3 blockRotation = new Vector3(0f, 0f, 45f);

    [SerializeField]
    private Vector3 blockPosition = new Vector3(-0.3f, 0f, 0.8f);

    [SerializeField]
    private float blockTransitionDuration = 0.15f;

    [Header("Lightning Charge")]
    [SerializeField]
    private Vector3 chargeRotation = new Vector3(-30f, 0f, 15f);

    [SerializeField]
    private Vector3 chargeOffset = new Vector3(0f, 0.4f, 0.3f);

    [Header("Sword Charge")]
    [SerializeField]
    private Vector3 swordChargeRotation = new Vector3(90f, 0f, 0f);

    [SerializeField]
    private Vector3 swordChargeOffset = new Vector3(0f, 0.1f, 0.65f);

    private Vector3 swordIdleRotation;
    private Vector3 swordIdlePosition;

    private void Awake()
    {
        swordIdlePosition = sword.localPosition;
        swordIdleRotation = sword.localEulerAngles;
    }

    public void BasicHit()
    {
        sword.DOKill();

        sword
            .DOLocalRotate(swordIdleRotation + basicHitRotation, basicHitSwingDuration)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo);
    }

    public void Block(float duration)
    {
        sword.DOKill();

        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            sword
                .DOLocalRotate(swordIdleRotation + blockRotation, blockTransitionDuration)
                .SetEase(Ease.OutQuad)
        );

        sequence.Join(
            sword
                .DOLocalMove(swordIdlePosition + blockPosition, blockTransitionDuration)
                .SetEase(Ease.OutQuad)
        );

        sequence.AppendInterval(Mathf.Max(0f, duration - blockTransitionDuration * 2f));

        sequence.Append(
            sword.DOLocalRotate(swordIdleRotation, blockTransitionDuration).SetEase(Ease.InQuad)
        );

        sequence.Join(
            sword.DOLocalMove(swordIdlePosition, blockTransitionDuration).SetEase(Ease.InQuad)
        );
    }

    public void Push()
    {
        sword.DOKill();

        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            sword
                .DOLocalRotate(swordIdleRotation + pushRotation, pushTransitionDuration)
                .SetEase(Ease.OutQuad)
        );

        sequence.Append(
            sword.DOLocalMove(swordIdlePosition + pushOffset, pushDuration).SetEase(Ease.OutQuad)
        );

        sequence.Append(sword.DOLocalMove(swordIdlePosition, pushDuration).SetEase(Ease.InQuad));

        sequence.Append(
            sword.DOLocalRotate(swordIdleRotation, pushTransitionDuration).SetEase(Ease.InQuad)
        );
    }

    public void ChargeSword(float duration)
    {
        sword.DOKill();

        Sequence sequence = DOTween.Sequence();
        float raiseTime = duration * 0.3f;
        float holdTime = duration * 0.4f;
        float returnTime = Mathf.Max(0.05f, duration - raiseTime - holdTime);

        // 1. Raise sword before chest frame
        sequence.Append(
            sword
                .DOLocalRotate(swordIdleRotation + chargeRotation, raiseTime)
                .SetEase(Ease.OutQuad)
        );
        sequence.Join(
            sword
                .DOLocalMove(swordIdlePosition + chargeOffset, raiseTime)
                .SetEase(Ease.OutQuad)
        );

        // 2. Gentle energy vibrations in the charged sword
        sequence.Append(
            sword.DOShakePosition(holdTime, 0.04f, 25, 90f, false, true)
        );

        // 3. Smooth return to idle position
        sequence.Append(
            sword
                .DOLocalRotate(swordIdleRotation, returnTime)
                .SetEase(Ease.OutQuad)
        );
        sequence.Join(
            sword
                .DOLocalMove(swordIdlePosition, returnTime)
                .SetEase(Ease.OutQuad)
        );
    }

    public void SwordCharge(float prepTime, float holdTime, float dashTime, float returnTime)
    {
        sword.DOKill();

        Sequence sequence = DOTween.Sequence();

        // 1. Extend sword forward ("Player wyciaga miecz przed siebie")
        sequence.Append(
            sword
                .DOLocalRotate(swordIdleRotation + swordChargeRotation, prepTime)
                .SetEase(Ease.OutQuad)
        );
        sequence.Join(
            sword
                .DOLocalMove(swordIdlePosition + swordChargeOffset, prepTime)
                .SetEase(Ease.OutQuad)
        );

        // 2. Hold sword ready before the burst ("trzyma go tak")
        if (holdTime > 0f)
        {
            sequence.AppendInterval(holdTime);
        }

        // 3. Maintain thrust steadily during the rapid dash forward ("i nagle szybko w 1 sek porusza sie do przodu")
        if (dashTime > 0f)
        {
            sequence.AppendInterval(dashTime);
        }

        // 4. Smooth recovery back to idle stance
        sequence.Append(
            sword
                .DOLocalRotate(swordIdleRotation, returnTime)
                .SetEase(Ease.OutQuad)
        );
        sequence.Join(
            sword
                .DOLocalMove(swordIdlePosition, returnTime)
                .SetEase(Ease.OutQuad)
        );
    }

    public void ResetVisuals()
    {
        sword.DOKill();
        sword.localPosition = swordIdlePosition;
        sword.localEulerAngles = swordIdleRotation;
    }
}

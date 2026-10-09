using DG.Tweening;
using UnityEngine;

public class PlayerActionVisuals : MonoBehaviour
{
    [SerializeField]
    private Transform sword;

    public Transform Sword => sword;

    [SerializeField]
    private Transform tornadoPivot;

    [Header("Push")]
    [SerializeField]
    private Vector3 pushRotation = new Vector3(90f, 0f, 0f);

    [SerializeField]
    private Vector3 pushOffset = new Vector3(0f, 0f, 0.5f);

    [SerializeField]
    private float pushTransitionDuration = 0.05f;

    [SerializeField]
    private float pushDuration = 0.2f;

    [Header("Tornado")]
    [SerializeField]
    private Vector3 tornadoRotation = new Vector3(0f, 0f, 90f);

    [SerializeField]
    private float tornadoDuration = 0.4f;

    [SerializeField]
    private float tornadoTransitionDuration = 0.1f;

    private Quaternion tornadoPivotIdleRotation;

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

    [Header("Jump Slam")]
    [SerializeField]
    private Vector3 jumpSlamPrepRotation = new Vector3(-60f, 0f, 0f);

    [SerializeField]
    private Vector3 jumpSlamPrepOffset = new Vector3(0f, 0.8f, -0.2f);

    [SerializeField]
    private Vector3 jumpSlamLandRotation = new Vector3(80f, 0f, 0f);

    [SerializeField]
    private Vector3 jumpSlamLandOffset = new Vector3(0f, -0.4f, 0.6f);

    [Header("Lightning Charge")]
    [SerializeField]
    private Vector3 chargeRotation = new Vector3(-30f, 0f, 15f);

    [SerializeField]
    private Vector3 chargeOffset = new Vector3(0f, 0.4f, 0.3f);

    private Vector3 swordIdleRotation;
    private Vector3 swordIdlePosition;

    private void Awake()
    {
        swordIdlePosition = sword.localPosition;
        swordIdleRotation = sword.localEulerAngles;

        tornadoPivotIdleRotation = tornadoPivot.localRotation;
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

    public void Tornado()
    {
        sword.DOKill();
        tornadoPivot.DOKill();

        tornadoPivot.localRotation = tornadoPivotIdleRotation;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            sword.DOLocalMove(swordIdlePosition, tornadoTransitionDuration).SetEase(Ease.OutQuad)
        );

        sequence.Join(
            sword
                .DOLocalRotate(swordIdleRotation + tornadoRotation, tornadoTransitionDuration)
                .SetEase(Ease.OutQuad)
        );

        sequence.Append(
            tornadoPivot
                .DOLocalRotate(new Vector3(0f, 360f, 0f), tornadoDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
        );

        sequence.Append(
            sword.DOLocalRotate(swordIdleRotation, tornadoTransitionDuration).SetEase(Ease.InQuad)
        );

        sequence.Join(
            tornadoPivot
                .DOLocalRotate(tornadoPivotIdleRotation.eulerAngles, tornadoTransitionDuration)
                .SetEase(Ease.InQuad)
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

    public void JumpSlam(float duration)
    {
        sword.DOKill();

        float prepTime = duration * 0.45f;
        float slamTime = duration * 0.25f;
        float returnTime = Mathf.Max(0.05f, duration - prepTime - slamTime);

        Sequence sequence = DOTween.Sequence();

        // 1. Rise & windup: raise sword above head
        sequence.Append(
            sword
                .DOLocalRotate(swordIdleRotation + jumpSlamPrepRotation, prepTime)
                .SetEase(Ease.OutQuad)
        );
        sequence.Join(
            sword
                .DOLocalMove(swordIdlePosition + jumpSlamPrepOffset, prepTime)
                .SetEase(Ease.OutQuad)
        );

        // 2. Drive sword down during the descent
        sequence.Append(
            sword
                .DOLocalRotate(swordIdleRotation + jumpSlamLandRotation, slamTime)
                .SetEase(Ease.InCubic)
        );
        sequence.Join(
            sword
                .DOLocalMove(swordIdlePosition + jumpSlamLandOffset, slamTime)
                .SetEase(Ease.InCubic)
        );

        // 3. Shake on impact when the sword hits the ground
        sequence.Append(
            sword.DOShakePosition(0.08f, 0.15f, 10, 90f, false, true)
        );

        // 4. Return to idle position
        sequence.Append(
            sword.DOLocalRotate(swordIdleRotation, returnTime).SetEase(Ease.OutQuad)
        );
        sequence.Join(
            sword.DOLocalMove(swordIdlePosition, returnTime).SetEase(Ease.OutQuad)
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

    public void ResetVisuals()
    {
        sword.DOKill();
        tornadoPivot.DOKill();
        sword.localPosition = swordIdlePosition;
        sword.localEulerAngles = swordIdleRotation;
        tornadoPivot.localRotation = tornadoPivotIdleRotation;
    }
}

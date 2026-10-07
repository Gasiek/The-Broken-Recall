using DG.Tweening;
using UnityEngine;

public class PlayerActionVisuals : MonoBehaviour
{
    [SerializeField]
    private Transform sword;

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
}

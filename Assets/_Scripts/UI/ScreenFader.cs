using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    private CanvasGroup canvasGroup;
    private Coroutine sleepRoutine;

    public bool IsSleepingSequenceRunning => sleepRoutine != null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public void FadeToBlack(float duration, Action onComplete = null)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.DOKill();

        canvasGroup.DOFade(1f, duration).OnComplete(() => onComplete?.Invoke());
    }

    public void FadeFromBlack(float duration, Action onComplete = null)
    {
        canvasGroup.DOKill();

        canvasGroup
            .DOFade(0f, duration)
            .OnComplete(() =>
            {
                canvasGroup.blocksRaycasts = false;
                onComplete?.Invoke();
            });
    }

    public void SleepSequence(
        float fadeOutTime,
        float sleepHoldTime,
        float fadeInTime,
        Action onSleep
    )
    {
        if (sleepRoutine != null)
        {
            Debug.LogWarning("[ScreenFader] A sleep sequence is already running.", this);

            return;
        }

        sleepRoutine = StartCoroutine(
            SleepRoutine(fadeOutTime, sleepHoldTime, fadeInTime, onSleep)
        );
    }

    private IEnumerator SleepRoutine(
        float fadeOutTime,
        float sleepHoldTime,
        float fadeInTime,
        Action onSleep
    )
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.DOKill();

        // 1. Fade to black.
        yield return canvasGroup.DOFade(1f, Mathf.Max(0f, fadeOutTime)).WaitForCompletion();

        // 2. Perform the sleep effect while the screen is black.
        onSleep?.Invoke();

        // 3. Hold the black screen.
        yield return new WaitForSeconds(Mathf.Max(0f, sleepHoldTime));

        // 4. Fade back in.
        yield return canvasGroup.DOFade(0f, Mathf.Max(0f, fadeInTime)).WaitForCompletion();

        canvasGroup.blocksRaycasts = false;
        sleepRoutine = null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        canvasGroup?.DOKill();
    }
}

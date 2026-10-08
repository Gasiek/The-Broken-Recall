using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    private CanvasGroup canvasGroup;

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
        canvasGroup.DOFade(0f, duration).OnComplete(() =>
        {
            canvasGroup.blocksRaycasts = false;
            onComplete?.Invoke();
        });
    }

    public void SleepSequence(float fadeOutTime, float sleepHoldTime, float fadeInTime, Action onSleep)
    {
        StartCoroutine(SleepRoutine(fadeOutTime, sleepHoldTime, fadeInTime, onSleep));
    }

    private IEnumerator SleepRoutine(float fadeOutTime, float sleepHoldTime, float fadeInTime, Action onSleep)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.DOKill();

        // 1. Ściemnianie
        yield return canvasGroup.DOFade(1f, fadeOutTime).WaitForCompletion();

        // 2. W szczycie czerni - sen, leczenie, dźwięk
        onSleep?.Invoke();

        // 3. Czekamy chwilę w ciemności
        yield return new WaitForSeconds(sleepHoldTime);

        // 4. Rozjaśnianie
        yield return canvasGroup.DOFade(0f, fadeInTime).WaitForCompletion();

        canvasGroup.blocksRaycasts = false;
    }
}

using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    private PanelRenderer panelRenderer;
    private VisualElement fadeOverlay;

    private Coroutine sleepRoutine;
    private Tween fadeTween;

    public bool IsSleepingSequenceRunning => sleepRoutine != null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        panelRenderer = GetComponent<PanelRenderer>();
    }

    private void OnEnable()
    {
        if (panelRenderer == null)
        {
            panelRenderer = GetComponent<PanelRenderer>();
        }

        if (panelRenderer != null)
        {
            panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }
    }

    private void OnDisable()
    {
        if (panelRenderer != null)
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        StopSleepSequence();
        KillFadeTween();
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        KillFadeTween();
        fadeOverlay = null;

        if (root == null)
        {
            Debug.LogError("[ScreenFader] UI reload received a null root.", this);
            return;
        }

        fadeOverlay = root.Q<VisualElement>("FadeOverlay");

        if (fadeOverlay == null)
        {
            Debug.LogError($"[ScreenFader] Could not find FadeOverlay in the UI.", this);
            return;
        }

        fadeOverlay.style.opacity = 0f;
        SetRaycastBlocking(false);
    }

    public void FadeToBlack(float duration, Action onComplete = null)
    {
        if (!IsOverlayReady())
        {
            return;
        }

        KillFadeTween();
        SetRaycastBlocking(true);

        fadeTween = CreateFadeTween(1f, duration, onComplete);
    }

    public void FadeFromBlack(float duration, Action onComplete = null)
    {
        if (!IsOverlayReady())
        {
            return;
        }

        KillFadeTween();

        fadeTween = CreateFadeTween(
            0f,
            duration,
            () =>
            {
                SetRaycastBlocking(false);
                onComplete?.Invoke();
            }
        );
    }

    public void SleepSequence(
        float fadeOutTime,
        float sleepHoldTime,
        float fadeInTime,
        Action onSleep
    )
    {
        if (!IsOverlayReady())
        {
            return;
        }

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
        SetRaycastBlocking(true);
        KillFadeTween();

        fadeTween = CreateFadeTween(1f, fadeOutTime);
        yield return fadeTween.WaitForCompletion();

        onSleep?.Invoke();

        yield return new WaitForSeconds(Mathf.Max(0f, sleepHoldTime));

        fadeTween = CreateFadeTween(0f, fadeInTime);
        yield return fadeTween.WaitForCompletion();

        SetRaycastBlocking(false);

        fadeTween = null;
        sleepRoutine = null;
    }

    private Tween CreateFadeTween(float targetOpacity, float duration, Action onComplete = null)
    {
        return DOTween
            .To(
                () => fadeOverlay.resolvedStyle.opacity,
                value => fadeOverlay.style.opacity = value,
                targetOpacity,
                Mathf.Max(0f, duration)
            )
            .SetUpdate(true)
            .OnComplete(() => onComplete?.Invoke());
    }

    private void SetRaycastBlocking(bool blocks)
    {
        if (fadeOverlay == null)
        {
            return;
        }

        fadeOverlay.pickingMode = blocks ? PickingMode.Position : PickingMode.Ignore;
    }

    private bool IsOverlayReady()
    {
        if (fadeOverlay != null)
        {
            return true;
        }

        Debug.LogError(
            "[ScreenFader] FadeOverlay is not initialized. "
                + "Check the PanelRenderer and UI document.",
            this
        );

        return false;
    }

    private void KillFadeTween()
    {
        if (fadeTween != null && fadeTween.IsActive())
        {
            fadeTween.Kill();
        }

        fadeTween = null;
    }

    private void StopSleepSequence()
    {
        if (sleepRoutine != null)
        {
            StopCoroutine(sleepRoutine);
            sleepRoutine = null;
        }
    }

    private void OnDestroy()
    {
        StopSleepSequence();
        KillFadeTween();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}

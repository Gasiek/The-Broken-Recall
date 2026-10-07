using System.Collections;
using UnityEngine;

public class ActionBarUI : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField]
    private ActionMemory actionMemory;

    [SerializeField]
    private ActionController actionController;

    [Header("Slots")]
    [SerializeField]
    private ActionSlotUI[] slots;

    private Coroutine cooldownRoutine;

    private void Awake()
    {
        if (slots == null || slots.Length != ActionMemory.SlotCount)
        {
            Debug.LogError($"ActionBarUI requires exactly {ActionMemory.SlotCount} slots.");
        }
    }

    private void OnEnable()
    {
        if (actionMemory != null)
        {
            actionMemory.MemoryChanged += RefreshSlots;
        }

        if (actionController != null)
        {
            actionController.ActionStarted += StartCooldown;
        }

        RefreshSlots();
    }

    private void OnDisable()
    {
        if (actionMemory != null)
        {
            actionMemory.MemoryChanged -= RefreshSlots;
        }

        if (actionController != null)
        {
            actionController.ActionStarted -= StartCooldown;
        }

        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
            cooldownRoutine = null;
        }
    }

    private void RefreshSlots()
    {
        if (actionMemory == null || slots == null)
        {
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                continue;
            }

            slots[i].SetAction(actionMemory.GetAction(i));
        }
    }

    private void StartCooldown(float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
        }

        cooldownRoutine = StartCoroutine(CooldownRoutine(duration));
    }

    private IEnumerator CooldownRoutine(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float remainingProgress = 1f - (elapsed / duration);

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null)
                {
                    slots[i].SetCooldown(remainingProgress);
                }
            }

            yield return null;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].SetCooldown(0f);
            }
        }

        cooldownRoutine = null;
    }
}

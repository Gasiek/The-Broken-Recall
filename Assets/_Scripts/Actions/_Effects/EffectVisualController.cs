using System.Collections.Generic;
using UnityEngine;

public class EffectVisualController : MonoBehaviour
{
    [Header("Attachment Points")]
    [SerializeField]
    private Transform characterAttachment;

    [SerializeField]
    private Transform weaponAttachment;

    private readonly Dictionary<string, List<GameObject>> activeVisuals =
        new Dictionary<string, List<GameObject>>();

    private void Awake()
    {
        if (characterAttachment == null)
            characterAttachment = transform;
    }

    public void ShowEffect(string effectKey, EffectDefinition definition)
    {
        if (string.IsNullOrEmpty(effectKey) || definition == null)
            return;

        // Keep existing visuals alive when the effect duration is refreshed.
        if (activeVisuals.ContainsKey(effectKey))
            return;

        List<GameObject> instances = new List<GameObject>();

        foreach (EffectVisualConfig visual in definition.Visuals)
        {
            if (visual == null || visual.Prefab == null)
                continue;

            Transform attachment = GetAttachment(visual.Attachment);

            if (attachment == null)
            {
                Debug.LogWarning(
                    $"{name}: No attachment assigned for {visual.Attachment} visual on effect '{definition.name}'.",
                    this
                );
                continue;
            }

            GameObject instance = Instantiate(visual.Prefab, attachment);

            Transform visualTransform = instance.transform;
            visualTransform.localPosition = visual.PositionOffset;
            visualTransform.localRotation = Quaternion.Euler(visual.RotationOffset);
            visualTransform.localScale = visual.Scale;

            instances.Add(instance);
        }

        if (instances.Count > 0)
            activeVisuals[effectKey] = instances;
    }

    public void RemoveEffect(string effectKey)
    {
        if (string.IsNullOrEmpty(effectKey))
            return;

        if (!activeVisuals.TryGetValue(effectKey, out List<GameObject> instances))
            return;

        foreach (GameObject instance in instances)
        {
            if (instance != null)
                Destroy(instance);
        }

        activeVisuals.Remove(effectKey);
    }

    public void ClearAllVisuals()
    {
        foreach (List<GameObject> instances in activeVisuals.Values)
        {
            foreach (GameObject instance in instances)
            {
                if (instance != null)
                    Destroy(instance);
            }
        }

        activeVisuals.Clear();
    }

    private Transform GetAttachment(EffectVisualAttachment attachment)
    {
        switch (attachment)
        {
            case EffectVisualAttachment.Character:
                return characterAttachment;

            case EffectVisualAttachment.Weapon:
                return weaponAttachment;

            default:
                return null;
        }
    }

    private void OnDisable()
    {
        ClearAllVisuals();
    }
}

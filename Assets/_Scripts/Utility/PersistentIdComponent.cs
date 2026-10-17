using System;
using System.Text.RegularExpressions;
using UnityEngine;

public abstract class PersistentIdComponent : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private string stableId;

    public string StableId => stableId;

    /// <summary>
    /// Assigns a stable ID at runtime.
    /// Intended for dynamically spawned persistent objects.
    /// </summary>
    public bool SetStableIdForRuntime(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogError($"[{GetType().Name}] Cannot assign an empty stable ID.", this);

            return false;
        }

        stableId = id;
        return true;
    }

#if UNITY_EDITOR
    public static string GenerateIdForName(
        string objectName,
        string fallbackName = "PersistentObject"
    )
    {
        objectName = Regex.Replace(objectName.Trim(), @"[^a-zA-Z0-9_-]+", "_").Trim('_');

        if (string.IsNullOrWhiteSpace(objectName))
            objectName = fallbackName;

        return $"{objectName}_{Guid.NewGuid():N}";
    }

    protected virtual void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(stableId))
        {
            stableId = GenerateIdForName(gameObject.name, GetType().Name);

            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PersistentIdComponent), true)]
[CanEditMultipleObjects]
public class PersistentIdComponentEditor : Editor
{
    private SerializedProperty stableIdProperty;

    private void OnEnable()
    {
        stableIdProperty = serializedObject.FindProperty("stableId");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (property.propertyPath == "m_Script" || property.propertyPath == "stableId")
            {
                continue;
            }

            EditorGUILayout.PropertyField(property, true);
        }

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PropertyField(stableIdProperty, new GUIContent("Stable ID"));

        if (GUILayout.Button("Regenerate ID", GUILayout.Width(120)))
        {
            RegenerateIds();
        }

        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void RegenerateIds()
    {
        serializedObject.Update();

        foreach (Object obj in targets)
        {
            PersistentIdComponent component = (PersistentIdComponent)obj;

            SerializedObject so = new SerializedObject(component);
            so.Update();

            SerializedProperty id = so.FindProperty("stableId");

            Undo.RecordObject(component, "Regenerate Stable ID");

            id.stringValue = PersistentIdComponent.GenerateIdForName(
                component.gameObject.name,
                component.GetType().Name
            );

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(component);
        }

        serializedObject.Update();
    }
}

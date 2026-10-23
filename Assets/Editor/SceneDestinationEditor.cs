using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneDestination))]
public sealed class SceneDestinationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty destinationName = serializedObject.FindProperty("destinationName");

        SerializedProperty sceneName = serializedObject.FindProperty("sceneName");

        EditorGUILayout.PropertyField(destinationName);

        string currentSceneName = sceneName.stringValue;

        string[] sceneNames = EditorBuildSettings
            .scenes.Where(scene => scene.enabled)
            .Select(scene => System.IO.Path.GetFileNameWithoutExtension(scene.path))
            .ToArray();

        if (sceneNames.Length == 0)
        {
            EditorGUILayout.HelpBox(
                "Add scenes to Build Settings before selecting a destination.",
                MessageType.Warning
            );
        }
        else
        {
            int selectedIndex = System.Array.IndexOf(sceneNames, currentSceneName);

            if (selectedIndex < 0)
                selectedIndex = 0;

            int newIndex = EditorGUILayout.Popup("Destination Scene", selectedIndex, sceneNames);

            sceneName.stringValue = sceneNames[newIndex];
        }

        serializedObject.ApplyModifiedProperties();
    }
}

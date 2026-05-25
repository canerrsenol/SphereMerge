using UnityEditor;
using UnityEngine;

// Adds level editing actions to the SpheresManager inspector.
[CustomEditor(typeof(SpheresManager))]
public sealed class SpheresManagerInspector : Editor
{
    // Draws normal fields and a button that opens the grid editor.
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();

        if (GUILayout.Button("Edit Level"))
        {
            SpheresManagerEditorWindow.Open((SpheresManager)target);
        }
    }
}

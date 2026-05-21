using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public sealed class TileObjectPickerWindow : EditorWindow
{
    private const float PreviewSize = 48f;

    private readonly List<GameObject> prefabs = new List<GameObject>();
    private Action<GameObject> onSelected;
    private Action onClosed;
    private string folderPath;
    private string searchText = string.Empty;
    private Vector2 scrollPosition;
    private bool selectionCompleted;

    public static void Open(string folderPath, Action<GameObject> onSelected, Action onClosed = null)
    {
        TileObjectPickerWindow window = CreateInstance<TileObjectPickerWindow>();
        window.titleContent = new GUIContent("Tile Objects");
        window.minSize = new Vector2(360f, 420f);
        window.Initialize(folderPath, onSelected, onClosed);
        window.ShowUtility();
    }

    private void Initialize(string path, Action<GameObject> selectedCallback, Action closedCallback)
    {
        folderPath = string.IsNullOrWhiteSpace(path)
            ? "Assets/Prefabs/TileObjects"
            : path.Replace('\\', '/');

        onSelected = selectedCallback;
        onClosed = closedCallback;
        RefreshPrefabs();
    }

    private void OnDestroy()
    {
        if (!selectionCompleted)
        {
            onClosed?.Invoke();
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(6f);

        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUIStyle searchFieldStyle = GUI.skin.FindStyle("ToolbarSeachTextField") ?? GUI.skin.textField;
            GUIStyle cancelButtonStyle = GUI.skin.FindStyle("ToolbarSeachCancelButton") ?? EditorStyles.toolbarButton;

            searchText = GUILayout.TextField(searchText, searchFieldStyle, GUILayout.ExpandWidth(true));

            if (GUILayout.Button(GUIContent.none, cancelButtonStyle))
            {
                searchText = string.Empty;
                GUI.FocusControl(null);
            }

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70f)))
            {
                RefreshPrefabs();
            }
        }

        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("Folder", folderPath, EditorStyles.miniLabel);

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            EditorGUILayout.HelpBox($"Folder not found: {folderPath}", MessageType.Warning);
            return;
        }

        if (prefabs.Count == 0)
        {
            EditorGUILayout.HelpBox("No prefabs found in this folder.", MessageType.Info);
            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < prefabs.Count; i++)
        {
            GameObject prefab = prefabs[i];
            if (prefab == null || !MatchesSearch(prefab))
            {
                continue;
            }

            DrawPrefabRow(prefab);
        }

        EditorGUILayout.EndScrollView();
    }

    private void RefreshPrefabs()
    {
        prefabs.Clear();

        if (string.IsNullOrWhiteSpace(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null)
            {
                prefabs.Add(prefab);
            }
        }

        prefabs.Sort((left, right) => string.Compare(left.name, right.name, StringComparison.OrdinalIgnoreCase));
    }

    private bool MatchesSearch(GameObject prefab)
    {
        return string.IsNullOrWhiteSpace(searchText)
            || prefab.name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void DrawPrefabRow(GameObject prefab)
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.Height(PreviewSize + 12f)))
        {
            Texture2D preview = AssetPreview.GetAssetPreview(prefab);
            if (preview == null)
            {
                preview = AssetPreview.GetMiniThumbnail(prefab);
            }

            GUILayout.Label(preview, GUILayout.Width(PreviewSize), GUILayout.Height(PreviewSize));

            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Space(8f);
                EditorGUILayout.LabelField(prefab.name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(prefab), EditorStyles.miniLabel);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Select", GUILayout.Width(72f), GUILayout.Height(28f)))
            {
                selectionCompleted = true;
                onSelected?.Invoke(prefab);
                Close();
            }
        }
    }
}

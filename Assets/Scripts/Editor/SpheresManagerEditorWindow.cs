using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class SpheresManagerEditorWindow : EditorWindow
{
    private const float CellSize = 56f;
    private const float CellSpacing = 4f;
    private const string WindowTitle = "Sphere Grid Editor";

    private SpheresManager spheresManager;
    private SphereColors selectedColor;
    private SphereColors[] colorOptions;
    private Vector2 scrollPosition;
    private GUIStyle centeredCellLabel;
    private GUIStyle centeredSmallLabel;

    public static void Open(SpheresManager spheresManager)
    {
        SpheresManagerEditorWindow window = GetWindow<SpheresManagerEditorWindow>(WindowTitle);
        window.SetTarget(spheresManager);
        window.Show();
    }

    private void OnEnable()
    {
        CacheColorOptions();
        CreateStyles();
    }

    private void OnGUI()
    {
        CreateStyles();

        EditorGUILayout.Space(6f);
        DrawTargetField();

        if (spheresManager == null)
        {
            EditorGUILayout.HelpBox("Select a SpheresManager to edit.", MessageType.Info);
            return;
        }

        DrawGridSettings();
        DrawSphereColorsField();
        EditorGUILayout.Space(8f);

        if (!spheresManager.IsGridSizeValid)
        {
            EditorGUILayout.HelpBox("Grid Size must be greater than (0, 0).", MessageType.Warning);
            return;
        }

        if (spheresManager.SpherePrefab == null)
        {
            EditorGUILayout.HelpBox("Assign a GlassSphere2D prefab before placing spheres.", MessageType.Warning);
        }

        spheresManager.EnsureGridDataSize();

        DrawColorPalette();
        EditorGUILayout.Space(8f);
        DrawGridControls();
        EditorGUILayout.Space(8f);
        DrawGrid();
    }

    private void SetTarget(SpheresManager target)
    {
        spheresManager = target;
        titleContent = new GUIContent(WindowTitle);
    }

    private void DrawTargetField()
    {
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ObjectField("Spheres Manager", spheresManager, typeof(SpheresManager), true);
        }
    }

    private void DrawColorPalette()
    {
        CacheColorOptions();

        EditorGUILayout.LabelField("Sphere Colors", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        for (int i = 0; i < colorOptions.Length; i++)
        {
            SphereColors color = colorOptions[i];
            Rect rect = GUILayoutUtility.GetRect(86f, 30f, GUILayout.Width(86f), GUILayout.Height(30f));

            DrawColorButtonBackground(rect, color, selectedColor == color);

            if (GUI.Button(rect, color.ToString(), GUIStyle.none))
            {
                selectedColor = color;
                Repaint();
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawGridSettings()
    {
        EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        Vector2Int gridSize = EditorGUILayout.Vector2IntField("Grid Size", spheresManager.GridSize);
        Vector2 tileOffset = EditorGUILayout.Vector2Field("Tile Offset", spheresManager.TileOffset);

        if (!EditorGUI.EndChangeCheck())
        {
            return;
        }

        bool shouldCenterTransform = gridSize != spheresManager.GridSize || tileOffset != spheresManager.TileOffset;
        bool gridSizeChanged = gridSize != spheresManager.GridSize;
        float topLocalY = spheresManager.GetGridTopLocalY();
        List<GlassSphere2D> spheresOutsideNewGrid = gridSizeChanged ? GetSpheresOutsideGrid(gridSize) : null;

        Undo.RecordObject(spheresManager, "Change Sphere Grid Settings");
        Undo.RecordObject(spheresManager.transform, "Change Sphere Grid Settings");

        GlassSphere2D[] childSpheres = spheresManager.GetComponentsInChildren<GlassSphere2D>(true);
        for (int i = 0; i < childSpheres.Length; i++)
        {
            GlassSphere2D sphere = childSpheres[i];
            if (sphere != null && sphere.transform.IsChildOf(spheresManager.transform))
            {
                Undo.RecordObject(sphere.transform, "Change Sphere Grid Settings");
            }
        }

        spheresManager.SetGridSettings(gridSize, tileOffset, shouldCenterTransform, topLocalY);
        DestroySpheres(spheresOutsideNewGrid);

        if (gridSizeChanged)
        {
            FillEmptyGridCells();
        }

        MarkDirty(spheresManager);
        MarkDirty(spheresManager.transform);
        for (int i = 0; i < childSpheres.Length; i++)
        {
            GlassSphere2D sphere = childSpheres[i];
            if (sphere != null && sphere.transform.IsChildOf(spheresManager.transform))
            {
                MarkDirty(sphere.transform);
            }
        }

        Repaint();
    }

    private List<GlassSphere2D> GetSpheresOutsideGrid(Vector2Int newGridSize)
    {
        List<GlassSphere2D> spheresOutsideGrid = new List<GlassSphere2D>();
        Vector2Int currentGridSize = spheresManager.GridSize;

        for (int x = 0; x < currentGridSize.x; x++)
        {
            for (int y = 0; y < currentGridSize.y; y++)
            {
                if (x < newGridSize.x && y < newGridSize.y)
                {
                    continue;
                }

                GlassSphere2D sphere = spheresManager.GetSphere(new Vector2Int(x, y));
                if (sphere != null && !spheresOutsideGrid.Contains(sphere))
                {
                    spheresOutsideGrid.Add(sphere);
                }
            }
        }

        return spheresOutsideGrid;
    }

    private void DestroySpheres(List<GlassSphere2D> spheresToDestroy)
    {
        if (spheresToDestroy == null)
        {
            return;
        }

        for (int i = 0; i < spheresToDestroy.Count; i++)
        {
            GlassSphere2D sphere = spheresToDestroy[i];
            if (sphere != null)
            {
                Undo.DestroyObjectImmediate(sphere.gameObject);
            }
        }
    }

    private void DrawSphereColorsField()
    {
        EditorGUI.BeginChangeCheck();
        SphereColorsSO sphereColors = (SphereColorsSO)EditorGUILayout.ObjectField(
            "Sphere Colors",
            spheresManager.SphereColors,
            typeof(SphereColorsSO),
            false);

        if (!EditorGUI.EndChangeCheck())
        {
            return;
        }

        Undo.RecordObject(spheresManager, "Change Sphere Colors");
        RecordChildLiquidObjects("Change Sphere Colors");
        spheresManager.SetSphereColors(sphereColors);
        MarkDirty(spheresManager);
        MarkChildLiquidObjectsDirty();
        Repaint();
    }

    private void FillEmptyGridCells()
    {
        if (spheresManager == null || !spheresManager.IsGridSizeValid || spheresManager.SpherePrefab == null)
        {
            return;
        }

        CacheColorOptions();
        SphereColors defaultColor = colorOptions != null && colorOptions.Length > 0 ? colorOptions[0] : SphereColors.Blue;
        Vector2Int gridSize = spheresManager.GridSize;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                if (spheresManager.GetSphere(position) != null)
                {
                    continue;
                }

                GlassSphere2D sphere = CreateSphere(position);
                if (sphere == null)
                {
                    continue;
                }

                sphere.SetSphereColor(defaultColor);
                spheresManager.SetSphere(position, sphere);
                MarkDirty(sphere.gameObject);
                MarkDirty(sphere.transform);
                MarkDirty(sphere);
                MarkDirty(sphere.SpriteLiquid);
            }
        }
    }

    private void DrawGridControls()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear Grid", GUILayout.Height(28f)))
        {
            ClearGrid();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField($"Grid: {spheresManager.GridSize.x} x {spheresManager.GridSize.y}", GUILayout.Width(120f));

        EditorGUILayout.EndHorizontal();
    }

    private void DrawGrid()
    {
        Vector2Int gridSize = spheresManager.GridSize;
        float gridWidth = gridSize.x * CellSize + Mathf.Max(0, gridSize.x - 1) * CellSpacing;
        float gridHeight = gridSize.y * CellSize + Mathf.Max(0, gridSize.y - 1) * CellSpacing;

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        Rect gridRect = GUILayoutUtility.GetRect(gridWidth, gridHeight, GUILayout.Width(gridWidth), GUILayout.Height(gridHeight));

        for (int visualY = 0; visualY < gridSize.y; visualY++)
        {
            int y = gridSize.y - 1 - visualY;

            for (int x = 0; x < gridSize.x; x++)
            {
                Vector2Int position = new Vector2Int(x, y);
                Rect cellRect = new Rect(
                    gridRect.x + x * (CellSize + CellSpacing),
                    gridRect.y + visualY * (CellSize + CellSpacing),
                    CellSize,
                    CellSize);

                DrawCell(cellRect, position);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawCell(Rect rect, Vector2Int position)
    {
        GlassSphere2D sphere = spheresManager.GetSphere(position);
        Color backgroundColor = sphere != null ? GetEditorColor(sphere.SphereColor) : new Color(0.18f, 0.18f, 0.18f, 1f);

        EditorGUI.DrawRect(rect, backgroundColor);
        DrawRectOutline(rect, sphere != null ? Color.white : new Color(0.45f, 0.45f, 0.45f, 1f), 1f);

        string label = sphere != null ? sphere.SphereColor.ToString() : $"{position.x},{position.y}";
        GUI.Label(rect, label, sphere != null ? centeredCellLabel : centeredSmallLabel);

        if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
        {
            PlaceOrUpdateSphere(position);
        }
    }

    private void PlaceOrUpdateSphere(Vector2Int position)
    {
        if (spheresManager == null || !spheresManager.IsPositionValid(position))
        {
            return;
        }

        if (spheresManager.SpherePrefab == null)
        {
            EditorUtility.DisplayDialog(WindowTitle, "Assign a GlassSphere2D prefab before placing spheres.", "OK");
            return;
        }

        GlassSphere2D sphere = spheresManager.GetSphere(position);
        string undoName = sphere == null ? "Place Sphere" : "Update Sphere Cell";

        Undo.RecordObject(spheresManager, undoName);
        spheresManager.EnsureGridDataSize();

        if (sphere == null)
        {
            sphere = CreateSphere(position);
            if (sphere == null)
            {
                return;
            }
        }
        else
        {
            Undo.RecordObject(sphere, undoName);
            Undo.RecordObject(sphere.transform, undoName);
            Undo.RecordObject(sphere.gameObject, undoName);
            RecordLiquidObject(sphere, undoName);
            SetupSphere(sphere, position);
        }

        spheresManager.SetSphere(position, sphere);
        sphere.SetSphereColor(selectedColor);

        MarkDirty(spheresManager);
        MarkDirty(sphere.gameObject);
        MarkDirty(sphere.transform);
        MarkDirty(sphere);
        MarkDirty(sphere.SpriteLiquid);
        Repaint();
    }

    private GlassSphere2D CreateSphere(Vector2Int position)
    {
        GameObject prefabObject = spheresManager.SpherePrefab.gameObject;
        GameObject instanceObject = PrefabUtility.InstantiatePrefab(prefabObject, spheresManager.transform) as GameObject;

        if (instanceObject == null)
        {
            instanceObject = UnityEngine.Object.Instantiate(prefabObject, spheresManager.transform);
        }

        Undo.RegisterCreatedObjectUndo(instanceObject, "Create Sphere");

        GlassSphere2D sphere = instanceObject.GetComponent<GlassSphere2D>();
        if (sphere == null)
        {
            Undo.DestroyObjectImmediate(instanceObject);
            EditorUtility.DisplayDialog(WindowTitle, "The assigned prefab does not contain GlassSphere2D.", "OK");
            return null;
        }

        SetupSphere(sphere, position);
        return sphere;
    }

    private void SetupSphere(GlassSphere2D sphere, Vector2Int position)
    {
        if (sphere == null)
        {
            return;
        }

        if (sphere.transform.parent != spheresManager.transform)
        {
            Undo.SetTransformParent(sphere.transform, spheresManager.transform, "Parent Sphere");
        }

        sphere.name = $"Sphere_{position.x}_{position.y}";
        sphere.transform.localPosition = spheresManager.GetLocalPosition(position);
        sphere.SetColorPalette(spheresManager.SphereColors);
    }

    private void ClearGrid()
    {
        if (spheresManager == null)
        {
            return;
        }

        Undo.RecordObject(spheresManager, "Clear Sphere Grid");

        GlassSphere2D[] childSpheres = spheresManager.GetComponentsInChildren<GlassSphere2D>(true);
        for (int i = childSpheres.Length - 1; i >= 0; i--)
        {
            GlassSphere2D sphere = childSpheres[i];
            if (sphere == null || !sphere.transform.IsChildOf(spheresManager.transform))
            {
                continue;
            }

            Undo.DestroyObjectImmediate(sphere.gameObject);
        }

        spheresManager.ClearGridData();
        MarkDirty(spheresManager);
        Repaint();
    }

    private void CacheColorOptions()
    {
        if (colorOptions != null && colorOptions.Length > 0)
        {
            return;
        }

        colorOptions = (SphereColors[])Enum.GetValues(typeof(SphereColors));
        if (colorOptions.Length > 0)
        {
            selectedColor = colorOptions[0];
        }
    }

    private void CreateStyles()
    {
        if (centeredCellLabel == null)
        {
            centeredCellLabel = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal = { textColor = Color.black }
            };
        }

        if (centeredSmallLabel == null)
        {
            centeredSmallLabel = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
        }
    }

    private void DrawColorButtonBackground(Rect rect, SphereColors color, bool isSelected)
    {
        EditorGUI.DrawRect(rect, GetEditorColor(color));
        DrawRectOutline(rect, isSelected ? Color.white : new Color(0f, 0f, 0f, 0.35f), isSelected ? 3f : 1f);
    }

    private static void DrawRectOutline(Rect rect, Color color, float thickness)
    {
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
    }

    private Color GetEditorColor(SphereColors color)
    {
        if (spheresManager != null && spheresManager.SphereColors != null)
        {
            Color paletteColor = spheresManager.SphereColors.GetLiquidColor(color);
            paletteColor.a = 1f;
            return paletteColor;
        }

        Color defaultColor = SphereColorsSO.GetDefaultLiquidColor(color);
        defaultColor.a = 1f;
        return defaultColor;
    }

    private void RecordChildLiquidObjects(string undoName)
    {
        if (spheresManager == null)
        {
            return;
        }

        GlassSphere2D[] childSpheres = spheresManager.GetComponentsInChildren<GlassSphere2D>(true);
        for (int i = 0; i < childSpheres.Length; i++)
        {
            if (childSpheres[i] != null)
            {
                Undo.RecordObject(childSpheres[i], undoName);
            }

            RecordLiquidObject(childSpheres[i], undoName);
        }
    }

    private static void RecordLiquidObject(GlassSphere2D sphere, string undoName)
    {
        if (sphere == null || sphere.SpriteLiquid == null)
        {
            return;
        }

        Undo.RecordObject(sphere.SpriteLiquid, undoName);
    }

    private void MarkChildLiquidObjectsDirty()
    {
        if (spheresManager == null)
        {
            return;
        }

        GlassSphere2D[] childSpheres = spheresManager.GetComponentsInChildren<GlassSphere2D>(true);
        for (int i = 0; i < childSpheres.Length; i++)
        {
            GlassSphere2D sphere = childSpheres[i];
            if (sphere == null)
            {
                continue;
            }

            MarkDirty(sphere);
            MarkDirty(sphere.SpriteLiquid);
        }
    }

    private static void MarkDirty(UnityEngine.Object target)
    {
        if (target == null)
        {
            return;
        }

        EditorUtility.SetDirty(target);

        if (PrefabUtility.IsPartOfPrefabInstance(target))
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        }

        GameObject gameObject = null;
        if (target is Component component)
        {
            gameObject = component.gameObject;
        }
        else if (target is GameObject targetGameObject)
        {
            gameObject = targetGameObject;
        }

        if (gameObject != null && gameObject.scene.IsValid() && gameObject.scene.isLoaded)
        {
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Provides a custom Unity editor for building sphere level grids.
public sealed class SpheresManagerEditorWindow : EditorWindow
{
    // Lists how a painted obstacle should change a sphere cell.
    private enum ObstaclePaintMode
    {
        Keep,
        Set,
        Clear
    }

    private const float CellSize = 56f;
    private const float CellSpacing = 4f;
    private const string WindowTitle = "Sphere Grid Editor";

    private SpheresManager spheresManager;
    private SphereColors selectedColor;
    private ObstacleBaseAbstract selectedObstaclePrefab;
    private ObstaclePaintMode obstaclePaintMode = ObstaclePaintMode.Keep;
    private SphereColors[] colorOptions;
    private Vector2 scrollPosition;
    private GUIStyle centeredCellLabel;
    private GUIStyle centeredSmallLabel;

    // Opens the grid editor for a selected spheres manager.
    public static void Open(SpheresManager spheresManager)
    {
        SpheresManagerEditorWindow window = GetWindow<SpheresManagerEditorWindow>(WindowTitle);
        window.SetTarget(spheresManager);
        window.Show();
    }

    // Prepares palette choices when the window opens.
    private void OnEnable()
    {
        CacheColorOptions();
    }

    // Draws all editor controls and the sphere grid.
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
        EditorGUILayout.Space(6f);
        DrawObstaclePalette();
        EditorGUILayout.Space(8f);
        DrawGridControls();
        EditorGUILayout.Space(8f);
        DrawGrid();
    }

    // Sets the manager edited by this window.
    private void SetTarget(SpheresManager target)
    {
        spheresManager = target;
        titleContent = new GUIContent(WindowTitle);
    }

    // Displays the current manager as a read-only field.
    private void DrawTargetField()
    {
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ObjectField("Spheres Manager", spheresManager, typeof(SpheresManager), true);
        }
    }

    // Draws buttons for selecting a sphere color to paint.
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

    // Draws buttons for keeping, clearing, or painting obstacles.
    private void DrawObstaclePalette()
    {
        EditorGUILayout.LabelField("Obstacles", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        DrawObstacleModeButton("Keep", obstaclePaintMode == ObstaclePaintMode.Keep, () =>
        {
            obstaclePaintMode = ObstaclePaintMode.Keep;
            selectedObstaclePrefab = null;
        });

        DrawObstacleModeButton("Clear", obstaclePaintMode == ObstaclePaintMode.Clear, () =>
        {
            obstaclePaintMode = ObstaclePaintMode.Clear;
            selectedObstaclePrefab = null;
        });

        IReadOnlyList<ObstacleBaseAbstract> obstaclePrefabs = spheresManager.Obstacles;
        if (obstaclePrefabs != null)
        {
            for (int i = 0; i < obstaclePrefabs.Count; i++)
            {
                ObstacleBaseAbstract obstaclePrefab = obstaclePrefabs[i];
                if (obstaclePrefab == null)
                {
                    continue;
                }

                bool isSelected = obstaclePaintMode == ObstaclePaintMode.Set
                    && selectedObstaclePrefab == obstaclePrefab;

                DrawObstacleModeButton(GetObstacleLabel(obstaclePrefab), isSelected, () =>
                {
                    obstaclePaintMode = ObstaclePaintMode.Set;
                    selectedObstaclePrefab = obstaclePrefab;
                });
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    // Draws one obstacle painting mode button.
    private static void DrawObstacleModeButton(string label, bool isSelected, Action onClick)
    {
        Color previousColor = GUI.backgroundColor;
        GUI.backgroundColor = isSelected ? new Color(0.55f, 0.75f, 1f, 1f) : previousColor;

        if (GUILayout.Button(label, GUILayout.Height(24f)))
        {
            onClick?.Invoke();
        }

        GUI.backgroundColor = previousColor;
    }

    // Draws and applies size and spacing settings for the grid.
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

        bool gridSizeChanged = gridSize != spheresManager.GridSize;
        SpheresGridEditorOperations.SetGridSettings(spheresManager, gridSize, tileOffset);

        if (gridSizeChanged)
        {
            FillEmptyGridCells();
        }

        Repaint();
    }

    // Draws and applies the shared sphere color palette field.
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

        SpheresGridEditorOperations.SetSphereColors(spheresManager, sphereColors);
        Repaint();
    }

    // Creates default spheres for every empty grid cell.
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

                GlassSphere2D sphere = SpheresGridEditorOperations.CreateSphere(spheresManager, position);
                if (sphere == null)
                {
                    continue;
                }

                sphere.SetSphereColor(defaultColor);
                spheresManager.SetSphere(position, sphere);
                SpheresGridEditorOperations.MarkSphereDirty(sphere);
            }
        }
    }

    // Draws grid actions and current grid dimensions.
    private void DrawGridControls()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear Grid", GUILayout.Height(28f)))
        {
            SpheresGridEditorOperations.ClearGrid(spheresManager);
            Repaint();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField($"Grid: {spheresManager.GridSize.x} x {spheresManager.GridSize.y}", GUILayout.Width(120f));

        EditorGUILayout.EndHorizontal();
    }

    // Draws every editable cell in the sphere grid.
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

    // Draws one grid cell and handles its click action.
    private void DrawCell(Rect rect, Vector2Int position)
    {
        GlassSphere2D sphere = spheresManager.GetSphere(position);
        Color backgroundColor = sphere != null ? GetEditorColor(sphere.SphereColor) : new Color(0.18f, 0.18f, 0.18f, 1f);
        ObstacleBaseAbstract obstacle = sphere != null ? SpheresGridEditorOperations.GetSphereObstacle(sphere) : null;

        EditorGUI.DrawRect(rect, backgroundColor);
        DrawRectOutline(rect, sphere != null ? Color.white : new Color(0.45f, 0.45f, 0.45f, 1f), 1f);

        string label = sphere != null ? GetCellLabel(sphere, obstacle) : $"{position.x},{position.y}";
        GUI.Label(rect, label, sphere != null ? centeredCellLabel : centeredSmallLabel);

        if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
        {
            PlaceOrUpdateSphere(position);
        }
    }

    // Creates or updates the sphere in a clicked cell.
    private void PlaceOrUpdateSphere(Vector2Int position)
    {
        if (spheresManager == null || !spheresManager.IsPositionValid(position))
        {
            return;
        }

        bool isNewSphere = spheresManager.GetSphere(position) == null;
        string undoName = isNewSphere ? "Place Sphere" : "Update Sphere Cell";
        GlassSphere2D sphere = SpheresGridEditorOperations.PlaceOrUpdateSphere(
            spheresManager,
            position,
            selectedColor,
            obstaclePaintMode == ObstaclePaintMode.Keep,
            undoName);

        if (sphere == null)
        {
            return;
        }

        ApplySelectedObstaclePaint(sphere, undoName);

        Repaint();
    }

    // Applies the selected obstacle paint mode to one sphere.
    private void ApplySelectedObstaclePaint(GlassSphere2D sphere, string undoName)
    {
        if (sphere == null)
        {
            return;
        }

        switch (obstaclePaintMode)
        {
            case ObstaclePaintMode.Keep:
                return;

            case ObstaclePaintMode.Clear:
                SpheresGridEditorOperations.ClearSphereObstacles(sphere);
                return;

            case ObstaclePaintMode.Set:
                if (selectedObstaclePrefab != null)
                {
                    SpheresGridEditorOperations.SetSphereObstacle(sphere, selectedObstaclePrefab, undoName);
                }

                return;
        }
    }

    // Builds the text shown inside one filled grid cell.
    private static string GetCellLabel(GlassSphere2D sphere, ObstacleBaseAbstract obstacle)
    {
        if (obstacle == null)
        {
            return sphere.SphereColor.ToString();
        }

        return $"{sphere.SphereColor}\n{GetObstacleLabel(obstacle)}";
    }

    // Builds a readable editor name for an obstacle prefab.
    private static string GetObstacleLabel(ObstacleBaseAbstract obstacle)
    {
        if (obstacle == null)
        {
            return string.Empty;
        }

        string obstacleName = obstacle.gameObject.name.Replace("(Clone)", string.Empty).Trim();
        return ObjectNames.NicifyVariableName(obstacleName);
    }

    // Loads available sphere color options for painting.
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

    // Creates label styles used by the grid cells.
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

    // Draws the fill and selection outline of a color button.
    private void DrawColorButtonBackground(Rect rect, SphereColors color, bool isSelected)
    {
        EditorGUI.DrawRect(rect, GetEditorColor(color));
        DrawRectOutline(rect, isSelected ? Color.white : new Color(0f, 0f, 0f, 0.35f), isSelected ? 3f : 1f);
    }

    // Draws a simple border around an editor rectangle.
    private static void DrawRectOutline(Rect rect, Color color, float thickness)
    {
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
    }

    // Gets a preview color from the active palette or defaults.
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

}

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Applies sphere grid edits and keeps Unity undo and dirty state correct.
internal static class SpheresGridEditorOperations
{
    // Changes grid layout and removes spheres outside the new bounds.
    public static void SetGridSettings(SpheresManager manager, Vector2Int gridSize, Vector2 tileOffset)
    {
        bool alignTransform = gridSize != manager.GridSize || tileOffset != manager.TileOffset;
        float topLocalY = manager.GetGridTopLocalY();
        List<GlassSphere2D> spheresOutsideGrid = GetSpheresOutsideGrid(manager, gridSize);

        Undo.RecordObject(manager, "Change Sphere Grid Settings");
        Undo.RecordObject(manager.transform, "Change Sphere Grid Settings");

        GlassSphere2D[] childSpheres = manager.GetComponentsInChildren<GlassSphere2D>(true);
        for (int i = 0; i < childSpheres.Length; i++)
        {
            GlassSphere2D sphere = childSpheres[i];
            if (sphere != null && sphere.transform.IsChildOf(manager.transform))
            {
                Undo.RecordObject(sphere.transform, "Change Sphere Grid Settings");
            }
        }

        manager.SetGridSettings(gridSize, tileOffset, alignTransform, topLocalY);
        DestroySpheres(spheresOutsideGrid);
        MarkDirty(manager);
        MarkDirty(manager.transform);

        for (int i = 0; i < childSpheres.Length; i++)
        {
            GlassSphere2D sphere = childSpheres[i];
            if (sphere != null && sphere.transform.IsChildOf(manager.transform))
            {
                MarkDirty(sphere.transform);
            }
        }
    }

    // Changes the shared palette and records affected sphere visuals.
    public static void SetSphereColors(SpheresManager manager, SphereColorsSO sphereColors)
    {
        Undo.RecordObject(manager, "Change Sphere Colors");
        RecordChildLiquidObjects(manager, "Change Sphere Colors");
        manager.SetSphereColors(sphereColors);
        MarkDirty(manager);
        MarkChildLiquidObjectsDirty(manager);
    }

    // Creates or updates one painted sphere cell.
    public static GlassSphere2D PlaceOrUpdateSphere(
        SpheresManager manager,
        Vector2Int position,
        SphereColors selectedColor,
        bool paintExistingColor,
        string undoName)
    {
        if (manager == null || !manager.IsPositionValid(position))
        {
            return null;
        }

        if (manager.SpherePrefab == null)
        {
            EditorUtility.DisplayDialog("Sphere Grid Editor", "Assign a GlassSphere2D prefab before placing spheres.", "OK");
            return null;
        }

        GlassSphere2D sphere = manager.GetSphere(position);
        bool isNewSphere = sphere == null;

        Undo.RecordObject(manager, undoName);
        manager.EnsureGridDataSize();

        if (isNewSphere)
        {
            sphere = CreateSphere(manager, position);
            if (sphere == null)
            {
                return null;
            }
        }
        else
        {
            Undo.RecordObject(sphere, undoName);
            Undo.RecordObject(sphere.transform, undoName);
            Undo.RecordObject(sphere.gameObject, undoName);
            RecordLiquidObject(sphere, undoName);
            SetupSphere(manager, sphere, position);
        }

        manager.SetSphere(position, sphere);
        if (isNewSphere || paintExistingColor)
        {
            sphere.SetSphereColor(selectedColor);
        }

        MarkDirty(manager);
        MarkSphereDirty(sphere);
        return sphere;
    }

    // Instantiates and sets up a sphere for one grid cell.
    public static GlassSphere2D CreateSphere(SpheresManager manager, Vector2Int position)
    {
        GameObject prefabObject = manager.SpherePrefab.gameObject;
        GameObject instanceObject = PrefabUtility.InstantiatePrefab(prefabObject, manager.transform) as GameObject;

        if (instanceObject == null)
        {
            instanceObject = Object.Instantiate(prefabObject, manager.transform);
        }

        Undo.RegisterCreatedObjectUndo(instanceObject, "Create Sphere");

        GlassSphere2D sphere = instanceObject.GetComponent<GlassSphere2D>();
        if (sphere == null)
        {
            Undo.DestroyObjectImmediate(instanceObject);
            EditorUtility.DisplayDialog("Sphere Grid Editor", "The assigned prefab does not contain GlassSphere2D.", "OK");
            return null;
        }

        SetupSphere(manager, sphere, position);
        return sphere;
    }

    // Parents and positions a sphere inside its grid cell.
    public static void SetupSphere(SpheresManager manager, GlassSphere2D sphere, Vector2Int position)
    {
        if (sphere == null)
        {
            return;
        }

        if (sphere.transform.parent != manager.transform)
        {
            Undo.SetTransformParent(sphere.transform, manager.transform, "Parent Sphere");
        }

        sphere.name = $"Sphere_{position.x}_{position.y}";
        sphere.transform.localPosition = manager.GetLocalPosition(position);
        sphere.SetColorPalette(manager.SphereColors);
    }

    // Replaces the current obstacle on a sphere with one prefab.
    public static void SetSphereObstacle(GlassSphere2D sphere, ObstacleBaseAbstract obstaclePrefab, string undoName)
    {
        ClearSphereObstacles(sphere);

        GameObject prefabObject = obstaclePrefab.gameObject;
        GameObject instanceObject = PrefabUtility.InstantiatePrefab(prefabObject, sphere.transform) as GameObject;
        if (instanceObject == null)
        {
            instanceObject = Object.Instantiate(prefabObject, sphere.transform);
        }

        Undo.RegisterCreatedObjectUndo(instanceObject, undoName);
        instanceObject.name = prefabObject.name;

        Transform instanceTransform = instanceObject.transform;
        Undo.RecordObject(instanceTransform, undoName);
        instanceTransform.localPosition = Vector3.zero;
        instanceTransform.localRotation = Quaternion.identity;

        MarkDirty(instanceObject);
        MarkDirty(instanceTransform);
        MarkDirty(instanceObject.GetComponent<ObstacleBaseAbstract>());
    }

    // Removes all obstacle objects attached to a sphere.
    public static void ClearSphereObstacles(GlassSphere2D sphere)
    {
        if (sphere == null)
        {
            return;
        }

        ObstacleBaseAbstract[] obstacles = sphere.GetComponentsInChildren<ObstacleBaseAbstract>(true);
        HashSet<GameObject> rootObjects = new HashSet<GameObject>();

        for (int i = 0; i < obstacles.Length; i++)
        {
            ObstacleBaseAbstract obstacle = obstacles[i];
            if (obstacle == null || obstacle.transform == sphere.transform)
            {
                continue;
            }

            GameObject rootObject = GetObstacleRootObject(sphere.transform, obstacle.transform);
            if (rootObject != null)
            {
                rootObjects.Add(rootObject);
            }
        }

        foreach (GameObject rootObject in rootObjects)
        {
            if (rootObject != null)
            {
                Undo.DestroyObjectImmediate(rootObject);
            }
        }
    }

    // Gets the first obstacle currently attached to a sphere.
    public static ObstacleBaseAbstract GetSphereObstacle(GlassSphere2D sphere)
    {
        if (sphere == null)
        {
            return null;
        }

        ObstacleBaseAbstract[] obstacles = sphere.GetComponentsInChildren<ObstacleBaseAbstract>(true);
        for (int i = 0; i < obstacles.Length; i++)
        {
            ObstacleBaseAbstract obstacle = obstacles[i];
            if (obstacle != null && obstacle.transform != sphere.transform)
            {
                return obstacle;
            }
        }

        return null;
    }

    // Deletes all sphere objects and clears grid references.
    public static void ClearGrid(SpheresManager manager)
    {
        Undo.RecordObject(manager, "Clear Sphere Grid");

        GlassSphere2D[] childSpheres = manager.GetComponentsInChildren<GlassSphere2D>(true);
        for (int i = childSpheres.Length - 1; i >= 0; i--)
        {
            GlassSphere2D sphere = childSpheres[i];
            if (sphere != null && sphere.transform.IsChildOf(manager.transform))
            {
                Undo.DestroyObjectImmediate(sphere.gameObject);
            }
        }

        manager.ClearGridData();
        MarkDirty(manager);
    }

    // Records one liquid component for an undo action.
    public static void RecordLiquidObject(GlassSphere2D sphere, string undoName)
    {
        if (sphere != null && sphere.SpriteLiquid != null)
        {
            Undo.RecordObject(sphere.SpriteLiquid, undoName);
        }
    }

    // Marks one sphere and its visible objects as edited.
    public static void MarkSphereDirty(GlassSphere2D sphere)
    {
        if (sphere == null)
        {
            return;
        }

        MarkDirty(sphere.gameObject);
        MarkDirty(sphere.transform);
        MarkDirty(sphere);
        MarkDirty(sphere.SpriteLiquid);
    }

    // Marks an edited object and its prefab or scene as dirty.
    public static void MarkDirty(Object target)
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

        GameObject gameObject = target is Component component
            ? component.gameObject
            : target as GameObject;

        if (gameObject != null && gameObject.scene.IsValid() && gameObject.scene.isLoaded)
        {
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
    }

    // Finds spheres that fall outside new grid bounds.
    private static List<GlassSphere2D> GetSpheresOutsideGrid(SpheresManager manager, Vector2Int newGridSize)
    {
        List<GlassSphere2D> spheresOutsideGrid = new List<GlassSphere2D>();
        Vector2Int currentGridSize = manager.GridSize;

        for (int x = 0; x < currentGridSize.x; x++)
        {
            for (int y = 0; y < currentGridSize.y; y++)
            {
                if (x < newGridSize.x && y < newGridSize.y)
                {
                    continue;
                }

                GlassSphere2D sphere = manager.GetSphere(new Vector2Int(x, y));
                if (sphere != null && !spheresOutsideGrid.Contains(sphere))
                {
                    spheresOutsideGrid.Add(sphere);
                }
            }
        }

        return spheresOutsideGrid;
    }

    // Deletes sphere objects removed by grid resizing.
    private static void DestroySpheres(List<GlassSphere2D> spheresToDestroy)
    {
        for (int i = 0; i < spheresToDestroy.Count; i++)
        {
            GlassSphere2D sphere = spheresToDestroy[i];
            if (sphere != null)
            {
                Undo.DestroyObjectImmediate(sphere.gameObject);
            }
        }
    }

    // Finds the root child object that owns an obstacle.
    private static GameObject GetObstacleRootObject(Transform sphereTransform, Transform obstacleTransform)
    {
        Transform root = obstacleTransform;
        while (root.parent != null && root.parent != sphereTransform)
        {
            root = root.parent;
        }

        return root.parent == sphereTransform ? root.gameObject : obstacleTransform.gameObject;
    }

    // Records all sphere visual objects for an undo action.
    private static void RecordChildLiquidObjects(SpheresManager manager, string undoName)
    {
        GlassSphere2D[] childSpheres = manager.GetComponentsInChildren<GlassSphere2D>(true);
        for (int i = 0; i < childSpheres.Length; i++)
        {
            GlassSphere2D sphere = childSpheres[i];
            if (sphere != null)
            {
                Undo.RecordObject(sphere, undoName);
            }

            RecordLiquidObject(sphere, undoName);
        }
    }

    // Marks all edited sphere visual objects as dirty.
    private static void MarkChildLiquidObjectsDirty(SpheresManager manager)
    {
        GlassSphere2D[] childSpheres = manager.GetComponentsInChildren<GlassSphere2D>(true);
        for (int i = 0; i < childSpheres.Length; i++)
        {
            GlassSphere2D sphere = childSpheres[i];
            if (sphere != null)
            {
                MarkDirty(sphere);
                MarkDirty(sphere.SpriteLiquid);
            }
        }
    }
}

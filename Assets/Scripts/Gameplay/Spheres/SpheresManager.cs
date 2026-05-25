using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

// Stores level sphere grid data and applies its shared layout values.
public sealed class SpheresManager : SerializedMonoBehaviour
{
    [SerializeField] private SphereObstacleCatalogSO obstacleCatalog;
    [SerializeField] private GlassSphere2D spherePrefab;
    [SerializeField] private SphereColorsSO sphereColors;
    [SerializeField, HideInInspector] private Vector2Int gridSize;
    [FormerlySerializedAs("spacing")]
    [SerializeField, HideInInspector] private Vector2 tileOffset = Vector2.one;
    [SerializeField, HideInInspector] private Vector2Int storedGridSize;
    [OdinSerialize, HideInInspector] private GlassSphere2D[,] spheres = new GlassSphere2D[0, 0];

    public GlassSphere2D SpherePrefab => spherePrefab;
    public SphereColorsSO SphereColors => sphereColors;
    public IReadOnlyList<ObstacleBaseAbstract> Obstacles => obstacleCatalog != null ? obstacleCatalog.Obstacles : null;
    public Vector2Int GridSize => gridSize;
    public Vector2 TileOffset => tileOffset;
    public bool IsGridSizeValid => gridSize.x > 0 && gridSize.y > 0;

    // Applies shared palette data when gameplay starts.
    private void Start()
    {
        ApplySphereColorPalettes();
    }

    // Keeps grid data, positions, and colors updated in the inspector.
    private void OnValidate()
    {
        NormalizeGridSettings();
        EnsureGridDataSize();
        ApplySpherePositions();
        ApplySphereColorPalettes();
    }

    // Updates grid dimensions, spacing, and optional alignment.
    public void SetGridSettings(Vector2Int newGridSize, Vector2 newTileOffset, bool alignTransform, float topLocalY)
    {
        gridSize = newGridSize;
        tileOffset = newTileOffset;

        NormalizeGridSettings();
        EnsureGridDataSize();
        ApplySpherePositions();

        if (alignTransform)
        {
            AlignTransformToGrid(topLocalY);
        }
    }

    // Gets the current local height of the grid top edge.
    public float GetGridTopLocalY()
    {
        if (!IsGridSizeValid)
        {
            return transform.localPosition.y;
        }

        return transform.localPosition.y + GetGridEdgeOffset(gridSize.y, Mathf.Abs(tileOffset.y));
    }

    // Positions this manager so the grid keeps its requested top height.
    private void AlignTransformToGrid(float topLocalY)
    {
        if (!IsGridSizeValid)
        {
            return;
        }

        Vector3 localPosition = transform.localPosition;
        localPosition.x = -GetGridCenterOffset(gridSize.x, tileOffset.x);
        localPosition.y = topLocalY - GetGridEdgeOffset(gridSize.y, Mathf.Abs(tileOffset.y));
        transform.localPosition = localPosition;
    }

    // Gets the sphere stored in one grid cell.
    public GlassSphere2D GetSphere(Vector2Int position)
    {
        if (!TryGetArrayPosition(position, out int x, out int y))
        {
            return null;
        }

        return spheres[x, y];
    }

    // Counts non-empty sphere cells in the grid.
    public int GetSphereCount()
    {
        EnsureGridDataSize();

        if (!IsGridSizeValid || spheres == null)
        {
            return 0;
        }

        int count = 0;
        int width = Mathf.Min(gridSize.x, spheres.GetLength(0));
        int height = Mathf.Min(gridSize.y, spheres.GetLength(1));

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (spheres[x, y] != null)
                {
                    count++;
                }
            }
        }

        return count;
    }

    // Stores a sphere in a grid cell and sets its palette.
    public void SetSphere(Vector2Int position, GlassSphere2D sphere)
    {
        if (!TryGetArrayPosition(position, out int x, out int y))
        {
            return;
        }

        spheres[x, y] = sphere;

        if (sphere != null && sphere.transform.parent != transform)
        {
            sphere.transform.SetParent(transform, true);
        }

        if (sphere != null)
        {
            sphere.SetColorPalette(sphereColors);
        }
    }

    // Changes the palette used by every stored sphere.
    public void SetSphereColors(SphereColorsSO newSphereColors)
    {
        sphereColors = newSphereColors;
        ApplySphereColorPalettes();
    }

    // Removes all sphere references from the grid data.
    public void ClearGridData()
    {
        EnsureGridDataSize();

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                spheres[x, y] = null;
            }
        }
    }

    // Calculates a sphere position from its grid cell.
    public Vector3 GetLocalPosition(Vector2Int position)
    {
        return new Vector3(position.x * tileOffset.x, position.y * Mathf.Abs(tileOffset.y), 0f);
    }

    // Checks whether a cell exists in the current grid.
    public bool IsPositionValid(Vector2Int position)
    {
        return IsGridSizeValid
            && position.x >= 0
            && position.y >= 0
            && position.x < gridSize.x
            && position.y < gridSize.y;
    }

    // Creates or resizes stored grid data when needed.
    public void EnsureGridDataSize()
    {
        if (!IsGridSizeValid)
        {
            spheres = new GlassSphere2D[0, 0];
            storedGridSize = gridSize;
            return;
        }

        if (storedGridSize == gridSize
            && spheres != null
            && spheres.GetLength(0) == gridSize.x
            && spheres.GetLength(1) == gridSize.y)
        {
            return;
        }

        ResizeSpheresPreservingPositions();
    }

    // Keeps grid dimensions and vertical spacing valid.
    private void NormalizeGridSettings()
    {
        if (gridSize.x < 0 || gridSize.y < 0)
        {
            gridSize = new Vector2Int(Mathf.Max(0, gridSize.x), Mathf.Max(0, gridSize.y));
        }

        if (tileOffset.y < 0f)
        {
            tileOffset.y = -tileOffset.y;
        }
    }

    // Calculates the offset from one grid edge to its center.
    private static float GetGridCenterOffset(int cellCount, float tileOffset)
    {
        return GetGridEdgeOffset(cellCount, tileOffset) * 0.5f;
    }

    // Calculates the distance between the first and last grid cells.
    private static float GetGridEdgeOffset(int cellCount, float tileOffset)
    {
        return Mathf.Max(0, cellCount - 1) * tileOffset;
    }

    // Converts a valid cell position to array coordinates.
    private bool TryGetArrayPosition(Vector2Int position, out int x, out int y)
    {
        EnsureGridDataSize();

        if (!IsPositionValid(position))
        {
            x = -1;
            y = -1;
            return false;
        }

        x = position.x;
        y = position.y;
        return true;
    }

    // Finds the grid cell that contains a sphere instance.
    public bool TryFindSpherePosition(GlassSphere2D targetSphere, out Vector2Int position)
    {
        position = default;

        if (!IsGridSizeValid || spheres == null || targetSphere == null)
        {
            return false;
        }

        int width = Mathf.Min(gridSize.x, spheres.GetLength(0));
        int height = Mathf.Min(gridSize.y, spheres.GetLength(1));

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (spheres[x, y] != targetSphere)
                {
                    continue;
                }

                position = new Vector2Int(x, y);
                return true;
            }
        }

        return false;
    }

    // Resizes grid storage while keeping spheres in matching cells.
    private void ResizeSpheresPreservingPositions()
    {
        GlassSphere2D[,] previousSpheres = spheres;
        Vector2Int previousGridSize = storedGridSize;

        spheres = new GlassSphere2D[gridSize.x, gridSize.y];

        if (previousSpheres != null && previousGridSize.x > 0 && previousGridSize.y > 0)
        {
            int copyWidth = Mathf.Min(previousGridSize.x, gridSize.x);
            int copyHeight = Mathf.Min(previousGridSize.y, gridSize.y);

            for (int x = 0; x < copyWidth; x++)
            {
                for (int y = 0; y < copyHeight; y++)
                {
                    if (x < previousSpheres.GetLength(0) && y < previousSpheres.GetLength(1))
                    {
                        spheres[x, y] = previousSpheres[x, y];
                    }
                }
            }
        }

        storedGridSize = gridSize;
    }

    // Moves stored spheres to their current grid positions.
    private void ApplySpherePositions()
    {
        if (!IsGridSizeValid || spheres == null)
        {
            return;
        }

        int width = Mathf.Min(gridSize.x, spheres.GetLength(0));
        int height = Mathf.Min(gridSize.y, spheres.GetLength(1));

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GlassSphere2D sphere = spheres[x, y];
                if (sphere == null)
                {
                    continue;
                }

                Transform sphereTransform = sphere.transform;
                if (sphereTransform.parent == transform)
                {
                    sphereTransform.localPosition = GetLocalPosition(new Vector2Int(x, y));
                }
            }
        }
    }

    // Sends the selected palette to every stored sphere.
    private void ApplySphereColorPalettes()
    {
        if (!IsGridSizeValid || spheres == null)
        {
            return;
        }

        int width = Mathf.Min(gridSize.x, spheres.GetLength(0));
        int height = Mathf.Min(gridSize.y, spheres.GetLength(1));

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GlassSphere2D sphere = spheres[x, y];
                if (sphere == null)
                {
                    continue;
                }

                sphere.SetColorPalette(sphereColors);
            }
        }
    }

}

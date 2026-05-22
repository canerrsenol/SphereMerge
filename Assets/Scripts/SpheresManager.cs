using System;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

public sealed class SpheresManager : SerializedMonoBehaviour, ISpheresManagerService
{
    [SerializeField] private GlassSphere2D spherePrefab;
    [SerializeField] private Vector2Int gridSize;
    [FormerlySerializedAs("spacing")]
    [SerializeField] private Vector2 tileOffset = Vector2.one;
    [SerializeField, UnityEngine.HideInInspector] private Vector2Int storedGridSize;
    [OdinSerialize, UnityEngine.HideInInspector] private GlassSphere2D[,] spheres = new GlassSphere2D[0, 0];

    public GlassSphere2D SpherePrefab => spherePrefab;
    public Vector2Int GridSize => gridSize;
    public Vector2 TileOffset => tileOffset;
    public bool IsGridSizeValid => gridSize.x > 0 && gridSize.y > 0;
    public int CellCount => IsGridSizeValid ? gridSize.x * gridSize.y : 0;

    private void OnValidate()
    {
        if (gridSize.x < 0 || gridSize.y < 0)
        {
            gridSize = new Vector2Int(Mathf.Max(0, gridSize.x), Mathf.Max(0, gridSize.y));
        }

        if (tileOffset.y < 0f)
        {
            tileOffset.y = -tileOffset.y;
        }

        EnsureGridDataSize();
        ApplySpherePositions();
    }

    public GlassSphere2D GetSphere(Vector2Int position)
    {
        if (!TryGetArrayPosition(position, out int x, out int y))
        {
            return null;
        }

        return spheres[x, y];
    }

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
    }

    public void ClearCellData(Vector2Int position)
    {
        if (!TryGetArrayPosition(position, out int x, out int y))
        {
            return;
        }

        spheres[x, y] = null;
    }

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

    public Vector3 GetLocalPosition(Vector2Int position)
    {
        return new Vector3(position.x * tileOffset.x, position.y * Mathf.Abs(tileOffset.y), 0f);
    }

    public bool IsPositionValid(Vector2Int position)
    {
        return IsGridSizeValid
            && position.x >= 0
            && position.y >= 0
            && position.x < gridSize.x
            && position.y < gridSize.y;
    }

    public bool TryGetIndex(Vector2Int position, out int index)
    {
        if (!IsPositionValid(position))
        {
            index = -1;
            return false;
        }

        index = position.y * gridSize.x + position.x;
        return true;
    }

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

    [Button("Edit Level")]
    [ShowIf(nameof(IsGridSizeValid))]
    private void OpenLevelEditor()
    {
#if UNITY_EDITOR
        Type editorWindowType = Type.GetType("SpheresManagerEditorWindow, Assembly-CSharp-Editor");
        MethodInfo openMethod = editorWindowType?.GetMethod("Open", BindingFlags.Public | BindingFlags.Static);
        openMethod?.Invoke(null, new object[] { this });
#endif
    }
}

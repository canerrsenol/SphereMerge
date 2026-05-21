using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(UnityEngine.Grid))]
public class GridManager : MonoBehaviour
{
    [TitleGroup("Grid Settings")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField, MinValue(1)] private Vector2Int gridSize = new Vector2Int(5, 5);
    [SerializeField, MinValue(0.1f)] private float tileSpacing = 1f;
    [SerializeField] private bool centerGrid = true;
    [SerializeField] private bool autoRefresh = true;
    [SerializeField] private bool showCoordinates = true;
    [SerializeField] private Transform tileParent;

    [TitleGroup("Object Placement")]
    [SerializeField, FolderPath] private string tileObjectsFolderPath = "Assets/Prefabs/TileObjects";
    [SerializeField, ToggleLeft, LabelText("Tile Object Picking Mode")]
    private bool tileObjectPickingMode;

    [TitleGroup("Runtime Cache")]
    [SerializeField, ReadOnly] private List<GridTile> tiles = new List<GridTile>();

    [NonSerialized] private Dictionary<GridCoordinates, GridTile> tileLookup;

    private static readonly GridCoordinates[] FourDirections =
    {
        new GridCoordinates(0, 1),
        new GridCoordinates(1, 0),
        new GridCoordinates(0, -1),
        new GridCoordinates(-1, 0)
    };

    private static readonly GridCoordinates[] EightDirections =
    {
        new GridCoordinates(0, 1),
        new GridCoordinates(1, 1),
        new GridCoordinates(1, 0),
        new GridCoordinates(1, -1),
        new GridCoordinates(0, -1),
        new GridCoordinates(-1, -1),
        new GridCoordinates(-1, 0),
        new GridCoordinates(-1, 1)
    };

    public IReadOnlyList<GridTile> Tiles => tiles;
    public Vector2Int GridSize => gridSize;
    public float TileSpacing => tileSpacing;

#if UNITY_EDITOR
    public bool TileObjectPickingMode => tileObjectPickingMode;
    public string TileObjectsFolderPath => tileObjectsFolderPath;
#endif

    private void Awake()
    {
        ClampSettings();
        RebuildCache();
    }

    private void OnEnable()
    {
        ClampSettings();
        RebuildCache();
    }

    public bool TryGetTile(GridCoordinates coordinates, out GridTile tile)
    {
        EnsureLookup();
        return tileLookup.TryGetValue(coordinates, out tile);
    }

    public bool TryGetTile(Vector2Int coordinates, out GridTile tile)
    {
        return TryGetTile(GridCoordinates.FromVector2Int(coordinates), out tile);
    }

    public GridTile GetTileOrNull(GridCoordinates coordinates)
    {
        return TryGetTile(coordinates, out GridTile tile) ? tile : null;
    }

    public bool ContainsCoordinates(GridCoordinates coordinates)
    {
        EnsureLookup();
        return tileLookup.ContainsKey(coordinates);
    }

    public IEnumerable<GridTile> GetNeighbors(GridTile tile, bool includeDiagonals = false)
    {
        if (tile == null)
        {
            yield break;
        }

        foreach (GridTile neighbor in GetNeighbors(tile.Coordinates, includeDiagonals))
        {
            yield return neighbor;
        }
    }

    public IEnumerable<GridTile> GetNeighbors(GridCoordinates coordinates, bool includeDiagonals = false)
    {
        GridCoordinates[] directions = includeDiagonals ? EightDirections : FourDirections;

        for (int i = 0; i < directions.Length; i++)
        {
            GridCoordinates neighborCoordinates = new GridCoordinates(
                coordinates.X + directions[i].X,
                coordinates.Y + directions[i].Y);

            if (TryGetTile(neighborCoordinates, out GridTile tile))
            {
                yield return tile;
            }
        }
    }

    public Vector3 GetLocalPosition(GridCoordinates coordinates)
    {
        float x = coordinates.X * tileSpacing;
        float z = coordinates.Y * tileSpacing;

        if (!centerGrid)
        {
            return new Vector3(x, 0f, z);
        }

        float widthOffset = (gridSize.x - 1) * tileSpacing * 0.5f;
        float heightOffset = (gridSize.y - 1) * tileSpacing * 0.5f;
        return new Vector3(x - widthOffset, 0f, z - heightOffset);
    }

    public Vector3 GetWorldPosition(GridCoordinates coordinates)
    {
        return transform.TransformPoint(GetLocalPosition(coordinates));
    }

    public GridCoordinates WorldToCoordinates(Vector3 worldPosition)
    {
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

        float widthOffset = centerGrid ? (gridSize.x - 1) * tileSpacing * 0.5f : 0f;
        float heightOffset = centerGrid ? (gridSize.y - 1) * tileSpacing * 0.5f : 0f;

        int x = Mathf.RoundToInt((localPosition.x + widthOffset) / tileSpacing);
        int y = Mathf.RoundToInt((localPosition.z + heightOffset) / tileSpacing);
        return new GridCoordinates(x, y);
    }

    [Button(ButtonSizes.Large), GUIColor(0.3f, 0.8f, 0.45f)]
    public void CreateOrRefreshGrid()
    {
        ClampSettings();
        EnsureTileParent();
        UpdateUnityGrid();

        if (tilePrefab == null)
        {
            Debug.LogError($"{nameof(GridManager)} requires a tile prefab.", this);
            return;
        }

        Dictionary<GridCoordinates, GridTile> existingTiles = CollectExistingTiles();

        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                GridCoordinates coordinates = new GridCoordinates(x, y);

                if (!existingTiles.TryGetValue(coordinates, out GridTile tile) || tile == null)
                {
                    tile = CreateTile(coordinates);
                }

                ConfigureTile(tile, coordinates);
            }
        }

        DeleteOutOfBoundsTiles();
        RebuildCache();
        MarkDirty();
    }

    [Button, GUIColor(0.95f, 0.35f, 0.35f)]
    public void ClearGrid()
    {
        RebuildCache();

        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            if (tiles[i] != null)
            {
                DestroyObject(tiles[i].gameObject);
            }
        }

        tiles.Clear();
        tileLookup?.Clear();
        MarkDirty();
    }

    [Button]
    public void ClearTileObjects()
    {
        RebuildCache();

        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] == null)
            {
                continue;
            }

            RecordObject(tiles[i], "Clear Tile Object");
            tiles[i].ClearPlacedObject();
            MarkObjectDirty(tiles[i]);
        }

        MarkDirty();
    }

    [Button]
    public void RebuildCache()
    {
        EnsureTileParent();

        if (tiles == null)
        {
            tiles = new List<GridTile>();
        }

        tiles.RemoveAll(tile => tile == null);

        GridTile[] childTiles = tileParent.GetComponentsInChildren<GridTile>(true);
        for (int i = 0; i < childTiles.Length; i++)
        {
            if (!tiles.Contains(childTiles[i]))
            {
                tiles.Add(childTiles[i]);
            }
        }

        tiles.Sort((left, right) =>
        {
            int yCompare = left.Coordinates.Y.CompareTo(right.Coordinates.Y);
            return yCompare != 0 ? yCompare : left.Coordinates.X.CompareTo(right.Coordinates.X);
        });

        tileLookup = new Dictionary<GridCoordinates, GridTile>(tiles.Count);
        for (int i = 0; i < tiles.Count; i++)
        {
            GridTile tile = tiles[i];
            if (tile != null && !tileLookup.ContainsKey(tile.Coordinates))
            {
                tileLookup.Add(tile.Coordinates, tile);
            }
        }
    }

    private Dictionary<GridCoordinates, GridTile> CollectExistingTiles()
    {
        RebuildCache();

        Dictionary<GridCoordinates, GridTile> existingTiles = new Dictionary<GridCoordinates, GridTile>(tiles.Count);
        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            GridTile tile = tiles[i];
            if (tile == null)
            {
                tiles.RemoveAt(i);
                continue;
            }

            GridCoordinates coordinates = tile.Coordinates;
            if (existingTiles.ContainsKey(coordinates))
            {
                DestroyObject(tile.gameObject);
                tiles.RemoveAt(i);
                continue;
            }

            existingTiles.Add(coordinates, tile);
        }

        return existingTiles;
    }

    private GridTile CreateTile(GridCoordinates coordinates)
    {
        GameObject tileObject = InstantiateTilePrefab();
        if (tileObject == null)
        {
            Debug.LogError($"Failed to instantiate tile prefab for {coordinates}.", this);
            return null;
        }

        tileObject.name = $"Tile_{coordinates.X}_{coordinates.Y}";

        GridTile tile = tileObject.GetComponent<GridTile>();
        if (tile == null)
        {
            tile = AddGridTileComponent(tileObject);
        }

        tiles.Add(tile);
        return tile;
    }

    private void ConfigureTile(GridTile tile, GridCoordinates coordinates)
    {
        if (tile == null)
        {
            return;
        }

        RecordObject(tile, "Configure Tile");
        RecordObject(tile.transform, "Move Tile");

        tile.transform.SetParent(tileParent, false);
        tile.Initialize(coordinates);
        tile.transform.localPosition = GetLocalPosition(coordinates);
        tile.transform.localRotation = Quaternion.identity;

        MarkObjectDirty(tile);
        MarkObjectDirty(tile.transform);
    }

    private void DeleteOutOfBoundsTiles()
    {
        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            GridTile tile = tiles[i];
            if (tile == null)
            {
                tiles.RemoveAt(i);
                continue;
            }

            GridCoordinates coordinates = tile.Coordinates;
            bool isOutOfBounds = coordinates.X < 0
                || coordinates.Y < 0
                || coordinates.X >= gridSize.x
                || coordinates.Y >= gridSize.y;

            if (isOutOfBounds)
            {
                DestroyObject(tile.gameObject);
                tiles.RemoveAt(i);
            }
        }
    }

    private void EnsureLookup()
    {
        if (tileLookup == null || tileLookup.Count != tiles.Count)
        {
            RebuildCache();
        }
    }

    private void EnsureTileParent()
    {
        if (tileParent == null)
        {
            tileParent = transform;
        }
    }

    private void UpdateUnityGrid()
    {
        UnityEngine.Grid unityGrid = GetComponent<UnityEngine.Grid>();
        if (unityGrid == null)
        {
            return;
        }

        RecordObject(unityGrid, "Update Grid Cell Size");
        unityGrid.cellSize = new Vector3(tileSpacing, tileSpacing, 1f);
        MarkObjectDirty(unityGrid);
    }

    private void ClampSettings()
    {
        gridSize = new Vector2Int(Mathf.Max(1, gridSize.x), Mathf.Max(1, gridSize.y));
        tileSpacing = Mathf.Max(0.1f, tileSpacing);
    }

    private GameObject InstantiateTilePrefab()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            GameObject editorTileObject = PrefabUtility.InstantiatePrefab(tilePrefab) as GameObject;
            if (editorTileObject != null)
            {
                Undo.RegisterCreatedObjectUndo(editorTileObject, "Create Grid Tile");
            }

            return editorTileObject;
        }
#endif

        return Instantiate(tilePrefab);
    }

    private GridTile AddGridTileComponent(GameObject tileObject)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return Undo.AddComponent<GridTile>(tileObject);
        }
#endif

        return tileObject.AddComponent<GridTile>();
    }

    private void DestroyObject(UnityEngine.Object target)
    {
        if (target == null)
        {
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Undo.DestroyObjectImmediate(target);
            return;
        }
#endif

        Destroy(target);
    }

    private void RecordObject(UnityEngine.Object target, string undoName)
    {
#if UNITY_EDITOR
        if (target != null && !Application.isPlaying)
        {
            Undo.RecordObject(target, undoName);
        }
#endif
    }

    private void MarkObjectDirty(UnityEngine.Object target)
    {
#if UNITY_EDITOR
        if (target != null && !Application.isPlaying)
        {
            EditorUtility.SetDirty(target);
        }
#endif
    }

    private void MarkDirty()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            return;
        }

        EditorUtility.SetDirty(this);
        if (gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }

    private void OnValidate()
    {
        ClampSettings();

#if UNITY_EDITOR
        if (!Application.isPlaying && autoRefresh)
        {
            EditorApplication.delayCall -= DelayedAutoRefresh;
            EditorApplication.delayCall += DelayedAutoRefresh;
        }
#endif
    }

#if UNITY_EDITOR
    private void DelayedAutoRefresh()
    {
        EditorApplication.delayCall -= DelayedAutoRefresh;

        if (this == null || Application.isPlaying)
        {
            return;
        }

        if (tilePrefab != null)
        {
            CreateOrRefreshGrid();
        }
        else
        {
            RebuildCache();
        }
    }

    public bool OwnsTile(GridTile tile)
    {
        if (tile == null)
        {
            return false;
        }

        Transform parent = tileParent != null ? tileParent : transform;
        return tile.transform == parent || tile.transform.IsChildOf(parent);
    }

    public void PlaceObjectOnTile(GridTile tile, GameObject prefab)
    {
        if (tile == null || prefab == null)
        {
            return;
        }

        RecordObject(tile, "Place Tile Object");
        tile.ClearPlacedObject();

        GameObject placedObject;
        if (!Application.isPlaying)
        {
            placedObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (placedObject != null)
            {
                Undo.RegisterCreatedObjectUndo(placedObject, "Place Tile Object");
            }
        }
        else
        {
            placedObject = Instantiate(prefab);
        }

        if (placedObject == null)
        {
            return;
        }

        tile.SetPlacedObject(placedObject);
        MarkObjectDirty(tile);
        MarkObjectDirty(placedObject);
        MarkDirty();
    }

    private void OnDrawGizmos()
    {
        if (!showCoordinates)
        {
            return;
        }

        RebuildCache();

        for (int i = 0; i < tiles.Count; i++)
        {
            GridTile tile = tiles[i];
            if (tile != null)
            {
                Handles.Label(tile.WorldPosition + Vector3.up * 0.05f, tile.Coordinates.ToString());
            }
        }
    }

#endif
}

using System.Collections.Generic;
using UnityEngine;

public sealed class GridManagerService : IGridManagerService
{
    private readonly GridManager gridManager;
    private readonly GridPathfinder pathfinder;

    public GridManagerService(GridManager gridManager, GridPathfinder pathfinder)
    {
        this.gridManager = gridManager;
        this.pathfinder = pathfinder;
    }

    public IReadOnlyList<GridTile> Tiles => gridManager != null ? gridManager.Tiles : System.Array.Empty<GridTile>();
    public Vector2Int GridSize => gridManager != null ? gridManager.GridSize : Vector2Int.zero;

    public bool TryGetTile(GridCoordinates coordinates, out GridTile tile)
    {
        tile = null;
        return gridManager != null && gridManager.TryGetTile(coordinates, out tile);
    }

    public bool TryGetTile(Vector2Int coordinates, out GridTile tile)
    {
        tile = null;
        return gridManager != null && gridManager.TryGetTile(coordinates, out tile);
    }

    public GridTile GetTileOrNull(GridCoordinates coordinates)
    {
        return gridManager != null ? gridManager.GetTileOrNull(coordinates) : null;
    }

    public bool ContainsCoordinates(GridCoordinates coordinates)
    {
        return gridManager != null && gridManager.ContainsCoordinates(coordinates);
    }

    public IReadOnlyList<GridTile> GetNeighbors(GridCoordinates coordinates, bool includeDiagonals = false)
    {
        if (gridManager == null)
        {
            return System.Array.Empty<GridTile>();
        }

        return new List<GridTile>(gridManager.GetNeighbors(coordinates, includeDiagonals));
    }

    public GridCoordinates WorldToCoordinates(Vector3 worldPosition)
    {
        return gridManager != null ? gridManager.WorldToCoordinates(worldPosition) : new GridCoordinates(-1, -1);
    }

    public Vector3 GetWorldPosition(GridCoordinates coordinates)
    {
        return gridManager != null ? gridManager.GetWorldPosition(coordinates) : Vector3.zero;
    }

    public GridPathResult FindPath(GridCoordinates start, GridCoordinates target)
    {
        if (pathfinder == null)
        {
            return GridPathResult.Failed("GridPathfinder is null.");
        }

        return pathfinder.FindPath(gridManager, start, target);
    }

    public GridPathResult FindPath(GridTile startTile, GridTile targetTile)
    {
        if (startTile == null)
        {
            return GridPathResult.Failed("Start tile is null.");
        }

        if (targetTile == null)
        {
            return GridPathResult.Failed("Target tile is null.");
        }

        return FindPath(startTile.Coordinates, targetTile.Coordinates);
    }
}

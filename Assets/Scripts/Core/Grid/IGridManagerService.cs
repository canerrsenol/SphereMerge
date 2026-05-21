using System.Collections.Generic;
using UnityEngine;

public interface IGridManagerService
{
    IReadOnlyList<GridTile> Tiles { get; }
    Vector2Int GridSize { get; }

    bool TryGetTile(GridCoordinates coordinates, out GridTile tile);
    bool TryGetTile(Vector2Int coordinates, out GridTile tile);
    GridTile GetTileOrNull(GridCoordinates coordinates);
    bool ContainsCoordinates(GridCoordinates coordinates);
    IReadOnlyList<GridTile> GetNeighbors(GridCoordinates coordinates, bool includeDiagonals = false);
    GridCoordinates WorldToCoordinates(Vector3 worldPosition);
    Vector3 GetWorldPosition(GridCoordinates coordinates);
    GridPathResult FindPath(GridCoordinates start, GridCoordinates target);
    GridPathResult FindPath(GridTile startTile, GridTile targetTile);
}

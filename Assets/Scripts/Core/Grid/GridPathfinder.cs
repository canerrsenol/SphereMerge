using System.Collections.Generic;

public sealed class GridPathfinder
{
    public GridPathResult FindPath(GridManager gridManager, GridCoordinates start, GridCoordinates target)
    {
        if (gridManager == null)
        {
            return GridPathResult.Failed("GridManager is null.");
        }

        if (!gridManager.TryGetTile(start, out GridTile startTile))
        {
            return GridPathResult.Failed($"Start tile does not exist: {start}.");
        }

        if (!gridManager.TryGetTile(target, out GridTile targetTile))
        {
            return GridPathResult.Failed($"Target tile does not exist: {target}.");
        }

        if (!targetTile.IsWalkable)
        {
            return GridPathResult.Failed($"Target tile is not walkable: {target}.");
        }

        if (start == target)
        {
            return GridPathResult.SuccessResult(
                new List<GridTile> { startTile },
                new List<GridCoordinates> { start });
        }

        Queue<GridCoordinates> frontier = new Queue<GridCoordinates>();
        HashSet<GridCoordinates> visited = new HashSet<GridCoordinates>();
        Dictionary<GridCoordinates, GridCoordinates> parents = new Dictionary<GridCoordinates, GridCoordinates>();

        frontier.Enqueue(start);
        visited.Add(start);

        while (frontier.Count > 0)
        {
            GridCoordinates current = frontier.Dequeue();

            foreach (GridTile neighbor in gridManager.GetNeighbors(current))
            {
                GridCoordinates next = neighbor.Coordinates;
                if (visited.Contains(next) || !neighbor.IsWalkable)
                {
                    continue;
                }

                visited.Add(next);
                parents[next] = current;

                if (next == target)
                {
                    return BuildResult(gridManager, parents, start, target);
                }

                frontier.Enqueue(next);
            }
        }

        return GridPathResult.Failed($"Path not found from {start} to {target}.");
    }

    private static GridPathResult BuildResult(
        GridManager gridManager,
        Dictionary<GridCoordinates, GridCoordinates> parents,
        GridCoordinates start,
        GridCoordinates target)
    {
        List<GridCoordinates> coordinates = new List<GridCoordinates>();
        GridCoordinates current = target;
        coordinates.Add(current);

        while (current != start)
        {
            if (!parents.TryGetValue(current, out current))
            {
                return GridPathResult.Failed("Path reconstruction failed.");
            }

            coordinates.Add(current);
        }

        coordinates.Reverse();

        List<GridTile> tiles = new List<GridTile>(coordinates.Count);
        for (int i = 0; i < coordinates.Count; i++)
        {
            if (!gridManager.TryGetTile(coordinates[i], out GridTile tile))
            {
                return GridPathResult.Failed($"Path tile missing during reconstruction: {coordinates[i]}.");
            }

            tiles.Add(tile);
        }

        return GridPathResult.SuccessResult(tiles, coordinates);
    }
}

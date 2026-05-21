using System.Collections.Generic;

public sealed class GridPathResult
{
    private static readonly IReadOnlyList<GridTile> EmptyTiles = new List<GridTile>(0);
    private static readonly IReadOnlyList<GridCoordinates> EmptyCoordinates = new List<GridCoordinates>(0);

    private GridPathResult(
        bool success,
        IReadOnlyList<GridTile> tiles,
        IReadOnlyList<GridCoordinates> coordinates,
        string errorMessage)
    {
        Success = success;
        Tiles = tiles;
        Coordinates = coordinates;
        ErrorMessage = errorMessage;
    }

    public bool Success { get; }
    public IReadOnlyList<GridTile> Tiles { get; }
    public IReadOnlyList<GridCoordinates> Coordinates { get; }
    public string ErrorMessage { get; }

    public static GridPathResult SuccessResult(List<GridTile> tiles, List<GridCoordinates> coordinates)
    {
        return new GridPathResult(true, tiles, coordinates, string.Empty);
    }

    public static GridPathResult Failed(string errorMessage)
    {
        return new GridPathResult(false, EmptyTiles, EmptyCoordinates, errorMessage);
    }
}

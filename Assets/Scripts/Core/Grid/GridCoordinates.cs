using System;
using UnityEngine;

[Serializable]
public struct GridCoordinates : IEquatable<GridCoordinates>
{
    public int X;
    public int Y;

    public GridCoordinates(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int(X, Y);
    }

    public static GridCoordinates FromVector2Int(Vector2Int value)
    {
        return new GridCoordinates(value.x, value.y);
    }

    public bool Equals(GridCoordinates other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object obj)
    {
        return obj is GridCoordinates other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (X * 397) ^ Y;
        }
    }

    public override string ToString()
    {
        return $"[{X},{Y}]";
    }

    public static bool operator ==(GridCoordinates left, GridCoordinates right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GridCoordinates left, GridCoordinates right)
    {
        return !left.Equals(right);
    }
}

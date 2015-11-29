
using System;


/// <summary>
/// A grid position represents a position in a grid.
/// It works much like and integer version of the Vector3 class.
/// </summary>
[Serializable]
public struct GridPosition
{
    public int X, Y, Layer;

    public GridPosition(int x, int y, int layer)
    {
        X = x;
        Y = y;
        Layer = layer;
    }

    public static GridPosition operator +(GridPosition gp1, GridPosition gp2)
    {
        return new GridPosition(gp1.X + gp2.X, gp1.Y + gp2.Y, gp1.Layer + gp2.Layer);
    }

    public static GridPosition operator -(GridPosition gp1, GridPosition gp2)
    {
        return new GridPosition(gp2.X - gp1.X, gp2.Y - gp1.Y, gp2.Layer - gp1.Layer);
    }
}

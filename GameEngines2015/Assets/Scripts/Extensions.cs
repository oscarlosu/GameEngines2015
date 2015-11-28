using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class Extensions
{
    public static string ToLayerString(this short[,] layer)
    {
        int sizeX = layer.GetLength(0);
        int sizeY = layer.GetLength(1);
        StringBuilder sb = new StringBuilder(layer.Length * 2);
        sb.AppendLine();
        for (int y = sizeY - 1; y >= 0; y--)
        {
            for (int x = 0; x < sizeX; x++)
            {
                sb.Append(layer[x, y]);
                if (x < sizeX - 1)
                {
                    sb.Append(",");
                }
                else
                {
                    sb.AppendLine();
                }
            }
        }
        return sb.ToString();
    }
}

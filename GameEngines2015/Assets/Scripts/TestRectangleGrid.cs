using UnityEngine;
using System.Collections.Generic;

public class TestRectangleGrid : MonoBehaviour
{
    public RectangleGrid MyGrid;
    public RenderingHandler Handler;
    public GameObject TestObject;
    public int Width, Depth, Height;
    public int X, Y, Layer;

    //public short[] Matrix;
    public int size;

    // Use this for initialization
    void Start()
    {
        MyGrid.SetGridSize(Width, Depth, Height);
        for (int layer = 0; layer < Height; layer++)
        {
            for (int y = 0; y < Depth; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    MyGrid.Place(0, x, y, layer);
                }
            }
        }
        Handler.Load();
        MyGrid.AddLayer();
        MyGrid.FillRect(1, 5, 5, Height - 3, Width - 1, Depth - 1, Height);
        //MyGrid.RemoveLayer(Height);
        //MyGrid.RemoveLayer(Height-1);
        //MyGrid.RemoveLayer(Height-2);
        //MyGrid.RemoveLayer(Height - 3);
        MyGrid.MoveRect(5, 5, Height - 3, Width - 1, Depth - 1, Height, 0, 0, 0);
        Handler.Load();
    }

    // Update is called once per frame
    void Update()
    {

    }
}

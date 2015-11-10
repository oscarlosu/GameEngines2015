using UnityEngine;
using System.Collections.Generic;

public class TestRectangleGrid : MonoBehaviour
{
    public RectangleGrid MyGrid;
    public GameObject TestObject;
    public int Width, Depth, Height;
    public int X, Y, Layer;

    //public short[] Matrix;
    public int size;

    // Use this for initialization
    void Start()
    {

        MyGrid.SetGridSize(Width, Depth, Height);
        MyGrid.FillRect(TestObject, 0, 0, 0, Width - 1, Depth - 1, Height - 1);
        //MyGrid.SetGridSize(1, 2, 1);
        //Matrix = new short[size];
        //for(int i = 0; i < size; ++i)
        //{
        //    Matrix[i] = new GameObject();
        //}
        

    }

    // Update is called once per frame
    void Update()
    {

    }
}

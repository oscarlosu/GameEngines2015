using UnityEngine;
using System.Collections;

public class TestRectangleGrid : MonoBehaviour
{
    public RectangleGrid MyGrid;
    public GameObject TestObject;
    public int Width, Depth, Height;
    public int X, Y, Layer;

    // Use this for initialization
    void Start()
    {

        MyGrid.SetGridSize(Width, Depth, Height);
        MyGrid.FillRect(TestObject, 0, 0, 0, Width - 1, Depth - 1, Height - 1);
        //MyGrid.SetGridSize(1, 2, 1);
    }

    // Update is called once per frame
    void Update()
    {

    }
}

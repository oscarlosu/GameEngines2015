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
        
        MyGrid.Place(Instantiate(TestObject), X, Y, Layer);
        MyGrid.Place(Instantiate(TestObject), X, Y, Layer + 2);
        MyGrid.Place(Instantiate(TestObject), X+2, Y, Layer);
        MyGrid.Place(Instantiate(TestObject), X, Y + 1, Layer);
        MyGrid.Place(Instantiate(TestObject), X, Y + 1, Layer + 1);

        //MyGrid.SetGridSize(1, 2, 1);
    }

    // Update is called once per frame
    void Update()
    {

    }
}

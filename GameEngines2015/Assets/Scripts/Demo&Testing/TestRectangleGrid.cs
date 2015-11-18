using UnityEngine;
using System.Collections.Generic;

public class TestRectangleGrid : MonoBehaviour
{
    public RectangleGrid MyGrid;
    public RenderingHandler Handler;
    public GameObject TestObject;
    public int NX, NY, NLayer;
    public int X, Y, Layer;

    // Use this for initialization
    void Start()
    {
        MyGrid.SetGridSize(NX, NY, NLayer);
        for (int layer = 0; layer < NLayer; layer++)
        {
            for (int y = 0; y < NY; y++)
            {
                for (int x = 0; x < NX; x++)
                {
                    MyGrid.Place(0, x, y, layer);
                }
            }
        }
        
        //MyGrid.AddLayer();
        //Handler.ZoomLoad();
		/*MyGrid.Remove(0, 0, 0);
		Handler.Load();*/
        //MyGrid.FillRect(1, 0, 0, NLayer - 1, NX - 1, NY - 1, NLayer-1);
        //MyGrid.SwapLayers(0, Height - 1);
        //MyGrid.RemoveLayer(Height);
        //MyGrid.RemoveLayer(Height-1);
        //MyGrid.RemoveLayer(Height-2);
        //MyGrid.RemoveLayer(Height - 3);
        //MyGrid.MoveRect(5, 5, Height - 3, Width - 1, Depth - 1, Height, 0, 0, 0);
        //MyGrid.Place(1, 5, 5, Height);
        //Handler.UpdateCell(5, 5, Height);

        //MyGrid.Move(0, 0, 0, 0, 0, Height - 1);
        //MyGrid.Swap(0, 0, 0, 0, 0, Height - 1);
        //MyGrid.Remove(0, 0, Height - 1);
        //MyGrid.MoveRect(0, 0, 0, 0, 0, 0, 0, 0, Height - 1);
        //MyGrid.Place(0, 5, 5, Height-1);
        //MyGrid.AddLayer();
        //MyGrid.Place(0, 5, 5, Height);
        //MyGrid.RemoveRect(0, 0, Height - 1, 4, 4, Height - 5);

        // Interesting small patch of grass with tall walls of dirt.
        //MyGrid.RemoveRect(0, 0, Height - 1, 4, 4, Height - 5);
        //MyGrid.FillRect(1, 0, 5, Height - 5, 5, 5, Height - 1);
        //MyGrid.FillRect(1, 5, 5, Height - 5, 5, 0, Height - 1);
    }

    // Update is called once per frame
    void Update()
    {

    }
}

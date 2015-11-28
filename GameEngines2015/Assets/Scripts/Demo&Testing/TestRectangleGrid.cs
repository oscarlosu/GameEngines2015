using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class TestRectangleGrid : MonoBehaviour
{
    public RectangleGrid MyGrid;
    public RenderingHandler Handler;
    public GameObject TestObject;
    public int NX, NY, NLayer;
    public int X, Y, Layer;
	public int FBmIterations;
	public int Seed;


    // Use this for initialization
    void Start()
    {
        //MyGrid.LoadGridFromFile(Directory.GetCurrentDirectory() + @"\..\testGridSave.txt");
        /*Debug.Log(Directory.GetCurrentDirectory());
        MyGrid.LoadGridFromFile(Directory.GetCurrentDirectory() + @"\..\testGridFile.txt");

        MyGrid.RemoveLayer(1);
        MyGrid.Place(0, 0, 0, 0);
        MyGrid.SaveGridToFile(Directory.GetCurrentDirectory() + @"\..\testGridSave.txt");*/

        MyGrid.SetGridSize(NX, NY, NLayer);
        
        for (int y = 0; y < NY; y++)
        {
            for (int x = 0; x < NX; x++)
            {
				float density = fBm(FBmIterations, x / (float)NX, y / (float)NY);
				for (int layer = 0; layer < NLayer; layer++)
				{
					// Earth
					if(NLayer * density > layer)
					{
						// Dirt
						if(NLayer * density > layer + 1)
						{
							MyGrid.Place(0, x, y, layer);
						}
						// Grass
						else
						{
							MyGrid.Place(1, x, y, layer);
						}

					}
					// Air
					else
					{

					}
                    
                }
            }
        }

        //Handler.HideFromLayer(5);

        /*StartCoroutine(MyGrid.SaveGridToFileCoroutine(Directory.GetCurrentDirectory() + @"\..\testGridSave.txt", true,
            delegate()
            {
                Debug.Log("The level has now been saved!");
            }));*/

        /**********************
        * Old test code.
        **********************/

        /*
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
        //Handler.Load();
        MyGrid.FillRect(1, 0, 0, NLayer - 1, NX - 1, NY - 1, NLayer - 1);
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
        MyGrid.RemoveRect(0, 0, NLayer - 1, 4, 4, NLayer - 5);
        MyGrid.FillRect(1, 0, 5, NLayer - 5, 5, 5, NLayer - 1);
        MyGrid.FillRect(1, 5, 5, NLayer - 5, 5, 0, NLayer - 1);
        */
    }

	float fBm(int iterations, float x, float y)
	{

		float value = 0;
		for(int i = 1; i <= iterations; ++i)
		{
			value += Mathf.PerlinNoise(Seed + x * iterations, Seed + y * iterations) / iterations;
		}
		return value;
	}


    // Update is called once per frame
    void Update()
    {

    }
}

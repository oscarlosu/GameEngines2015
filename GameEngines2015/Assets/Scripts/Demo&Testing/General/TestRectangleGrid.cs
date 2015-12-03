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

    public SpriteList Tile1, Tile2, Tile3, Tile4;


    // Use this for initialization
    void Start()
    {
        /*Handler.Tiles.Add(Tile1);
        Handler.Tiles.Add(Tile2);*/
        

        //MyGrid.LoadGridFromFile(Directory.GetCurrentDirectory() + @"\..\testGridSave.txt");
        /*Debug.Log(Directory.GetCurrentDirectory());
        MyGrid.LoadGridFromFile(Directory.GetCurrentDirectory() + @"\..\testGridFile.txt");*/
        /*
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

using UnityEngine;
using System.Collections.Generic;

public class TestRectangleGrid : MonoBehaviour
{
    public RectangleGrid MyGrid;
    public RenderingHandler Handler;
    public GameObject TestObject;
    public int NX, NY, NLayer;
    public int X, Y, Layer;
	public int FBmIterations;

    // Use this for initialization
    void Start()
    {
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
    }
	float fBm(int iterations, float x, float y)
	{
		float value = 0;
		for(int i = 1; i <= iterations; ++i)
		{
			value += Mathf.PerlinNoise(x * iterations, y * iterations) / iterations;
		}
		return value;
	}


    // Update is called once per frame
    void Update()
    {

    }
}

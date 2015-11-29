using UnityEngine;
using System.Collections;
using System.IO;

public class TrippyTiles : MonoBehaviour
{

    public RectangleGrid Grid;
    public RenderingHandler RendHandler;

    // Use this for initialization
    void Start()
    {

        Grid.SetGridSize(20, 50, 3);
        Grid.FillRect(0, 0, 49, 0, 19, 40, 0);
        Grid.FillRect(1, 0, 39, 0, 19, 0, 0);
        Grid.FillRect(2, 0, 39, 2, 19, 39, 2);

        // TODO It should be possible to get the grid's current size from the grid.

        // Place bushes and trees.
        for (int x = 0; x < 20; x++)
        {
            // 30% chance for a bush.
            if (Random.Range(0, 100) < 30)
            {
                Grid.Place(9, x, 40, 1);
            }
            // 30% chance for a tree.
            else if (Random.Range(0, 100) < 30)
            {
                Grid.Place(6, x, 40, 1);
                Grid.Place(6, x, 41, 1);
                Grid.Place(7, x, 42, 1);
                Grid.Place(7, x - 1, 42, 1);
                Grid.Place(7, x + 1, 42, 1);
                Grid.Place(7, x, 43, 1);
                // Ensure that two trees can't be directly besides each other.
                x++;
            }
        }

        // Place underground blocks.
        for (int y = 38; y >= 0; y--)
        {
            for (int x = 0; x < 20; x++)
            {
                // 20% chance for stone.
                if (Random.Range(0, 100) < 20)
                {
                    Grid.Place(4, x, y, 2);
                }
                // 10% chance for gold (if not stone).
                else if (Random.Range(0, 100) < 10)
                {
                    Grid.Place(5, x, y, 2);
                }
                // 80% chance for dirt (if not stone or gold).
                else if (Random.Range(0, 100) < 80)
                {
                    Grid.Place(3, x, y, 2);
                }
            }
        }

        // Place three underground rooms with a mushroom in each.
        int startX, startY, roomWidth, roomHeight;
        roomWidth = 3;
        roomHeight = 2;
        for (int i = 0; i < 3; i++)
        {
            startX = Random.Range(1, 19 - roomWidth - 1);
            startY = Random.Range(2 + roomHeight, 20);
            Grid.FillRect(-1, startX, startY, 2, startX + roomWidth - 1, startY - roomHeight + 1, 2);
            Grid.Place(8, startX + 1, startY - 1, 2);
        }


    }

    // Update is called once per frame
    void Update()
    {
		if(Input.GetKeyDown (KeyCode.S))
		{
			StartCoroutine(Grid.SaveGridToFileCoroutine(Directory.GetCurrentDirectory() + @"\..\TrippyTilesDemo.txt"));
		}
		if(Input.GetKeyDown (KeyCode.L))
		{
			Grid.LoadGridFromFile(Directory.GetCurrentDirectory() + @"\..\TrippyTilesDemo.txt");
		}
    }
}

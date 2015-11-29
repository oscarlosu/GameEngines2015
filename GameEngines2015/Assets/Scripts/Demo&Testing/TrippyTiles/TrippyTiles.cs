using UnityEngine;
using System.Collections;

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
        //for ()

        // Place underground blocks.
        for (int y = 38; y >= 0; y--)
        {
            for (int x = 0; x < 20; x++)
            {
                // 20% chance for stone.
                if (Random.Range(0, 100) < 20)
                {
                    Grid.Place(4, x, y, 2);
                    Debug.Log("Placing stone");
                }
                // 10% chance for gold (if not stone).
                else if (Random.Range(0, 100) < 10)
                {
                    Grid.Place(5, x, y, 2);
                    Debug.Log("Placing gold");
                }
                // 80% chance for dirt (if not stone or gold).
                else if (Random.Range(0, 100) < 80)
                {
                    Grid.Place(3, x, y, 2);
                    Debug.Log("Placing dirt");
                }
            }
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}

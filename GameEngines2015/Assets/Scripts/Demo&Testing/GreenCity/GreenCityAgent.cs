using UnityEngine;
using System.Collections.Generic;

public class GreenCityAgent : GridAgent
{

    public List<short> RiverTileIndexes;

    // Logic variables.
    private int housesFree;

	// Use this for initialization
	new void Start ()
	{
	    base.Start();
	}
	
	// Update is called once per frame
	void Update () {

        // If moving up.
	    if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
	    {
            Move(HorizontalDirection.North, VerticalDirection.None, new CanAgentMoveByDelegate(CanGhostMoveBy));
        }

        // If moving right.
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            Move(HorizontalDirection.East, VerticalDirection.None, new CanAgentMoveByDelegate(CanGhostMoveBy));
        }

        // If moving down.
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            Move(HorizontalDirection.South, VerticalDirection.None, new CanAgentMoveByDelegate(CanGhostMoveBy));
        }

        // If moving left.
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            Move(HorizontalDirection.West, VerticalDirection.None, new CanAgentMoveByDelegate(CanGhostMoveBy));
        }

        // Place buildings.

        // Place a house.
	    if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
	    {
            // If the cell is free.
	        if (CanPlaceBuilding(CellCoords.X, CellCoords.Y, CellCoords.Layer))
	        {
	            Grid.Place(6, CellCoords.X, CellCoords.Y, CellCoords.Layer);
	            housesFree++;
	        }
	    }

        // Place a power station.
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            // If there is workers for the power station and the cell is free.
            if (housesFree > 0 && CanPlaceBuilding(CellCoords.X, CellCoords.Y, CellCoords.Layer))
            {
                Grid.Place(7, CellCoords.X, CellCoords.Y, CellCoords.Layer);
                housesFree--;
            }
        }

        // Place a power station.
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            // If there is workers for the power station and the cell is free.
            if (housesFree > 0  && CanPlaceBuilding(CellCoords.X, CellCoords.Y, CellCoords.Layer))
            {
                Grid.Place(8, CellCoords.X, CellCoords.Y, CellCoords.Layer);
                housesFree--;
            }
        }
    }

    private bool CanPlaceBuilding(int x, int y, int layer)
    {
        short tile;
        return Grid.IsCellFree(CellCoords.X, CellCoords.Y, CellCoords.Layer) && Grid.TryGetTile(x, y, layer - 1, out tile) && !RiverTileIndexes.Contains(tile);
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrippyAgent : GridAgent
{
	public int MaxYIndex;

	public int SkyBackground = 0;
	public int GroundBackground = 1;
	public int Grass = 2;
	public int Dirt = 3;
	public int Stone = 4;
	public int Gold = 5;
	public int TreeTrunk = 6;
	public int TreeLeaves = 7;
	public int Mushrooms = 8;
	public int Bush = 9;

	public int GoldCounter;
	public int MushroomCounter;

	// Use this for initialization
	new void Start ()
	{
		base.Start ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		HorizontalDirection hDir = HorizontalDirection.None;
		VerticalDirection vDir = VerticalDirection.None;
		if(Input.GetKeyDown(KeyCode.UpArrow))
		{
			hDir = HorizontalDirection.North;
		}
		else if(Input.GetKeyDown(KeyCode.DownArrow))
		{
			hDir = HorizontalDirection.South;
		}
		else if(Input.GetKeyDown(KeyCode.LeftArrow))
		{
			hDir = HorizontalDirection.West;
		}
		else if(Input.GetKeyDown(KeyCode.RightArrow))
		{
			hDir = HorizontalDirection.East;
		}
		Move (hDir, vDir, Action);
	}

	public bool Action(int x, int y, int layer, out int outX, out int outY, out int outLayer)
	{
		outX = x;
		outY = y;
		outLayer = layer;
		short tile;
		// Cant move outside of the grid
		if(!Grid.IsInsideGrid(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer))
		{
			return false;
		}
		// Cant move to cells occupied by non-walkable tiles
		if(Grid.TryGetTile(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer, out tile))
		{
			return false;
		}
		// Can remove grass and dirt
		if(tile == Grass || tile == Dirt)
		{
			Grid.Remove(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer);
			// Play digging sound

			return true;
		}
		// Can remove gold increasing the gold counter
		if(tile == Gold)
		{
			Grid.Remove(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer);
			++GoldCounter;
			// Play gold mining sound

			return true;
		}
		// Can eat mushrooms
		if(tile == Mushrooms)
		{
			Grid.Remove(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer);
			++MushroomCounter;
			// Play mushroom eating sound

			return true;
		}

		return false;
	}
}

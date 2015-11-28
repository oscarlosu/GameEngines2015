using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridAgent : MonoBehaviour
{
	public RectangleGrid Grid;
	public Vector3 CellCoords;
	public List<int> NotWalkableTileIndexes = new List<int>();

	private SpriteRenderer rend;

	public enum HorizontalDirection
	{
		North,
		NorthEast,
		East,
		SouthEast,
		South,
		SouthWest,
		West,
		NorthWest,
		None
	}
	public enum VerticalDirection
	{
		Up,
		Down,
		None
	}


	// Use this for initialization
	protected void Start ()
	{
		transform.position = new Vector3(CellCoords.x * Grid.CellWidth, CellCoords.y * Grid.CellDepth + CellCoords.z * Grid.CellHeight, 0);
		rend = GetComponent<SpriteRenderer>();
		rend.sortingOrder = (int)CellCoords.z - (int)CellCoords.y;
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public void Move(HorizontalDirection hDir, VerticalDirection vDir, CanAgentMoveByDelegate CanMove)
	{
		if(hDir == HorizontalDirection.None && vDir == VerticalDirection.None)
		{
			return;
		}
		int xInc = 0, yInc = 0, layerInc = 0;
		switch(hDir)
		{
		case HorizontalDirection.North:
			yInc = 1;
			break;
		case HorizontalDirection.NorthEast:
			yInc = 1;
			xInc = 1;
			break;
		case HorizontalDirection.East:
			xInc = 1;
			break;
		case HorizontalDirection.SouthEast:
			yInc = - 1;
			xInc = 1;
			break;
		case HorizontalDirection.South:
			yInc = - 1;
			break;
		case HorizontalDirection.SouthWest:
			yInc = - 1;
			xInc = - 1;
			break;
		case HorizontalDirection.West:
			xInc = - 1;
			break;
		case HorizontalDirection.NorthWest:
			yInc = 1;
			xInc = - 1;
			break;
		}
		switch(vDir)
		{
		case VerticalDirection.Up:
			layerInc = 1;
			break;
		case VerticalDirection.Down:
			layerInc = - 1;
			break;
		}
		// Call delegate method to determine if the agent can move to that cell
		int outX, outY, outLayer;
		if(CanMove(xInc, yInc, layerInc, out outX, out outY, out outLayer))
		{
			// Move to cell
			CellCoords += new Vector3(outX, outY, outLayer);
			transform.position += new Vector3(outX * Grid.CellWidth, outY * Grid.CellDepth + outLayer * Grid.CellHeight, 0);
			rend.sortingOrder = (int)CellCoords.z - (int)CellCoords.y;
		}
	}

	public delegate bool CanAgentMoveByDelegate(int x, int y, int layer, out int outX, out int outY, out int outLayer);

	public bool CanGhostMoveBy(int x, int y, int layer, out int outX, out int outY, out int outLayer)
	{
		outX = x;
		outY = y;
		outLayer = layer;
		// Inside the world
		if(Grid.IsInsideGrid((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + layer))
		{
			return true;
		}
		return false;
	}
	public bool CanFlyMoveBy(int x, int y, int layer, out int outX, out int outY, out int outLayer)
	{
		outX = x;
		outY = y;
		outLayer = layer;
		// Inside the world
		if(Grid.IsInsideGrid((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + layer))
		{
			// Only to an empty cell
			short tile;
			GameObject obj;
			if(!Grid.TryGetTile((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + layer, out tile) &&
			   !Grid.TryGetObject((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + layer, out obj))
			{
				return true;
			}
		}
		return false;
	}
	public bool CanWalkStrictMoveBy(int x, int y, int layer, out int outX, out int outY, out int outLayer)
	{
		outX = x;
		outY = y;
		outLayer = layer;
		// Inside the world
		if(Grid.IsInsideGrid((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + layer))
		{
			// Only to an empty cell
			short tile;
			GameObject obj;
			if(!Grid.TryGetTile((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + layer, out tile) &&
			   !Grid.TryGetObject((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + layer, out obj))
			{
				// Only over walkable tiles and never over game objects
				if(Grid.IsInsideGrid((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + layer - 1) &&
				   Grid.TryGetTile((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + layer - 1, out tile) && 
				   !NotWalkableTileIndexes.Contains(tile) &&
				   !Grid.TryGetObject((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + layer - 1, out obj))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CanWalkClimbMoveBy(int x, int y, int layer, out int outX, out int outY, out int outLayer)
	{
		outX = x;
		outY = y;
		outLayer = layer;
		// Inside the world
		if(Grid.IsInsideGrid((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + layer))
		{
			// To an empty cell
			bool empty = false;
			short tile;
			GameObject obj;
			if(!Grid.TryGetTile((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + layer, out tile) &&
			   !Grid.TryGetObject((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + layer, out obj))
			{
				empty = true;
				// Climb down from cell
				if(!Grid.TryGetTile((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + layer - 1, out tile) && 
				   !Grid.TryGetObject((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + layer - 1, out obj) &&
				   !Grid.TryGetTile((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + layer, out tile) && 
				   !Grid.TryGetObject((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + layer, out obj))
				{
					// Change destination cell to move on top of original destination
					--outLayer;
				}
			}
			// Climb on top of cell
			else if(!Grid.TryGetTile((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + layer + 1, out tile) && 
			   !Grid.TryGetObject((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + layer + 1, out obj) &&
			   !Grid.TryGetTile((int)CellCoords.x, (int)CellCoords.y, (int)CellCoords.z + layer + 1, out tile) && 
			   !Grid.TryGetObject((int)CellCoords.x, (int)CellCoords.y, (int)CellCoords.z + layer + 1, out obj))
			{
				empty = true;
				// Change destination cell to move on top of original destination
				++outLayer;
			}


			// Only over walkable tiles and never over game objects
			if(empty && 
			   Grid.IsInsideGrid((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + outLayer - 1) &&
			   Grid.TryGetTile((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + outLayer - 1, out tile) && 
			   !NotWalkableTileIndexes.Contains(tile) &&
			   !Grid.TryGetObject((int)CellCoords.x + x, (int)CellCoords.y + y, (int)CellCoords.z + outLayer - 1, out obj))
			{
				return true;
			}
		}
		return false;
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Basic grid agent class that can be used as a base for objects that need to move inside the grid according to certain rules.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class GridAgent : MonoBehaviour
{
	/// <summary>
	/// The grid in which the agent will move.
	/// </summary>
	public RectangleGrid Grid;
	/// <summary>
	/// The cell coords of the agent in the grid.
	/// </summary>
	public GridPosition CellCoords;
	/// <summary>
	/// A list of the tiles over which the agent cannot move idenfied by their index in the <see cref="RenderingHandler"/>
	/// </summary>
	public List<int> NotWalkableTileIndexes = new List<int>();

	/// <summary>
	/// The sprite renderer for the agent.
	/// </summary>
	protected SpriteRenderer rend;

	///<summary>
	/// An enumeration of the posible movement directions in the horizontal plane (x, y)
	/// </summary>
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
	///<summary>
	/// An enumeration of the posible movement directions perpedicular to the horizontal plane (along the z/layer axis)
	/// </summary>
	public enum VerticalDirection
	{
		Up,
		Down,
		None
	}


	/// <summary>
	/// Initializes the agent by updating its transform and its depth sorting order according to its <see cref="CellCoords"/>>,
	/// stores a reference to the SpriteRenderer component attached to this object
	/// </summary>
	protected void Start ()
	{
		transform.position = new Vector3(CellCoords.X * Grid.CellWidth, CellCoords.Y * Grid.CellDepth + CellCoords.Layer * Grid.CellHeight, 0);
		rend = GetComponent<SpriteRenderer>();
		rend.sortingOrder = CellCoords.Layer - CellCoords.Y;
	}
	/// <summary>
	/// Tries to move the agent in the specified horizontal and vertical direction using the specified CanAgentMoveBy method
	/// to check whether the agent can move in that direction from it's current position.
	/// </summary>
	/// <param name="hDir">Horizontal dir.</param>
	/// <param name="vDir">Vertical dir.</param>
	/// <param name="CanMove">Method that will be used to check whether the agent can move in that direction from it's current position.</param>
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
			CellCoords += new GridPosition(outX, outY, outLayer);
			transform.position += new Vector3(outX * Grid.CellWidth, outY * Grid.CellDepth + outLayer * Grid.CellHeight, 0);
			rend.sortingOrder = CellCoords.Layer - CellCoords.Y;
		}
	}
	///<summary>
	/// Delegate definition for methods that can be passed to the <see cref="Move"/> method to define whether an agent is able to move from it's
	/// current position in the grid the cell that is in CellCoords + (x, y, layer). 
	/// This method should also set the outX, outY and outLayer values, which allow the method to modify where the agent will move to in this method.
	/// <returns><c>true</c> the agent is allowed to move; otherwise, <c>false</c>.</returns>
	/// <param name="x">X axis distance from the agent's position to the destination</param>
	/// <param name="y">Y axis distance from the agent's position to the destination</param>
	/// <param name="layer">Layer distance from the agent's position to the destination</param>
	/// <param name="outX">Modified x axis distance from the agent's position to the destination</param>
	/// <param name="outY">Modified y axis distance from the agent's position to the destination</param>
	/// <param name="outLayer">Modified layer distance from the agent's position to the destination</param>
	/// </summary>
	public delegate bool CanAgentMoveByDelegate(int x, int y, int layer, out int outX, out int outY, out int outLayer);

	/// <summary>
	/// Determines whether an agent that should move like a ghost can move by the specified x, y, layer.
	/// Specifically, this method only prevents the agent from going outside the boundaries of the grid.
	/// Sets outX, outY and outLayer, which will be used in the <see cref="Move"/> method to move the agent.
	/// </summary>
	/// <returns><c>true</c> if this instance can ghost move by the specified x, y, layer; otherwise, <c>false</c>.</returns>
	/// <param name="x">The x axis distance.</param>
	/// <param name="y">The y axis distance.</param>
	/// <param name="layer">The layer axis distance.</param>
	/// <param name="outX">The x axis distance that the agent will use if it can move.</param>
	/// <param name="outY">The y axis distance that the agent will use if it can move.</param>
	/// <param name="outLayer">The layer axis distance that the agent will use if it can move.</param>
	public bool CanGhostMoveBy(int x, int y, int layer, out int outX, out int outY, out int outLayer)
	{
		outX = x;
		outY = y;
		outLayer = layer;
		// Inside the world
		if(Grid.IsInsideGrid(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + layer))
		{
			return true;
		}
		return false;
	}
	/// <summary>
	/// Determines whether an agent that should move as if flying can move by the specified x, y, layer.
	/// Specifically, this method:
	/// 	- Prevents the agent from going outside the boundaries of the grid.
	/// 	- Only allows the agent to move to empty cells.
	/// Sets outX, outY and outLayer, which will be used in the <see cref="Move"/> method to move the agent.
	/// </summary>
	/// <returns><c>true</c> if this instance can ghost move by the specified x, y, layer; otherwise, <c>false</c>.</returns>
	/// <param name="x">The x axis distance.</param>
	/// <param name="y">The y axis distance.</param>
	/// <param name="layer">The layer axis distance.</param>
	/// <param name="outX">The x axis distance that the agent will use if it can move.</param>
	/// <param name="outY">The y axis distance that the agent will use if it can move.</param>
	/// <param name="outLayer">The layer axis distance that the agent will use if it can move.</param>
	public bool CanFlyMoveBy(int x, int y, int layer, out int outX, out int outY, out int outLayer)
	{
		outX = x;
		outY = y;
		outLayer = layer;
		// Inside the world
		if(Grid.IsInsideGrid(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + layer))
		{
			// Only to an empty cell
			short tile;
			GameObject obj;
			if(!Grid.TryGetTile(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + layer, out tile) &&
			   !Grid.TryGetObject(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + layer, out obj))
			{
				return true;
			}
		}
		return false;
	}
	/// <summary>
	/// Determines whether an agent that should move as if walking can move by the specified x, y, layer.
	/// Specifically, this method:
	/// 	- Prevents the agent from going outside the boundaries of the grid.
	/// 	- Only allows the agent to move to empty cells.
	/// 	- Only allows the agent to move to cells that have a tile below them and aren't in the NonWalkableTiles list (not a game object).
	/// Sets outX, outY and outLayer, which will be used in the <see cref="Move"/> method to move the agent.
	/// </summary>
	/// <returns><c>true</c> if this instance can ghost move by the specified x, y, layer; otherwise, <c>false</c>.</returns>
	/// <param name="x">The x axis distance.</param>
	/// <param name="y">The y axis distance.</param>
	/// <param name="layer">The layer axis distance.</param>
	/// <param name="outX">The x axis distance that the agent will use if it can move.</param>
	/// <param name="outY">The y axis distance that the agent will use if it can move.</param>
	/// <param name="outLayer">The layer axis distance that the agent will use if it can move.</param>
	public bool CanWalkStrictMoveBy(int x, int y, int layer, out int outX, out int outY, out int outLayer)
	{
		outX = x;
		outY = y;
		outLayer = layer;
		// Inside the world
		if(Grid.IsInsideGrid(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + layer))
		{
			// Only to an empty cell
			short tile;
			GameObject obj;
			if(!Grid.TryGetTile(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + layer, out tile) &&
			   !Grid.TryGetObject(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + layer, out obj))
			{
				// Only over walkable tiles and never over game objects
				if(Grid.IsInsideGrid(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + layer - 1) &&
				   Grid.TryGetTile(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + layer - 1, out tile) && 
				   !NotWalkableTileIndexes.Contains(tile) &&
				   !Grid.TryGetObject(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + layer - 1, out obj))
				{
					return true;
				}
			}
		}
		return false;
	}
	/// <summary>
	/// Determines whether an agent that should move as if walking or climbing can move by the specified x, y, layer.
	/// Specifically, this method:
	/// 	- Prevents the agent from going outside the boundaries of the grid.
	/// 	- Only allows the agent to move to empty cells.
	/// 	- Only allows the agent to move to cells that have a tile below them and aren't in the NonWalkableTiles list (not a game object).
	/// 	- Allows the agent to move one layer down or up while moving horizontally as long as it doesnt move through
	/// 	  non-empty cells (climbing up and down tiles).
	/// Sets outX, outY and outLayer, which will be used in the <see cref="Move"/> method to move the agent.
	/// </summary>
	/// <returns><c>true</c> if this instance can ghost move by the specified x, y, layer; otherwise, <c>false</c>.</returns>
	/// <param name="x">The x axis distance.</param>
	/// <param name="y">The y axis distance.</param>
	/// <param name="layer">The layer axis distance.</param>
	/// <param name="outX">The x axis distance that the agent will use if it can move.</param>
	/// <param name="outY">The y axis distance that the agent will use if it can move.</param>
	/// <param name="outLayer">The layer axis distance that the agent will use if it can move.</param>
	public bool CanWalkClimbMoveBy(int x, int y, int layer, out int outX, out int outY, out int outLayer)
	{
		outX = x;
		outY = y;
		outLayer = layer;
		// Inside the world
		if(Grid.IsInsideGrid(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + layer))
		{
			// To an empty cell
			bool empty = false;
			short tile;
			GameObject obj;
			if(!Grid.TryGetTile(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + layer, out tile) &&
			   !Grid.TryGetObject(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + layer, out obj))
			{
				empty = true;
				// Climb down from cell
				if(!Grid.TryGetTile(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + layer - 1, out tile) && 
				   !Grid.TryGetObject(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + layer - 1, out obj) &&
				   !Grid.TryGetTile(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + layer, out tile) && 
				   !Grid.TryGetObject(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + layer, out obj))
				{
					// Change destination cell to move on top of original destination
					--outLayer;
				}
			}
			// Climb on top of cell
			else if(!Grid.TryGetTile(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + layer + 1, out tile) && 
			   !Grid.TryGetObject(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + layer + 1, out obj) &&
			   !Grid.TryGetTile(CellCoords.X, CellCoords.Y, CellCoords.Layer + layer + 1, out tile) && 
			   !Grid.TryGetObject(CellCoords.X, CellCoords.Y, CellCoords.Layer + layer + 1, out obj))
			{
				empty = true;
				// Change destination cell to move on top of original destination
				++outLayer;
			}


			// Only over walkable tiles and never over game objects
			if(empty && 
			   Grid.IsInsideGrid(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + outLayer - 1) &&
			   Grid.TryGetTile(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + outLayer - 1, out tile) && 
			   !NotWalkableTileIndexes.Contains(tile) &&
			   !Grid.TryGetObject(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer + outLayer - 1, out obj))
			{
				return true;
			}
		}
		return false;
	}
}

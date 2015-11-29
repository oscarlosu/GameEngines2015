using UnityEngine;
using System.Collections;

/// <summary>
/// Demonstration of a possible way of using the GridAgent class to control a character with keyboard input and move inside the grid following 
/// certain rules.
/// </summary>
public class GridAgentTest : GridAgent
{
	///<summary>
	/// Enumerates the different rules by which this agent can move inside the grid.
	/// </summary>
	public enum MoveByDelegate
	{
		Ghost,
		Fly,
		WalkStrict,
		WalkClimb
	}
	/// <summary>
	/// The type the agent. <see cref="MoveByDelegate"/>
	/// </summary>
	public MoveByDelegate AgentType;

	/// <summary>
	/// If the GridAgent is used as a base class the GridAgent Start method should always be called.
	/// </summary>
	new void Start ()
	{
		base.Start();
	}
	
	/// <summary>
	/// Detects keyboard input and translates it into calls to the <see cref="GridAgent.Move"/> method with the appropriate movement directions.
	/// Also selects which delegate should be used to restrict the movement of the agent in the grid.
	/// </summary>
	void Update ()
	{
		HorizontalDirection hDir;
		VerticalDirection vDir;
		// Horizontal direction
		if(Input.GetKeyDown(KeyCode.UpArrow))
		{
			if(Input.GetKeyDown(KeyCode.LeftArrow))
			{
				hDir = HorizontalDirection.NorthWest;
			}
			else if(Input.GetKeyDown(KeyCode.RightArrow))
			{
				hDir = HorizontalDirection.NorthEast;
			}
			else
			{
				hDir = HorizontalDirection.North;
			}
		} 
		else if(Input.GetKeyDown(KeyCode.DownArrow))
		{
			if(Input.GetKeyDown(KeyCode.LeftArrow))
			{
				hDir = HorizontalDirection.SouthWest;
			}
			else if(Input.GetKeyDown(KeyCode.RightArrow))
			{
				hDir = HorizontalDirection.SouthEast;
			}
			else
			{
				hDir = HorizontalDirection.South;
			}
		}
		else if(Input.GetKeyDown(KeyCode.LeftArrow))
		{
			hDir = HorizontalDirection.West;
		}
		else if(Input.GetKeyDown(KeyCode.RightArrow))
		{
			hDir = HorizontalDirection.East;
		}
		else
		{
			hDir = HorizontalDirection.None;
		}
		// Vertical direction
		if(Input.GetKeyDown(KeyCode.W))
		{
			vDir = VerticalDirection.Up;
		}
		else if(Input.GetKeyDown(KeyCode.S))
		{
			vDir = VerticalDirection.Down;
		}
		else
		{
			vDir = VerticalDirection.None;
		}
		// Movement type
		CanAgentMoveByDelegate moveByDelegate;
		switch(AgentType)
		{
		case MoveByDelegate.Ghost:
			moveByDelegate = new CanAgentMoveByDelegate(CanGhostMoveBy);
			break;
		case MoveByDelegate.Fly:
			moveByDelegate = new CanAgentMoveByDelegate(CanFlyMoveBy);
			break;
		case MoveByDelegate.WalkStrict:
			moveByDelegate = new CanAgentMoveByDelegate(CanWalkStrictMoveBy);
			break;
		case MoveByDelegate.WalkClimb:
			moveByDelegate = new CanAgentMoveByDelegate(CanWalkClimbMoveBy);
			break;
		default:
			moveByDelegate = new CanAgentMoveByDelegate(CanGhostMoveBy);
			break;
		}
		// Try to move
		Move(hDir, vDir, moveByDelegate);
	}
}

using UnityEngine;
using System.Collections;

public class GridAgentTest : GridAgent
{
	public enum MoveByDelegate
	{
		Ghost,
		Fly,
		WalkStrict,
		WalkClimb
	}
	public MoveByDelegate AgentType;

	// Use this for initialization
	void Start ()
	{
		base.Start();
	}
	
	// Update is called once per frame
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

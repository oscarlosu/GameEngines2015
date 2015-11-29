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

	public AudioClip DiggingSoundFX;
	public AudioClip GoldSoundFX;
	public AudioClip MushroomSoundFX;

	public UnityEngine.UI.Text CollectedGoldGUI;

	public Sprite FacingRight;
	public Sprite FacingLeft;

	public int GoldCounter;
	public int MushroomCounter;

	private AudioSource audioPlayer;
	private SpriteRenderer rend;

	// Use this for initialization
	new void Start ()
	{
		base.Start ();
		audioPlayer = GetComponent<AudioSource>();
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
			rend.sprite = FacingLeft;
		}
		else if(Input.GetKeyDown(KeyCode.RightArrow))
		{
			hDir = HorizontalDirection.East;
			rend.sprite = FacingRight;
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
		Grid.TryGetTile(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer, out tile);
		if(NotWalkableTileIndexes.Contains (tile))
		{
			return false;
		}
		// Cant move above the MaxYIndex (sky)
		if(CellCoords.Y + y > MaxYIndex)
		{
			return false;
		}
		// Can remove grass and dirt
		if(tile == Grass || tile == Dirt)
		{
			Grid.Remove(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer);
			// Play digging sound
			audioPlayer.clip = DiggingSoundFX;
			audioPlayer.Play();
			return true;
		}
		// Can remove gold increasing the gold counter
		if(tile == Gold)
		{
			Grid.Remove(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer);
			++GoldCounter;
			CollectedGoldGUI.text = "Gold: " + GoldCounter;
			// Play gold mining sound
			audioPlayer.clip = GoldSoundFX;
			audioPlayer.Play();
			return true;
		}
		// Can eat mushrooms
		if(tile == Mushrooms)
		{
			Grid.Remove(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer);
			++MushroomCounter;
			// Play mushroom eating sound
			audioPlayer.clip = MushroomSoundFX;
			audioPlayer.Play();
			return true;
		}
		// Can move into empty tiles if over background ground or sky
		Grid.TryGetTile(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer - 2, out tile);
		if(tile == GroundBackground || tile == SkyBackground)
		{
			return true;
		}
		return false;
	}
}

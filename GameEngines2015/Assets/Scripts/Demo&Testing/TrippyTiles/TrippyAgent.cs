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

    public RenderingHandler RendHandler;
    public List<Sprite> AltDirtSprites;

    public UnityEngine.UI.Text CollectedGoldGUI;

    public Sprite FacingRight;
    public Sprite FacingLeft;

    public int GoldCounter;
    public int MushroomCounter;

    private AudioSource audioPlayer;
    private Camera cam;

    // Use this for initialization
    new void Start()
    {
        base.Start();
        audioPlayer = GetComponent<AudioSource>();
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        HorizontalDirection hDir = HorizontalDirection.None;
        VerticalDirection vDir = VerticalDirection.None;
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            hDir = HorizontalDirection.North;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            hDir = HorizontalDirection.South;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            hDir = HorizontalDirection.West;
            rend.sprite = FacingLeft;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            hDir = HorizontalDirection.East;
            rend.sprite = FacingRight;
        }
        Move(hDir, vDir, Action);
    }

    public bool Action(int x, int y, int layer, out int outX, out int outY, out int outLayer)
    {
        bool allowed = false;
        outX = x;
        outY = y;
        outLayer = layer;
        short tile;
        // Cant move outside of the grid
        if (!Grid.IsInsideGrid(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer))
        {
            return false;
        }
        // Cant move to cells occupied by non-walkable tiles
        Grid.TryGetTile(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer, out tile);
        if (NotWalkableTileIndexes.Contains(tile))
        {
            return false;
        }
        // Cant move above the MaxYIndex (sky)
        if (CellCoords.Y + y > MaxYIndex)
        {
            return false;
        }
        // Can remove grass and dirt
        if (tile == Grass || tile == Dirt)
        {
            Grid.Remove(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer);
            // Play digging sound
            audioPlayer.clip = DiggingSoundFX;
            audioPlayer.Play();
            allowed = true;
        }
        // Can remove gold increasing the gold counter
        if (tile == Gold)
        {
            Grid.Remove(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer);
            ++GoldCounter;
            CollectedGoldGUI.text = "Gold: " + GoldCounter;
            // Play gold mining sound
            audioPlayer.clip = GoldSoundFX;
            audioPlayer.Play();
            allowed = true;
        }
        // Can eat mushrooms
        if (tile == Mushrooms)
        {
            Grid.Remove(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer);
            ++MushroomCounter;
            Trippiness();
            // Play mushroom eating sound
            audioPlayer.clip = MushroomSoundFX;
            audioPlayer.Play();
            allowed = true;
        }
        // Can move into empty tiles if over background ground or sky
        Grid.TryGetTile(CellCoords.X + x, CellCoords.Y + y, CellCoords.Layer - 2, out tile);
        if (tile == GroundBackground || tile == SkyBackground)
        {
            allowed = true;
        }

        if (allowed)
        {
            if (cam.transform.position.y > 360 && y < 0)
            {

                cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y + y * Grid.CellDepth, cam.transform.position.z);
            }
            else if (y > 0)
            {

                cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y + y * Grid.CellDepth, cam.transform.position.z);
            }

        }

        return allowed;
    }

    void Trippiness()
    {
        if (MushroomCounter <= AltDirtSprites.Count)
        {
            Sprite[] newTiles = new Sprite[RendHandler.Tiles[Dirt].Length + 1];
            RendHandler.Tiles[Dirt].Tile.CopyTo(newTiles, 0);
            newTiles[RendHandler.Tiles[Dirt].Length] = AltDirtSprites[MushroomCounter - 1];
            RendHandler.Tiles[Dirt].Tile = newTiles;
        }
    }
}

using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// The rendering handler renders the assigned grid to the given camera (or the main camera, if none given).
/// It should have tiles for each possible short value in the grid in order to render it.
/// </summary>
public class RenderingHandler : MonoBehaviour
{
    /// <summary>
    /// The grid to render.
    /// </summary>
    public RectangleGrid HandledGrid;
    /// <summary>
    /// A list of all the tiles used in the grid.
    /// A SpriteList object can hold more than one sprite and if they do, they will be animated tiles.
    /// </summary>
    public List<SpriteList> Tiles = new List<SpriteList>();
    /// <summary>
    /// A list of indexes (from the tiles list) that are not obstructing the view.
    /// This is used, when a tile has transparent areas, for instance.
    /// When rendering tiles in this list, tiles below and behind the tile will always be rendered as well.
    /// </summary>
    public List<int> NonViewObstructingTiles = new List<int>();
    /// <summary>
    /// The buffers are how many tiles should be rendered extra around the camera.
    /// This is used to make sure that the camera will always have tiles rendered that are visible on the screen.
    /// </summary>
    public int BufferX, BufferY;
    /// <summary>
    /// The lighting mode is used to choose how shadows are handled in the engine.
    /// The layered lighting mode will tint each layer differently, the lowest layer the darkest. This can make tiles more distinguishable.
    /// The adjecent with sun mode will tint tiles that should be in the shadow of other surrounding tiles taking into account the direction of the sun.
    /// </summary>
    public LightingMode CurrentLightingMode;
    /// <summary>
    /// The time of day dictates how to colour-tint the tiles. This can be used to make the scene into a night scene or a morning/evening scene.
    /// Different multipliers can be adjusted to change the details of the tinting.
    /// </summary>
    public TimeOfDay SelectedTimeOfDay;
    /// <summary>
    /// These multipliers are used to change the colour-tint of tiles at different times of day (select the time of day you want).
    /// The x,y,z values correspond to multipliers for the red, green and blue values used to tint the tiles.
    /// </summary>
    public Vector3 MiddayColourMultipliers, MorningColourMultipliers, NightColourMultipliers;
    /// <summary>
    /// The max tint is used in the lighting system to make sure that the tiles cannot be tinted too much (become black, for instance).
    /// Having the max tint set to 1 means that tiles can become completely tinted. Having it set to 0.5 means that they can at most be half tinted.
    /// </summary>
    public float MaxTint;
    /// <summary>
    /// TintIncrease is used in lighting systems with additive tinting.
    /// For instance in the "adjecent with sun" lighting mode, each surrounding tile on the layer above will block light from the sun. Each block will increase the tint with this value.
    /// </summary>
    public float TintIncrease;
    /// <summary>
    /// This is the time interval at which animated tiles should switch to the next sprite in their animation.
    /// The time is in seconds, so if it is set to 1, animated tiles will switch sprite once every second.
    /// </summary>
    public float AnimationNextTime;
    /// <summary>
    /// The camera to use for rendering the grid. If this is not set, the rendering handler will try and find the main camera.
    /// </summary>
    public Camera Cam;

    private int lowestHiddenLayer;
    private Dictionary<int, Vector2> firstCell = new Dictionary<int, Vector2>();
    private int sizeX, sizeY;

    private float lastCameraSize;
    private float lastCameraX, lastCameraY;

    private int animationIteration;
    private Dictionary<GridPosition, SpriteRenderer> animatedTiles = new Dictionary<GridPosition, SpriteRenderer>();

    // Use this for initialization
    void Awake()
    {
        if (Cam == null)
        {
            Cam = Camera.main;

        }
        lastCameraSize = 0;
        Vector3 camPos = transform.TransformPoint(Cam.transform.position);
        lastCameraX = camPos.x;
        lastCameraY = camPos.y;
        // No layer is hidden by default (zero index)
        lowestHiddenLayer = HandledGrid.LayerCount;
        InvokeRepeating("RunAnimations", 0, AnimationNextTime);
    }

    private void RunAnimations()
    {
        // Animation iteration.
        animationIteration++;
        foreach (KeyValuePair<GridPosition, SpriteRenderer> entry in animatedTiles)
        {
            short tile;
            if (HandledGrid.TryGetTile((int)entry.Key.X, (int)entry.Key.Y, (int)entry.Key.Layer, out tile))
            {
                entry.Value.sprite = Tiles[tile][animationIteration % Tiles[tile].Length];
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Cam != null)
        {
            Vector3 camPos = transform.TransformPoint(Cam.transform.position);
            // Move with camera
            if (Mathf.Abs(lastCameraX - camPos.x) >= HandledGrid.CellWidth ||
                Mathf.Abs(lastCameraY - camPos.y) >= HandledGrid.CellDepth)
            {
                MoveUpdate();
                lastCameraX = camPos.x;
                lastCameraY = camPos.y;
            }
            // Zoom out
            if (lastCameraSize < Cam.orthographicSize)
            {
                ZoomLoad();
                lastCameraSize = Cam.orthographicSize;
            }
            // Zoom in
            if (lastCameraSize > Cam.orthographicSize)
            {
                ZoomUnload();
                lastCameraSize = Cam.orthographicSize;
            }
        }
    }

    /// <summary>
    /// This method will be called every time the camera has moved.
    /// It updates what is rendered on the screen depending on where it is placed in the scene view.
    /// </summary>
    private void MoveUpdate()
    {
        int countUnload = 0, countLoad = 0;
        for (int layer = 0; layer < HandledGrid.LayerCount; ++layer)
        {
            Vector2 oldFirstCell = UpdateFirstCellXYInLayer(layer);

            // Unload
            for (int y = (int)oldFirstCell.y; y < oldFirstCell.y + sizeY; ++y)
            {
                // We want to skip any updates outside the grid
                if (y < 0 || y >= HandledGrid.SizeY)
                {
                    continue;
                }
                for (int x = (int)oldFirstCell.x; x < oldFirstCell.x + sizeX; ++x)
                {
                    ++countUnload;
                    // Unload all the cells inside hidden layers
                    if (layer >= lowestHiddenLayer)
                    {
                        UnloadCell(x, y, layer);
                        continue;
                    }
                    // If current cell is inside the new viewport, skip to the other side of the camera
                    if (y >= firstCell[layer].y && y < firstCell[layer].y + sizeY &&
                       x >= firstCell[layer].x && x < firstCell[layer].x + sizeX)
                    {
                        x += sizeX - (int)Mathf.Abs(firstCell[layer].x - oldFirstCell.x);
                    }
                    if (x < oldFirstCell.x + sizeX)
                    {
                        UnloadCell(x, y, layer);
                    }
                }
            }
            // Load
            for (int y = (int)firstCell[layer].y; y < firstCell[layer].y + sizeY; ++y)
            {
                // Skip all the layers that are hidden
                if (layer >= lowestHiddenLayer)
                {
                    continue;
                }
                // We want to skip any updates outside the grid
                if (y < 0 || y >= HandledGrid.SizeY)
                {
                    continue;
                }
                for (int x = (int)firstCell[layer].x; x < firstCell[layer].x + sizeX; ++x)
                {
                    // If current cell is inside the old viewport, skip to the other side of the old camera
                    if (y >= (int)oldFirstCell.y && y < (int)oldFirstCell.y + sizeY &&
                       x >= (int)oldFirstCell.x && x < (int)oldFirstCell.x + sizeX)
                    {
                        x += sizeX - (int)Mathf.Abs(firstCell[layer].x - oldFirstCell.x);
                    }
                    if (x < firstCell[layer].x + sizeX)
                    {
                        LoadCell(x, y, layer);
                    }
                    ++countLoad;
                }
            }
        }
        Debug.Log("MoveUnload iterations count: " + countUnload);
        Debug.Log("MoveLoad iterations count: " + countLoad);
    }

    /// <summary>
    /// ZoomUpdate is called every time the camera's size is changed.
    /// It updated what is rendered on the screen depending on where it is placed in the scene and how much of the grid should be inside the view.
    /// </summary>
    public void ZoomUpdate()
    {
        int oldSizeX = sizeX;
        int oldSizeY = sizeY;
        ZoomUnload();
        sizeX = oldSizeX;
        sizeY = oldSizeY;
        ZoomLoad();
    }

    private void ZoomLoad()
    {
        int oldSizeX = sizeX;
        int oldSizeY = sizeY;
        UpdateSizeXY();
        int count = 0;
        for (int layer = 0; layer < HandledGrid.LayerCount; ++layer)
        {
            Vector2 oldFirstCell = UpdateFirstCellXYInLayer(layer);
            // Skip all the layers that are hidden
            if (layer >= lowestHiddenLayer)
            {
                continue;
            }
            for (int y = (int)firstCell[layer].y; y < firstCell[layer].y + sizeY; ++y)
            {
                // We want to skip any updates outside the grid
                if (y < 0 || y >= HandledGrid.SizeY)
                {
                    continue;
                }
                for (int x = (int)firstCell[layer].x; x < firstCell[layer].x + sizeX; ++x)
                {
                    // If current cell is inside the old viewport, skip to the other side of the old camera
                    if (y >= (int)oldFirstCell.y && y < (int)oldFirstCell.y + oldSizeY && x == (int)oldFirstCell.x)
                    {
                        x += oldSizeX;
                    }
                    LoadCell(x, y, layer);
                    ++count;
                }
            }
        }
        Debug.Log("ZoomLoad iterations count: " + count);
    }

    private void ZoomUnload()
    {
        int oldSizeX = sizeX;
        int oldSizeY = sizeY;
        UpdateSizeXY();
        int count = 0;
        for (int layer = 0; layer < HandledGrid.LayerCount; ++layer)
        {
            Vector2 oldFirstCell = UpdateFirstCellXYInLayer(layer);
            for (int y = (int)oldFirstCell.y; y < oldFirstCell.y + oldSizeY; ++y)
            {
                // We want to skip any updates outside the grid
                if (y < 0 || y >= HandledGrid.SizeY)
                {
                    continue;
                }
                for (int x = (int)oldFirstCell.x; x < oldFirstCell.x + oldSizeX; ++x)
                {
                    ++count;
                    // Unload all the cells inside hidden layers
                    if (layer >= lowestHiddenLayer)
                    {
                        UnloadCell(x, y, layer);
                        continue;
                    }
                    // If current cell is inside the new viewport, skip to the other side of the camera
                    if (y >= firstCell[layer].y && y < firstCell[layer].y + sizeY && x == firstCell[layer].x)
                    {
                        x += sizeX;
                    }
                    UnloadCell(x, y, layer);
                }
            }
        }
        Debug.Log("ZoomUnload iterations count: " + count);
    }

    /// <summary>
    /// This updates the cell on screen in the given position in the grid.
    /// If it should be visible, it will load the cell and render it.
    /// If it should not be visible, it will unload it and stop rendering it.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="layer">The layer.</param>
    public void UpdateCell(int x, int y, int layer)
    {
        if (!LoadCell(x, y, layer))
        {
            UnloadCell(x, y, layer);
        }
    }

    public bool LoadCell(int x, int y, int layer)
    {
        // Reveal cell
        short tile;
        if (HandledGrid.TryGetTile(x, y, layer, out tile))
        {
            if (IsCellVisible(x, y, layer))
            {
                GameObject obj = GameObjectPoolHandler.Instance.GetPoolObject(new Vector3(x, y, layer));
                // Move renderer to corrent position
                obj.transform.position = new Vector3(x * HandledGrid.CellWidth, y * HandledGrid.CellDepth + layer * HandledGrid.CellHeight, 0);
                // Update the sprite and the z-depth
                SpriteRenderer rend = obj.GetComponent<SpriteRenderer>();
                if (rend != null)
                {
                    rend.sprite = Tiles[tile][animationIteration % Tiles[tile].Length];
                    rend.sortingOrder = layer - y;
                    // Tint
                    /*float tintVal = Mathf.Lerp (1 - MaxTint, 1, layer / (float)HandledGrid.LayerCount);*/ // Old layer-based lighting system.
                    rend.color = GetTintColour(x, y, layer);
                    if (Tiles[tile].Length > 1 && !animatedTiles.ContainsKey(new GridPosition(x, y, layer)))
                    {
                        animatedTiles.Add(new GridPosition(x, y, layer), rend);
                    }
                }
                // Update adjacent cells that might now be hidden
                // Cell below
                if (!IsCellVisible(x, y, layer - 1))
                {
                    GameObjectPoolHandler.Instance.DisablePoolObject(new Vector3(x, y, layer - 1));
                }
                // Cell behind
                if (!IsCellVisible(x, y + 1, layer))
                {
                    GameObjectPoolHandler.Instance.DisablePoolObject(new Vector3(x, y + 1, layer));
                }
                return true;
            }
        }
        return false;
    }

    public void UnloadCell(int x, int y, int layer)
    {
        animatedTiles.Remove(new GridPosition(x, y, layer));
        if (GameObjectPoolHandler.Instance.DisablePoolObject(new Vector3(x, y, layer)))
        {
            // Update adjacent cells that might now be visible
            // Cell below
            LoadCell(x, y, layer - 1);
            // Cell behind
            LoadCell(x, y + 1, layer);
        }
    }

    /// <summary>
    /// Returns the tint colour depending on the lighting mode selected.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="layer">The layer.</param>
    /// <returns>The tint colour.</returns>
    private Color GetTintColour(int x, int y, int layer)
    {
        // Lighting is coming from up-left.
        float tintVal = 1;
        switch (CurrentLightingMode)
        {
            case LightingMode.LayerBased:
                tintVal = Mathf.Lerp(1 - MaxTint, 1, layer / (float)HandledGrid.LayerCount);
                break;
            case LightingMode.AdjecentWithSun:
                tintVal = 1;
                if (layer < HandledGrid.LayerCount - 1)
                {
                    if (!HandledGrid.IsCellFree(x - 1, y, layer + 1))
                    {
                        tintVal -= TintIncrease;
                    }
                    if (!HandledGrid.IsCellFree(x - 1, y + 1, layer + 1))
                    {
                        tintVal -= TintIncrease;
                    }
                    if (!HandledGrid.IsCellFree(x, y + 1, layer + 1))
                    {
                        tintVal -= TintIncrease;
                    }
                }
                break;
        }
        // Make sure nothing becomes black.
        if (tintVal < MaxTint) tintVal = MaxTint;
        float tintValR, tintValG, tintValB;

        // If midday, just add the tint value as is to all colours.
        tintValR = tintVal * MiddayColourMultipliers.x;
        tintValG = tintVal * MiddayColourMultipliers.y;
        tintValB = tintVal * MiddayColourMultipliers.z;
        if (SelectedTimeOfDay == TimeOfDay.Morning)
        {
            tintValR = tintVal * MorningColourMultipliers.x;
            tintValG = tintVal * MorningColourMultipliers.y;
            tintValB = tintVal * MorningColourMultipliers.z;
        }
        if (SelectedTimeOfDay == TimeOfDay.Night)
        {
            tintValR = tintVal * NightColourMultipliers.x;
            tintValG = tintVal * NightColourMultipliers.y;
            tintValB = tintVal * NightColourMultipliers.z;
        }
        return new Color(tintValR, tintValG, tintValB);
    }

    /// <summary>
    /// Hides all the layers from the given layer and above.
    /// </summary>
    /// <param name="index">The layer to start hiding from.</param>
	public void HideFromLayer(int index)
    {
        index = Mathf.Clamp(index, 0, HandledGrid.LayerCount);
        lowestHiddenLayer = index;
        ZoomUpdate();
    }

    /// <summary>
    /// Find the first cell in the given layer.
    /// </summary>
    /// <param name="layer">The layer.</param>
    /// <returns>A Vector2 with the coordinates of the first cell in this layer.</returns>
    private Vector2 UpdateFirstCellXYInLayer(int layer)
    {
        Vector3 camPos = transform.TransformPoint(Cam.transform.position);
        float camHalfWidth = Cam.aspect * Cam.orthographicSize;
        int firstX = Mathf.FloorToInt((camPos.x - camHalfWidth) / HandledGrid.CellWidth) - BufferX;
        int firstY = Mathf.FloorToInt((camPos.y - Cam.orthographicSize - HandledGrid.CellHeight * (layer)) / HandledGrid.CellDepth) - BufferY;
        // If there is an old value for that layer, return it
        Vector2 old;
        if (firstCell.ContainsKey(layer))
        {
            old = firstCell[layer];
            firstCell[layer] = new Vector2(firstX, firstY);
        }
        // If there wasn't an old value for the layer, return the new value
        else
        {
            old = new Vector2(firstX, firstY);
            firstCell[layer] = old;
        }
        return old;
    }

    private void UpdateSizeXY()
    {
        float camHalfWidth = Cam.aspect * Cam.orthographicSize;
        sizeX = Mathf.CeilToInt(2 * camHalfWidth / HandledGrid.CellWidth) + 2 * BufferX;
        sizeY = Mathf.CeilToInt(2 * Cam.orthographicSize / HandledGrid.CellDepth) + 2 * BufferY;
    }


    /// <summary>
    /// Determines whether the cell at the specified position is available.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="layer">The layer.</param>
    /// <returns>True if the cell is available.</returns>
    private bool IsCellVisible(int x, int y, int layer)
    {
        // If the requested layer is not part of the grid, return false
        if (layer < 0 || layer >= HandledGrid.LayerCount)
        {
            return false;
        }
        // If the cell is within a hidden layer, it is not visible
        if (layer >= lowestHiddenLayer)
        {
            return false;
        }
        // Is the cell within the viewport
        UpdateFirstCellXYInLayer(layer);
        UpdateSizeXY();
        if (firstCell[layer].x <= x && firstCell[layer].y <= y && firstCell[layer].x + sizeX > x && firstCell[layer].y + sizeY > y)
        {
            // Is the cell hidden by other cells
            short tileFront, tileAbove;
            // If there are tiles above and in front and they are view-obstructing, the cell is not visible
            // and they are not in a hidden layer
            if (HandledGrid.TryGetTile(x, y - 1, layer, out tileFront) && HandledGrid.TryGetTile(x, y, layer + 1, out tileAbove) &&
               !NonViewObstructingTiles.Contains(tileFront) && !NonViewObstructingTiles.Contains(tileAbove) &&
               layer + 1 < lowestHiddenLayer)
            {
                return false;
            }
            return true;
        }
        // If the cell is outside the viewport, it isn't visible
        return false;
    }

    /// <summary>
    /// Lighting modes.
    /// </summary>
    public enum LightingMode
    {
        LayerBased, AdjecentWithSun
    }

    /// <summary>
    /// Times of day.
    /// </summary>
    public enum TimeOfDay
    {
        Midday, Morning, Night
    }
}

[Serializable]
public class SpriteList
{
    public Sprite[] Tile;

    public Sprite this[int i]
    {
        get { return Tile[i]; }
        set { Tile[i] = value; }
    }

    public int Length
    {
        get { return Tile.Length; }
    }
}

using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class RenderingHandler : MonoBehaviour
{
    /// <summary>
    /// The handled grid.
    /// </summary>
    public RectangleGrid HandledGrid;
    //public GameObjectPoolHandler RendererPool;
    public List<SpriteList> Tiles = new List<SpriteList>();
    public List<int> NonViewObstructingTiles = new List<int>();

    public bool RenderHidden;
    public int BufferX, BufferY;
    public LightingMode CurrentLightingMode;
    public TimeOfDay SelectedTimeOfDay;
    public Vector3 MiddayColourMultipliers, MorningColourMultipliers, NightColourMultipliers;
    public float MaxTint;
    public float TintIncrease;

    public int lowestHiddenLayer;
    public float AnimationNextTime;

    private Dictionary<int, Vector2> firstCell = new Dictionary<int, Vector2>();
    private int sizeX, sizeY;

    private float lastCameraSize;
    private float lastCameraX, lastCameraY;
	public Camera cam;

    private int animationIteration;
    private Dictionary<GridPosition, SpriteRenderer> animatedTiles = new Dictionary<GridPosition, SpriteRenderer>();

    // Use this for initialization
    void Awake()
    {
		if (cam == null)
		{
			cam = Camera.main;

		}
		lastCameraSize = 0;
		Vector3 camPos = transform.TransformPoint(cam.transform.position);
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
        if (cam != null)
        {
			Vector3 camPos = transform.TransformPoint(cam.transform.position);
            // Move with camera
			if (Mathf.Abs(lastCameraX - camPos.x) > HandledGrid.CellWidth ||
			    Mathf.Abs(lastCameraY - camPos.y) > HandledGrid.CellDepth)
            {
                MoveUpdate();
				lastCameraX = camPos.x;
				lastCameraY = camPos.y;
            }
            // Zoom out
            if (lastCameraSize < cam.orthographicSize)
            {
                ZoomLoad();
                lastCameraSize = cam.orthographicSize;
            }
            // Zoom in
            if (lastCameraSize > cam.orthographicSize)
            {
                ZoomUnload();
                lastCameraSize = cam.orthographicSize;
            }
        }
    }

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
		UpdateSizeXY ();
		int count = 0;
		for(int layer = 0; layer < HandledGrid.LayerCount; ++layer)
		{
			Vector2 oldFirstCell = UpdateFirstCellXYInLayer (layer);
			// Skip all the layers that are hidden
			if(layer >= lowestHiddenLayer)
			{
				continue;
			}
			for(int y = (int)firstCell[layer].y; y < firstCell[layer].y + sizeY; ++y)
			{
				// We want to skip any updates outside the grid
				if(y < 0 || y >= HandledGrid.SizeY)
				{
					continue;
				}
				for(int x = (int)firstCell[layer].x; x < firstCell[layer].x + sizeX; ++x)
				{
					// If current cell is inside the old viewport, skip to the other side of the old camera
					if(y >= (int)oldFirstCell.y && y < (int)oldFirstCell.y + oldSizeY && x == (int)oldFirstCell.x)
					{
						x += oldSizeX;
					}
					LoadCell(x, y, layer);
					++count;
				}
			}			
		}
		Debug.Log ("ZoomLoad iterations count: " + count);
    }

	private void ZoomUnload()
	{
		int oldSizeX = sizeX;
		int oldSizeY = sizeY;
		UpdateSizeXY ();
		int count = 0;
		for(int layer = 0; layer < HandledGrid.LayerCount; ++layer)
		{
			Vector2 oldFirstCell = UpdateFirstCellXYInLayer(layer);
			for(int y = (int)oldFirstCell.y; y < oldFirstCell.y + oldSizeY; ++y)
			{
				// We want to skip any updates outside the grid
				if(y < 0 || y >= HandledGrid.SizeY)
				{
					continue;
				}
				for(int x = (int)oldFirstCell.x; x < oldFirstCell.x + oldSizeX; ++x)
				{
					++count;
					// Unload all the cells inside hidden layers
					if(layer >= lowestHiddenLayer)
					{
						UnloadCell(x, y, layer);
						continue;
					}
					// If current cell is inside the new viewport, skip to the other side of the camera
					if(y >= firstCell[layer].y && y < firstCell[layer].y + sizeY && x == firstCell[layer].x)
					{
						x += sizeX;
					}
					UnloadCell(x, y, layer);
				}
			}		
		}
		Debug.Log ("ZoomUnload iterations count: " + count);
	}

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
<<<<<<< HEAD
        animatedTiles.Remove(new GridPosition(x, y, layer));
        if (RendererPool.DisablePoolObject(new Vector3(x, y, layer)))
=======
        animatedTiles.Remove(new Vector3(x, y, layer));
		if (GameObjectPoolHandler.Instance.DisablePoolObject(new Vector3(x, y, layer)))
>>>>>>> 17415c43f90cacf0efd1297949574f1569d33e21
        {
            // Update adjacent cells that might now be visible
            // Cell below
            LoadCell(x, y, layer - 1);
            // Cell behind
            LoadCell(x, y + 1, layer);
        }
    }

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

	public void HideFromLayer(int index)
	{
		index = Mathf.Clamp(index, 0, HandledGrid.LayerCount);
		lowestHiddenLayer = index;
		ZoomUpdate();
	}

    private Vector2 UpdateFirstCellXYInLayer(int layer)
    {
		Vector3 camPos = transform.TransformPoint(cam.transform.position);
        float camHalfWidth = cam.aspect * cam.orthographicSize;
		int firstX = Mathf.FloorToInt((camPos.x - camHalfWidth) / HandledGrid.CellWidth) - BufferX;
		int firstY = Mathf.FloorToInt((camPos.y - cam.orthographicSize - HandledGrid.CellHeight * (layer)) / HandledGrid.CellDepth) - BufferY;
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
        float camHalfWidth = cam.aspect * cam.orthographicSize;
        sizeX = Mathf.CeilToInt(2 * camHalfWidth / HandledGrid.CellWidth) + 2 * BufferX;
        sizeY = Mathf.CeilToInt(2 * cam.orthographicSize / HandledGrid.CellDepth) + 2 * BufferY;
    }
	
	private bool IsCellVisible(int x, int y, int layer)
	{
		// If the requested layer is not part of the grid, return false
		if(layer < 0 || layer >= HandledGrid.LayerCount)
		{
			return false;
		}
		// If the cell is within a hidden layer, it is not visible
		if(layer >= lowestHiddenLayer)
		{
			return false;
		}
		// Is the cell within the viewport
		UpdateFirstCellXYInLayer (layer);
		UpdateSizeXY ();
		if(firstCell[layer].x <= x && firstCell[layer].y <= y && firstCell[layer].x + sizeX > x && firstCell[layer].y + sizeY > y)
		{
			// Is the cell hidden by other cells
			short tileFront, tileAbove;
			// If there are tiles above and in front and they are view-obstructing, the cell is not visible
			// and they are not in a hidden layer
			if(HandledGrid.TryGetTile(x, y - 1, layer, out tileFront) && HandledGrid.TryGetTile(x, y, layer + 1, out tileAbove) &&
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

    public enum LightingMode
    {
        LayerBased, AdjecentWithSun
    }

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

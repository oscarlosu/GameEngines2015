using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Stopwatch = System.Diagnostics.Stopwatch;
using System.Linq;
using System.IO;
using System.Text;
using UnityEditor;

/// <summary>
/// This class represents a grid that can hold tiles and game objects.
/// </summary>
public class RectangleGrid : MonoBehaviour
{
	/// <summary>
	/// The width of the cells.
	/// </summary>
	public int CellWidth;
	/// <summary>
	/// The depth of the cells.
	/// </summary>
	public int CellDepth;
	/// <summary>
	/// The height of the cell.
	/// </summary>
	public int CellHeight;
	/// <summary>
	/// The rendering handler. This class takes care of rendering only the tiles that are inside the camera view at any given point.
	/// </summary>
    public RenderingHandler RendHandler;
	/// <summary>
	/// The actual grid that holds the tiles
	/// </summary>
    public List<short[,]> grid = new List<short[,]>();
	/// <summary>
	/// The game objects are stored in a dictionary using their position in the grid for rapid access.
	/// </summary>
    private Dictionary<GridPosition, GameObject> gameObjects = new Dictionary<GridPosition, GameObject>();
	/// <summary>
	/// Gets the layer count.
	/// </summary>
	/// <value>The layer count.</value>
    public int LayerCount
    {
        get
        {
            return grid.Count;
        }
    }
	/// <summary>
	/// Gets the size of the grid in the x axis.
	/// </summary>
	/// <value>The size x.</value>
    public int SizeX { get; private set; }
	/// <summary>
	/// Gets the size of the grid in the y axis.
	/// </summary>
	/// <value>The size y.</value>
    public int SizeY { get; private set; }

	/// <summary>
	/// Determines whether this instance is initialized.
	/// </summary>
	/// <returns><c>true</c> if this instance is initialized; otherwise, <c>false</c>.</returns>
    private bool IsInitialized()
    {
        return grid.Count > 0;
    }
	/// <summary>
	/// Determines whether the given cell coords are inside the grid.
	/// </summary>
	/// <returns><c>true</c> if the cell is inside the grid; otherwise, <c>false</c>.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="layer">The layer.</param>
    public bool IsInsideGrid(int x, int y, int layer)
    {
        return layer >= 0 && x >= 0 && y >= 0 && layer < grid.Count && x < grid[layer].GetLength(0) && y < grid[layer].GetLength(1);
    }
	/// <summary>
	/// Makes the rendering handler update all the cells in the given layer.
	/// </summary>
	/// <param name="layerIndex">Layer index.</param>
    private void UpdateRendererLayer(int layerIndex)
    {
        for (int x = 0; x < grid[layerIndex].GetLength(0); ++x)
        {
            for (int y = 0; y < grid[layerIndex].GetLength(1); ++y)
            {
                RendHandler.UpdateCell(x, y, layerIndex);
            }
        }
    }
	/// <summary>
	/// Tries to the get a tile from the grid at the given cell coordinates.
	/// </summary>
	/// <returns><c>true</c>, if there was a tile in the cell, <c>false</c> otherwise.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="layer">The layer.</param>
	/// <param name="tile">The tile at specified coordinates.</param>
    public bool TryGetTile(int x, int y, int layer, out short tile)
    {
        if (IsInsideGrid(x, y, layer) && grid[layer][x, y] != -1)
        {
            tile = grid[layer][x, y];
            return true;
        }
        else
        {
            tile = -1;
            return false;
        }
    }
	/// <summary>
	/// Tries to the get a game object from the grid at the specified coordinates.
	/// </summary>
	/// <returns><c>true</c>, if get object was tryed, <c>false</c> otherwise.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="layer">The layer.</param>
	/// <param name="obj">The Game Object at the specified coordinates.</param>
    public bool TryGetObject(int x, int y, int layer, out GameObject obj)
    {
        return gameObjects.TryGetValue(new GridPosition(x, y, layer), out obj);
    }
	/// <summary>
	/// Determines whether the grid has a tile or an object in the specified cell.
	/// </summary>
	/// <returns><c>true</c> if the grid doesnt have a tile or an object in the cell; otherwise, <c>false</c>.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="layer">The layer.</param>
    public bool IsCellFree(int x, int y, int layer)
    {
        short tile;
		GameObject obj;
		return !TryGetTile(x, y, layer, out tile) && !TryGetObject(x, y, layer, out obj);
    }
	/// <summary>
	/// Place the specified Game Object at the specified cell in the grid.
	/// </summary>
	/// <param name="obj">Game Object to be placed in the grid.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="layer">The layer.</param>
    public void Place(GameObject obj, int x, int y, int layer)
    {
        // Clear the dest cell
        Remove(x, y, layer);
        // Add to go dictionary
        gameObjects.Add(new GridPosition(x, y, layer), obj);
        // Set world position and z-depth
        obj.transform.position = new Vector3(x * CellWidth, y * CellDepth + layer * CellHeight, 0);
        SpriteRenderer rend = obj.GetComponent<SpriteRenderer>();
        if (rend != null)
        {
            rend.sortingOrder = layer - y;
        }
    }
	/// <summary>
	/// Place the specified tile at the specified cell in the grid. Removes anything that was at the given cell before.
	/// </summary>
	/// <param name="tile">The tile to be placed in the grid.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="layer">The layer.</param>
    public void Place(short tile, int x, int y, int layer)
    {
        // Clear dest cell
        Remove(x, y, layer);
        // Add to the grid
	    if (x >= 0 && x < SizeX && y >= 0 && y < SizeY && layer >= 0 && layer < LayerCount)
	    {
	        grid[layer][x, y] = tile;
	        RendHandler.UpdateCell(x, y, layer);
	    }
    }
	/// <summary>
	/// Moves the content of the specified source cell to the specified destination cell. Removes anything at the given destiantion cell.
	/// </summary>
	/// <param name="sourceX">Source x coordinate.</param>
	/// <param name="sourceY">Source y coordiante.</param>
	/// <param name="sourceLayer">Source layer.</param>
	/// <param name="destX">Destination x coordinate.</param>
	/// <param name="destY">Destination y coordinate.</param>
	/// <param name="destLayer">Destination layer.</param>
    public void Move(int sourceX, int sourceY, int sourceLayer, int destX, int destY, int destLayer)
    {
        GridPosition sourcePos = new GridPosition(sourceX, sourceY, sourceLayer);
        GameObject obj;
        if (gameObjects.TryGetValue(sourcePos, out obj))
        {
            Place(obj, destX, destY, destLayer);
            // Clear the source cell, but dont destroy the object
            gameObjects.Remove(sourcePos);
        }
        else
        {
            Place(grid[sourceLayer][sourceX, sourceY], destX, destY, destLayer);
            // Clear the source cell
            Remove(sourceX, sourceY, sourceLayer);
        }
    }
	/// <summary>
	/// Remove the any tile or Game Object held by the grid at specified cell.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="layer">The layer.</param>
    public void Remove(int x, int y, int layer)
    {
        GridPosition pos = new GridPosition(x, y, layer);
        GameObject obj;
        if (gameObjects.TryGetValue(pos, out obj))
        {
            // Remove reference from dictionary
            gameObjects.Remove(pos);
            // Remove object from scene
            Destroy(obj);
        }
        else if (IsInsideGrid(x, y, layer))
        {
            grid[layer][x, y] = -1;
            RendHandler.UpdateCell(x, y, layer);
            //RendHandler.UnloadCell(x, y, layer);
        }
    }
	/// <summary>
	/// Swap the content of the specified cells.
	/// </summary>
	/// <param name="x1">The first x coordinate.</param>
	/// <param name="y1">The first y coordinate.</param>
	/// <param name="layer1">The first layer.</param>
	/// <param name="x2">The second x coordinate.</param>
	/// <param name="y2">The second y coordinate.</param>
	/// <param name="layer2">The second layer.</param>
    public void Swap(int x1, int y1, int layer1, int x2, int y2, int layer2)
    {
        // Copy and clear x2, y2, layer2
        GridPosition pos2 = new GridPosition(x2, y2, layer2);
        GameObject obj = null;
        short tile;
        if (gameObjects.TryGetValue(pos2, out obj))
        {
            // Remove reference from dictionary
            gameObjects.Remove(pos2);
            // Move x1, y1, layer1 to x2, y2, layer2
            Move(x1, y1, layer1, x2, y2, layer2);
            // Place copy at x1, y1, layer1
            Place(obj, x1, y1, layer1);
        }
        else if (IsInsideGrid(x2, y2, layer2))
        {
            // Copy and clear x2, y2, layer2
            tile = grid[layer2][x2, y2];
            grid[layer2][x2, y2] = -1;
            // Move x1, y1, layer1 to x2, y2, layer2
            Move(x1, y1, layer1, x2, y2, layer2);
            // Place copy at x1, y1, layer1
            Place(tile, x1, y1, layer1);
        }
    }
	/// <summary>
	/// Sets the size of the grid.
	/// </summary>
	/// <param name="nX">Number of cells in the x axis.</param>
	/// <param name="nY">Number of cells iun the y axis.</param>
	/// <param name="nLayers">Number of layers.</param>
    public void SetGridSize(int nX, int nY, int nLayers)
    {
        SizeX = nX;
        SizeY = nY;
        for (int layer = 0; layer < nLayers; layer++)
        {
            // If the layer already exists we want to resize it
            if (layer < grid.Count)
            {
                if (layer < nLayers)
                {
                    // Resize x, y
                    short[,] resizedLayer = new short[nX, nY];
                    int maxX = Mathf.Max(nX, grid[layer].GetLength(0));
                    int maxY = Mathf.Max(nY, grid[layer].GetLength(1));
                    // Copy to resized layer or delete
                    for (int y = 0; y < maxY; y++)
                    {
                        for (int x = 0; x < maxX; x++)
                        {
                            if (x < nX && y < nY)
                            {
                                if (x < grid[layer].GetLength(0) && y < grid[layer].GetLength(1))
                                {
                                    resizedLayer[x, y] = grid[layer][x, y];
                                }
                                else
                                {
                                    resizedLayer[x, y] = -1;
                                }

                            }
                            else
                            {
                                Remove(x, y, layer);
                            }
                        }
                    }
                    // Reassign layer
                    grid[layer] = resizedLayer;
                }
            }
            // If the layer doesnt exist and we want to create it
            else
            {
                // Add layer with width and depth size
                short[,] newLayer = new short[nX, nY];
                grid.Add(newLayer);
                // Initialize layer cells to default/empty
                for (int y = 0; y < nY; y++)
                {
                    for (int x = 0; x < nX; x++)
                    {
                        newLayer[x, y] = -1;
                    }
                }
            }
        }
        // Remove excess layers
        if (grid.Count > nLayers)
        {
            for (int layer = nLayers; layer < grid.Count; layer++)
            {
                for (int y = 0; y < grid[layer].GetLength(1); y++)
                {
                    for (int x = 0; x < grid[layer].GetLength(0); x++)
                    {
                        Remove(x, y, layer);
                    }
                }
            }
            grid.RemoveRange(nLayers, grid.Count - nLayers);
        }
        RendHandler.HideFromLayer(LayerCount);
    }
	/// <summary>
	/// Fills all the cells in between the given from and to cells (both inclusive) with the specified tile.
	/// </summary>
	/// <param name="tile">The tile that will be placed in the specified interval.</param>
	/// <param name="fromX">Starting x coordinate.</param>
	/// <param name="fromY">Starting y coordinate.</param>
	/// <param name="fromLayer">Starting layer.</param>
	/// <param name="toX">End x coordinate.</param>
	/// <param name="toY">End y coordinate.</param>
	/// <param name="toLayer">End layer.</param>
    public void FillRect(short tile, int fromX, int fromY, int fromLayer, int toX, int toY, int toLayer)
    {
        if (IsInitialized() && IsInsideGrid(fromX, fromY, fromLayer) && IsInsideGrid(toX, toY, toLayer))
        {
            // Find min/max of x, y and layer.
            int minLayer = Mathf.Min(fromLayer, toLayer);
            int maxLayer = Mathf.Max(fromLayer, toLayer);
            int minX = Mathf.Min(fromX, toX);
            int maxX = Mathf.Max(fromX, toX);
            int minY = Mathf.Min(fromY, toY);
            int maxY = Mathf.Max(fromY, toY);
            // Loop over all cells and place the tile.
            for (int layerIndex = minLayer; layerIndex <= maxLayer; layerIndex++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        Place(tile, x, y, layerIndex);
                    }
                }
            }
        }
    }
	/// <summary>
	/// Removes all the cells in between the given from and to cells (both inclusive).
	/// </summary>
	/// <param name="fromX">Starting x coordinate.</param>
	/// <param name="fromY">Starting y coordinate.</param>
	/// <param name="fromLayer">From layer.</param>
	/// <param name="toX">End x coordinate.</param>
	/// <param name="toY">End y coordinate.</param>
	/// <param name="toLayer">End layer.</param>
    public void RemoveRect(int fromX, int fromY, int fromLayer, int toX, int toY, int toLayer)
    {
        if (IsInitialized() && IsInsideGrid(fromX, fromY, fromLayer) && IsInsideGrid(toX, toY, toLayer))
        {
            int minLayer = Mathf.Min(fromLayer, toLayer);
            int maxLayer = Mathf.Max(fromLayer, toLayer);
            int minX = Mathf.Min(fromX, toX);
            int maxX = Mathf.Max(fromX, toX);
            int minY = Mathf.Min(fromY, toY);
            int maxY = Mathf.Max(fromY, toY);
            // Loop over all cells and remove anything on it.
            for (int layerIndex = minLayer; layerIndex <= maxLayer; layerIndex++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        Remove(x, y, layerIndex);
                    }
                }
            }
        }
    }
	/// <summary>
	/// Moves all the cells in the specified source interval to the specified destination interval in the grid.
	/// </summary>
	/// <param name="sourceFromX">Source starting x coordinate.</param>
	/// <param name="sourceFromY">Source starting y coordinate.</param>
	/// <param name="sourceFromLayer">Source starting layer.</param>
	/// <param name="sourceToX">Source end x coordinate.</param>
	/// <param name="sourceToY">Source end y coordinate.</param>
	/// <param name="sourceToLayer">Source to layer.</param>
	/// <param name="destFromX">Destination starting x coordinate.</param>
	/// <param name="destFromY">Destination starting y coordinate.</param>
	/// <param name="destFromLayer">Destination starting layer.</param>
    public void MoveRect(int sourceFromX, int sourceFromY, int sourceFromLayer, int sourceToX, int sourceToY, int sourceToLayer, int destFromX, int destFromY, int destFromLayer)
    {
        if (IsInitialized() &&
            IsInsideGrid(sourceFromX, sourceFromY, sourceFromLayer) && IsInsideGrid(sourceToX, sourceToY, sourceToLayer) &&
            IsInsideGrid(destFromX, destFromY, destFromLayer))
        {
            for (int layerInc = 0; layerInc <= sourceToLayer - sourceFromLayer; layerInc++)
            {
                for (int yInc = 0; yInc <= sourceToY - sourceFromY; yInc++)
                {
                    for (int xInc = 0; xInc <= sourceToX - sourceFromX; xInc++)
                    {
                        Move(sourceFromX + xInc, sourceFromY + yInc, sourceFromLayer + layerInc, destFromX + xInc, destFromY + yInc, destFromLayer + layerInc);
                    }
                }
            }
        }
    }
	/// <summary>
	/// Adds a layer after the last layer in the grid.
	/// </summary>
    public void AddLayer()
    {
        short[,] newLayer = new short[grid[0].GetLength(0), grid[0].GetLength(1)];
        grid.Add(newLayer);
        // Initialize layer cells to default/empty
        for (int y = 0; y < grid[0].GetLength(1); y++)
        {
            for (int x = 0; x < grid[0].GetLength(0); x++)
            {
                newLayer[x, y] = -1;
            }
        }

    }
	/// <summary>
	/// Removes the layer indicated by the specified index.
	/// </summary>
	/// <param name="layerIndex">Layer index.</param>
    public void RemoveLayer(int layerIndex)
    {
        if (IsInitialized() && layerIndex < grid.Count)
        {
            // Destroy game objects in the layer
            for (int y = 0; y < grid[layerIndex].GetLength(1); y++)
            {
                for (int x = 0; x < grid[layerIndex].GetLength(0); x++)
                {
                    Remove(x, y, layerIndex);
                }
            }
            // Remove layer from grid
            grid.RemoveAt(layerIndex);
            // Update positions for layers above the removed layer
            //for (int layer = layerIndex; layer < grid.Count; layer++)
            //{
            //    UpdateLayerPositions(layer);
            //}
        }
    }
	/// <summary>
	/// Swaps the layers witht the specified indices.
	/// </summary>
	/// <param name="layerIndex1">Index of the first layer.</param>
	/// <param name="layerIndex2">ndex of the second layer.</param>
    public void SwapLayers(int layerIndex1, int layerIndex2)
    {
        if (IsInitialized() && layerIndex1 < grid.Count && layerIndex2 < grid.Count)
        {
            // Swap tiles
            // Make the swap
            short[,] temp = grid[layerIndex1];
            grid[layerIndex1] = grid[layerIndex2];
            grid[layerIndex2] = temp;
            // Update layers view
            UpdateRendererLayer(layerIndex1);
            UpdateRendererLayer(layerIndex2);



            // Swap game objects
            Dictionary<GridPosition, GameObject> layer1Dict = new Dictionary<GridPosition, GameObject>();
            for (int x = 0; x < grid[layerIndex1].GetLength(0); ++x)
            {
                for (int y = 0; y < grid[layerIndex1].GetLength(1); ++y)
                {
                    GridPosition pos = new GridPosition(x, y, layerIndex1);
                    GameObject obj = null;
                    if (gameObjects.TryGetValue(pos, out obj))
                    {
                        // Add reference in temporary dictionary with updated layer
                        layer1Dict.Add(new GridPosition(x, y, layerIndex2), obj);
                        // Remove reference from game objects dictionary
                        gameObjects.Remove(pos);
                        // Update transform and z depth
                        obj.transform.position = new Vector3(x * CellWidth, y * CellDepth + layerIndex2 * CellHeight, 0);
                        SpriteRenderer rend = obj.GetComponent<SpriteRenderer>();
                        if (rend != null)
                        {
                            rend.sortingOrder = layerIndex2 - y;
                        }
                    }
                }
            }
            Dictionary<GridPosition, GameObject> layer2Dict = new Dictionary<GridPosition, GameObject>();
            for (int x = 0; x < grid[layerIndex2].GetLength(0); ++x)
            {
                for (int y = 0; y < grid[layerIndex2].GetLength(1); ++y)
                {
                    GridPosition pos = new GridPosition(x, y, layerIndex2);
                    GameObject obj = null;
                    if (gameObjects.TryGetValue(pos, out obj))
                    {
                        // Add reference in temporary dictionary with updated layer
                        layer2Dict.Add(new GridPosition(x, y, layerIndex1), obj);
                        // Remove reference from game objects dictionary
                        gameObjects.Remove(pos);
                        // Update transform
                        obj.transform.position = new Vector3(x * CellWidth, y * CellDepth + layerIndex1 * CellHeight, 0);
                        SpriteRenderer rend = obj.GetComponent<SpriteRenderer>();
                        if (rend != null)
                        {
                            rend.sortingOrder = layerIndex1 - y;
                        }
                    }
                }
            }
            // Add objects to dictionary
            gameObjects.Concat(layer1Dict);
            gameObjects.Concat(layer2Dict);
            // Update layer1 positions
            //UpdateLayerPositions(layerIndex1);
            // Update layer2 positions
            //UpdateLayerPositions(layerIndex2);
        }

    }

    /****************
    * SAVE/LOAD
    ****************/

	/// <summary>
	/// Loads the grid from the specified file.
	/// </summary>
	/// <param name="filePath">File path.</param>
    public void LoadGridFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Couldn't find file '" + filePath + "'");
        }

        int[] gridSizes = new int[3];

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (StreamReader reader = new StreamReader(filePath))
        {
            // First line is the grid size (width, depth, layers).
            string gridSizeLine = reader.ReadLine();
            if (gridSizeLine == null)
            {
                throw new IOException("Line missing (line 1: grid size)");
            }
            string[] sizes = gridSizeLine.Split(' ');
            if (sizes.Length != 3)
            {
                throw new IOException("Malformed input - missing value (line 1: grid size)");
            }

            // Transform sizes to ints.
            if (!int.TryParse(sizes[0], out gridSizes[0]))
            {
                throw new IOException("Malformed input - size needs to be an int (line 1: grid size)");
            }
            if (!int.TryParse(sizes[1], out gridSizes[1]))
            {
                throw new IOException("Malformed input - size needs to be an int (line 1: grid size)");
            }
            if (!int.TryParse(sizes[2], out gridSizes[2]))
            {
                throw new IOException("Malformed input - size needs to be an int (line 1: grid size)");
            }

            // Create empty grid of correct size.
            SetGridSize(gridSizes[0], gridSizes[1], gridSizes[2]);

            int currentLine = 1;
            // For each layer, read in the grid.
            for (int layer = 0; layer < gridSizes[2]; layer++)
            {
                // Read backwards, since unity's y axis is from bottom to top.
                for (int y = gridSizes[1] - 1; y >= 0; y--)
                {
                    currentLine++;
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        throw new IOException("Malformed input - size of grid larger than input (line " + currentLine +
                                              ": layer " + (layer + 1) + ")");
                    }
                    // If the line is empty, go to the next line (without counting).
                    if (line.Length == 0)
                    {
                        y++;
                        continue;
                    }

                    // Start reading in the values of this line.
                    string[] row = line.Split(',');
                    if (row.Length != gridSizes[0])
                    {
                        throw new IOException("Malformed input - row length is not the same as given width (line " + currentLine +
                                              ": layer " + (layer + 1) + ")");
                    }

                    // Parse and add the shorts to the new grid.
                    for (int x = 0; x < gridSizes[0]; x++)
                    {
                        short value;
                        if (!short.TryParse(row[x], out value))
                        {
                            throw new IOException("Malformed input - row input needs to be a short (line " + currentLine +
                                              ": layer " + (layer + 1) + ")");
                        }
                        Place(value, x, y, layer);
                    }
                }
            }
        }
        stopwatch.Stop();
        Debug.Log("Loading done (" + stopwatch.Elapsed + ")");
    }
	/// <summary>
	/// Coroutine that saves the grid to the specified file.
	/// </summary>
	/// <param name="filePath">File path.</param>
	/// <param name="overwrite">If set to <c>true</c> overwrites the contents of the specified file.</param>
	/// <param name="callBack">Call back.</param>
    public IEnumerator SaveGridToFileCoroutine(string filePath, bool overwrite = true, Action callBack = null)
    {
        if (LayerCount <= 0 || SizeX <= 0 || SizeY <= 0)
        {
            throw new Exception("Grid with size 0 cannot be saved");
        }
        // If set to not overwrite and file already exists, throw an exception.
        if (!overwrite && File.Exists(filePath))
        {
            throw new Exception("File already exists '" + filePath + "'");
        }

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        List<string> lines = new List<string>();
        lines.Add(SizeX + " " + SizeY + " " + LayerCount);
        foreach (var layer in grid)
        {
            lines.Add(layer.ToLayerString());
            yield return null;
        }
        File.WriteAllLines(filePath, lines.ToArray());
        stopwatch.Stop();
        Debug.Log("Saving done (" + stopwatch.Elapsed + ")");
        // If a callback method was passed, invoke it!
        if (callBack != null)
        {
            callBack();
        }
    }

}
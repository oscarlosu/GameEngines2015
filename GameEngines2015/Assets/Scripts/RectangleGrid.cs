using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;

public class RectangleGrid : MonoBehaviour
{
    public int CellWidth, CellDepth, CellHeight;
    public RenderingHandler RendHandler;
    public List<short[,]> grid = new List<short[,]>();
    private Dictionary<Vector3, GameObject> gameObjects = new Dictionary<Vector3, GameObject>();
    public int LayerCount
    {
        get
        {
            return grid.Count;
        }
    }
    public int SizeX { get; private set; }
    public int SizeY { get; private set; }


    private bool IsInitialized()
    {
        return grid.Count > 0;
    }

    public bool IsInsideGrid(int x, int y, int layer)
    {
        return layer >= 0 && x >= 0 && y >= 0 && layer < grid.Count && x < grid[layer].GetLength(0) && y < grid[layer].GetLength(1);
    }

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

    //private void UpdateRendererCell(int x, int y, int layer)
    //{
    //    // Unnecessary?
    //    throw new System.NotImplementedException();
    //}

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

    public bool TryGetObject(int x, int y, int layer, out GameObject obj)
    {
        return gameObjects.TryGetValue(new Vector3(x, y, layer), out obj);
    }

    public bool IsCellFree(int x, int y, int layer)
    {
        short tile;
        return !TryGetTile(x, y, layer, out tile);
    }

    public void Place(GameObject obj, int x, int y, int layer)
    {
        // Clear the dest cell
        Remove(x, y, layer);
        // Add to go dictionary
        gameObjects.Add(new Vector3(x, y, layer), obj);
        // Set world position and z-depth
        obj.transform.position = new Vector3(x * CellWidth, y * CellDepth + layer * CellHeight, 0);
        SpriteRenderer rend = obj.GetComponent<SpriteRenderer>();
        if (rend != null)
        {
            rend.sortingOrder = layer - y;
        }
    }

    public void Place(short tile, int x, int y, int layer)
    {
        // Clear dest cell
        Remove(x, y, layer);
        // Add to the grid
        grid[layer][x, y] = tile;
        RendHandler.UpdateCell(x, y, layer);
        //RendHandler.LoadCell(x, y, layer);
    }

    public void Move(int sourceX, int sourceY, int sourceLayer, int destX, int destY, int destLayer)
    {
        Vector3 sourcePos = new Vector3(sourceX, sourceY, sourceLayer);
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

    public void Remove(int x, int y, int layer)
    {
        Vector3 pos = new Vector3(x, y, layer);
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

    public void Swap(int x1, int y1, int layer1, int x2, int y2, int layer2)
    {
        // Copy and clear x2, y2, layer2
        Vector3 pos2 = new Vector3(x2, y2, layer2);
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

    public void SetGridSize(int width, int depth, int height)
    {
        SizeX = width;
        SizeY = depth;
        for (int layer = 0; layer < height; layer++)
        {
            // If the layer already exists we want to resize it
            if (layer < grid.Count)
            {
                if (layer < height)
                {
                    // Resize x, y
                    short[,] resizedLayer = new short[width, depth];
                    int maxX = Mathf.Max(width, grid[layer].GetLength(0));
                    int maxY = Mathf.Max(depth, grid[layer].GetLength(1));
                    // Copy to resized layer or delete
                    for (int y = 0; y < maxY; y++)
                    {
                        for (int x = 0; x < maxX; x++)
                        {
                            if (x < width && y < depth)
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
                short[,] newLayer = new short[width, depth];
                grid.Add(newLayer);
                // Initialize layer cells to default/empty
                for (int y = 0; y < depth; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        newLayer[x, y] = -1;
                    }
                }
            }
        }
        // Remove excess layers
        if (grid.Count > height)
        {
            for (int layer = height; layer < grid.Count; layer++)
            {
                for (int y = 0; y < grid[layer].GetLength(1); y++)
                {
                    for (int x = 0; x < grid[layer].GetLength(0); x++)
                    {
                        Remove(x, y, layer);
                    }
                }
            }
            grid.RemoveRange(height, grid.Count - height);
        }
        RendHandler.HideFromLayer(LayerCount);
    }

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
            Dictionary<Vector3, GameObject> layer1Dict = new Dictionary<Vector3, GameObject>();
            for (int x = 0; x < grid[layerIndex1].GetLength(0); ++x)
            {
                for (int y = 0; y < grid[layerIndex1].GetLength(1); ++y)
                {
                    Vector3 pos = new Vector3(x, y, layerIndex1);
                    GameObject obj = null;
                    if (gameObjects.TryGetValue(pos, out obj))
                    {
                        // Add reference in temporary dictionary with updated layer
                        layer1Dict.Add(new Vector3(x, y, layerIndex2), obj);
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
            Dictionary<Vector3, GameObject> layer2Dict = new Dictionary<Vector3, GameObject>();
            for (int x = 0; x < grid[layerIndex2].GetLength(0); ++x)
            {
                for (int y = 0; y < grid[layerIndex2].GetLength(1); ++y)
                {
                    Vector3 pos = new Vector3(x, y, layerIndex2);
                    GameObject obj = null;
                    if (gameObjects.TryGetValue(pos, out obj))
                    {
                        // Add reference in temporary dictionary with updated layer
                        layer2Dict.Add(new Vector3(x, y, layerIndex1), obj);
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

    public void LoadGridFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Couldn't find file '" + filePath + "'");
        }

        int[] gridSizes = new int[3];

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
    }

    public void SaveGridToFile(string filePath)
    {

    }

}
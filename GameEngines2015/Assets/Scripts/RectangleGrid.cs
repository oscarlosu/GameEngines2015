using UnityEngine;
using System.Collections.Generic;

public class RectangleGrid : MonoBehaviour
{
    private List<GameObject[,]> grid = new List<GameObject[,]>();
    public int width, depth, height;

    
    private bool IsInitialized()
    {
        return grid.Count > 0;
    }

    private bool IsInsideGrid(int x, int y, int layer)
    {
        return layer >= 0 && x >= 0 && y >= 0 && layer < grid.Count && x < grid[layer].GetLength(0) && y < grid[layer].GetLength(1);
    }
    private void UpdateLayerPositions(int layerIndex)
    {
        for (int y = 0; y < grid[layerIndex].GetLength(1); y++)
        {
            for (int x = 0; x < grid[layerIndex].GetLength(0); x++)
            {
                UpdateCellPosition(x, y, layerIndex);
            }
        }
    }

    private void UpdateCellPosition(int x, int y, int layer)
    {
        GameObject obj = grid[layer][x, y];
        if (obj != null)
        {
            // Update position and z depth
            obj.transform.position = new Vector3(x * width, y * depth + layer * height, 0);
            SpriteRenderer rend = obj.GetComponent<SpriteRenderer>();
            if (rend != null)
            {
                rend.sortingOrder = layer - y;
            }
        }
    }

    public GameObject Get(int x, int y, int layer)
    {
        if(IsInsideGrid(x, y, layer))
        {
            return grid[layer][x, y];
        }
        throw new System.IndexOutOfRangeException();        
    }

    public void Place(GameObject obj, int x, int y, int layer)
    {
        // Add to grid
        Remove(x, y, layer);
        grid[layer][x, y] = Instantiate(obj);
        // Update position and z depth
        UpdateCellPosition(x, y, layer);
    }

    public void Move(int sourceX, int sourceY, int sourceLayer, int destX, int destY, int destLayer)
    {
        Remove(destX, destY, destLayer);
        Swap(sourceX, sourceY, sourceLayer, destX, destY, destLayer);
    }

    public void Remove(int x, int y, int layer)
    {
        if(IsInsideGrid(x, y, layer))
        {
            if (grid[layer][x, y] != null)
            {
                GameObject.Destroy(grid[layer][x, y]);
                grid[layer][x, y] = null;
            }
        }
        else
        {
            throw new System.IndexOutOfRangeException();
        }
    }

    public void Swap(int x1, int y1, int layer1, int x2, int y2, int layer2)
    {
        if(IsInitialized() && IsInsideGrid(x1, y1, layer1) && IsInsideGrid(x2, y2, layer2))
        {
            // Swap cells
            GameObject temp = grid[layer1][x1, y1];
            grid[layer1][x1, y1] = grid[layer2][x2, y2];
            grid[layer2][x2, y2] = temp;
            // Update positions
            UpdateCellPosition(x1, y1, layer1);
            UpdateCellPosition(x2, y2, layer2);
        }
    }

    public void SetGridSize(int width, int depth, int height)
    {
        for(int layer = 0; layer < height; layer++)
        {
            // If the layer already exists we want to resize it
            if(layer < grid.Count)
            {
                if(layer < height)
                {
                    // Resize x, y
                    GameObject[,] resizedLayer = new GameObject[width, depth];
                    int maxX = Mathf.Max(width, grid[layer].GetLength(0));
                    int maxY = Mathf.Max(depth, grid[layer].GetLength(1));
                    // Copy to resized layer or delete
                    for(int y = 0; y < maxY; y++)
                    {
                        for(int x = 0; x < maxX; x++)
                        {
                            if(x < width && y < depth)
                            {
                                resizedLayer[x, y] = grid[layer][x, y];
                            }
                            else
                            {
                                GameObject.Destroy(grid[layer][x, y]);
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
                grid.Add(new GameObject[width, depth]);
            }
        }
        // Remove excess layers
        if(grid.Count > height)
        {
            for (int layer = height; layer < grid.Count; layer++)
            {
                for (int y = 0; y < grid[layer].GetLength(1); y++)
                {
                    for (int x = 0; x < grid[layer].GetLength(0); x++)
                    {
                        GameObject.Destroy(grid[layer][x, y]);
                    }
                }
            }
                    grid.RemoveRange(height, grid.Count - height);
        }
    }

    public void SetCellSize(int width, int depth, int height)
    {
        throw new System.NotImplementedException();
    }

    public void FillRect(GameObject obj, int fromX, int fromY, int fromLayer, int toX, int toY, int toLayer)
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
            // Loop over all cells and place the object.
            for (int layerIndex = minLayer; layerIndex <= maxLayer; layerIndex++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        Place(obj, x, y, layerIndex);
                    }
                }
            }
        }
    }

    public void RemoveRect(int fromX, int fromY, int fromLayer, int toX, int toY, int toLayer)
    {
        throw new System.NotImplementedException();
    }

    public void MoveRect(int sourceFromX, int sourceFromY, int sourceFromLayer, int sourceToX, int sourceToY, int sourceToLayer, int destFromX, int destFromY, int destFromLayer, int destToX, int destToY, int destToLayer)
    {
        throw new System.NotImplementedException();
    }

    public void AddLayer()
    {
        if(IsInitialized())
        {
            grid.Add(new GameObject[grid[0].GetLength(0), grid[0].GetLength(1)]);
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
                    GameObject.Destroy(grid[layerIndex][x, y]);
                }
            }
            // Remove layer from grid
            grid.RemoveAt(layerIndex);
            // Update positions for layers above the removed layer
            for (int layer = layerIndex; layer < grid.Count; layer++)
            {
                UpdateLayerPositions(layer);
            }
        }
    }

    public void SwapLayers(int layerIndex1, int layerIndex2)
    {
        if(IsInitialized() && layerIndex1 < grid.Count && layerIndex2 < grid.Count)
        {
            // Make the swap
            GameObject[,] temp = grid[layerIndex1];
            grid[layerIndex1] = grid[layerIndex2];
            grid[layerIndex2] = temp;
            // Update layer1 positions
            UpdateLayerPositions(layerIndex1);
            // Update layer2 positions
            UpdateLayerPositions(layerIndex2);
        }        
    }
}

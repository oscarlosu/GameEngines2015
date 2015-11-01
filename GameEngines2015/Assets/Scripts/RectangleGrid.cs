using UnityEngine;
using System.Collections.Generic;

public class RectangleGrid : MonoBehaviour
{
    private List<GameObject[,]> grid = new List<GameObject[,]>();
    public int width, depth, height;

    

    private bool IsInsideGrid(int x, int y, int layer)
    {
        return layer >= 0 && x >= 0 && y >= 0 && layer < grid.Count && x < grid[layer].GetLength(0) && y < grid[layer].GetLength(1);
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
        grid[layer][x, y] = obj;
        // Update position and z depth
        obj.transform.position = new Vector3(x * width, y * depth + layer * height, 0);
        SpriteRenderer rend = obj.GetComponent<SpriteRenderer>();
        if(rend != null)
        {
            rend.sortingOrder = layer - y;
        }

    }

    public void Place(int sourceX, int sourceY, int sourceLayer, int destX, int destY, int destLayer)
    {
        throw new System.NotImplementedException();
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
        throw new System.NotImplementedException();
    }

    public void SetGridSize(int width, int depth, int height)
    {
        for(int layer = 0; layer < height; ++layer)
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
                    for(int y = 0; y < maxY; ++y)
                    {
                        for(int x = 0; x < maxX; ++x)
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
            for (int layer = height; layer < grid.Count; ++layer)
            {
                for (int y = 0; y < grid[layer].GetLength(1); ++y)
                {
                    for (int x = 0; x < grid[layer].GetLength(0); ++x)
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
        throw new System.NotImplementedException();
    }

    public void RemoveRect(int fromX, int fromY, int fromLayer, int toX, int toY, int toLayer)
    {
        throw new System.NotImplementedException();
    }

    public void MoveRect(int sourceFromX, int sourceFromY, int sourceFromLayer, int sourceToX, int sourceToY, int sourceToLayer, int destFromX, int destFromY, int destFromLayer, int destToX, int destToY, int destToLayer)
    {
        throw new System.NotImplementedException();
    }

    public void AddLayer(int layerIndex)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveLayer(int layerIndex)
    {
        throw new System.NotImplementedException();
    }

    public void SwapLayers(int layerIndex1, int layerIndex2)
    {
        throw new System.NotImplementedException();
    }
}

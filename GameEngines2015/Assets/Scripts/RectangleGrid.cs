using UnityEngine;
using System.Collections.Generic;

public class RectangleGrid : MonoBehaviour
{
    public int Width, Depth, Height;

    public List<short[,]> grid = new List<short[,]>(5);
    private Dictionary<Vector3, GameObject> gameObjects;
    public int LayerCount
    {
        get
        {
            return grid.Count;
        }
    }


    private bool IsInitialized()
    {
        return grid.Count > 0;
    }

    public bool IsInsideGrid(int x, int y, int layer)
    {
        return layer >= 0 && x >= 0 && y >= 0 && layer < grid.Count && x < grid[layer].GetLength(0) && y < grid[layer].GetLength(1);
    }
    private bool IsInsideViewport(int x, int y, int layer)
    {
        float halfWidth = Camera.main.aspect * Camera.main.orthographicSize;
        float xPos = x * Width;
        float yPos = y * Depth + layer * Height; 
        return xPos > Camera.main.transform.position.x - halfWidth && xPos < Camera.main.transform.position.x + halfWidth &&
               yPos > Camera.main.transform.position.y - Camera.main.orthographicSize && yPos < Camera.main.transform.position.y + Camera.main.orthographicSize;
    }
    private void UpdateLayerPositions(int layerIndex)
    {
        throw new System.NotImplementedException();
    }

    private void UpdateRendererCell(int x, int y, int layer)
    {
        throw new System.NotImplementedException();
    }

    public bool TryGetTile(int x, int y, int layer, out short tile)
    {
        if(IsInsideGrid(x, y, layer))
        {
            tile = grid[layer][x, y];
            return true;
        }
        else
        {
            tile = 0;
            return false;
        }
    }

    public bool TryGetObject(int x, int y, int layer, out GameObject obj)
    {
        throw new System.NotImplementedException();
    }

    public void Place(GameObject obj, int x, int y, int layer)
    {
        throw new System.NotImplementedException();
    }

    public void Place(short tile, int x, int y, int layer)
    {
        throw new System.NotImplementedException();
    }

    public void Move(int sourceX, int sourceY, int sourceLayer, int destX, int destY, int destLayer)
    {
        throw new System.NotImplementedException();
    }

    public void Remove(int x, int y, int layer)
    {
        throw new System.NotImplementedException();
    }

    public void Swap(int x1, int y1, int layer1, int x2, int y2, int layer2)
    {
        throw new System.NotImplementedException();
    }

    public void SetGridSize(int width, int depth, int height)
    {
        throw new System.NotImplementedException();
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

    public void AddLayer()
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

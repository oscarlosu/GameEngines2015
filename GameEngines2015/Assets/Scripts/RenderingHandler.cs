using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class RenderingHandler : MonoBehaviour
{
    public int BufferX, BufferY;
    public List<Sprite> Tiles = new List<Sprite>();
    public RectangleGrid HandledGrid;
    private Dictionary<Vector3, GameObject> renderers = new Dictionary<Vector3, GameObject>();

    private int sizeX, sizeY;
    private Vector2 first, last, dif;

    // Use this for initialization
    void Start()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Application.isPlaying)
        {
            Vector2 newFirst = FirstCellXY();
            Vector2 newLast = LastCellXY();
            Vector2 newDif = newLast - newFirst;
            if (first != newFirst || last != newLast)
            {
                if (dif == newDif)
                {
                    // TEST
                    //Load();
                    //return;
                    // Move right
                    for (int x = (int) first.x; x < newFirst.x; ++x)
                    {
                        MoveColumn(x, x + (int) dif.x);
                    }
                    // Move left
                    for (int x = (int) newFirst.x; x < first.x; ++x)
                    {
                        MoveColumn(x + (int) dif.x, x);
                    }
                    first.x = newFirst.x;
                    last.x = newLast.x;

                    // Move up
                    for (int y = (int) first.y; y < newFirst.y; ++y)
                    {
                        MoveRow(y, y + (int) dif.y);
                    }
                    // Move down
                    for (int y = (int) newFirst.y; y < first.y; ++y)
                    {
                        MoveRow(y + (int) dif.y, y);
                    }
                    // Update first and last
                    first.y = newFirst.y;
                    last.y = newLast.y;
                    //dif = newDif;
                }
                else
                {
                    Debug.Log("Zoom");
                    Load();
                }
            }
        }
    }

    public void UpdateCell(int x, int y, int layer)
    {
        GameObject obj;
        if(renderers.TryGetValue(new Vector3(x, y, layer), out obj))
        {
            // Read new sprite from HandledGrid
            short tileID;
            if(HandledGrid.TryGetTile(x, y, layer, out tileID))
            {
                SpriteRenderer rend = obj.GetComponent<SpriteRenderer>();
                rend.sprite = Tiles[tileID];
                rend.sortingOrder = layer - y;
            }
            else
            {
                SpriteRenderer rend = obj.GetComponent<SpriteRenderer>();
                rend.sprite = null;
            }            
        }
    }

    public void Load()
    {
        // Clear dictionary       
        foreach(KeyValuePair<Vector3, GameObject> pair in renderers)
        {
            Destroy(pair.Value);
        }
        renderers.Clear();
        // Calculate number of renderers necessary to cover the viewport
        first = FirstCellXY();
        last = LastCellXY();
        dif = last - first;

        sizeX = (int)Mathf.Abs(dif.x);
        sizeY = (int)Mathf.Abs(dif.y);
        // Create a GameObject with a renderer
        GameObject model = new GameObject("GridRenderer");
        model.AddComponent<SpriteRenderer>();
        // Create renderer GameObjects
        for (int x = 0; x < sizeX; ++x)
        {
            for (int y = 0; y < sizeY; ++y)
            {
                for (int layer = 0; layer < HandledGrid.LayerCount; ++layer)
                {
                    // Copy model
                    GameObject newObj = Instantiate(model);
                    // Set world position
                    newObj.transform.position = new Vector3((first.x + x) * HandledGrid.Width, (first.y + y) * HandledGrid.Depth + layer * HandledGrid.Height, 0);


                    // Calculate grid position
                    Vector3 gridPos = new Vector3(first.x + x, first.y + y, layer);
                    // Initialize with sprite indicated by HandledGrid
                    short tileID;
                    if (HandledGrid.TryGetTile((int)gridPos.x, (int)gridPos.y, (int)gridPos.z, out tileID))
                    {
                        newObj.GetComponent<SpriteRenderer>().sprite = Tiles[tileID];
                        // DEBUGGING
                        //newObj.GetComponent<SpriteRenderer>().sprite = Tiles[0];
                        // Set z-depth
                        newObj.GetComponent<SpriteRenderer>().sortingOrder = (int)gridPos.z - (int)gridPos.y;
                    }
                    // Make child of this object
                    newObj.transform.SetParent(HandledGrid.gameObject.transform);
                    // Add to dictionary
                    renderers.Add(gridPos, newObj);
                }
            }
        }
        Destroy(model);

        Debug.Log("Grid renderer count: " + sizeX * sizeY * HandledGrid.LayerCount);
    }

    private void MoveColumn(int sourceX, int destX)
    {
        for (int y = (int)first.y; y < last.y; ++y)
        {
            for (int layer = 0; layer < HandledGrid.LayerCount; ++layer)
            {

                Vector3 key = new Vector3(sourceX, y, layer);
                GameObject go;
                if (renderers.TryGetValue(key, out go))
                {
                    // Extract GameObject from dictionary
                    renderers.Remove(key);
                    // Move Game Object
                    go.transform.position = new Vector3(destX * HandledGrid.Width, y * HandledGrid.Depth + layer * HandledGrid.Height);
                    // Change sprite
                    short tileID;
                    if (HandledGrid.TryGetTile(destX, y, layer, out tileID))
                    {
                        go.GetComponent<SpriteRenderer>().sprite = Tiles[tileID];
                        go.GetComponent<SpriteRenderer>().sortingOrder = layer - y;
                    }
                    else
                    {
                        go.GetComponent<SpriteRenderer>().sprite = null;
                    }
                    // Insert in dictionary
                    renderers.Add(new Vector3(destX, y, layer), go);

                }
            }
        }
    }

    private void MoveRow(int sourceY, int destY)
    {
        for (int x = (int)first.x; x < last.x; ++x)
        {
            for (int layer = 0; layer < HandledGrid.LayerCount; ++layer)
            {

                Vector3 key = new Vector3(x, sourceY, layer);
                GameObject go;
                if (renderers.TryGetValue(key, out go))
                {
                    // Extract GameObject from dictionary
                    renderers.Remove(key);
                    // Move Game Object
                    go.transform.position = new Vector3(x * HandledGrid.Width, destY * HandledGrid.Depth + layer * HandledGrid.Height);
                    // Change sprite
                    short tileID;
                    if (HandledGrid.TryGetTile(x, destY, layer, out tileID))
                    {
                        go.GetComponent<SpriteRenderer>().sprite = Tiles[tileID];
                        go.GetComponent<SpriteRenderer>().sortingOrder = layer - destY;
                    }
                    else
                    {
                        go.GetComponent<SpriteRenderer>().sprite = null;
                    }
                    // Insert in dictionary
                    renderers.Add(new Vector3(x, destY, layer), go);

                }
            }
        }
    }

    private Vector2 FirstCellXY()
    {
        Camera cam;
        if (GetCurrentCamera(out cam))
        {
            float camHalfWidth = cam.aspect*cam.orthographicSize;
            int firstX = Mathf.FloorToInt((Camera.main.transform.position.x - camHalfWidth)/HandledGrid.Width) - BufferX;
            int firstY =
                Mathf.FloorToInt((Camera.main.transform.position.y - Camera.main.orthographicSize -
                                  HandledGrid.Height*HandledGrid.LayerCount)/HandledGrid.Depth) - BufferY;
            return new Vector2(firstX, firstY);
        }
        return Vector2.zero;
    }

    private Vector2 LastCellXY()
    {
        Camera cam;
        if (GetCurrentCamera(out cam))
        {
            float camHalfWidth = cam.aspect*cam.orthographicSize;
            int lastX = Mathf.CeilToInt((cam.transform.position.x + camHalfWidth)/HandledGrid.Width) + BufferX + 1;
            int lastY = Mathf.CeilToInt((cam.transform.position.y + cam.orthographicSize)/HandledGrid.Depth) + BufferY +
                        1;
            return new Vector2(lastX, lastY);
        }
        return Vector2.zero;
    }

    private bool GetCurrentCamera(out Camera cam)
    {
        if (Application.isPlaying)
        {
            cam = Camera.main;
            return true;
        }
        else
        {
            cam = Camera.current;
            return cam != null;
        }
    }


}

using UnityEngine;
using System.Collections;
using System.IO;

public class DemoGreenCity : MonoBehaviour {

    public RectangleGrid Grid;
    public RenderingHandler Renderer;

    public SpriteList Tile1, Tile2, Tile3, Tile4;

    // Use this for initialization
    void Start ()
    {
        Renderer.Tiles.Add(Tile1);
        Renderer.Tiles.Add(Tile2);
        Renderer.Tiles.Add(Tile3);
        Renderer.Tiles.Add(Tile4);

        Grid.LoadGridFromFile(Directory.GetCurrentDirectory() + @"\..\GreenCityDemo.txt");

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}

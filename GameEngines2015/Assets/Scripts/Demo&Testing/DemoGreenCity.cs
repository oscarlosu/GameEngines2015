using UnityEngine;
using System.Collections;
using System.IO;

public class DemoGreenCity : MonoBehaviour {

    public RectangleGrid Grid;
    public RenderingHandler Renderer;

    // Use this for initialization
    void Start ()
    {

        Grid.LoadGridFromFile(Directory.GetCurrentDirectory() + @"\..\GreenCityDemo.txt");

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}

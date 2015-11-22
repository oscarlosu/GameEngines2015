using UnityEngine;
using System.Collections;
using UnityEditor;


public class EditorTest : MonoBehaviour
{

    public EditorObjectTest EOTest;

	// Use this for initialization
	void Start ()
	{
	    Debug.Log("Start");
	}
	
	// Update is called once per frame
	void Update ()
	{
	    Debug.Log("Update");

	    foreach (var i in EOTest.IntArray)
	    {
	        Debug.Log(i);
	    }
    }
}
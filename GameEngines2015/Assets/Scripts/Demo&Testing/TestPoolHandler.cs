using UnityEngine;
using System.Collections.Generic;

public class TestPoolHandler : GameObjectPoolHandler
{
	public List<int> KeyGenerators = new List<int>();

	// Use this for initialization
	void Start ()
	{
		Initialize();
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Request pool object with key
		if(Input.GetKeyDown (KeyCode.G))
		{
			KeyGenerators.Add (KeyGenerators.Count);
			GameObject obj = GetPoolObject(new Vector3(0, 0, KeyGenerators.Count));
			if(obj != null)
			{
				Debug.Log ("Pool object retrieved!");
			}
			else
			{
				Debug.Log ("Hard size limit reached!");
			}
		}
		// Disable random pool object
		if(Input.GetKeyDown (KeyCode.D))
		{
			int index = Random.Range (0, KeyGenerators.Count);
			if(DisablePoolObject(new Vector3(0, 0, KeyGenerators[index])))
			{
				KeyGenerators.RemoveAt(index);
				Debug.Log ("Pool object disabled!");
			}
			else
			{
				Debug.Log ("Object not found!");
			}
		}
		// Print status info
		/*
		 * private Queue<GameObject> inactivePool;
		 * private Dictionary<KeyType, GameObject> activePool;
		 * private int poolSize;
		 * private List<int> poolSizeHistory;
		 * 
		 */
		if(Input.GetKeyDown (KeyCode.I))
		{
			Debug.Log("Inactive pool size: " + inactivePool.Count + " Active pool size: " + activePool.Count + " Total: " + poolSize);
			Debug.Log("Pool size history: " + poolSizeHistory.ToString());
		}
	}
}

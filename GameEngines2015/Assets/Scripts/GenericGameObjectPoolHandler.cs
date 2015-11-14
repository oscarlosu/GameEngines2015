using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class GenericGameObjectPoolHandler<KeyType> : MonoBehaviour
{
	public GameObject PoolObjectPrefab;
	public int DefaultSize;
	public int SizePredictionFrequency;
	public int SizeSamplingFrequency;
	public float BufferFactor;
	public int HardSizeLimit;
	
	protected Queue<GameObject> inactivePool;
	protected Dictionary<KeyType, GameObject> activePool;
	protected int poolSize;
	protected List<int> poolSizeHistory;
	
	public void Initialize()
	{
		ClearPool();
		inactivePool = new Queue<GameObject>();
		activePool = new Dictionary<KeyType, GameObject>();
		poolSizeHistory = new List<int>();
		int size = Mathf.Min (DefaultSize, HardSizeLimit);
		for(int n = 0; n < size; ++n)
		{
			GameObject obj = Instantiate (PoolObjectPrefab);
			obj.transform.parent = transform;
			obj.SetActive(false);
			inactivePool.Enqueue(obj);
			++poolSize;
		}
		// Start prediction routines
		StartCoroutine(SamplePoolSize());
		StartCoroutine(PredictPoolSize());
	}
	
	public GameObject GetPoolObject(KeyType key)
	{
		GameObject obj = null;

		if(!activePool.TryGetValue(key, out obj))
		{
			// If there is no object with the given key in the active pool and the inactive pool is not empty
			if(inactivePool.Count > 0)
			{
				// Move pool object to active pool with given key and enable it
				obj = inactivePool.Dequeue();
				activePool.Add(key, obj);
				obj.SetActive(true);
			}
			// Increasing the size of the pool is only allowed up to the hard size limit
			else if(poolSize < HardSizeLimit)
			{
				// Create new pool object, enable it and add it to the active pool with the given key
				obj = Instantiate (PoolObjectPrefab);
				obj.transform.parent = transform;
				obj.SetActive(true);
				activePool.Add(key, obj);
				// Update pool size
				++poolSize;
			}
		}
		return obj;
	}
	
	public bool DisablePoolObject(KeyType key)
	{
		GameObject obj = null;
		if(activePool.TryGetValue(key, out obj))
		{
			// Disable object and move to inactive pool
			obj.SetActive(false);
			activePool.Remove(key);
			inactivePool.Enqueue(obj);
			return true;
		}
		else
		{
			return false;
		}
	}
	
	protected IEnumerator SamplePoolSize()
	{
		yield return new WaitForSeconds(SizeSamplingFrequency);
		while(true)
		{
			poolSizeHistory.Add (activePool.Count);
			yield return new WaitForSeconds(SizeSamplingFrequency);
		}
	}
	
	protected IEnumerator PredictPoolSize()
	{
		yield return new WaitForSeconds(SizePredictionFrequency);
		while(true)
		{
			// Calculate average size and standard deviation since last execution
			int avg = (int)poolSizeHistory.Average();
			float sd = 0;
			foreach(int sample in poolSizeHistory)
			{
				sd += Mathf.Abs(sample - avg);
			}
			sd /= poolSizeHistory.Count;
			int predictedSize = avg + (int)(sd * BufferFactor);
			poolSizeHistory.Clear ();
			// Reduce pool size if the predicted size is smaller than the current pool size and the inactive pool is not empty
			while(inactivePool.Count > 0 && predictedSize < poolSize)
			{
				// Remove pool object from the inactive pool and destroy it
				GameObject obj = inactivePool.Dequeue();
				Destroy (obj);
				// Update pool size
				--poolSize;
			}
			Debug.Log ("Avg: " + avg + " Sd: " + sd + " PredictedSize: " + predictedSize);
			yield return new WaitForSeconds(SizePredictionFrequency);
		}
	}
	
	protected void ClearPool()
	{
		if(inactivePool != null && activePool != null && poolSizeHistory != null)
		{
			// Clear inactive pool
			for(int n = 0; n < inactivePool.Count; ++n)
			{
				GameObject obj = inactivePool.Dequeue();
				Destroy (obj);
			}
			// Clear active pool
			foreach(KeyType key in activePool.Keys)
			{
				GameObject obj = activePool[key];
				activePool.Remove(key);
				Destroy(obj);
			}
			// Clear pool size history
			poolSizeHistory.Clear();
			poolSize = 0;
		}
	}
}

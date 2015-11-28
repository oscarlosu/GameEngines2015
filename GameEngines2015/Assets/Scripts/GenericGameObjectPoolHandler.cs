using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class GenericGameObjectPoolHandler<KeyType> : MonoBehaviour
{
	/// <summary>
	/// The pool object prefab. This is the object that will be used in the pool.
	/// </summary>
	public GameObject PoolObjectPrefab;
	/// <summary>
	/// The default size of the pool. Specifically, it is the starting size of the inactive pool.
	/// </summary>
	public int DefaultSize;
	/// <summary>
	/// The interval of time in seconds between executions of the size prediction method.
	/// </summary>
	public int SizePredictionFrequency;
	/// <summary>
	/// The interval of time in seconds between executions of the size sampling method.
	/// </summary>
	public int SizeSamplingFrequency;
	/// <summary>
	/// A factor that determines how many times standard deviation should be added in order to calculate the predicted pool size.
	/// </summary>
	public float BufferFactor;
	/// <summary>
	/// A strict limit for the total size of the pool. The pool size will never go beyond this limit.
	/// </summary>
	public int HardSizeLimit;

	/// <summary>
	/// The inactive pool. These are the objects that are currently not in use and are disabled by the pool handler.
	/// </summary>
	protected Queue<GameObject> inactivePool;
	/// <summary>
	/// The active pool. These are the objects currently in use and enabled. These objects can be retrieved throught their key.
	/// </summary>
	protected Dictionary<KeyType, GameObject> activePool;
	/// <summary>
	/// The current total size of the pool.
	/// </summary>
	protected int poolSize;
	/// <summary>
	/// A log of the size of the active pool over a period of time. The <see cref="SamplePoolSize"/> method stores values here and the <see cref="PredictPoolSize"/>
	/// method uses this information to adjust the size of the pool dinamically.
	/// </summary>
	protected List<int> poolSizeHistory;

	void Awake()
	{
		Initialize ();
	}
	/// <summary>
	/// Initialize the pool with <paramref name="DefaultSize"/> <paramref name="PoolObjectPrefab"/> in the inactive pool.
	/// </summary>
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
	/// <summary>
	/// Depending on the state of the pool, either creates a new instance of <paramref name="PoolObjectPrefab"/> or enables one of the
	/// instances in the inactive pool or returns an already active instance if the given key was found in the active pool.
	/// Will return null if the <paramref name="HardSizeLimit"/> is reached.
	/// </summary>
	/// <returns>The pool object.</returns>
	/// <param name="key">Key.</param>
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
	/// <summary>
	/// Disables the pool object with the given key.
	/// </summary>
	/// <returns><c>true</c>, if pool object was disabled, <c>false</c> otherwise.</returns>
	/// <param name="key">Key.</param>
	virtual public bool DisablePoolObject(KeyType key)
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
	/// <summary>
	/// Samples the size of the pool.
	/// </summary>
	/// <returns>The pool size.</returns>
	protected IEnumerator SamplePoolSize()
	{
		yield return new WaitForSeconds(SizeSamplingFrequency);
		while(true)
		{
			poolSizeHistory.Add (activePool.Count);
			yield return new WaitForSeconds(SizeSamplingFrequency);
		}
	}
	/// <summary>
	/// Predicts the size of the pool using the information retrieved by the <see cref="SamplePoolSize"/> method and adjusts
	/// the size of the inactive pool accordingly.
	/// </summary>
	/// <returns>The pool size.</returns>
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
	/// <summary>
	/// Clears all the collections in the pool and destroys all the game objects in them.
	/// </summary>
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

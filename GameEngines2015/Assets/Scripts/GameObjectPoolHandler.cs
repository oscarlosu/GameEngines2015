using UnityEngine;
using System.Collections;

/// <summary>
/// This class handles a pool of game objects with sprite renderers that are used to render the part of the grid that
/// is inside of the camera.
/// </summary>
/// 
/// The singleton implementation in this script was taken from http://wiki.unity3d.com/index.php/Singleton
/// 

public class GameObjectPoolHandler : GenericGameObjectPoolHandler<Vector3>
{
	private static GameObjectPoolHandler _instance;
	
	private static object _lock = new object();

	/// <summary>
	/// Gets the GameObjectPoolHandler singleton instance.
	/// </summary>
	/// <value>The GameObjectPoolHandler instance.</value>
	public static GameObjectPoolHandler Instance
	{
		get
		{
			if (applicationIsQuitting) {
				Debug.LogWarning("[Singleton] Instance '"+ typeof(GameObjectPoolHandler) +
				                 "' already destroyed on application quit." +
				                 " Won't create again - returning null.");
				return null;
			}

			lock(_lock)
			{
				if (_instance == null)
				{
					_instance = (GameObjectPoolHandler) FindObjectOfType(typeof(GameObjectPoolHandler));
					
					if ( FindObjectsOfType(typeof(GameObjectPoolHandler)).Length > 1 )
					{
						Debug.LogError("[Singleton] Something went really wrong " +
						               " - there should never be more than 1 singleton!" +
						               " Reopening the scene might fix it.");
						return _instance;
					}
					
					if (_instance == null)
					{
						GameObject singleton = new GameObject();
						_instance = singleton.AddComponent<GameObjectPoolHandler>();
						singleton.name = "(singleton) "+ typeof(GameObjectPoolHandler).ToString();
						_instance.Initialize();

					} else {
						_instance.Initialize(false);
						Debug.Log("[Singleton] Using instance already created: " +
						          _instance.gameObject.name);
					}
				}
				
				return _instance;
			}
		}
	}
	
	private static bool applicationIsQuitting = false;
	/// <summary>
	/// When Unity quits, it destroys objects in a random order.
	/// In principle, a Singleton is only destroyed when application quits.
	/// If any script calls Instance after it have been destroyed, 
	///   it will create a buggy ghost object that will stay on the Editor scene
	///   even after stopping playing the Application. Really bad!
	/// So, this was made to be sure we're not creating that buggy ghost object.
	/// </summary>
	public void OnDestroy () {
		applicationIsQuitting = true;
	}

	void Initialize(bool setDefaultValues = true)
	{
		if(setDefaultValues)
		{
			PoolObjectPrefab = (GameObject)Resources.Load("Renderer");
			DefaultSize = 20;
			SizePredictionFrequency = 5;
			SizeSamplingFrequency = 1;
			BufferFactor = 1;
			HardSizeLimit = 3000;
		}
		base.Initialize();
	}

}
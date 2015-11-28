using UnityEngine;
using System.Collections;

public class RendererPoolHandler : GenericGameObjectPoolHandler<Vector3>
{
	new public bool DisablePoolObject(Vector3 key)
	{
		// Reset transparency
		GameObject obj;
		if(activePool.TryGetValue(key, out obj))
		{
			SpriteRenderer rend = obj.GetComponent<SpriteRenderer>();
			if(rend != null && rend.color.a != 1)
			{
				rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, 1);
			}
		}
		return base.DisablePoolObject(key);
	}
}
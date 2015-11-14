using UnityEngine;
using System.Collections;
/*
 * Test results:
 * 
 * When the repeated operations are FAST:
 * 
 * ++i; 
 * 
 * the multicall approach shows a considerable overhead (Iterations: > 100000, Ratio 2.6) over 
 * singlecall. 
 * 
 * However, when the repeated operation is SLOW:
 * 
 * GameObject go = new GameObject();
 * Destroy (go);
 * 
 * the overhead seems to be minimal (Iterations: > 10000, Ratio < 1.1), in fact, when the number of iterations is below 1000, the multicall 
 * approach outperforms the single call (Ratio 0.9).
 * 
 * Conclusion:
 * 
 * The multicall approach is probably affordable in our case.
 * 
 */ 
public class MethodCallPerformanceTest : MonoBehaviour
{
	public int Iterations;

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.Space))
		{
			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			watch.Reset();
			// Single call test
			watch.Start ();

			SingleCall ();

			watch.Stop ();
			long elapsedSingle = watch.ElapsedTicks;
			Debug.Log ("Single call: " + elapsedSingle + " ticks");
			watch.Reset();
			// Multicall test
			watch.Start ();

			for(int t = 0; t < Iterations; ++t)
			{
				MultiCall();
			}

			watch.Stop ();
			long elapsedMulti = watch.ElapsedTicks;
			Debug.Log ("Multi call: " + elapsedMulti + " ticks");

			Debug.Log ("Ratio (Multi/Single): " + elapsedMulti / (double)elapsedSingle);

		}
	}

	void SingleCall()
	{
		/*int i = 0;*/
		for(int t = 0; t < Iterations; ++t)
		{
			GameObject go = new GameObject();
			Destroy (go);
			/*++i;*/
		}

	}

	void MultiCall()
	{
		GameObject go = new GameObject();
		Destroy (go);
		/*int i = 0;
		++i;*/
	}
}

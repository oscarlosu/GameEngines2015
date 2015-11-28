using UnityEngine;
using UnityEditor;

public class GridEditor : MonoBehaviour
{

    [MenuItem("GameObject/Oscillip/Rectangle Grid %g", false, 10)]
    static void CreateRectangleGrid()
    {
        GameObject go = new GameObject("RectangleGrid");
        var grid = go.AddComponent<RectangleGrid>();
        var rendHandler = go.AddComponent<RenderingHandler>();
        var objectPool = go.AddComponent<GameObjectPoolHandler>();

        /////////////////////////////////////////////
        // Set up grid with necessities and defaults.
        /////////////////////////////////////////////
        grid.RendHandler = rendHandler;

        ////////////////////////////////////////////////////
        // Set up rendHandler with necessities and defaults.
        ////////////////////////////////////////////////////
        rendHandler.HandledGrid = grid;
        rendHandler.RendererPool = objectPool;

        // Buffers.
        rendHandler.BufferX = 2;
        rendHandler.BufferY = 2;

        // Time of day.
        rendHandler.MiddayColourMultipliers = new Vector3(1, 1, 1);
        rendHandler.MorningColourMultipliers = new Vector3(1, 0.5f, 0.5f);
        rendHandler.NightColourMultipliers = new Vector3(0.4f, 0.4f, 1);

        // Shadows.
        rendHandler.MaxTint = 0.5f;
        rendHandler.TintIncrease = 0.06f;

        // Animations.
        rendHandler.AnimationNextTime = 0.5f;

        ///////////////////////////////////////////////////
        // Set up objectPool with necessities and defaults.
        ///////////////////////////////////////////////////
        objectPool.PoolObjectPrefab = (GameObject)Resources.Load("Renderer");
        objectPool.DefaultSize = 20;
        objectPool.SizePredictionFrequency = 5;
        objectPool.SizeSamplingFrequency = 1;
        objectPool.HardSizeLimit = 3000;

    }

}

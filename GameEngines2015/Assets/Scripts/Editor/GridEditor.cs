﻿using UnityEngine;
using UnityEditor;

public class GridEditor : MonoBehaviour
{

    [MenuItem("GameObject/Grid Engine/Rectangle Grid %g", false, 10)]
    static void CreateRectangleGrid()
    {
        GameObject go = new GameObject("RectangleGrid");
        var grid = go.AddComponent<RectangleGrid>();
        var rendHandler = go.AddComponent<RenderingHandler>();

        /////////////////////////////////////////////
        // Set up grid with necessities and defaults.
        /////////////////////////////////////////////
        grid.RendHandler = rendHandler;

        ////////////////////////////////////////////////////
        // Set up rendHandler with necessities and defaults.
        ////////////////////////////////////////////////////
        rendHandler.HandledGrid = grid;

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
    }

}

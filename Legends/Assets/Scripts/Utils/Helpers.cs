using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helpers
{
    public static void RecursiveLayerChange(Transform root, int layer)
    {
        root.gameObject.layer = layer;
        foreach (Transform t in root)
            RecursiveLayerChange(t, layer);
    }
}

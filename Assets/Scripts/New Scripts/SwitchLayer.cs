using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchLayer : MonoBehaviour
{
    [Header("Layer Setup (Use Numbers 0-31)")]
    public int defaultLayerIndex = 0;
    public int xRayLayerIndex = 3;

    private bool xRayActive = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            xRayActive = !xRayActive;

            int targetLayer = xRayActive ? xRayLayerIndex : defaultLayerIndex;
            string statusMessage = xRayActive ? "ACTIVATING X-RAY SHADER" : "RETURNING TO DEFAULT VIEW";

            // Print out exactly what is happening to the Console
            Debug.Log($"[X-Ray System] Key Pressed! {statusMessage}. Setting target layer index to: {targetLayer}", gameObject);

            SetLayerAllChildren(transform, targetLayer);
        }
    }

    void SetLayerAllChildren(Transform root, int layer)
    {
        root.gameObject.layer = layer;

        var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
            child.gameObject.layer = layer;
        }
    }
}
using UnityEngine;
using System.Collections;

public class GazeRayLayerAssigner : MonoBehaviour
{
    public bool toggle; // Set to true to toggle on gaze rays to see in real time where user is looking
    void Start()
    { 
        if(toggle){
        StartCoroutine(AssignLayer());
        }
    }

    IEnumerator AssignLayer()
    {
        yield return null; // Wait 1 frame to ensure the layer exists

        int interactiveUILayer = LayerMask.NameToLayer("InteractiveUI");
        if (interactiveUILayer == -1)
        {
            Debug.LogWarning("Layer 'InteractiveUI' not found.");
            yield break;
        }

        // Assign the layer to this GameObject
        gameObject.layer = interactiveUILayer;

        // Optionally assign it to all children too
        foreach (Transform child in transform)
        {
            child.gameObject.layer = interactiveUILayer;
        }

        Debug.Log($"Gaze Ray Sample assigned to layer 'InteractiveUI' ({interactiveUILayer})");
    }
}

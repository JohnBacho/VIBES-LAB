using UnityEngine;
using System.Collections;

public class VRPlayerReposition : MonoBehaviour
{
    public Transform xrRig;             // The parent XR Rig GameObject
    public Transform xrCamera;          // The HMD / Main Camera
    public Transform cameraSpawnPoint;  // The desired world-space location for the camera to end up

    private void Start()
    {
        StartCoroutine(WaitAndRecenter());
    }

    private IEnumerator WaitAndRecenter()
{
    // Wait a few frames to ensure tracking has fully initialized
    yield return new WaitForSeconds(1.5f);

    if (xrRig == null || xrCamera == null || cameraSpawnPoint == null)
    {
        Debug.LogWarning("VRPlayerReposition: Missing references.");
        yield break;
    }

    // Step 1: Get the real-world tracked camera position
    Vector3 currentCameraWorldPos = xrCamera.position;

    // Step 2: Compute how far off it is from where we want the camera to be
    Vector3 offset = cameraSpawnPoint.position - currentCameraWorldPos;

    // Step 3: Only apply X and Z offset, keep current Y position
    offset.y = 0;

    // Step 4: Apply the offset to the XR Rig
    xrRig.position += offset;

    Debug.Log($"[VRPlayerReposition] Moved XR Rig by offset (X,Z only): {offset}");
}

}

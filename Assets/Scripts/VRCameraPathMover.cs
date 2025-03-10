using UnityEngine;
using System.Collections;

public class VRCameraPathMover : MonoBehaviour
{
    public Transform[] waypoints; // Array of waypoints
    public float moveSpeed = 2.0f; // Speed of movement
    public float rotationSpeed = 5.0f; // Speed of rotation towards next waypoint
    public float startWaitTime = 2.0f; // Time to wait at the start
    public float[] waitTimes; // Time to wait at specific waypoints

    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private Transform xrRigTransform; // The parent XR Rig transform
    private bool hasStopped = false;

    void Start()
    {
        xrRigTransform = transform; // Move the XR Rig, not the camera
        StartCoroutine(WaitAtStart());
    }

    void Update()
    {
        if (waypoints.Length == 0 || isWaiting || hasStopped) return;

        MoveAlongPath();
    }

    void MoveAlongPath()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // Move the XR Rig (not the camera)
        xrRigTransform.position = Vector3.MoveTowards(xrRigTransform.position, targetWaypoint.position, moveSpeed * Time.deltaTime);

        // Rotate the XR Rig towards the waypoint
        Vector3 direction = (targetWaypoint.position - xrRigTransform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            xrRigTransform.rotation = Quaternion.Slerp(xrRigTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Check if we reached the waypoint
        if (Vector3.Distance(xrRigTransform.position, targetWaypoint.position) < 0.1f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtStart()
    {
        isWaiting = true;
        yield return new WaitForSeconds(startWaitTime);
        isWaiting = false;
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        float waitTime = (currentWaypointIndex < waitTimes.Length) ? waitTimes[currentWaypointIndex] : 0f;
        yield return new WaitForSeconds(waitTime);

        currentWaypointIndex++;

        if (currentWaypointIndex >= waypoints.Length)
        {
            hasStopped = true; // Stop moving at the final waypoint
        }

        isWaiting = false;
    }
}

using UnityEngine;
using System.Collections;

public class VRCameraPathMover : MonoBehaviour
{
    public Transform[] waypoints; // Array of waypoints
    public float moveSpeed = 2.0f; // Speed of movement
    public float rotationSpeed = 5.0f; // Speed of rotation towards next waypoint
    public float startWaitTime = 2.0f; // Time to wait at the start
    public float[] waitTimes; // Time to wait at specific waypoints

    private int currentWaypointIndex = 0; // Track the current waypoint
    private bool isWaiting = false; // Track if waiting

    void Start()
    {
        StartCoroutine(WaitAtStart());
    }

    void Update()
    {
        if (waypoints.Length == 0 || isWaiting) return;

        MoveAlongPath();
    }

    void MoveAlongPath()
    {
        // Get the current target waypoint
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // Move towards the target waypoint
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, moveSpeed * Time.deltaTime);

        // Rotate smoothly towards the target waypoint
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Check if we reached the waypoint
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
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

        // Get wait time for the current waypoint (default to 0 if out of range)
        float waitTime = (currentWaypointIndex < waitTimes.Length) ? waitTimes[currentWaypointIndex] : 0f;
        yield return new WaitForSeconds(waitTime);

        currentWaypointIndex++;

        // Loop back to the first waypoint (optional)
        if (currentWaypointIndex >= waypoints.Length)
        {
            currentWaypointIndex = 0; // Restart the path
        }

        isWaiting = false;
    }
}

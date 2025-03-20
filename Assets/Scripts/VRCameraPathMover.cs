using UnityEngine;
using System.Collections;

public class VRCameraPathMover : MonoBehaviour
{
    public Transform[] waypoints;
    public float moveSpeed = 2.0f;
    public float rotationSpeed = 5.0f;
    public float startWaitTime = 2.0f;
    public float[] waitTimes;

    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private Transform xrRigTransform;
    private bool hasStopped = true;

    void Start()
    {
        xrRigTransform = transform;
    }

    void Update()
    {
        if (waypoints.Length == 0 || isWaiting || hasStopped) return;

        MoveAlongPath();
    }

    public void StartMoving() // this event is triggered in the experiment script
    {
        hasStopped = false; 
        StartCoroutine(WaitAtStart());
    }

    void MoveAlongPath()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        xrRigTransform.position = Vector3.MoveTowards(xrRigTransform.position, targetWaypoint.position, moveSpeed * Time.deltaTime);

        Vector3 direction = (targetWaypoint.position - xrRigTransform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            xrRigTransform.rotation = Quaternion.Slerp(xrRigTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

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
            hasStopped = true;
        }

        isWaiting = false;
    }
}

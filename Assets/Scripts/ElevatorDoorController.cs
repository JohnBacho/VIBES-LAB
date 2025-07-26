using UnityEngine;

public class ElevatorDoorController : MonoBehaviour
{
    public GameObject leftDoor;
    public GameObject rightDoor;

    public float openDistance = 0.01f; // Distance each door moves
    public float openSpeed = 1.0f;    // How fast the doors open
    public float closeSpeed = 0.5f;     // How fast the doors close



    private Vector3 leftDoorClosedPos;
    private Vector3 rightDoorClosedPos;
    private Vector3 leftDoorOpenPos;
    private Vector3 rightDoorOpenPos;

    private bool isOpening = false;

    void Start()
    {
        // Store closed positions
        leftDoorClosedPos = leftDoor.transform.localPosition;
        rightDoorClosedPos = rightDoor.transform.localPosition;

        // Calculate open positions
        leftDoorOpenPos = leftDoorClosedPos + Vector3.left * openDistance;
        rightDoorOpenPos = rightDoorClosedPos + Vector3.right * openDistance;
    }

    void Update()
    {
        if (isOpening)
        {
            leftDoor.transform.localPosition = Vector3.Lerp(leftDoor.transform.localPosition, leftDoorOpenPos, Time.deltaTime * openSpeed);
            rightDoor.transform.localPosition = Vector3.Lerp(rightDoor.transform.localPosition, rightDoorOpenPos, Time.deltaTime * openSpeed);
        }
    }

    public void OpenDoors()
    {
        isOpening = true;
    }

    public void ShutDoors()
    {
        isOpening = false;
        StartCoroutine(CloseDoorsSmoothly());
    }

    private System.Collections.IEnumerator CloseDoorsSmoothly()
{
    while (Vector3.Distance(leftDoor.transform.localPosition, leftDoorClosedPos) > 0.00005f)
    {
        leftDoor.transform.localPosition = Vector3.MoveTowards(
            leftDoor.transform.localPosition, leftDoorClosedPos, closeSpeed * Time.deltaTime); // Use closeSpeed
        rightDoor.transform.localPosition = Vector3.MoveTowards(
            rightDoor.transform.localPosition, rightDoorClosedPos, closeSpeed * Time.deltaTime); // Use closeSpeed
        yield return null;
    }

    // Snap to exact closed positions
    leftDoor.transform.localPosition = leftDoorClosedPos;
    rightDoor.transform.localPosition = rightDoorClosedPos;
}
}

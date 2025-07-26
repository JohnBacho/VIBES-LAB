using UnityEngine;

public class DoorOpener : MonoBehaviour
{
    public GameObject Door;
    public Transform doorHinge;
    public float openAngle = -90f;
    public float speed = 4f;

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = doorHinge.rotation;
        openRotation = doorHinge.rotation * Quaternion.Euler(0, openAngle, 0);
    }

    void Update()
    {
        doorHinge.rotation = Quaternion.Slerp(doorHinge.rotation, isOpen ? openRotation : closedRotation, Time.deltaTime * speed);
    }

    public void ShutDoor()
    {
        speed = 20f;
        isOpen = false;
    }

    public void OpenDoor()
    {
        speed = 4f;
        isOpen = true;
    }
}

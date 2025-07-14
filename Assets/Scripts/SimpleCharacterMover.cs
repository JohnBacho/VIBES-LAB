using UnityEngine;

public class SimpleCharacterMover : MonoBehaviour
{
    public float startSpeed = 0.5f;
    public float endSpeed = 5f;
    public float scareDuration = 1f;
    public Transform head;
    public Transform playerCamera;
    public float heightOffset = 0.3f;
    public Transform Left;
    public Transform Middle;
    public Transform Right;

    private Transform targetPlayer;
    private float timer = 0f;
    private bool isScaring = false;
    private Vector3 originalPosition;

    private Quaternion originalRotation;
    

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    void Update()
    {
        if (isScaring && targetPlayer != null)
        {
            timer += Time.deltaTime;

            Vector3 lookPos = targetPlayer.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);

            transform.rotation *= Quaternion.Euler(0f, Random.Range(-5f, 5f), 0f);

            if (head != null && targetPlayer != null)
            {
                Vector3 directionToPlayer = targetPlayer.position - head.position;
                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);

                // Smooth the head turn
                head.rotation = Quaternion.Slerp(head.rotation, lookRotation, Time.deltaTime * 5f);
            }


            // Ramp speed
            float t = timer / scareDuration;
            float currentSpeed = Mathf.Lerp(startSpeed, endSpeed, t);

            // Move forward
            transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

            if (timer >= scareDuration)
            {
                isScaring = false;
            }
        }
    }

    // Call this from another script
    public void StartScare(StimulusLocation Position)
    {
        targetPlayer = playerCamera;
        timer = 0f;
        isScaring = true;

        Vector3 pos = transform.position;
        pos.y = playerCamera.position.y + heightOffset;
        transform.position = pos;

        switch (Position)
        {
            case StimulusLocation.Left:
                transform.position = Left.position;
                break;
            case StimulusLocation.Middle:
                transform.position = Middle.position;
                break;
            case StimulusLocation.Right:
                transform.position = Right.position;
                break;
        }
    }

    public void ResetPosition()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        isScaring = false;
        targetPlayer = null;
    }
}

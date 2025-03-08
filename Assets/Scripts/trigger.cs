using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TriggerWithDelay : MonoBehaviour
{
    [SerializeField] private string tagFilter;

    [Header("Initial Trigger Events")]
    [SerializeField] private UnityEvent onTriggerEnterEvent;
    [SerializeField] private UnityEvent onTriggerExitEvent;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip triggerSound;
    [SerializeField] private float initialAudioDuration = 8f; // Time before next action

    [Header("Delayed Actions")]
    [SerializeField] private UnityEvent delayedEvent;
    [SerializeField] private float delayBetweenActions = 0.3f; // Fine-tuning event timing

    [Header("Object Destruction")]
    [SerializeField] private GameObject objectToDestroy;
    [SerializeField] private float destroyAfterTime = 5f; // Time before object disappears

    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(tagFilter) && !other.gameObject.CompareTag(tagFilter)) return;

        // Play initial sound
        if (audioSource != null && triggerSound != null)
        {
            audioSource.PlayOneShot(triggerSound);
        }

        // Start the event sequence
        StartCoroutine(DelayedActionsRoutine());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!string.IsNullOrEmpty(tagFilter) && !other.gameObject.CompareTag(tagFilter)) return;

        onTriggerExitEvent.Invoke();
    }

    private IEnumerator DelayedActionsRoutine()
    {
        yield return new WaitForSeconds(initialAudioDuration);

        // Trigger first delayed event
        onTriggerEnterEvent.Invoke();
        
        yield return new WaitForSeconds(delayBetweenActions);

        // Trigger second delayed event
        delayedEvent.Invoke();

        // Destroy object after a set time
        if (objectToDestroy != null)
        {
            yield return new WaitForSeconds(destroyAfterTime);
            Destroy(objectToDestroy);
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TriggerWithDelay : MonoBehaviour
{
    [SerializeField] private bool destroyOnTriggerEnter;
    [SerializeField] private string tagFilter;

    [SerializeField] private UnityEvent onTriggerEnterEvent;
    [SerializeField] private UnityEvent onTriggerExitEvent;
    [SerializeField] private UnityEvent delayedEvent;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip triggerSound;
    [SerializeField] private float delay = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(tagFilter) && !other.gameObject.CompareTag(tagFilter)) return;

        // Play sound immediately
        if (audioSource != null && triggerSound != null)
        {
            audioSource.PlayOneShot(triggerSound);
        }

        // Start delayed sequence
        StartCoroutine(DelayedActionsRoutine());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!string.IsNullOrEmpty(tagFilter) && !other.gameObject.CompareTag(tagFilter)) return;

        onTriggerExitEvent.Invoke();
    }

    private IEnumerator DelayedActionsRoutine()
    {
        yield return new WaitForSeconds(delay);
        
        // Trigger both events after delay
        onTriggerEnterEvent.Invoke();
        delayedEvent.Invoke();

        // Destroy if needed
        if (destroyOnTriggerEnter)
        {
            Destroy(gameObject);
        }
    }
}
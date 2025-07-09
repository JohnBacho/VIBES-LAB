using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FearConditioningTrigger : MonoBehaviour
{
    [SerializeField] private string tagFilter;

    [Header("CS+ (Conditioned Stimulus)")]
    [SerializeField] private GameObject CSObject; // CS+ Object (Visual Cue)
    [SerializeField] private float CSDuration = 8f; // CS+ total duration

    [Header("US (Unconditioned Stimuli)")]
    [SerializeField] private float USStartDelay = 6f; // Time before US starts (during CS+)
    [SerializeField] private float USDuration = 2f; // US duration
    [SerializeField] private UnityEvent OnUSStart; // Event triggered when US starts
    [SerializeField] private UnityEvent OnUSStop; // Event triggered when US ends

    [Header("Events")]
    [SerializeField] private UnityEvent onTriggerEnterEvent;
    [SerializeField] private UnityEvent onTriggerExitEvent;
    [SerializeField] private UnityEvent delayedEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(tagFilter) && !other.gameObject.CompareTag(tagFilter)) return;

        onTriggerEnterEvent.Invoke();
        
        if (CSObject != null)
        {
            CSObject.SetActive(true);
        }

        StartCoroutine(ConditioningSequence());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!string.IsNullOrEmpty(tagFilter) && !other.gameObject.CompareTag(tagFilter)) return;

        onTriggerExitEvent.Invoke();
    }

    private IEnumerator ConditioningSequence()
    {
        yield return new WaitForSeconds(USStartDelay);

        OnUSStart.Invoke();

        yield return new WaitForSeconds(USDuration);

        OnUSStop.Invoke();

        float remainingCSDuration = CSDuration - USStartDelay - USDuration;
        if (remainingCSDuration > 0)
        {
            yield return new WaitForSeconds(remainingCSDuration);
        }

        if (CSObject != null)
        {
            CSObject.SetActive(false);
        }

        delayedEvent.Invoke();
    }
}
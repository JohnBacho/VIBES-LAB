using System.Collections;
using UnityEngine;
using ViveSR.anipal.Eye;

public class EyeTrackerManager : MonoBehaviour
{
    [Tooltip("Auto-launch calibration when the framework is ready")]
    public bool EyeCalibration = true;

    public float calibrationTimeout = 5f; // seconds
    public bool enableRetry = true;

    void Start()
    {
        if (EyeCalibration)
        {
            StartCoroutine(LaunchEyeCalibrationCoroutine());
        }
    }

    public IEnumerator LaunchEyeCalibrationCoroutine()
    {
        float timer = 0f;
        Debug.Log("[EyeTrackerManager] Waiting for SRanipal framework to become WORKING...");

        while (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING && timer < calibrationTimeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING)
        {
            Debug.LogError($"[EyeTrackerManager] SRanipal did not become WORKING after {calibrationTimeout} seconds. Current status: {SRanipal_Eye_Framework.Status}");
            yield break;
        }

        Debug.Log("[EyeTrackerManager] SRanipal is WORKING. Attempting eye calibration...");

        if (!TryLaunchEyeCalibration())
        {
            Debug.Log("[EyeTrackerManager] --> RETRYING calibration now.");

            if (enableRetry)
            {
                Debug.Log("[EyeTrackerManager] Retrying calibration in 0.5 seconds...");
                yield return new WaitForSeconds(0.5f);

                if (!TryLaunchEyeCalibration())
                {
                    Debug.LogError("[EyeTrackerManager] Calibration still failed after retry.");
                }
                else
                {
                    Debug.Log("[EyeTrackerManager] Calibration successful on retry.");
                }
            }
        }
        else
        {
            Debug.Log("[EyeTrackerManager] Eye calibration launched successfully.");
        }
    }

    private bool TryLaunchEyeCalibration()
    {
        if (SRanipal_Eye.LaunchEyeCalibration())
            return true;
        if (SRanipal_Eye_v2.LaunchEyeCalibration())
            return true;

        Debug.LogWarning("[EyeTrackerManager] Both v1 and v2 calibration calls returned false.");
        return false;
    }
}

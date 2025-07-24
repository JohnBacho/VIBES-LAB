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
            sxr.DisplayImage("EyeError");
            yield break;
        }

        Debug.Log("[EyeTrackerManager] SRanipal is WORKING. Attempting eye calibration...");

        bool calibrationLaunched = TryLaunchEyeCalibration();

        if (!calibrationLaunched && enableRetry)
        {
            Debug.Log("[EyeTrackerManager] Calibration failed. Retrying in 0.5 seconds...");
            yield return new WaitForSeconds(0.5f);
            calibrationLaunched = TryLaunchEyeCalibration();
        }

        if (calibrationLaunched)
        {
            Debug.Log("[EyeTrackerManager] Calibration launched. Waiting to verify success...");
            yield return StartCoroutine(WaitAndCheckEyeData());
        }
        else
        {
            Debug.LogError("[EyeTrackerManager] Calibration failed after retry.");
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

    private IEnumerator WaitAndCheckEyeData()
    {
        yield return new WaitForSeconds(5f); // Give time for user to complete calibration

        bool enableEye = SRanipal_Eye_Framework.Instance.EnableEye;

        VerboseData eyeData = new VerboseData();
        bool gotData = SRanipal_Eye.GetVerboseData(out eyeData);
        bool validEyeData = gotData &&
                            eyeData.left.eye_data_validata_bit_mask > 0 &&
                            eyeData.right.eye_data_validata_bit_mask > 0;

        if (enableEye || validEyeData)
        {
            Debug.Log("[EyeTrackerManager] Calibration likely successful: eye tracking is enabled OR valid eye data received.");
        }
        else
        {
            Debug.LogWarning("[EyeTrackerManager] Calibration may have failed: eye tracking disabled AND invalid eye data.");
            sxr.DisplayImage("EyeError");
        }
    }
}

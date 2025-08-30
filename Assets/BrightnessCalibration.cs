using UnityEngine;
using UnityEngine.Rendering;  // Needed for AmbientMode
using System.Collections.Generic;

public class BrightnessCalibration : MonoBehaviour
{
    public Light calibrationLight;  // The light used for brightness ramp
    public int steps = 9;
    public float duration = 60f;
    public Color calibrationAmbientColor = Color.black; // Ambient color during calibration

    private float stepDuration;
    private int currentStep;
    private float elapsed;
    private bool isRunning = false;

    private List<Light> disabledLights = new List<Light>();

    private AmbientMode originalAmbientMode;
    private Color originalAmbientColor;

    void Start()
    {
        stepDuration = duration / steps;
    }

    public void StartCalibration()
    {
        originalAmbientMode = RenderSettings.ambientMode;
        originalAmbientColor = RenderSettings.ambientLight;

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = calibrationAmbientColor;

        Light[] allLights = FindObjectsOfType<Light>();
        disabledLights.Clear();

        foreach (Light l in allLights)
        {
            if (l != calibrationLight && l.enabled)
            {
                l.enabled = false;
                disabledLights.Add(l);
            }
        }

        elapsed = 0f;
        currentStep = 0;
        isRunning = true;
    }

    public void RestoreLights()
    {
        foreach (Light l in disabledLights)
        {
            if (l != null) l.enabled = true;
        }
        disabledLights.Clear();
        calibrationLight.intensity = 0;
        RenderSettings.ambientMode = originalAmbientMode;
        RenderSettings.ambientLight = originalAmbientColor;
    }

    void Update()
{
    if (!isRunning) return;

    elapsed += Time.deltaTime;

    if (currentStep < steps && elapsed >= stepDuration * currentStep)
    {
        // Compute step size directly
        float stepSize = (8f / (steps - 1) )*0.5f; // how much intensity each step should add
        calibrationLight.intensity = stepSize * currentStep;
        sxr.SetStage("PupilCalibration:" + currentStep.ToString());
        currentStep++;
    }

    if (currentStep >= steps)
    {
        isRunning = false;
        calibrationLight.intensity = 8f; // ensure it ends at max
        Debug.Log("Brightness calibration finished.");
    }
}

}

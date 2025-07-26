using UnityEngine;
using UnityEngine.XR;

public class DynamicResolutionManager : MonoBehaviour
{
    public float targetFPS = 90f;
    public float minScale = 0.7f;
    public float maxScale = 1.0f;
    public int sampleCount = 20;
    public float lerpSpeed = 1.5f;

    private float[] frameTimes;
    private int frameIndex = 0;
    private float smoothedScale;
    private bool isVR;

    private void Start()
    {
        frameTimes = new float[sampleCount];
        smoothedScale = maxScale;

        isVR = XRSettings.isDeviceActive;

        Application.targetFrameRate = (int)targetFPS;
        QualitySettings.vSyncCount = 0;

        if (isVR)
            XRSettings.eyeTextureResolutionScale = maxScale;
        else
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
    }

    private void Update()
    {
        // Frame time averaging
        frameTimes[frameIndex] = Time.unscaledDeltaTime;
        frameIndex = (frameIndex + 1) % sampleCount;

        float totalTime = 0f;
        foreach (float t in frameTimes)
            totalTime += t;
        float avgFPS = sampleCount / totalTime;

        float targetScale = Mathf.Clamp(avgFPS / targetFPS, minScale, maxScale);
        smoothedScale = Mathf.Lerp(smoothedScale, targetScale, Time.unscaledDeltaTime * lerpSpeed);

        if (isVR)
        {
            XRSettings.eyeTextureResolutionScale = smoothedScale;
        }
        else
        {
            // Dynamically scale 2D screen resolution
            int width = Mathf.RoundToInt(Screen.currentResolution.width * smoothedScale);
            int height = Mathf.RoundToInt(Screen.currentResolution.height * smoothedScale);
            Screen.SetResolution(width, height, true);
        }
    }
}

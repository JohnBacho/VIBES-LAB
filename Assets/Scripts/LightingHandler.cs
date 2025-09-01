using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingHandler : MonoBehaviour
{
    public GameObject SpotLight;
    public GameObject Glow;

    private Color originalLightMatColor;
    private Color originalColor;
    public bool Stop = false;

    void Start()
    {
        if (Glow.TryGetComponent<Renderer>(out var renderer))
        {
            originalLightMatColor = renderer.material.color;
        }

        if (SpotLight.TryGetComponent<Light>(out var light))
        {
            originalColor = light.color;
        }
    }

    public void ChangeLightColor(Color newColor)
    {
        if (SpotLight.TryGetComponent<Light>(out var light))
        {
            light.color = newColor;
            light.intensity = 2.2f;
        }

        ChangeColorTo(newColor);
    }

    public void ChangeColorTo(Color newColor)
    {
        if (Glow.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.SetColor("_EmissionColor", newColor);
        }
    }

    public void ReduceLightIntensity()
    {
        if (SpotLight.TryGetComponent<Light>(out var light))
        {
            light.intensity = 0f;
        }

        ChangeColorTo(Color.black);
    }


    public void ResetLight()
    {
        Stop = true;
        if (Glow.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.SetColor("_EmissionColor", originalLightMatColor);
        }

        if (SpotLight.TryGetComponent<Light>(out var light))
        {
            light.color = originalColor;
            light.intensity = 2.2f;
            light.range = 3f;
        }
    }

    public IEnumerator PatternLight(Color newColor)
    {
        const float duration = 2.0f; 
        const float maxIntensity = 3f;
        const float minIntensity = 0.1f;

        if (!SpotLight.TryGetComponent<Light>(out var light)) yield break;
        if (!Glow.TryGetComponent<Renderer>(out var renderer)) yield break;

        Material glowMat = renderer.material;

        while (!Stop)
        {
            float t = 0f;
            while (t < duration && !Stop)
            {
                t += Time.deltaTime;
                float pulse = Mathf.PingPong(t * (maxIntensity - minIntensity), maxIntensity - minIntensity) + minIntensity;

                light.intensity = pulse;
                light.color = newColor;

                glowMat.SetColor("_EmissionColor", newColor * pulse);

                yield return null;
            }
        }

        yield break;
    }



}

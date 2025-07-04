using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingHandler : MonoBehaviour
{
    public GameObject SpotLight;
    public GameObject Glow;

    private Color originalLightMatColor;
    private Color originalColor;

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
        if (Glow.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.SetColor("_EmissionColor", originalLightMatColor);
        }

        if (SpotLight.TryGetComponent<Light>(out var light))
        {
            light.color = originalColor;
            light.intensity = 1.5f;
        }
    }
}

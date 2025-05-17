using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(XRBaseInteractable))]
public class ChangeMaterialOnHover : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    public bool IsSelected => isSelected;


    public event Action<bool> OnBoolChanged; // Sends data over to memory.cs

    [Header("Materials")]
    public Material originalMaterial;
    public Material hoverMaterial;
    public Material selectedMaterial;
    public Material hoverMaterialSelected;
    private bool isSelected = false;



    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        if (originalMaterial == null)
            originalMaterial = meshRenderer.material;

        var interactable = GetComponent<XRBaseInteractable>();
        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
        interactable.selectEntered.AddListener(OnSelectEnter);
    }

    void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (!isSelected && hoverMaterial != null)
        {
            meshRenderer.material = hoverMaterial;
        }
        if (isSelected && hoverMaterial != null)
        {
            meshRenderer.material = hoverMaterialSelected;
        }
    }

    void OnHoverExit(HoverExitEventArgs args)
    {
        meshRenderer.material = isSelected ? selectedMaterial : originalMaterial;
    }

    void OnSelectEnter(SelectEnterEventArgs args)
    {
        isSelected = !isSelected;
        OnBoolChanged?.Invoke(isSelected); // 👈 Fire the event
        meshRenderer.material = isSelected ? selectedMaterial : originalMaterial;
    }
}

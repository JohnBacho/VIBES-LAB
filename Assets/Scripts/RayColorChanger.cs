using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RayColorChanger : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    public XRBaseController controller; // XRController for input

    void Update()
    {
        if (rayInteractor != null && rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            // Check trigger press
            if (controller.selectInteractionState.active)
            {
                GameObject hitObject = hit.collider.gameObject;

                if (hitObject.CompareTag("ColorTarget"))
                {
                    MeshRenderer renderer = hitObject.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = Color.red;
                    }
                }
            }
        }
    }
}

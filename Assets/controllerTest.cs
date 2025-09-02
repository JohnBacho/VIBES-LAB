using UnityEngine;
using UnityEngine.InputSystem;

public class ViveTriggerTest : MonoBehaviour
{
    void Update()
    {
        var gamepads = InputSystem.devices;
        foreach (var dev in gamepads)
        {
            if (dev is UnityEngine.InputSystem.XR.XRController controller)
            {
                if (controller.TryGetChildControl<UnityEngine.InputSystem.Controls.AxisControl>("trigger") is var trigger && trigger != null)
                {
                    Debug.Log("Trigger value: " + trigger.ReadValue());
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple Script made by Jace that turns the controller on and off
public class VRControllerHandler : MonoBehaviour
{

    public GameObject RIGHTContorl;
    public GameObject RIGHTControlStabilized;
    public GameObject RightEnviormentController;
    public GameObject RightControllerEnvironmentStabilized;
    private bool controllerOn = false;


    public void ToggleController()
    {
        controllerOn = !controllerOn;

        RIGHTContorl.SetActive(controllerOn);
        RIGHTControlStabilized.SetActive(controllerOn);
        RightEnviormentController.SetActive(controllerOn);
        RightControllerEnvironmentStabilized.SetActive(controllerOn);

    }
}

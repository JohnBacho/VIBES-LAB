using System.Collections;
using System.Collections.Generic;
using UnityEngine;
﻿using System;
using System.IO;
using Unity.VisualScripting;
public class AbortRun : MonoBehaviour
{
    private float holdDuration = 0.0f;
    private bool ImageIsDisplayed = false;
    // Update is called once per frame
    void Update()
    {
        if (sxr.CheckController(sxr_internal.ControllerButton.Trigger) || Input.GetAxis("Fire1") > 0 || Input.GetMouseButton((int)MouseButton.Left))
        {
            holdDuration += Time.deltaTime;
            if (holdDuration >= 5.0f) // 5 second hold
            {
                Debug.Log("aborting run");
                Application.Quit(); // Ends the experiment
            }

            if (holdDuration >= 2.5f)
            {
                sxr.DisplayImage("WarningMessage");
                ImageIsDisplayed = true;

            }
        }
        else
        {
            holdDuration = 0.0f;
            if (ImageIsDisplayed)
            {
                sxr.HideImagesUI();
                ImageIsDisplayed = false;
            }
        }
    }
}

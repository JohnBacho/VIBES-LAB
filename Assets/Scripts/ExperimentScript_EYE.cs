using UnityEngine;
using ViveSR.anipal.Eye;
﻿using sxr_internal;

namespace SampleExperimentScene
{
    public class ExperimentScript_EYE : MonoBehaviour
    {
        public bool EyeCalibration; // toggle for eye tracking
        int trialCounter = 0;
        bool hasStarted = false; // Flag to track if movement should start in vr mover script

        private string currentFocus = "";
        private Ray testRay;
        private FocusInfo focusInfo;
        private Vector3 gazeHitPoint;
        private bool hasExecuted = false;
        private bool StartEyeTracker = false;
        private string headers = "GazeHitPointX,GazeHitPointY,GazeHitPointZ";

        void Start() // set to true in the inspector if you would like to auto launch SRanipal
        {

            if (EyeCalibration)
            {
                sxr.LaunchEyeCalibration();
            }
        }

        void CheckFocus()
        {

            currentFocus = "";

            if (SRanipal_Eye.Focus(GazeIndex.COMBINE, out testRay, out focusInfo)) { }
            else if (SRanipal_Eye.Focus(GazeIndex.LEFT, out testRay, out focusInfo)) { }
            else if (SRanipal_Eye.Focus(GazeIndex.RIGHT, out testRay, out focusInfo)) { }
            else return;

            currentFocus = focusInfo.collider.gameObject.name;
            sxr.ChangeExperimenterTextbox(4, "Current Game Object: " + currentFocus);

            gazeHitPoint = focusInfo.point;
            sxr.ChangeExperimenterTextbox(5, "Gaze Hit Position: " + gazeHitPoint);

            sxr.WriteToTaggedFile("mainFile", gazeHitPoint.ToString());

            // Vector3 screenPoint = Camera.main.WorldToScreenPoint(gazeHitPoint);
            // Debug.Log("Screen coordinates: " + screenPoint);
        }

        void Update()
        {

            if(StartEyeTracker){
            CheckFocus();
            }

            //var gazeInfo = sxr.GetFullGazeInfo();
            //sxr.ChangeExperimenterTextbox(5, "Gaze Info: " + gazeInfo);

            switch (sxr.GetPhase())
            {
                case 0: // Start Screen Phase
                
                    break;

                case 1: // Instruction Phase
                 StartEyeTracker = true;
                 sxr.StartRecordingCameraPos();
                 sxr.StartRecordingEyeTrackerInfo();
                    if(!hasExecuted){
                    sxr.WriteHeaderToTaggedFile("mainFile", headers);
                    sxr.StartTimer(20);
                    sxr.DisplayText("In this experiment, you will see different colored shapes in the 3d environment. Please look at the screen at all times. You will also hear loud sounds. There may or may not be a relationship between the colored shapes and the loud sounds.");
                    hasExecuted = true;
                    }
                    
                    if(sxr.CheckTimer()){
                        sxr.HideAllText();
                        sxr.MoveObjectTo("sXR_prefab", 23.0f, 0f, 0f);
                    }

                    switch (sxr.GetStepInTrial())
                    {
                        case 0:
                            sxr.HideImagesUI();
                            break;
                        case 1:
                            break;
                    }
                    break;
            }
        }
    }
}
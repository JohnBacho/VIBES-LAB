using UnityEngine;
using ViveSR.anipal.Eye;

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

        void Start() // set to true in the inspector if you would like to auto launch SRanipal
        {

            sxr.StartRecordingCameraPos();
            sxr.StartRecordingEyeTrackerInfo();
            if (EyeCalibration)
            {
                sxr.DebugLog("Launching Eye Calibration");
                sxr.LaunchEyeCalibration();

                if (sxr.LaunchEyeCalibration()) // checks the SRanipal api to see (pun intended) if eye calibration was successful
                {
                    sxr.DebugLog("Eye Calibration Successful");
                    return;
                }
                else
                {
                    sxr.DebugLog("Eye Calibration Failed");
                    Debug.Break(); // if calibration fails unity project will pause to stop project from going on without eyetracking data
                }
            }
            else
            {
                sxr.DebugLog("Skipping Eye Calibration");
                return;
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
            sxr.ChangeExperimenterTextbox(6, "Current Game Object: " + currentFocus);

        }
        void Update()
        {

            CheckFocus();
            var gazeInfo = sxr.GetFullGazeInfo();
            sxr.ChangeExperimenterTextbox(4, "Gaze Info: " + gazeInfo);

            switch (sxr.GetPhase())
            {
                case 0: // Start Screen Phase
                    sxr.ChangeExperimenterTextbox(5, ""); // gazeinfo was overlapping with textbox 5
                    break;

                case 1: // Instruction Phase

                    if (trialCounter < 1)
                    {
                        FindObjectOfType<VRCameraPathMover>().StartMoving(); // talks to the VRCameraPathMover script and triggers the StartMoving Function
                        sxr.StartTimer(50); // Start a 50s trial timer
                        trialCounter++;
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
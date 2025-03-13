using UnityEngine;

namespace SampleExperimentScene
{
    public class ExperimentScript : MonoBehaviour
    {
        public bool EyeCalibration; // toggle for eye tracking
        int trialCounter = 0;
        bool hasStarted = false; // Flag to track if movement should start in vr mover script

        void Start() // set to true in the inspector if you would like to auto launch SRanipal
        {
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


        void Update()
        {
            var gazeInfo = sxr.GetFullGazeInfo();
            sxr.ChangeExperimenterTextbox(4, "Gaze Info: " + gazeInfo);

            switch (sxr.GetPhase())
            {
                case 0: // Start Screen Phase
                    sxr.StartRecordingCameraPos();
                    sxr.StartRecordingEyeTrackerInfo();
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
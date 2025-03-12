using UnityEngine;

namespace SampleExperimentScene
{
    public class ExperimentScript : MonoBehaviour
    {
        int trialCounter = 0;
        bool hasStarted = false; // Flag to track if movement should start in vr mover script

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
                    hasStarted = true;
                    FindObjectOfType<VRCameraPathMover>().StartMoving(); // talks to the VRCameraPathMover script and triggers the StartMoving Function
                    if (trialCounter < 1)
                    {
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

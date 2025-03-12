using UnityEngine;

namespace SampleExperimentScene
{
    public class ExperimentScript : MonoBehaviour
    {

        void Update()
        {
            var gazeInfo = sxr.GetFullGazeInfo();
            sxr.ChangeExperimenterTextbox(4, "Gaze Info: " + gazeInfo);
            switch (sxr.GetPhase())
            {
                case 0: // Start Screen Phase
                    sxr.StartRecordingCameraPos();
                    sxr.StartRecordingEyeTrackerInfo();
                    sxr.ChangeExperimenterTextbox(5, "");
                    // gazeinfo was overlapping with textbox 5
                    break;

                case 1: // Instruction Phase
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
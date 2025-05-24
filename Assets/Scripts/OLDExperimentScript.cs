using UnityEngine;
using ViveSR.anipal.Eye;

namespace SampleExperimentScene
{
    public class ExperimentScript : MonoBehaviour
    {
        public bool EyeCalibration; // toggle for eye tracking
        int trialCounter = 0;
        private string currentFocus = "";
        private Ray testRay;
        private FocusInfo focusInfo;

        void Start() // set to true in the inspector if you would like to auto launch SRanipal
        {

            sxr.StartRecordingCameraPos();
            sxr.StartRecordingEyeTrackerInfo();
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

        }
        void Update()
        {


            CheckFocus();
            var gazeInfo = sxr.GetFullGazeInfo();
            sxr.ChangeExperimenterTextbox(5, "Gaze Info: " + gazeInfo);

            switch (sxr.GetPhase())
            {
                case 0: // Start Screen Phase
                    break;

                case 1: // Instruction Phase

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
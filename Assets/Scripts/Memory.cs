using UnityEngine;
using ViveSR.anipal.Eye;
using sxr_internal;
using System.Collections; // Required for IEnumerator
using UnityEditor;


namespace SampleExperimentScene
{
    public class Memory : MonoBehaviour
    {
        public bool EyeCalibration; // toggle for eye tracking
        public GameObject InteractiveUI;
        private bool[] lastKnownStates;
        private int score;
        public ChangeMaterialOnHover[] targets;
        public GameObject RightController;
        public GameObject RightControllerStablized;
        public Material ChangeCubeColor;
        public Material OringalCubeColor;
        public GameObject[] cubes;

        private float ResponseTime;
        private PreviewRenderUtility previewRenderUtility; // Stops unity from bitching
        private bool hasExecuted = false; //  used as a way to execute one block of code only once
        private string CubeHeaders;
        private bool[] CubeValues;
        private bool[] Pattern1 = new bool[] { true, false, true, false, true,
                                               false, true, false, true, true,
                                               false, true, false, true, true,
                                               false, true, false, true, true,
                                               false, true, false, true, true };
        private bool[] Pattern2 = new bool[] { true, false, true, false, true,
                                               false, true, false, true, false,
                                               false, true, false, false, false,
                                               false, false, false, true, false,
                                               false, true, false, true, true };
        private bool[] Pattern3 = new bool[] { true, false, false, true, true,
                                               false, true, false, false, true,
                                               true, true, false, false, true,
                                               false, false, true, true, false,
                                               false, true, false, true, false
                                            };

        private bool isPressed = false;

        public void SetPressedTrue()
        {
            isPressed = true;
        }

        public void DisplayPattern(bool[] cubeFlags, int SetTimer)
        {
            if (cubeFlags.Length != cubes.Length)
            {
                Debug.LogError("cubeFlags doesn't match the length of targets");
            }
            sxr.StartTimer(SetTimer);

            for (int i = 0; i < cubeFlags.Length; i++)
            {
                if (cubeFlags[i])
                {
                    ChangeColor(cubes[i], ChangeCubeColor);
                }
            }
            hasExecuted = true;

        }

        public void ParticipantInteraction()
        {
            RightController.SetActive(true);
            RightControllerStablized.SetActive(true);
            for (int i = 0; i < cubes.Length; i++)
            {
                ChangeColor(cubes[i], OringalCubeColor);
            }
            InteractiveUI.SetActive(true);
            sxr.StartTimer(999);
            hasExecuted = true;

        }

        public void ButtonPressed(bool[] Answers)
        {
            ResponseTime = sxr.TimePassed();
            InteractiveUI.SetActive(false);
            isPressed = false;
            for (int i = 0; i < targets.Length; i++)
            {
                bool currentState = targets[i].IsSelected;
                CubeValues[i] = currentState;

                if (currentState == Answers[i])
                {
                    score++;
                }
                targets[i].ResetState();
            }
            Debug.Log(score);
            InteractiveUI.SetActive(false);
            isPressed = false;
            sxr.WriteToTaggedFile("CubeFile", string.Join(",", CubeValues) + "," + score.ToString() + ',' + ResponseTime.ToString());
            score = 0;
            RightController.SetActive(false);
            RightControllerStablized.SetActive(false);
        }


        public void ChangeColor(GameObject Cube, Material color)
        {
            Renderer renderer = Cube.GetComponent<Renderer>();
            renderer.material = color;
        }

        public void InterTrial(float InterTrialIntervalTime)  // used to wait till start of next trial
        {
            if (!hasExecuted)
            {
                sxr.StartTimer(InterTrialIntervalTime); // // inter trial interval time
                hasExecuted = true; // sets has Executed Flag to true so that it only executes once
            }

            if (sxr.CheckTimer())
            {
                sxr.NextStep();
                hasExecuted = false; // sets has Executed Flag to false for the next trial
            }
        }


        void HandleBoolChanged(bool isSelected)
        {
            Debug.Log("ChangeMaterialOnHover changed: " + isSelected);
        }

        // Coroutine to disable the object after a delay

        void HandleSelectionChanged(bool isNowSelected)
        {
            Debug.Log("Selection state changed to: " + isNowSelected);
        }

        void OnDestroy()
        {
            if (targets != null)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i] != null)
                    {
                        targets[i].OnBoolChanged -= HandleSelectionChanged;
                    }
                }
            }

            if (previewRenderUtility != null)
            {
                previewRenderUtility.Cleanup();
                previewRenderUtility = null;
            }
        }

        void Start()
        {
            CubeValues = new bool[targets.Length];
            lastKnownStates = new bool[targets.Length];
            for (int i = 0; i < targets.Length; i++)
            {
                lastKnownStates[i] = targets[i].IsSelected;
            }


            if (EyeCalibration) // set to true in the inspector if you would like to auto launch SRanipal eye tracker calibration
            {
                sxr.LaunchEyeCalibration();
            }

            for (int i = 0; i < cubes.Length; i++)
            {
                CubeHeaders += "Cube" + (i + 1) + ",";
            }

            CubeHeaders += "Score,ResponseTime";
        }

        void Update()
        {

            switch (sxr.GetPhase()) // gets the phase
            {
                case 0: // Start Screen Phase
                    break;

                case 1: // Instruction Phase
                    RightController.SetActive(false);
                    RightControllerStablized.SetActive(false);
                    sxr.StartRecordingCameraPos();
                    sxr.StartRecordingEyeTrackerInfo();
                    if (!hasExecuted)
                    {
                        sxr.WriteHeaderToTaggedFile("CubeFile", CubeHeaders);
                        hasExecuted = true; // set to true so this block of code only runs once
                        sxr.StartTimer(8);
                    }

                    if (sxr.CheckTimer()) // checks if the timer has reached zero
                    {
                        sxr.HideAllText();
                        sxr.NextPhase(); // go to the next phase and set has Executed to false
                        hasExecuted = false;
                    }
                    break;

                case 2:
                    switch (sxr.GetTrial())
                    {

                        case 0:
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:
                                    if (!hasExecuted)
                                    {
                                        DisplayPattern(Pattern1, 3);
                                    }

                                    if (sxr.CheckTimer())
                                    {
                                        hasExecuted = false;
                                        sxr.NextStep();
                                    }
                                    break;

                                case 1:
                                    if (!hasExecuted)
                                    {
                                        ParticipantInteraction();
                                    }

                                    if (isPressed)
                                    {
                                        ButtonPressed(Pattern1);
                                        hasExecuted = false;
                                        sxr.NextTrial();
                                    }

                                    break;
                            }
                            break;

                        case 1:
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:
                                    InterTrial(3);
                                    break;

                                case 1:
                                    if (!hasExecuted)
                                    {
                                        DisplayPattern(Pattern2, 5);
                                    }

                                    if (sxr.CheckTimer())
                                    {
                                        hasExecuted = false;
                                        sxr.NextStep();
                                    }

                                    break;
                                case 2:
                                    if (!hasExecuted)
                                    {
                                        ParticipantInteraction();
                                    }

                                    if (isPressed)
                                    {
                                        ButtonPressed(Pattern2);
                                        hasExecuted = false;
                                        sxr.NextTrial();
                                    }
                                    break;

                            }
                            break;

                        case 2:
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:
                                    InterTrial(3);
                                    break;

                                case 1:
                                    if (!hasExecuted)
                                    {
                                        DisplayPattern(Pattern3, 4);
                                    }

                                    if (sxr.CheckTimer())
                                    {
                                        hasExecuted = false;
                                        sxr.NextStep();
                                    }

                                    break;
                                case 2:
                                    if (!hasExecuted)
                                    {
                                        ParticipantInteraction();
                                    }

                                    if (isPressed)
                                    {
                                        ButtonPressed(Pattern3);
                                        hasExecuted = false;
                                        sxr.NextTrial();
                                    }
                                    break;

                            }
                            break;

                        case 3: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    break;

                                case 1: // inter trial interval
                                    if (!hasExecuted)
                                    {
                                        sxr.StartTimer(5); // // inter trial interval time
                                        hasExecuted = true; // sets has Executed Flag to true so that it only executes once
                                    }

                                    if (sxr.CheckTimer())
                                    {
                                        sxr.NextPhase(); // Goes to the next trial
                                        hasExecuted = false; // sets has Executed Flag to false for the next trial
                                    }
                                    break;

                            }
                            break;

                    }
                    break; // End of phase case 2
            }
        }

    }
}

using UnityEngine;
using ViveSR.anipal.Eye;
using sxr_internal;
using System.Collections; // Required for IEnumerator


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
        public GameObject Cube1;
        public GameObject Cube2;
        public GameObject Cube3;
        public GameObject Cube4;
        public GameObject Cube5;
        public GameObject Cube6;
        public GameObject Cube7;
        public GameObject Cube8;
        public GameObject Cube9;

        private string FocusedGameObject = ""; // used for Sranipal
        private float ResponseTime;
        private Ray testRay; // used for Sranipal
        private FocusInfo focusInfo; // used for Sranipal
        private Vector3 gazeHitPoint; // used in calculating eye tracking data with collisions
        private bool hasExecuted = false; //  used as a way to execute one block of code only once
        private bool hasStartedCS = false; // used to execute the start of the CS+ only once
        private bool StartEyeTracker = false; //used to start the CheckFocus(); function which calculates the eye 
        // tracking data ensuring that all three cameratrack / eyetracker / mainfile are all started at the exact same time
        private string headers = "GazeHitPointX,GazeHitPointY,GazeHitPointZ,GameObjectInFocus"; // used to write headers to the mainfile
        private string CubeHeaders = "Cube1,Cube2,Cube3,Cube4,Cube5,Cube6,Cube7,Cube8,Cube9,Score";
        private bool[] CubeValues;
        private bool[] Answers1 = new bool[] { true, false, true, false, true, false, true, false, true };

        public void StartCS(GameObject CS_Object, GameObject CS_Sound, float CS_Sound_Delay, float CS_Object_Interval, float timeUntilCSStarts, float TotalTrialTimeCs)
        {
            if (!hasExecuted)
            {
                sxr.StartTimer(TotalTrialTimeCs); // sets the timer based on TimeBeforeCS + TimeAfterCS;
                hasExecuted = true;
            }

            // since TimeRemaining is a float point it doesn't exactly reach ie 10s on the dot instead it's 10.0123s so we have to do less than zero and hasStartedCSPlus/Minus is so it only executes once
            if (!hasStartedCS && (sxr.TimeRemaining() - timeUntilCSStarts) <= 0)
            {
                // Activate object and play sound after delay
                hasStartedCS = true;
                CS_Object.SetActive(true);
                StartCoroutine(DisableObjects(CS_Object, CS_Object_Interval)); // calls function to deactivate sound with delay
            }

            if (sxr.CheckTimer()) // checks if timer is zero
            {
                sxr.NextStep(); // advances to inter trial interval and sets hasExecuted to false
                hasExecuted = false;
                hasStartedCS = false;
            }
        }

        private bool isPressed = false;

        public void SetPressedTrue()
        {
            isPressed = true;
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
                sxr.NextTrial(); // Goes to the next trial
                hasExecuted = false; // sets has Executed Flag to false for the next trial
            }
        }


        void HandleBoolChanged(bool isSelected)
        {
            Debug.Log("ChangeMaterialOnHover changed: " + isSelected);
            // Do your logic here
        }

        // Coroutine to disable the object after a delay
        IEnumerator DisableObjects(GameObject objectToDisable, float ObjectDelay)
        {
            yield return new WaitForSeconds(ObjectDelay); // Delay determines how long it should wait to deactivate object
            if (objectToDisable != null)
            {
                objectToDisable.SetActive(false); // will deactivate object
            }
            else
            {
                Debug.LogWarning("The GameObject to disable is null!"); // error handling
            }
        }

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
        }


        void CheckFocus() // Main driver of eye tracking 
        {

            FocusedGameObject = "";

            if (SRanipal_Eye.Focus(GazeIndex.COMBINE, out testRay, out focusInfo)) { }
            else if (SRanipal_Eye.Focus(GazeIndex.LEFT, out testRay, out focusInfo)) { }
            else if (SRanipal_Eye.Focus(GazeIndex.RIGHT, out testRay, out focusInfo)) { }
            else return;

            FocusedGameObject = focusInfo.collider.gameObject.name; // gets the name of the object that the player is currently looking at
            sxr.ChangeExperimenterTextbox(4, "Current Game Object: " + FocusedGameObject); // displays the object currently being looked at on the text box

            gazeHitPoint = focusInfo.point; // gets the X / Y / Z coordinate of where the player is looking
            sxr.ChangeExperimenterTextbox(5, "Gaze Hit Position: " + gazeHitPoint); // displays the X / Y / Z coordinates currently being looked at on the text box


            string DataPoints = (gazeHitPoint.ToString() + "," + FocusedGameObject);
            sxr.WriteToTaggedFile("mainFile", DataPoints); // // saves the gazehitpoint which is the gaze with object collision and also 
            // FocusedGameObject which is the name of the object where the user is looking at to file 
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
        }

        void Update()
        {

            if (StartEyeTracker)
            {
                CheckFocus();
            }

            switch (sxr.GetPhase()) // gets the phase
            {
                case 0: // Start Screen Phase
                    break;

                case 1: // Instruction Phase
                    RightController.SetActive(false);
                    RightControllerStablized.SetActive(false);
                    StartEyeTracker = true;
                    sxr.StartRecordingCameraPos();
                    sxr.StartRecordingEyeTrackerInfo();
                    if (!hasExecuted)
                    {
                        sxr.WriteHeaderToTaggedFile("mainFile", headers);
                        sxr.WriteHeaderToTaggedFile("CubeFile", CubeHeaders);
                        hasExecuted = true; // set to true so this block of code only runs once
                        sxr.StartTimer(5);
                    }

                    if (sxr.CheckTimer()) // checks if the timer has reached zero
                    {
                        sxr.HideAllText();
                        sxr.NextPhase(); // go to the next phase and set has Executed to false
                        hasExecuted = false;
                    }
                    break;

                case 2: // Habituation Phase
                    switch (sxr.GetTrial())
                    {

                        case 0: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    if (!hasExecuted)
                                    {
                                        sxr.StartTimer(3);
                                        ChangeColor(Cube1, ChangeCubeColor);
                                        ChangeColor(Cube5, ChangeCubeColor);
                                        ChangeColor(Cube9, ChangeCubeColor);
                                        ChangeColor(Cube7, ChangeCubeColor);
                                        ChangeColor(Cube3, ChangeCubeColor);
                                        hasExecuted = true;
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
                                        RightController.SetActive(true);
                                        RightControllerStablized.SetActive(true);
                                        ChangeColor(Cube1, OringalCubeColor);
                                        ChangeColor(Cube5, OringalCubeColor);
                                        ChangeColor(Cube9, OringalCubeColor);
                                        ChangeColor(Cube7, OringalCubeColor);
                                        ChangeColor(Cube3, OringalCubeColor);
                                        InteractiveUI.SetActive(true);
                                        sxr.StartTimer(99);
                                        hasExecuted = true;
                                    }

                                    if (isPressed)
                                    {
                                        ResponseTime = sxr.TimePassed();
                                        InteractiveUI.SetActive(false);
                                        isPressed = false;
                                        Debug.Log("Button Pressed .......................");
                                        for (int i = 0; i < targets.Length; i++)
                                        {
                                            bool currentState = targets[i].IsSelected;
                                            CubeValues[i] = currentState;

                                            if (currentState == true && Answers1[i] == true)
                                            {
                                                score++;
                                            }
                                        }

                                        Debug.Log(score);
                                        InteractiveUI.SetActive(false);
                                        isPressed = false;
                                        sxr.WriteToTaggedFile("CubeFile", string.Join(",", CubeValues) + "," + score.ToString() + ',' + ResponseTime.ToString());
                                        sxr.NextTrial();
                                    }

                                    break;
                            }
                            break;

                        case 1: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(14f);
                                    break;
                            }
                            break;

                        case 2: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(10f);

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

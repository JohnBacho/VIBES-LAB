using UnityEngine;
using ViveSR.anipal.Eye;
using sxr_internal;
using System.Collections; // Required for IEnumerator


namespace SampleExperimentScene
{
    public class ExperimentScript_2 : MonoBehaviour
    {
        public bool EyeCalibration; // toggle for eye tracking
        public bool InstructionPhase; // Set to true to toggle on gaze rays to see in real time where user is looking
        public GameObject CS_plus_Object; // drag and drop CS+ object
        public float CS_plus_Object_Interval; // Enter time for CS+ object to stay active
        public GameObject CS_minus_Object; // drag and drop CS- object
        public float CS_minus_Object_Interval; // Enter time for CS- object to stay active
        public GameObject US_Sound; // drag and drop CS+ sound to get it to play
        public float US_Sound_Delay; // Enter time for sound delay to play after CS+ object is activated.
        public bool ABATesting; // Used to determine what context you want Extinction to 
        public Color ABA_Environment_Color = Color.green; // set the color of the room for the ABA testing

        // drag and drop room objects into this so that it can change the color for ABA testing
        public GameObject Ceiling;
        public GameObject LeftWall;
        public GameObject RightWall;
        public GameObject BackWall;
        public GameObject FrontWall;
        public GameObject Floor;

        public GameObject RIGHTContorl;
        public GameObject RIGHTControlStabilized;
        public GameObject RightEnviormentController;
        public GameObject RightControllerEnvironmentStabilized;
        public GameObject NoSoundObject;
        public float NoSoundDelay;


        private string FocusedGameObject = ""; // used for Sranipal
        private Ray testRay; // used for Sranipal
        private FocusInfo focusInfo; // used for Sranipal
        private Vector3 gazeHitPoint; // used in calculating eye tracking data with collisions
        private bool hasExecuted = false; //  used as a way to execute one block of code only once
        private bool hasStartedCS = false; // used to execute the start of the CS+ only once
        private bool StartEyeTracker = false; //used to start the CheckFocus(); function which calculates the eye 
        // tracking data ensuring that all three cameratrack / eyetracker / mainfile are all started at the exact same time
        private string headers = "GazeHitPointX,GazeHitPointY,GazeHitPointZ,GameObjectInFocus"; // Used to write headers to the mainfile
        private string Anticipateheaders = "Anticipated,ResponseTime"; // Used to write headers to Anticipatedfile
        private float TotalTrialTimeCsPlus; //Used to calculate the total time of the trial for CS Plus trial
        private float TotalTrialTimeCsMinus; //Used to calculate the total time of the trial for CS Minus trial
        private float timeUntilCSMinusStarts; // Used to calculate when the when to display CS minus object
        private float timeUntilCSPlusStarts; // Used to calculate when the when to display CS plus object
        private int AnticipatedNumber; // Used for when the user enters if they anticipated US
        private bool userInputComplete = false; // Used for a check if the user has submitted a value  
        private float TimeForUserToRespond = 999; // Used to determine how long the user has to respond
        private float WaitTimeTillUserInput = 5; // Used to determine how long to wait into the CS to display Slider

        private Color originalCeilingColor;
        private Color originalLeftWallColor;
        private Color originalRightWallColor;
        private Color originalBackWallColor;
        private Color originalFrontWallColor;
        private Color originalFloorColor;


        public void StartCS(GameObject CS_Object, GameObject CS_Sound, float CS_Sound_Delay, float CS_Object_Interval, bool GetAnticipation)
        {
            if (!hasExecuted)
            {
                sxr.StartTimer(CS_Object_Interval); // sets the timer
                hasExecuted = true;
            }

            // since TimeRemaining is a float point it doesn't exactly reach ie 10s on the dot instead it's 10.0123s so we have to do less than zero so it only executes once
            if (!hasStartedCS)
            {
                // Activate object and play sound after delay
                hasStartedCS = true;
                CS_Object.SetActive(true);
                StartCoroutine(PlaySoundAfterDelay(CS_Sound, CS_Sound_Delay, GetAnticipation)); // calls function to play sound with delay
                StartCoroutine(DisableObjects(CS_Object, CS_Object_Interval, GetAnticipation)); // calls function to deactivate sound with delay
            }

            if (sxr.CheckTimer()) // checks if timer is zero
            {
                sxr.NextStep(); // advances to inter trial interval and sets hasExecuted and hasStartedCS to false
                hasExecuted = false;
                hasStartedCS = false;
            }
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

        // Coroutine to play the sound after a delay
        IEnumerator PlaySoundAfterDelay(GameObject soundObject, float soundDelay, bool waitForUserInput)
        {
            if (waitForUserInput)
            {

                while (!userInputComplete) // waits for the user to input a response into input 
                {
                    yield return null; // Wait until input is complete
                }

                // Wait the rest of the delay if any
                if (WaitTimeTillUserInput < soundDelay)
                {
                    yield return new WaitForSeconds(soundDelay - WaitTimeTillUserInput);
                }

                AudioSource audioSource = soundObject.GetComponent<AudioSource>(); // grabs audio source from object
                if (audioSource != null)
                {
                    audioSource.Play(); // plays sound
                }
                else
                {
                    Debug.LogWarning("No AudioSource found on " + soundObject.name);
                }

            }
            else
            {
                yield return new WaitForSeconds(soundDelay); // soundDelay determines how long it should wait to play the sound
                AudioSource audioSource = soundObject.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.Play(); // plays audio attached to object
                }
                else
                {
                    Debug.LogWarning("No AudioSource found on " + soundObject.name); // error handling
                }
            }
        }

        // Coroutine to disable the object after a delay
        IEnumerator DisableObjects(GameObject objectToDisable, float objectDelay, bool waitForUserInput)
        {
            if (waitForUserInput)
            {
                yield return new WaitForSeconds(WaitTimeTillUserInput); // waits 5 seconds
                float TempStoreTime = sxr.TimeRemaining(); // stores the trial timer so that it can be restored later

                sxr.StartTimer(TimeForUserToRespond); // starts a new timer for 50s allowing the user to respond 
                Debug.Log("Paused before disabling object. Waiting for user input...");
                // enables the user to move the right controller 
                RIGHTContorl.SetActive(true);
                RIGHTControlStabilized.SetActive(true);
                RightEnviormentController.SetActive(true);
                RightControllerEnvironmentStabilized.SetActive(true);

                int TempStoreAnticipateNum = -1;
                while (!sxr.ParseInputUI(out AnticipatedNumber))
                {
                    sxr.InputSlider(0, 9, $"How likely is it that a scream will follow? 0 (certainly no scream) to 9 (certainly a scream) [{AnticipatedNumber}]", true); // displays slider that user can input 
                    TempStoreAnticipateNum = AnticipatedNumber; // for some reason I am unable to get anticipatedNumber to save to file out side of the loop so we create a new var to save it 
                    Debug.Log($"User entered: {AnticipatedNumber}");
                    yield return null;
                }
                userInputComplete = true; // this bool is used to tell PlaySoundAfterDelay that it can continue with it's delay.

                // disables the Right Controller
                RIGHTContorl.SetActive(false);
                RIGHTControlStabilized.SetActive(false);
                RightEnviormentController.SetActive(false);
                RightControllerEnvironmentStabilized.SetActive(false);

                float ResponseTime = TimeForUserToRespond - sxr.TimeRemaining(); // used to calculate response time
                sxr.StartTimer(TempStoreTime); // restores the original timer 
                sxr.WriteToTaggedFile("AnticipateFile", TempStoreAnticipateNum.ToString() + "," + ResponseTime.ToString()); // writes user response as well as response time to AnticipateFile


                // Wait remaining time if any
                if (objectDelay > WaitTimeTillUserInput)
                {
                    yield return new WaitForSeconds(objectDelay - WaitTimeTillUserInput);
                    objectToDisable.SetActive(false); // will deactivate object
                }
            }
            else
            {
                yield return new WaitForSeconds(objectDelay);
            }

            if (objectToDisable != null)
            {
                objectToDisable.SetActive(false);
            }
            else
            {
                Debug.LogWarning("The GameObject to disable is null!");
            }


            userInputComplete = false; // rests flag
        }


        void CheckFocus()
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

        public void ChangeColorTo(GameObject obj, Color newColor) // changes each object individually to the color specified in the inspector
        {
            Renderer objRenderer = obj.GetComponent<Renderer>();
            if (objRenderer != null)
            {
                objRenderer.material.color = newColor;
            }
        }

        public void ChangeAllColors(Color newColor) // changes all colors to the color entered in the inspector
        {
            ChangeColorTo(Ceiling, newColor);
            ChangeColorTo(LeftWall, newColor);
            ChangeColorTo(RightWall, newColor);
            ChangeColorTo(BackWall, newColor);
            ChangeColorTo(FrontWall, newColor);
            ChangeColorTo(Floor, newColor);
        }

        public void RevertAllColors() // reverts all the objects back to there original colors 
        {
            ChangeColorTo(Ceiling, originalCeilingColor);
            ChangeColorTo(LeftWall, originalLeftWallColor);
            ChangeColorTo(RightWall, originalRightWallColor);
            ChangeColorTo(BackWall, originalBackWallColor);
            ChangeColorTo(FrontWall, originalFrontWallColor);
            ChangeColorTo(Floor, originalFloorColor);
        }

        void Start()
        {

            if (EyeCalibration) // set to true in the inspector if you would like to auto launch SRanipal eye tracker calibration
            {
                sxr.LaunchEyeCalibration();
            }

            // used to save the original color of the object before it changes them for the B part of ABA testing
            originalCeilingColor = Ceiling.GetComponent<Renderer>().material.color;
            originalLeftWallColor = LeftWall.GetComponent<Renderer>().material.color;
            originalRightWallColor = RightWall.GetComponent<Renderer>().material.color;
            originalBackWallColor = BackWall.GetComponent<Renderer>().material.color;
            originalFrontWallColor = FrontWall.GetComponent<Renderer>().material.color;
            originalFloorColor = Floor.GetComponent<Renderer>().material.color;


            // error handling
            if (NoSoundDelay > CS_minus_Object_Interval || US_Sound_Delay > CS_plus_Object_Interval)
            {
                Debug.LogWarning("CS minus or CS plus sound delay should be less than CS plus object_interval");
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
                    StartEyeTracker = true;
                    sxr.StartRecordingCameraPos();
                    sxr.StartRecordingEyeTrackerInfo();
                    if (InstructionPhase)
                    { // Dr. Thomas wanted the InstructionPhase to be toggleable 
                        if (!hasExecuted)
                        {
                            sxr.WriteHeaderToTaggedFile("mainFile", headers);
                            sxr.WriteHeaderToTaggedFile("AnticipateFile", Anticipateheaders);
                            sxr.StartTimer(20);
                            sxr.DisplayText("In this experiment, you will see different colored shapes in the 3d environment. Please keep your focus on the screen at all times. You will also hear loud sounds. There may or may not be a relationship between the colored shapes and the loud sounds.");
                            hasExecuted = true; // set to true so this block of code only runs once
                        }

                        if (sxr.CheckTimer()) // checks if the timer has reached zero
                        {
                            sxr.HideAllText();
                            sxr.NextPhase(); // go to the next phase and set has Executed to false
                            hasExecuted = false;
                        }
                    }
                    else
                    {
                        sxr.WriteHeaderToTaggedFile("mainFile", headers);
                        sxr.WriteHeaderToTaggedFile("AnticipateFile", Anticipateheaders);
                        sxr.NextPhase();
                    }

                    break;

                case 2: // Habituation Phase
                    switch (sxr.GetTrial())
                    {

                        case 0: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(9f);
                                    break;
                            }
                            break;

                        case 1: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
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
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, true);
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
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    if (!hasExecuted)
                                    {
                                        sxr.StartTimer(12f); // // inter trial interval time
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

                case 3: // Fear Acquisition training
                    switch (sxr.GetTrial())
                    {
                        case 0: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    if (ABATesting)
                                    {
                                        ChangeAllColors(ABA_Environment_Color);
                                    }
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(11f);
                                    break;
                            }
                            break;

                        case 1: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
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
                                    StartCS(CS_plus_Object, US_Sound, US_Sound_Delay, CS_plus_Object_Interval, true);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(9f);
                                    break;
                            }
                            break;
                        case 3:  // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS+
                                    StartCS(CS_plus_Object, US_Sound, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(15f);

                                    break;
                            }
                            break;
                        case 4:  // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(12f);
                                    break;
                            }
                            break;
                        case 5:   // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS+
                                    StartCS(CS_plus_Object, US_Sound, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(10f);
                                    break;
                            }
                            break;
                        case 6:   // CS+ without US
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS+ without US
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(13f);
                                    break;
                            }
                            break;
                        case 7:  // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(11f);
                                    break;
                            }
                            break;
                        case 8:   // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS+
                                    StartCS(CS_plus_Object, US_Sound, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(9f);
                                    break;
                            }
                            break;
                        case 9:  // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(12f);
                                    break;
                            }
                            break;
                        case 10:   // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS+
                                    StartCS(CS_plus_Object, US_Sound, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(15f);
                                    break;
                            }
                            break;
                        case 11:  // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(12f);
                                    break;
                            }
                            break;
                        case 12:  // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(10f);
                                    break;
                            }
                            break;
                        case 13:   // CS+ without US
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS+ without US
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(13f);
                                    break;
                            }
                            break;
                        case 14:  // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(9f);
                                    break;
                            }
                            break;
                        case 15:   // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS+
                                    StartCS(CS_plus_Object, US_Sound, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    if (!hasExecuted)
                                    {
                                        sxr.StartTimer(12); // // inter trial interval time
                                        hasExecuted = true; // sets has Executed Flag to true so that it only executes once
                                    }

                                    if (sxr.CheckTimer())
                                    {
                                        sxr.NextPhase(); // Goes to the next Phase
                                        hasExecuted = false; // sets has Executed Flag to false for the next trial
                                        if (ABATesting)
                                        {
                                            RevertAllColors();
                                        }
                                    }
                                    break;
                            }
                            break;


                    }
                    break; // End of phase case 3

                case 4: // Fear Extinction
                    switch (sxr.GetTrial())
                    {
                        case 0: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(12f);
                                    break;
                            }
                            break;

                        case 1: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(10f);

                                    break;
                            }
                            break;
                        case 2: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(15f);

                                    break;
                            }
                            break;
                        case 3: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(9f);
                                    break;
                            }
                            break;
                        case 4: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(11f);
                                    break;
                            }
                            break;
                        case 5: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(14f);

                                    break;
                            }
                            break;
                        case 6: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(13f);
                                    break;
                            }
                            break;
                        case 7: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(10f);

                                    break;
                            }
                            break;
                        case 8: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(15f);

                                    break;
                            }
                            break;
                        case 9: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(12f);
                                    break;
                            }
                            break;
                        case 10: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(9f);

                                    break;
                            }
                            break;
                        case 11: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(12f);
                                    break;
                            }
                            break;
                        case 12: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(10f);
                                    break;
                            }
                            break;
                        case 13: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(13f);

                                    break;
                            }
                            break;
                        case 14: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(14f);

                                    break;
                            }
                            break;
                        case 15: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(10f);
                                    break;
                            }
                            break;
                        case 16: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(12f);

                                    break;
                            }
                            break;
                        case 17: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(15f);
                                    break;
                            }
                            break;
                        case 18: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(11f);
                                    break;
                            }
                            break;
                        case 19: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(13f);

                                    break;
                            }
                            break;
                        case 20: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(10f);

                                    break;
                            }
                            break;
                        case 21: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(14f);
                                    break;
                            }
                            break;
                        case 22: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(9f);

                                    break;
                            }
                            break;
                        case 23: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(12f);

                                    break;
                            }
                            break;
                        case 24: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(15f);
                                    break;
                            }
                            break;
                        case 25: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(11f);

                                    break;
                            }
                            break;
                        case 26: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(13f);

                                    break;
                            }
                            break;
                        case 27: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(10f);
                                    break;
                            }
                            break;
                        case 28: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(14f);
                                    break;
                            }
                            break;
                        case 29: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(12f);

                                    break;
                            }
                            break;
                        case 30: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(9f);
                                    break;
                            }
                            break;
                        case 31: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(15f);

                                    break;
                            }
                            break;
                        case 32: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(11f);
                                    break;
                            }
                            break;
                        case 33: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(13f);

                                    break;
                            }
                            break;
                        case 34: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(10f);

                                    break;
                            }
                            break;
                        case 35: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(14f);
                                    break;
                            }
                            break;
                        case 36: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(12f);

                                    break;
                            }
                            break;
                        case 37: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(13f);
                                    break;
                            }
                            break;
                        case 38: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(CS_minus_Object, NoSoundObject, NoSoundDelay, CS_minus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(9f);
                                    break;
                            }
                            break;
                        case 39: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, NoSoundObject, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    if (!hasExecuted)
                                    {
                                        sxr.DisplayText("Experiment Complete. Thank You!");
                                        hasExecuted = true;
                                    }
                                    InterTrial(55f);
                                    break;
                            }
                            break;
                    }
                    break; // End of phase case 4
            }

        }
    }
}

using UnityEngine;
using sxr_internal;
using System.Collections; // Required for IEnumerator
using UnityEditor;


namespace SampleExperimentScene
{
    public class ExperimentScriptV4 : MonoBehaviour
    {
        public bool InstructionPhase; // Set to true to toggle on gaze rays to see in real time where user is looking
        public Color CS_Plus_LightColor = Color.blue; // set the color of the room for the ABA testing
        public bool CSPlusPattern = false; // used to determine if to make the light flash a pattern or not
        public float CS_plus_Object_Interval; // Enter time for CS+ object to stay active
        public Color CS_Minus_LightColor = Color.green; // set the color of the room for the ABA testing
        public bool CSMinusPattern = false; // used to determine if to make the light flash a pattern or not
        public float CS_minus_Object_Interval; // Enter time for CS- object to stay active
        public GameObject US_Sound; // drag and drop US sound to get it to play
        public GameObject US_Sound2; // drag and drop US sound to get it to play
        public GameObject US_Sound3; // drag and drop US sound to get it to play
        public GameObject US_Object; // drag and drop the object you want to be the US
        public float US_Sound_Delay; // Enter time for sound delay to play after CS+ object is activated.
        public bool ABATesting; // Used to determine what context you want Extinction to 

        public DoorOpener doorOpenerLeft; // Pulls in the door script that enables the door to open and close
        public DoorOpener doorOpenerMiddle; // Pulls in the door script that enables the door to open and close
        public DoorOpener doorOpenerRight; // Pulls in the door script that enables the door to open and close

        public VRControllerHandler controllerHandler; // handles when the controller is on and when it is off
        public SimpleCharacterMover characterMover; // handles how the monster should move

        public LightingHandler lightHandlerLeft; // handles how the light should work
        public LightingHandler lightHandlerMiddle; // handles how the light should work
        public LightingHandler lightHandlerRight; // handles how the light should work

        private bool hasExecuted = false; //  used as a way to execute one block of code only once
        private bool hasStartedCS = false; // used to execute the start of the CS+ only once
        private string Anticipateheaders = "Anticipated,ResponseTime"; // Used to write headers to Anticipatedfile
        private float TotalTrialTimeCsPlus; //Used to calculate the total time of the trial for CS Plus trial
        private float TotalTrialTimeCsMinus; //Used to calculate the total time of the trial for CS Minus trial
        private float timeUntilCSMinusStarts; // Used to calculate when the when to display CS minus object
        private float timeUntilCSPlusStarts; // Used to calculate when the when to display CS plus object
        private int AnticipatedNumber; // Used for when the user enters if they anticipated US
        private bool userInputComplete = false; // Used for a check if the user has submitted a value  
        private float TimeForUserToRespond = 999; // Used to determine how long the user has to respond
        private float WaitTimeTillUserInput = 5; // Used to determine how long to wait into the CS to display Slider
        private int InstructionSlider = 0; // Used for slider
        private LightingHandler LightSCRIPT; // Used to dynamically change the lighting script
        private DoorOpener DoorOpenerSCRIPT; // Used to dynamically change the door script
        private string result;


        public void StartCS(bool IsCSPlus, string Position, bool PlaySound, float CS_Sound_Delay, float CS_Object_Interval, bool GetAnticipation)
        {
            if (!hasExecuted)
            {
                sxr.StartTimer(CS_Object_Interval); // sets the timer


                switch (Position)
                {
                    case "Left":
                        LightSCRIPT = lightHandlerLeft;
                        DoorOpenerSCRIPT = doorOpenerLeft;
                        lightHandlerMiddle.ReduceLightIntensity();
                        lightHandlerRight.ReduceLightIntensity();
                        result = IsCSPlus ? "Left_CS+" : "Left_CS-";
                        sxr.SetStage(result);
                        break;
                    case "Middle":
                        LightSCRIPT = lightHandlerMiddle;
                        DoorOpenerSCRIPT = doorOpenerMiddle;
                        lightHandlerRight.ReduceLightIntensity();
                        lightHandlerLeft.ReduceLightIntensity();
                        result = IsCSPlus ? "Middle_CS+" : "Middle_CS-";
                        sxr.SetStage(result);
                        break;
                    case "Right":
                        LightSCRIPT = lightHandlerRight;
                        DoorOpenerSCRIPT = doorOpenerRight;
                        lightHandlerLeft.ReduceLightIntensity();
                        lightHandlerMiddle.ReduceLightIntensity();
                        result = IsCSPlus ? "Right_CS+" : "Right_CS-";
                        sxr.SetStage(result);
                        break;
                }
                hasExecuted = true;
            }

            if (!hasStartedCS)
            {
                // Activate object and play sound after delay
                hasStartedCS = true;
                Color LightColor = IsCSPlus ? CS_Plus_LightColor : CS_Minus_LightColor;
                LightSCRIPT.Stop = false;
                if (CSPlusPattern && IsCSPlus || CSMinusPattern && !IsCSPlus)
                {
                    StartCoroutine(LightSCRIPT.PatternLight(LightColor));
                }
                else
                {
                    LightSCRIPT.ChangeLightColor(LightColor);
                }

                StartCoroutine(PlaySoundAfterDelay(PlaySound, CS_Sound_Delay, GetAnticipation)); // calls function to play sound with delay
                StartCoroutine(DisableObjects(CS_Object_Interval, GetAnticipation)); // calls function to deactivate sound with delay
                StartCoroutine(JumpScare(US_Object, PlaySound, CS_Sound_Delay, GetAnticipation, Position));
            }

            if (sxr.CheckTimer()) // checks if timer is zero
            {
                DoorOpenerSCRIPT.ShutDoor();
                sxr.NextStep(); // advances to inter trial interval and sets hasExecuted and hasStartedCS to false
                hasExecuted = false;
                hasStartedCS = false;
            }
        }

        public void InterTrial(float InterTrialIntervalTime)  // used to wait till start of next trial
        {
            sxr.SetStage("InterTrial");
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
        IEnumerator PlaySoundAfterDelay(bool PlaySound, float soundDelay, bool waitForUserInput)
        {
            AudioSource audioSource = US_Sound.GetComponent<AudioSource>(); // grabs audio source from object
            AudioSource audioSource2 = US_Sound2.GetComponent<AudioSource>();
            AudioSource audioSource3 = US_Sound3.GetComponent<AudioSource>();
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

                if (audioSource != null && PlaySound)
                {
                    sxr.SetStage("US");
                    US_Object.SetActive(true);
                    audioSource.Play(); // plays sound
                    audioSource2.Play();
                    audioSource3.Play();
                }
            }
            else
            {
                yield return new WaitForSeconds(soundDelay); // soundDelay determines how long it should wait until it plays the sound
                if (audioSource != null && PlaySound)
                {
                    sxr.SetStage("US");
                    US_Object.SetActive(true);
                    audioSource.Play(); // plays sound
                    audioSource2.Play();
                    audioSource3.Play();
                }
            }
        }


        IEnumerator JumpScare(GameObject US_Object, bool PlaySound, float soundDelay, bool waitForUserInput, string position)
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
                if (PlaySound)
                {
                    DoorOpenerSCRIPT.OpenDoor();
                    characterMover.ResetPosition();
                    characterMover.StartScare(position);
                }

                yield return new WaitForSeconds(1); // waits for 1 second
                if (PlaySound)
                {
                    characterMover.ResetPosition();
                    US_Object.SetActive(false);
                }
            }
            else
            {
                yield return new WaitForSeconds(soundDelay); // soundDelay determines how long it should wait to play the sound
                if (PlaySound)
                {
                    DoorOpenerSCRIPT.OpenDoor();
                    characterMover.ResetPosition();
                    characterMover.StartScare(position);
                    US_Object.SetActive(true);
                }

                yield return new WaitForSeconds(1); // waits for 1 second
                if (PlaySound)
                {
                    characterMover.ResetPosition();
                    US_Object.SetActive(false);
                }
            }
        }

        // Coroutine to disable the object after a delay
        IEnumerator DisableObjects(float objectDelay, bool waitForUserInput)
        {
            if (waitForUserInput)
            {
                yield return new WaitForSeconds(WaitTimeTillUserInput); // waits 5 seconds
                float TempStoreTime = sxr.TimeRemaining(); // stores the trial timer so that it can be restored later

                sxr.StartTimer(TimeForUserToRespond); // starts a new timer for 50s allowing the user to respond 
                Debug.Log("Paused before disabling object. Waiting for user input...");
                // enables the user to move the right controller 
                controllerHandler.ToggleController();

                int TempStoreAnticipateNum = -1;
                string storeStage = sxr.GetStage();
                sxr.SetStage("InputSlider");
                while (!sxr.ParseInputUI(out AnticipatedNumber))
                {
                    sxr.InputSlider(0, 9, $"How likely is it that a scream will follow? 0 (certainly no scream) to 9 (certainly a scream) [{AnticipatedNumber}]", true); // displays slider that user can input 
                    TempStoreAnticipateNum = AnticipatedNumber; // for some reason I am unable to get anticipatedNumber to save to file out side of the loop so we create a new var to save it 
                    Debug.Log($"User entered: {AnticipatedNumber}");
                    yield return null;
                }
                userInputComplete = true; // this bool is used to tell PlaySoundAfterDelay that it can continue with it's delay.
                sxr.SetStage(storeStage);
                // disables the Right Controller
                controllerHandler.ToggleController();

                float ResponseTime = TimeForUserToRespond - sxr.TimeRemaining(); // used to calculate response time
                sxr.StartTimer(TempStoreTime); // restores the original timer 
                sxr.WriteToTaggedFile("AnticipateFile", TempStoreAnticipateNum.ToString() + "," + ResponseTime.ToString()); // writes user response as well as response time to AnticipateFile


                // Wait remaining time if any
                if (objectDelay > WaitTimeTillUserInput)
                {
                    yield return new WaitForSeconds(objectDelay - WaitTimeTillUserInput);
                    RestAllLights();
                }
            }
            else
            {
                yield return new WaitForSeconds(objectDelay);
            }
            RestAllLights();
            userInputComplete = false; // rests flag
        }

        public void RestAllLights()
        {
            lightHandlerLeft.ResetLight();
            lightHandlerMiddle.ResetLight();
            lightHandlerRight.ResetLight();
        }

        public void ChangeColorTo(GameObject obj, Color newColor) // changes each object individually to the color specified in the inspector
        {
            Renderer objRenderer = obj.GetComponent<Renderer>();
            if (objRenderer != null)
            {
                objRenderer.material.color = newColor;
            }
        }

        void Start()
        {
            // error handling
            if (US_Sound_Delay > CS_plus_Object_Interval)
            {
                Debug.LogWarning("CS minus or CS plus sound delay should be less than CS plus object_interval");
            }

        }

        void Update()
        {
            switch (sxr.GetPhase()) // gets the phase
            {
                case 0: // Start Screen Phase
                    break;

                case 1: // Instruction Phase
                    sxr.StartRecordingCameraPos();
                    sxr.StartRecordingEyeTrackerInfo();
                    switch (sxr.GetStepInTrial())
                    {
                        case 0: // CS+
                            sxr.SetStage("InstructionPhase");
                            if (InstructionPhase)
                            { // Dr. Thomas wanted the InstructionPhase to be toggleable 
                                if (!hasExecuted)
                                {
                                    sxr.WriteHeaderToTaggedFile("AnticipateFile", Anticipateheaders);
                                    sxr.StartTimer(20);
                                    sxr.DisplayText("In this experiment, you will see different colored shapes in the 3d environment. Please keep your focus on the screen at all times. You will also hear loud sounds. There may or may not be a relationship between the colored shapes and the loud sounds.");
                                    hasExecuted = true; // set to true so this block of code only runs once
                                }

                                if (sxr.CheckTimer()) // checks if the timer has reached zero
                                {
                                    sxr.HideAllText();
                                    sxr.NextStep(); // go to the next phase and set has Executed to false
                                    hasExecuted = false;
                                }
                            }
                            else
                            {
                                sxr.WriteHeaderToTaggedFile("AnticipateFile", Anticipateheaders);
                                sxr.NextStep();
                            }
                            break;

                        case 1: // trigger image
                            sxr.DisplayImage("trigger");
                            if (sxr.GetTrigger())
                            {
                                sxr.HideImagesUI();
                                sxr.NextStep();

                            }
                            break;
                        case 2: // display slider
                            if (!hasExecuted)
                            {
                                controllerHandler.ToggleController();
                                hasExecuted = true;

                            }
                            sxr.InputSlider(0, 9, $"Using the Controller and Trigger Adjust the value to 9 and click submit [{InstructionSlider}]", true); // displays slider that user can input 
                            if (sxr.ParseInputUI(out InstructionSlider))
                            {
                                controllerHandler.ToggleController();
                                sxr.NextPhase();
                                hasExecuted = false;
                            }
                            break;

                    }

                    break;

                case 2: // Habituation Phase
                    switch (sxr.GetTrial())
                    {

                        case 0: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Right", false, US_Sound_Delay, CS_plus_Object_Interval, true);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                        // PlaceHolder
                                    }
                                    StartCS(false, "Middle", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(false, "Right", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Left", true, US_Sound_Delay, CS_plus_Object_Interval, true);
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
                                    StartCS(true, "Right", true, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Middle", true, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Middle", true, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Middle", true, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Middle", true, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                            // Placeholder
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(false, "Left", false, 7, CS_minus_Object_Interval, false);
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
                                    StartCS(true, "Middle", false, US_Sound_Delay, CS_plus_Object_Interval, false);
                                    break;

                                case 1: // inter trial interval
                                    if (!hasExecuted)
                                    {
                                        sxr.DisplayText("Experiment Complete. Thank You!");
                                        hasExecuted = true;
                                    }
                                    InterTrial(10f);
                                    if (sxr.CheckTimer())
                                    {
                                        EditorApplication.isPlaying = false;
                                    }
                                    break;
                            }
                            break;
                    }
                    break; // End of phase case 4
            }

        }
    }
}

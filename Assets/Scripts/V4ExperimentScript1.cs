using UnityEngine;
using sxr_internal;
using System.Collections;
using UnityEditor;
public enum StimulusLocation
{
    Left,
    Middle,
    Right
}
public enum StimulusType
{
    CS_Plus,
    CS_Minus
}

public enum ContextTest
{
    AAA,
    BBB,
    ABA,
    BAB,
}

public enum ContextType
{
    A,
    B,
}

namespace SampleExperimentScene
{
    public class ExperimentScriptV4 : MonoBehaviour
    {
        public bool InstructionPhase; // Set to true to toggle on gaze rays to see in real time where user is looking
        public Color CSPlusLightColor = Color.blue; // set the color of the room for the ABA testing
        public bool CSPlusDisplayPattern = false; // used to determine if to make the light flash a pattern or not
        public Color CSMinusLightColor = Color.green; // set the color of the room for the ABA testing
        public bool CSMinusDisplayPattern = false; // used to determine if to make the light flash a pattern or not
        public GameObject USSound; // drag and drop US sound to get it to play
        public GameObject USSound2; // drag and drop US sound to get it to play
        public GameObject USSound3; // drag and drop US sound to get it to play
        public GameObject USObject; // drag and drop the object you want to be the US
        public ContextTest ContextTest = ContextTest.AAA;
        private ContextType ActiveContext;
        public GameObject ContextA; // used for context switch in ABA
        public GameObject ContextB; // used for context switch in ABA
        public VRControllerHandler controllerHandler; // handles when the controller is on and when it is off
        public SimpleCharacterMover characterMover; // handles how the monster should move
        public ScriptHandler scriptHandler; // manages the doors, elevator, and light scripts
        private bool HasExecuted = false; //  used as a way to execute one block of code only once
        private bool HasStartedCS = false; // used to execute the start of the CS+ only once
        private string Anticipateheaders = "Anticipated,ResponseTime"; // Used to write headers to Anticipatedfile
        private int AnticipatedNumber; // Used for when the user enters if they anticipated US
        private bool UserInputComplete = false; // Used for a check if the user has submitted a value  
        private const float TimeForUserToRespond = 999; // Used to determine how long the user has to respond
        private const float DisplayTimeBeforeSlider = 5; // Used to determine how long to wait into the CS to display Slider
        private const float DisplayDuration = 8; // Determines how long the CS is displayed on screen for
        private const float TimeUntilUnconditionedStimulusSound = 7; // Determines how long to wait into a trial to activate the US
        private int InstructionSliderValue = 0; // Used for instruction slider

        public void StartCS(StimulusType type, StimulusLocation position, bool ActivateUS, bool GetAnticipation)
        {
            if (!HasExecuted)
            {
                sxr.StartTimer(DisplayDuration); // sets the timer
                scriptHandler.AssignLightingAndDoorControllerForStimulusLocation(position, ActiveContext);

                string label = position.ToString();
                string csType = type == StimulusType.CS_Plus ? "CS+" : "CS-";

                string result = $"{label}_{csType}";
                sxr.SetStage(result);

                HasExecuted = true;
            }

            if (!HasStartedCS)
            {
                // Activate object and play sound after delay
                HasStartedCS = true;
                Color LightColor = type == StimulusType.CS_Plus ? CSPlusLightColor : CSMinusLightColor;
                scriptHandler.SetStop();
                if (CSPlusDisplayPattern && type == StimulusType.CS_Plus || CSMinusDisplayPattern && !(type == StimulusType.CS_Plus))
                {
                    scriptHandler.StartLightPattern(LightColor);
                }
                else
                {
                    scriptHandler.ChangeLightColor(LightColor);
                }

                StartCoroutine(PlaySoundAfterDelay(ActivateUS, GetAnticipation)); // calls function to play sound with delay
                StartCoroutine(DisableObjects(GetAnticipation)); // calls function to deactivate sound with delay
                StartCoroutine(JumpScare(USObject, ActivateUS, GetAnticipation, position));
            }

            if (sxr.CheckTimer()) // checks if timer is zero
            {
                scriptHandler.TriggerEntryClose(ActiveContext);
                sxr.NextStep(); // advances to inter trial interval and sets HasExecuted and HasStartedCS to false
                HasExecuted = false;
                HasStartedCS = false;
            }
        }

        public void InterTrial(float InterTrialWaitTime)  // used to wait till start of next trial
        {
            sxr.SetStage("InterTrial");
            if (!HasExecuted)
            {
                sxr.StartTimer(InterTrialWaitTime); // // inter trial interval time
                HasExecuted = true; // sets has Executed Flag to true so that it only executes once
            }

            if (sxr.CheckTimer())
            {
                sxr.NextTrial(); // Goes to the next trial
                HasExecuted = false; // sets has Executed Flag to false for the next trial
            }
        }

        // Coroutine to play the sound after a delay
        IEnumerator PlaySoundAfterDelay(bool ActivateUS, bool waitForUserInput)
        {
            AudioSource audioSource = USSound.GetComponent<AudioSource>(); // grabs audio source from object
            AudioSource audioSource2 = USSound2.GetComponent<AudioSource>();
            AudioSource audioSource3 = USSound3.GetComponent<AudioSource>();
            if (waitForUserInput)
            {

                while (!UserInputComplete)
                {
                    yield return null; // Wait until input is complete
                }

                // Wait the rest of the delay if any
                if (DisplayTimeBeforeSlider < TimeUntilUnconditionedStimulusSound)
                {
                    yield return new WaitForSeconds(TimeUntilUnconditionedStimulusSound - DisplayTimeBeforeSlider);
                }

                if (audioSource != null && ActivateUS)
                {
                    sxr.SetStage("US");
                    USObject.SetActive(true);
                    audioSource.Play(); // plays sound
                    audioSource2.Play();
                    audioSource3.Play();
                }
            }
            else
            {
                yield return new WaitForSeconds(TimeUntilUnconditionedStimulusSound); // TimeUntilUnconditionedStimulusSound determines how long it should wait into a trial to play US
                if (audioSource != null && ActivateUS)
                {
                    sxr.SetStage("US");
                    USObject.SetActive(true);
                    audioSource.Play(); // plays sound
                    audioSource2.Play();
                    audioSource3.Play();
                }
            }
        }


        IEnumerator JumpScare(GameObject USObject, bool ActivateUS, bool waitForUserInput, StimulusLocation position)
        {
            if (waitForUserInput)
            {

                while (!UserInputComplete) // waits for the user to input a response into input 
                {
                    yield return null; // Wait until input is complete
                }

                // Wait the rest of the delay if any
                if (DisplayTimeBeforeSlider < TimeUntilUnconditionedStimulusSound)
                {
                    yield return new WaitForSeconds(TimeUntilUnconditionedStimulusSound - DisplayTimeBeforeSlider);
                }
                if (ActivateUS)
                {
                    scriptHandler.TriggerEntryOpen(ActiveContext);
                    characterMover.ResetPosition();
                    characterMover.StartScare(position);
                }

                yield return new WaitForSeconds(1); // waits for 1 second
                if (ActivateUS)
                {
                    characterMover.ResetPosition();
                    USObject.SetActive(false);
                }
            }
            else
            {
                yield return new WaitForSeconds(TimeUntilUnconditionedStimulusSound); // TimeUntilUnconditionedStimulusSound determines how long it should wait to play the sound
                if (ActivateUS)
                {
                    scriptHandler.TriggerEntryOpen(ActiveContext);
                    characterMover.ResetPosition();
                    characterMover.StartScare(position);
                    USObject.SetActive(true);
                }

                yield return new WaitForSeconds(1); // waits for 1 second
                if (ActivateUS)
                {
                    characterMover.ResetPosition();
                    USObject.SetActive(false);
                }
            }
        }

        // Coroutine to disable the object after a delay
        IEnumerator DisableObjects(bool waitForUserInput)
        {
            if (waitForUserInput)
            {
                yield return new WaitForSeconds(DisplayTimeBeforeSlider); // waits 5 seconds
                float TempStoreTime = sxr.TimeRemaining(); // stores the trial timer so that it can be restored later

                sxr.StartTimer(TimeForUserToRespond); // starts a new timer for 999s allowing the user to respond 
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
                UserInputComplete = true; // this bool is used to tell PlaySoundAfterDelay that it can continue with it's delay.
                sxr.SetStage(storeStage);
                // disables the Right Controller
                controllerHandler.ToggleController();

                float ResponseTime = TimeForUserToRespond - sxr.TimeRemaining(); // used to calculate response time
                sxr.StartTimer(TempStoreTime); // restores the original timer 
                sxr.WriteToTaggedFile("AnticipateFile", TempStoreAnticipateNum.ToString() + "," + ResponseTime.ToString()); // writes user response as well as response time to AnticipateFile


                // Wait remaining time if any
                if (DisplayDuration > DisplayTimeBeforeSlider)
                {
                    yield return new WaitForSeconds(DisplayDuration - DisplayTimeBeforeSlider);
                    scriptHandler.RestAllLights();
                }
            }
            else
            {
                yield return new WaitForSeconds(DisplayDuration);
            }
            scriptHandler.RestAllLights();
            UserInputComplete = false; // rests flag
        }

        void Start()
        {
            switch (ContextTest)
            {
                case ContextTest.AAA:
                    ActiveContext = ContextType.A;
                    break;
                case ContextTest.BBB:
                    ActiveContext = ContextType.B;
                    break;
                case ContextTest.ABA:
                    ContextA.SetActive(true);
                    ContextB.SetActive(false);
                    ActiveContext = ContextType.A;
                    break;
                case ContextTest.BAB:
                    ContextA.SetActive(false);
                    ContextB.SetActive(true);
                    ActiveContext = ContextType.B;
                    break;

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
                                if (!HasExecuted)
                                {
                                    sxr.WriteHeaderToTaggedFile("AnticipateFile", Anticipateheaders);
                                    sxr.StartTimer(20);
                                    sxr.DisplayText("In this experiment, you will see different colored shapes in the 3d environment. Please keep your focus on the screen at all times. You will also hear loud sounds. There may or may not be a relationship between the colored shapes and the loud sounds.");
                                    HasExecuted = true; // set to true so this block of code only runs once
                                }

                                if (sxr.CheckTimer()) // checks if the timer has reached zero
                                {
                                    sxr.HideAllText();
                                    sxr.NextStep(); // go to the next phase and set has Executed to false
                                    HasExecuted = false;
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
                            if (!HasExecuted)
                            {
                                controllerHandler.ToggleController();
                                HasExecuted = true;

                            }
                            sxr.InputSlider(0, 9, $"Using the Controller and Trigger Adjust the value to 9 and click submit [{InstructionSliderValue}]", true); // displays slider that user can input 
                            if (sxr.ParseInputUI(out InstructionSliderValue))
                            {
                                controllerHandler.ToggleController();
                                sxr.NextPhase();
                                HasExecuted = false;
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
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 9f);
                                    break;
                            }
                            break;

                        case 1: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 14f);
                                    break;
                            }
                            break;

                        case 2: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Right, ActivateUS: false, GetAnticipation: true);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 10f);

                                    break;
                            }
                            break;
                        case 3: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    if (!HasExecuted)
                                    {
                                        sxr.SetStage("InterTrial");
                                        sxr.StartTimer(12f); // inter trial interval time
                                        HasExecuted = true; // sets has Executed Flag to true so that it only executes once
                                    }

                                    if (sxr.CheckTimer())
                                    {
                                        sxr.NextPhase(); // Goes to the next trial
                                        HasExecuted = false; // sets has Executed Flag to false for the next trial
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

                                    switch (ContextTest)
                                    {
                                        case ContextTest.AAA:
                                            break;
                                        case ContextTest.BBB:
                                            break;
                                        case ContextTest.ABA:
                                            ContextA.SetActive(false);
                                            ContextB.SetActive(true);
                                            ActiveContext = ContextType.B;
                                            break;
                                        case ContextTest.BAB:
                                            ContextA.SetActive(true);
                                            ContextB.SetActive(false);
                                            ActiveContext = ContextType.A;
                                            break;
                                    }

                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 11f);
                                    break;
                            }
                            break;

                        case 1: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Right, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 14f);
                                    break;
                            }
                            break;

                        case 2: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Left, ActivateUS: true, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 9f);
                                    break;
                            }
                            break;
                        case 3:  // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Right, ActivateUS: true, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 15f);
                                    break;
                            }
                            break;
                        case 4:  // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 12f);
                                    break;
                            }
                            break;
                        case 5:   // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: true, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 10f);
                                    break;
                            }
                            break;
                        case 6:   // CS+ without US
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS+ without US
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 13f);
                                    break;
                            }
                            break;
                        case 7:  // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 11f);
                                    break;
                            }
                            break;
                        case 8:   // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: true, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 9f);
                                    break;
                            }
                            break;
                        case 9:  // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 12f);
                                    break;
                            }
                            break;
                        case 10:   // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: true, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 15f);
                                    break;
                            }
                            break;
                        case 11:  // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 12f);
                                    break;
                            }
                            break;
                        case 12:  // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 10f);
                                    break;
                            }
                            break;
                        case 13:   // CS+ without US
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS+ without US
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 13f);
                                    break;
                            }
                            break;
                        case 14:  // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 9f);
                                    break;
                            }
                            break;
                        case 15:   // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:  // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: true, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    if (!HasExecuted)
                                    {
                                        sxr.SetStage("InterTrial");
                                        sxr.StartTimer(12); // // inter trial interval time
                                        HasExecuted = true; // sets has Executed Flag to true so that it only executes once
                                    }

                                    if (sxr.CheckTimer())
                                    {
                                        sxr.NextPhase(); // Goes to the next Phase
                                        HasExecuted = false; // sets has Executed Flag to false for the next trial
                                        switch (ContextTest)
                                        {
                                            case ContextTest.AAA:
                                                break;
                                            case ContextTest.BBB:
                                                break;
                                            case ContextTest.ABA:
                                                ContextA.SetActive(true);
                                                ContextB.SetActive(false);
                                                ActiveContext = ContextType.A;
                                                break;
                                            case ContextTest.BAB:
                                                ContextA.SetActive(false);
                                                ContextB.SetActive(true);
                                                ActiveContext = ContextType.B;
                                                break;
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
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 12f);
                                    break;
                            }
                            break;

                        case 1: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 10f);

                                    break;
                            }
                            break;
                        case 2: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 15f);

                                    break;
                            }
                            break;
                        case 3: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 9f);
                                    break;
                            }
                            break;
                        case 4: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 11f);
                                    break;
                            }
                            break;
                        case 5: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 14f);

                                    break;
                            }
                            break;
                        case 6: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 13f);
                                    break;
                            }
                            break;
                        case 7: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 10f);

                                    break;
                            }
                            break;
                        case 8: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 15f);

                                    break;
                            }
                            break;
                        case 9: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 12f);
                                    break;
                            }
                            break;
                        case 10: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 9f);

                                    break;
                            }
                            break;
                        case 11: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 12f);
                                    break;
                            }
                            break;
                        case 12: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 10f);
                                    break;
                            }
                            break;
                        case 13: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 13f);

                                    break;
                            }
                            break;
                        case 14: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 14f);

                                    break;
                            }
                            break;
                        case 15: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 10f);
                                    break;
                            }
                            break;
                        case 16: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 12f);

                                    break;
                            }
                            break;
                        case 17: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 15f);
                                    break;
                            }
                            break;
                        case 18: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 11f);
                                    break;
                            }
                            break;
                        case 19: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 13f);

                                    break;
                            }
                            break;
                        case 20: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 10f);

                                    break;
                            }
                            break;
                        case 21: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 14f);
                                    break;
                            }
                            break;
                        case 22: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 9f);

                                    break;
                            }
                            break;
                        case 23: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 12f);

                                    break;
                            }
                            break;
                        case 24: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 15f);
                                    break;
                            }
                            break;
                        case 25: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 11f);

                                    break;
                            }
                            break;
                        case 26: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 13f);

                                    break;
                            }
                            break;
                        case 27: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 10f);
                                    break;
                            }
                            break;
                        case 28: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 14f);
                                    break;
                            }
                            break;
                        case 29: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 12f);

                                    break;
                            }
                            break;
                        case 30: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 9f);
                                    break;
                            }
                            break;
                        case 31: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 15f);

                                    break;
                            }
                            break;
                        case 32: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 11f);
                                    break;
                            }
                            break;
                        case 33: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 13f);

                                    break;
                            }
                            break;
                        case 34: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 10f);

                                    break;
                            }
                            break;
                        case 35: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 14f);
                                    break;
                            }
                            break;
                        case 36: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 12f);

                                    break;
                            }
                            break;
                        case 37: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 13f);
                                    break;
                            }
                            break;
                        case 38: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    StartCS(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    InterTrial(InterTrialWaitTime: 9f);
                                    break;
                            }
                            break;
                        case 39: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false);
                                    break;

                                case 1: // inter trial interval
                                    if (!HasExecuted)
                                    {

                                        sxr.DisplayText("Experiment Complete. Thank You!");
                                        HasExecuted = true;
                                    }
                                    InterTrial(InterTrialWaitTime: 10f);
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

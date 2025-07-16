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

namespace ExperimentScene
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
        private string Anticipateheaders = "Anticipated,ResponseTime"; // Used to write headers to Anticipatedfile
        private int AnticipatedNumber; // Used for when the user enters if they anticipated US
        private bool UserInputComplete = false; // Used for a check if the user has submitted a value  
        private const float TimeForUserToRespond = 999; // Used to determine how long the user has to respond
        private const float DisplayTimeBeforeSlider = 5; // Used to determine how long to wait into the CS to display Slider
        private const float DisplayDuration = 8; // Determines how long the CS is displayed on screen for
        private const float TimeUntilUnconditionedStimulusSound = 7; // Determines how long to wait into a trial to activate the US
        private int InstructionSliderValue = 0; // Used for instruction slider

        private void StartCS(StimulusType type, StimulusLocation position, bool ActivateUS, bool GetAnticipation)
        {
            sxr.StartTimer(DisplayDuration); // sets the timer
            scriptHandler.AssignLightingAndDoorControllerForStimulusLocation(position, ActiveContext);

            string label = position.ToString();
            string csType = type == StimulusType.CS_Plus ? "CS+" : "CS-";

            string result = $"{label}_{csType}";
            sxr.SetStage(result);

            // Activate object and play sound after delay
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

        private IEnumerator InterTrial(float InterTrialWaitTime)  // used to wait till start of next trial
        {
            sxr.SetStage("InterTrial");
            sxr.StartTimer(InterTrialWaitTime); // // inter trial interval time
            yield return new WaitForSeconds(InterTrialWaitTime);
            sxr.NextTrial(); // Goes to the next trial
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

        private IEnumerator RunTrial(StimulusType type, StimulusLocation location, bool ActivateUS, bool GetAnticipation, float InterTrialWaitTime)
        {
            StartCS(type, location, ActivateUS, GetAnticipation);
            yield return new WaitForSeconds(DisplayDuration);
            scriptHandler.TriggerEntryClose(ActiveContext);
            sxr.NextStep();
            yield return InterTrial(InterTrialWaitTime);
        }

        void Start()
        {
            switch (ContextTest)
            {
                case ContextTest.AAA:
                    ActiveContext = ContextType.A;
                    ContextB.SetActive(false);
                    break;
                case ContextTest.BBB:
                    ActiveContext = ContextType.B;
                    ContextA.SetActive(false);
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

            sxr.SetContext(ActiveContext.ToString());
        }

        void Update()
        {
            int phase = sxr.GetPhase();

            switch (phase)
            {
                case 0: // Start Screen Phase
                    break;

                case 1: // Instruction Phase
                    if (!HasExecuted)
                    {
                        StartInstructionPhase();
                        HasExecuted = true;
                    }
                    break;
            }
        }

        public void StartInstructionPhase()
        {
            sxr.StartRecordingCameraPos();
            sxr.StartRecordingEyeTrackerInfo();
            StartCoroutine(InstructionSteps());
        }

        private IEnumerator InstructionSteps()
        {
            sxr.SetStage("InstructionPhase");

            sxr.WriteHeaderToTaggedFile("AnticipateFile", Anticipateheaders);

            if (InstructionPhase)
            {
                sxr.DisplayText("In this experiment, you will see different colored shapes in the 3d environment. Please keep your focus on the screen at all times. You will also hear loud sounds. There may or may not be a relationship between the colored shapes and the loud sounds.");
                sxr.StartTimer(20);
                yield return new WaitForSeconds(20);
                sxr.HideAllText();
            }
            sxr.DisplayImage("trigger");
            yield return new WaitUntil(() => sxr.GetTrigger());
            sxr.HideImagesUI();
            controllerHandler.ToggleController();
            bool submitted = false;

            // Continuously check for slider input
            while (!submitted)
            {
                sxr.InputSlider(0, 9, $"Using the Controller and Trigger Adjust the value to 9 and click submit [{InstructionSliderValue}]", true);

                if (sxr.ParseInputUI(out InstructionSliderValue))
                {
                    submitted = true;
                }

                yield return null; // wait for next frame
            }

            controllerHandler.ToggleController();
            sxr.NextPhase();
            StartCoroutine(RunHabituationTrials());
        }

        private IEnumerator RunHabituationTrials()
        {
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 9f);   // Trial 0
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 14f);  // Trial 1
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Right, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 10f);  // Trial 2
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 12f);  // Trial 3
            sxr.NextPhase();
            StartCoroutine(RunFearAcquisitionTrials());
        }

        private IEnumerator RunFearAcquisitionTrials()
        {
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
            sxr.SetContext(ActiveContext.ToString());
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 11f); // Trial 0
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Right, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 14f); // Trial 1
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Left, ActivateUS: true, GetAnticipation: false, InterTrialWaitTime: 9f); // Trial 2
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Right, ActivateUS: true, GetAnticipation: false, InterTrialWaitTime: 15f); // Trial 3
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 12f); // Trial 4
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: true, GetAnticipation: false, InterTrialWaitTime: 10f); // Trial 5
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 13f); // Trial 6
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 11f); // Trial 7
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: true, GetAnticipation: false, InterTrialWaitTime: 9f); // Trial 8
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 12f); // Trial 9
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: true, GetAnticipation: false, InterTrialWaitTime: 15f); // Trial 10
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 12f); // Trial 11
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 10f); // Trial 12
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 13f); // Trial 13
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 9f); // Trial 14
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: true, GetAnticipation: false, InterTrialWaitTime: 12f); // // Trial 15
            sxr.NextPhase();
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
            sxr.SetContext(ActiveContext.ToString());
            StartCoroutine(RunFearExtinctionTrials());
        }

        private IEnumerator RunFearExtinctionTrials()
        {
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 12f); // Trial 0
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 10f); // Trial 1
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 15f); // Trial 2
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 9f); // Trial 3
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 11f); // Trial 4
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 14f); // Trial 5
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 13f); // Trial 6
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 10f); // Trial 7
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 15f); // Trial 8
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 12f); // Trial 9
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 9f); // Trial 10
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 12f); // Trial 11
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 10f); // Trial 12
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 13f); // Trial 13
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 14f); // Trial 14
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 10f); // Trial 15
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 12f); // Trial 16
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 15f); // Trial 17
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 11f); // Trial 18
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 13f); // Trial 19
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 10f); // Trial 20
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 14f); // Trial 21
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 9f); // Trial 22
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 12f); // Trial 23
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 15f); // Trial 24
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 11f); // Trial 25
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 13f); // Trial 26
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 10f); // Trial 27
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 14f); // Trial 28
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 12f); // Trial 29
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 9f);  // Trial 30
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 15f); // Trial 31
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 11f); // Trial 32
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 13f); // Trial 33
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 10f); // Trial 34
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 14f); // Trial 35
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 12f); // Trial 36
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 13f); // Trial 37
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 9f); // Trial 38
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, GetAnticipation: false, InterTrialWaitTime: 10f); // Trial 39
            EditorApplication.isPlaying = false;
        }

    }
}


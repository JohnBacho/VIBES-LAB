using UnityEngine;
using sxr_internal;
using System.Collections;
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
        public Color CSPlusLightColor = Color.blue; // set the color of the room for the ABA testing
        public bool CSPlusDisplayPattern = false; // used to determine if to make the light flash a pattern or not
        public Color CSMinusLightColor = Color.green; // set the color of the room for the ABA testing
        public bool CSMinusDisplayPattern = false; // used to determine if to make the light flash a pattern or not
        public AudioSource[] USAudiosSources; // takes in an array of audio sources
        public AudioSource Testsound; // used to test if the sound is working
        public GameObject USObject; // drag and drop the object you want to be the US
        public ContextTest ContextTest = ContextTest.AAA; // determines context testing
        private ContextType ActiveContext; // used to keep track of the current context the user is in
        public GameObject ContextA; // used for context switch in ABA
        public GameObject ContextB; // used for context switch in ABA

        public VRControllerHandler controllerHandler; // handles when the controller is on and when it is off
        public SimpleCharacterMover characterMover; // handles how the monster should move
        public ScriptHandler scriptHandler; // manages the doors, elevator, and light scripts
        private bool HasExecuted = false; //  used as a way to execute one block of code only once
        private const float DisplayTimeBeforeSlider = 5; // Used to determine how long to wait into the CS to display Slider
        private const float DisplayDuration = 8; // Determines how long the CS is displayed on screen for
        private const float TimeUntilUnconditionedStimulusSound = 7; // Determines how long to wait into a trial to activate the US

        private void StartCS(StimulusType type, StimulusLocation position, bool ActivateUS)
        {
            SetupTimerAndControllers(position);
            SetStageLabel(type, position);
            HandleLighting(type);
            StartCoroutine(RunStimulus());

            if (ActivateUS)
            {
                StartCoroutine(RunUnconditionalStimuli(USObject, position));
            }
        }
        private void SetStageLabel(StimulusType type, StimulusLocation position)
        {
            string label = position.ToString();
            string csType = type == StimulusType.CS_Plus ? "CS+" : "CS-";
            string result = $"{label}_{csType}";
            sxr.SetStage(result);
        }

        private void SetupTimerAndControllers(StimulusLocation position)
        {
            sxr.StartTimer(DisplayDuration);
            scriptHandler.AssignLightingAndDoorControllerForStimulusLocation(position, ActiveContext);
        }
        private void HandleLighting(StimulusType type)
        {
            Color lightColor = type == StimulusType.CS_Plus ? CSPlusLightColor : CSMinusLightColor;
            scriptHandler.SetStop();
            if (CSPlusDisplayPattern && type == StimulusType.CS_Plus || CSMinusDisplayPattern && !(type == StimulusType.CS_Plus))
            {
                scriptHandler.StartLightPattern(lightColor);
            }
            else
            {
                scriptHandler.ChangeLightColor(lightColor);
            }
        }

        private IEnumerator InterTrial(float InterTrialWaitTime)  // used to wait till start of next trial
        {
            sxr.SetStage("InterTrial");
            sxr.StartTimer(InterTrialWaitTime); // // inter trial interval time
            yield return new WaitForSeconds(InterTrialWaitTime - 1);
            sxr.SetStage("Baseline" + sxr.GetPhase().ToString() + sxr.GetTrial().ToString());
            yield return new WaitForSeconds(1);
            sxr.NextTrial(); // Goes to the next trial
        }

        // Coroutine to play the sound after a delay
        IEnumerator RunUnconditionalStimuli(GameObject USObject, StimulusLocation position)
        {
            yield return new WaitForSeconds(TimeUntilUnconditionedStimulusSound); // TimeUntilUnconditionedStimulusSound determines how long it should wait into a trial to play US
            sxr.SetStage("US");
            USObject.SetActive(true);
            for (int i = 0; i < USAudiosSources.Length; i++)
            {
                USAudiosSources[i].Play();
            }
            scriptHandler.TriggerEntryOpen(ActiveContext);
            characterMover.ResetPosition();
            characterMover.StartScare(position);
            sxr.SendHaptic(amp: 1.0f, dur: 1.0f, rightHand: true, chan: 0); // for right hand
            sxr.SendHaptic(amp: 1.0f, dur: 1.0f, rightHand: false, chan: 0); // for left hand
            yield return new WaitForSeconds(1); // waits for 1 second
            characterMover.ResetPosition();
            USObject.SetActive(false);
        }
        IEnumerator RunStimulus()
        {
            yield return new WaitForSeconds(DisplayDuration - 1);
            scriptHandler.TriggerEntryOpen(ActiveContext);
            yield return new WaitForSeconds(1);
            scriptHandler.TriggerEntryClose(ActiveContext);
            scriptHandler.RestAllLights();
        }

        private IEnumerator RunTrial(StimulusType type, StimulusLocation location, bool ActivateUS, float InterTrialWaitTime)
        {
            StartCS(type, location, ActivateUS);
            yield return new WaitForSeconds(DisplayDuration);
            sxr.NextStep();
            yield return InterTrial(InterTrialWaitTime);
        }

        void Awake()
        {
            AudioListener.pause = true;
        }

        void Start()
        {
            switch (ContextTest)
            {
                case ContextTest.AAA:
                    ActiveContext = ContextType.A;
                    ContextA.SetActive(true);
                    ContextB.SetActive(false);
                    RenderSettings.ambientIntensity = 1.4f;
                    break;
                case ContextTest.BBB:
                    ActiveContext = ContextType.B;
                    ContextA.SetActive(false);
                    ContextB.SetActive(true);
                    RenderSettings.ambientIntensity = 1.25f;
                    break;
                case ContextTest.ABA:
                    ContextA.SetActive(true);
                    ContextB.SetActive(false);
                    ActiveContext = ContextType.A;
                    RenderSettings.ambientIntensity = 1.4f;
                    break;
                case ContextTest.BAB:
                    ContextA.SetActive(false);
                    ContextB.SetActive(true);
                    ActiveContext = ContextType.B;
                    RenderSettings.ambientIntensity = 1.25f;
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
            controllerHandler.ToggleController();
            sxr.DisplayImage("trigger");
            yield return null;
            yield return new WaitUntil(() => sxr.GetTrigger());
            sxr.HideImagesUI();

            yield return new WaitForSeconds(0.2f);
            sxr.DisplayText("Press the trigger to play a test sound.");
            AudioListener.pause = false;
            yield return new WaitUntil(() => sxr.GetTrigger());
            sxr.HideAllText();
            Testsound.Play();
            yield return new WaitForSeconds(0.8f);
            sxr.DisplayText("Did you hear the sound? If you did not hear the sound, please inform the researcher. (Press trigger to continue)");
            yield return new WaitUntil(() => sxr.GetTrigger());
            sxr.HideAllText();

            yield return new WaitForSeconds(0.2f);
            sxr.DisplayText("In this experiment, you will see different colored lights in the 3d environment. Please keep your focus on the screen at all times. You will also hear loud sounds. There may or may not be a relationship between the colored lights and the loud sounds. (Press trigger to continue)");
            yield return new WaitForSeconds(1f);
            yield return new WaitUntil(() => sxr.GetTrigger());
            sxr.HideAllText();
            controllerHandler.ToggleController();
            sxr.NextPhase();
            StartCoroutine(RunHabituationTrials());
        }

        private IEnumerator RunHabituationTrials()
        {
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Left, ActivateUS: false, InterTrialWaitTime: 9f);   // Trial 0
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 12f);  // Trial 1
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, InterTrialWaitTime: 10f);  // Trial 2
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Middle, ActivateUS: false, InterTrialWaitTime: 12f);  // Trial 3
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
                    RenderSettings.ambientIntensity = 1.25f;
                    break;
                case ContextTest.BAB:
                    ContextA.SetActive(true);
                    ContextB.SetActive(false);
                    ActiveContext = ContextType.A;
                    RenderSettings.ambientIntensity = 1.4f;
                    break;
            }
            sxr.SetContext(ActiveContext.ToString());
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, InterTrialWaitTime: 11f); // Trial 0
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Middle, ActivateUS: false, InterTrialWaitTime: 9f); // Trial 1
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Right, ActivateUS: true, InterTrialWaitTime: 11f); // Trial 2
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Left, ActivateUS: true, InterTrialWaitTime: 13f); // Trial 3
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Middle, ActivateUS: false, InterTrialWaitTime: 12f); // Trial 4
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Left, ActivateUS: true, InterTrialWaitTime: 10f); // Trial 5
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 11f); // Trial 6
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 12f); // Trial 7
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Left, ActivateUS: true, InterTrialWaitTime: 10f); // Trial 8
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Middle, ActivateUS: false, InterTrialWaitTime: 9f); // Trial 9
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: true, InterTrialWaitTime: 13f); // Trial 10
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 12f); // Trial 11
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, InterTrialWaitTime: 10f); // Trial 12
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 12f); // Trial 13
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, InterTrialWaitTime: 9f); // Trial 14
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: true, InterTrialWaitTime: 12f); // // Trial 15
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
                    RenderSettings.ambientIntensity = 1.4f;
                    break;
                case ContextTest.BAB:
                    ContextA.SetActive(false);
                    ContextB.SetActive(true);
                    ActiveContext = ContextType.B;
                    RenderSettings.ambientIntensity = 1.25f;
                    break;
            }
            sxr.SetContext(ActiveContext.ToString());
            StartCoroutine(RunFearExtinctionTrials());
        }

        private IEnumerator RunFearExtinctionTrials()
        {
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 12f); // Trial 0
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, InterTrialWaitTime: 10f); // Trial 1
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 12f); // Trial 2
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, InterTrialWaitTime: 9f); // Trial 3
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, InterTrialWaitTime: 11f); // Trial 4
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 9f); // Trial 5
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Middle, ActivateUS: false, InterTrialWaitTime: 13f); // Trial 6
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, InterTrialWaitTime: 10f); // Trial 7
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Left, ActivateUS: false, InterTrialWaitTime: 14f); // Trial 8
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Middle, ActivateUS: false, InterTrialWaitTime: 12f); // Trial 9
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 9f); // Trial 10
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, InterTrialWaitTime: 12f); // Trial 11
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Middle, ActivateUS: false, InterTrialWaitTime: 10f); // Trial 12
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Left, ActivateUS: false, InterTrialWaitTime: 13f); // Trial 13
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 12f); // Trial 14
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, InterTrialWaitTime: 10f); // Trial 15
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Left, ActivateUS: false, InterTrialWaitTime: 12f); // Trial 16
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 9f); // Trial 17
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Middle, ActivateUS: false, InterTrialWaitTime: 11f); // Trial 18
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 13f); // Trial 19
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, InterTrialWaitTime: 10f); // Trial 20
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Middle, ActivateUS: false, InterTrialWaitTime: 12f); // Trial 21
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 9f); // Trial 22
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, InterTrialWaitTime: 12f); // Trial 23
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, InterTrialWaitTime: 10f); // Trial 24
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 11f); // Trial 25
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, InterTrialWaitTime: 13f); // Trial 26
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, InterTrialWaitTime: 10f); // Trial 27
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 9f); // Trial 28
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 12f); // Trial 29
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Middle, ActivateUS: false, InterTrialWaitTime: 9f);  // Trial 30
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Left, ActivateUS: false, InterTrialWaitTime: 13f); // Trial 31
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Middle, ActivateUS: false, InterTrialWaitTime: 11f); // Trial 32
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 9f); // Trial 33
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 10f); // Trial 34
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, InterTrialWaitTime: 12f); // Trial 35
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Middle, ActivateUS: false, InterTrialWaitTime: 11f); // Trial 36
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Middle, ActivateUS: false, InterTrialWaitTime: 13f); // Trial 37
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 9f); // Trial 38
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Left, ActivateUS: false, InterTrialWaitTime: 10f); // Trial 39
            sxr.NextPhase();
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
                    RenderSettings.ambientIntensity = 1.25f;
                    break;
                case ContextTest.BAB:
                    ContextA.SetActive(true);
                    ContextB.SetActive(false);
                    ActiveContext = ContextType.A;
                    RenderSettings.ambientIntensity = 1.4f;
                    break;
            }
            sxr.SetContext(ActiveContext.ToString());
            StartCoroutine(RunFearRenewalTrials());

        }

        private IEnumerator RunFearRenewalTrials()
        {
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Left, ActivateUS: false, InterTrialWaitTime: 11f); // Trial 0
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Middle, ActivateUS: false, InterTrialWaitTime: 9f); // Trial 1
            yield return RunTrial(StimulusType.CS_Plus, StimulusLocation.Right, ActivateUS: false, InterTrialWaitTime: 11f); // Trial 2
            yield return RunTrial(StimulusType.CS_Minus, StimulusLocation.Left, ActivateUS: false, InterTrialWaitTime: 10f); // Trial 3
            Application.Quit(); // Ends the experiment
        }

    }
}

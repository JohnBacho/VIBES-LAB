using UnityEngine;
using ViveSR.anipal.Eye;
using sxr_internal;
using System.Collections; // Required for IEnumerator


namespace SampleExperimentScene
{
    public class ExperimentScript_EYE : MonoBehaviour
    {
        public bool EyeCalibration; // toggle for eye tracking
        public float TimeBeforeCS; // Enter time for trial for before the CS
        public float TimeAfterCS; // Enter time for trial for After the CS
        public float tolerance; // Enter Tolerance to trigger CS

        public GameObject CS_plus_Object; // drag and drop CS+ object
        public GameObject CS_plus_Sound; // drag and drop CS+ sound to get it to play
        public float CS_plus_Sound_Delay; // Enter time for sound delay to play after CS+ object is activated.
        public float CS_plus_Object_Interval; // Enter time for CS+ object to stay active

        public GameObject CS_minus_Object; // drag and drop CS- object
        public GameObject CS_minus_Sound; // drag and drop CS- sound to get it to play
        public float CS_minus_Sound_Delay; // Enter time for sound delay to play after CS- object is activated.
        public float CS_minus_Object_Interval; // Enter time for CS- object to stay active

        private string FocusedGameObject = ""; // used for Sranipal
        private Ray testRay; // used for Sranipal
        private FocusInfo focusInfo; // used for Sranipal
        private Vector3 gazeHitPoint; // used in calculating eye tracking data with collisions
        private bool hasExecuted = false; //  used as a way to execute one block of code only once
        private bool StartEyeTracker = false; //used to start the CheckFocus(); function which calculates the eye 
        // tracking data ensuring that all three cameratrack / eyetracker / mainfile are all started at the exact same time
        private string headers = "GazeHitPointX,GazeHitPointY,GazeHitPointZ,GameObjectInFocus"; // used to write headers to the mainfile
        private float TotalTrialTime; //Used to calculate the total time of the trial

        void Start()
        {

            if (EyeCalibration) // set to true in the inspector if you would like to auto launch SRanipal
            {
                sxr.LaunchEyeCalibration();
            }

            if (TimeAfterCS < 0 )
                break;
                
            TotalTrialTime = TimeBeforeCS + TimeAfterCS; // Calculates total time 
        }
        // Coroutine to play the sound after a delay
        IEnumerator PlaySoundAfterDelay(GameObject soundObject, float soundDelay)
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


        void CheckFocus()
        {

            FocusedGameObject = "";

            if (SRanipal_Eye.Focus(GazeIndex.COMBINE, out testRay, out focusInfo)) { }
            else if (SRanipal_Eye.Focus(GazeIndex.LEFT, out testRay, out focusInfo)) { }
            else if (SRanipal_Eye.Focus(GazeIndex.RIGHT, out testRay, out focusInfo)) { }
            else return;

            FocusedGameObject = focusInfo.collider.gameObject.name;
            sxr.ChangeExperimenterTextbox(4, "Current Game Object: " + FocusedGameObject);

            gazeHitPoint = focusInfo.point;
            sxr.ChangeExperimenterTextbox(5, "Gaze Hit Position: " + gazeHitPoint);


            string DataPoints = (gazeHitPoint.ToString() + "," + FocusedGameObject); 
            sxr.WriteToTaggedFile("mainFile", DataPoints); // // saves the gazehitpoint which is the gaze with object collision and also 
            // FocusedGameObject which is the object the user is looking at to file 

        }

        void Update()
        {

            if (StartEyeTracker)
            {
                CheckFocus();
            }

            //var gazeInfo = sxr.GetFullGazeInfo();
            //sxr.ChangeExperimenterTextbox(5, "Gaze Info: " + gazeInfo);

            switch (sxr.GetPhase()) // gets the phase
            {
                case 0: // Start Screen Phase
                    break;

                case 1: // Instruction Phase
                    StartEyeTracker = true;
                    sxr.StartRecordingCameraPos();
                    sxr.StartRecordingEyeTrackerInfo();
                    if (!hasExecuted)
                    {
                        sxr.WriteHeaderToTaggedFile("mainFile", headers);
                        sxr.StartTimer(20);
                        sxr.DisplayText("In this experiment, you will see different colored shapes in the 3d environment. Please look at the screen at all times. You will also hear loud sounds. There may or may not be a relationship between the colored shapes and the loud sounds.");
                        hasExecuted = true; // set to true so this block of code only runs once
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
                        case 0:
                            sxr.NextTrial();
                            break;

                        case 1: // Start of CS+ and Inter Trial Interval
                            switch (sxr.GetStepInTrial())
                            {   
                                case 0: // CS+
                                    if (!hasExecuted)
                                    {

                                        sxr.MoveObjectTo("sXR_prefab", 23.0f, 0f, 0f); // teleports player to CS+ environment
                                        sxr.StartTimer(TotalTrialTime); // sets the timer based on TimeBeforeCS + TimeAfterCS;
                                        hasExecuted = true;
                                    }

                                    // doesn't exactly reach seconds ie 10s on the dot since it is a float so we have to add a tolerance so that it still executes 
                                    if (Mathf.Abs(sxr.TimeRemaining() - Mathf.Abs(TimeAfterCS)) <= tolerance)
                                    {
                                        // Activate object and play sound after delay
                                        CS_plus_Object.SetActive(true);
                                        StartCoroutine(PlaySoundAfterDelay(CS_plus_Sound, CS_plus_Sound_Delay)); // calls function to play sound with delay
                                        StartCoroutine(DisableObjects(CS_plus_Object, CS_plus_Object_Interval)); // calls function to deactivate sound with delay
                                    }
                                    if (sxr.CheckTimer()) // checks if timer is zero
                                    {
                                        sxr.NextStep(); // advances to inter trial interval and sets hasExecuted to false
                                        hasExecuted = false;
                                    }
                                    break;

                                case 1: // inter trial interval
                                    if (!hasExecuted)
                                    {
                                        sxr.MoveObjectTo("sXR_prefab", 0.0f, 0f, 0f); // teleports player back to spawn
                                        sxr.StartTimer(9f); // // inter trial interval time
                                        hasExecuted = true;
                                    }

                                    if (sxr.CheckTimer())
                                    {
                                        sxr.NextTrial();
                                        hasExecuted = false;
                                    }
                                    break;
                            }
                            break;

                        case 2: // Start of CS- and Inter Trial Interval
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:
                                    if (!hasExecuted)
                                    {
                                        // Move player to CS- environment
                                        sxr.MoveObjectTo("sXR_prefab", 61.39f, 0f, 0f);
                                        sxr.StartTimer(TotalTrialTime);
                                        hasExecuted = true;
                                    }
                                    if (Mathf.Abs(sxr.TimeRemaining() - TimeAfterCS) <= tolerance)
                                    {
                                        // Activate object and play sound after delay
                                        CS_minus_Object.SetActive(true);
                                        StartCoroutine(PlaySoundAfterDelay(CS_minus_Sound, CS_minus_Sound_Delay));
                                        StartCoroutine(DisableObjects(CS_minus_Object, CS_minus_Object_Interval));
                                    }

                                    if (sxr.CheckTimer())
                                    {
                                        sxr.NextStep();
                                        hasExecuted = false;
                                    }
                                    break;

                                case 1: // inter trial interval
                                    if (!hasExecuted)
                                    {
                                        sxr.MoveObjectTo("sXR_prefab", 0.0f, 0.0f, 0f);
                                        sxr.StartTimer(14f); // inter trial interval time
                                        hasExecuted = true;
                                    }

                                    if (sxr.CheckTimer())
                                    {
                                        sxr.NextTrial(); // Proceed to step 3, or handle the transition as required
                                        hasExecuted = false;
                                    }
                                    break;
                            }
                            break;

                        case 3: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    if (!hasExecuted)
                                    {
                                        // Move player to CS+ environment
                                        sxr.MoveObjectTo("sXR_prefab", 23.0f, 0f, 0f);
                                        sxr.StartTimer(TotalTrialTime);
                                        hasExecuted = true;
                                    }

                                    if (Mathf.Abs(sxr.TimeRemaining() - Mathf.Abs(TimeAfterCS)) <= tolerance)
                                    {
                                        // Activate object and play sound after delay
                                        CS_plus_Object.SetActive(true);
                                        StartCoroutine(PlaySoundAfterDelay(CS_plus_Sound, CS_plus_Sound_Delay));
                                        StartCoroutine(DisableObjects(CS_plus_Object, CS_plus_Object_Interval));
                                    }
                                    if (sxr.CheckTimer())
                                    {
                                        sxr.NextStep();
                                        hasExecuted = false;
                                    }
                                    break;

                                case 1: // inter trial interval
                                    if (!hasExecuted)
                                    {
                                        sxr.MoveObjectTo("sXR_prefab", 0.0f, 0f, 0f);
                                        sxr.StartTimer(10f); // // inter trial interval time
                                        hasExecuted = true;
                                    }

                                    if (sxr.CheckTimer())
                                    {
                                        sxr.NextTrial();
                                        hasExecuted = false;
                                    }
                                    break;
                            }
                            break;
                        case 4: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0:
                                    if (!hasExecuted)
                                    {
                                        // Move player to CS- environment
                                        sxr.MoveObjectTo("sXR_prefab", 61.39f, 0f, 0f);
                                        sxr.StartTimer(TotalTrialTime);
                                        hasExecuted = true;
                                    }
                                    if (Mathf.Abs(sxr.TimeRemaining() - TimeAfterCS) <= tolerance)
                                    {
                                        // Activate object and play sound after delay
                                        CS_minus_Object.SetActive(true);
                                        StartCoroutine(PlaySoundAfterDelay(CS_minus_Sound, CS_minus_Sound_Delay));
                                        StartCoroutine(DisableObjects(CS_minus_Object, CS_minus_Object_Interval));
                                    }

                                    if (sxr.CheckTimer())
                                    {
                                        sxr.NextStep();
                                        hasExecuted = false;
                                    }
                                    break;

                                case 1: // inter trial interval
                                    if (!hasExecuted)
                                    {
                                        sxr.MoveObjectTo("sXR_prefab", 0.0f, 0.0f, 0f);
                                        sxr.StartTimer(40f); // inter trial interval time
                                        hasExecuted = true;
                                    }

                                    if (sxr.CheckTimer())
                                    {
                                        sxr.NextTrial(); // Proceed to step 3, or handle the transition as required
                                        hasExecuted = false;
                                    }
                                    break;
                            }
                            break;

                    }
                    break; // End of main case 2
            }

        }
    }
}

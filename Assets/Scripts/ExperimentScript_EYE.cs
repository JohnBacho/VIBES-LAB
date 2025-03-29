using UnityEngine;
using ViveSR.anipal.Eye;
﻿using sxr_internal;
using System.Collections; // Required for IEnumerator


namespace SampleExperimentScene
{
    public class ExperimentScript_EYE : MonoBehaviour
    {
        public bool EyeCalibration; // toggle for eye tracking
        public float trialTimeBefore; // Enter time for trial for before the CS
        public float trialTimeAfter; // Enter time for trial for After the CS
        public float tolerance; // Enter Tolerance to trigger CS

        public GameObject CS_plus_Object; // drag and drop CS+ object
        public GameObject CS_plus_Sound; // drag and drop CS+ sound to get it to play
        public float CS_plus_Sound_Delay; // Enter time for sound delay to play after CS+ object is activated.
        public float CS_plus_Object_Interval; // Enter time for CS+ object to stay active
        
        public GameObject CS_minus_Object; // drag and drop CS- object
        public GameObject CS_minus_Sound; // drag and drop CS- sound to get it to play
        public float CS_minus_Sound_Delay; // Enter time for sound delay to play after CS- object is activated.
        public float CS_minus_Object_Interval; // Enter time for CS- object to stay active

        private string currentFocus = "";
        private Ray testRay;
        private FocusInfo focusInfo;
        private Vector3 gazeHitPoint;
        private bool hasExecuted = false;
        private bool StartEyeTracker = false;
        private string headers = "GazeHitPointX,GazeHitPointY,GazeHitPointZ";
        private float timeElapsed; //Used to calculate when the CS starts 
        private float TotalTrialTime; //Used to calculate the total time of the trial

        void Start() // set to true in the inspector if you would like to auto launch SRanipal
        {

            if (EyeCalibration)
            {
                sxr.LaunchEyeCalibration();
            }

            timeElapsed = Mathf.Abs(trialTimeBefore + trialTimeAfter - trialTimeBefore);
            TotalTrialTime = trialTimeBefore + trialTimeAfter;
        }
        // Coroutine to play the sound after a delay
        IEnumerator PlaySoundAfterDelay(GameObject soundObject, float soundDelay)
        {
            yield return new WaitForSeconds(soundDelay);
            AudioSource audioSource = soundObject.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning("No AudioSource found on " + soundObject.name);
            }
        }

        // Coroutine to disable the object after a delay
        IEnumerator DisableObjects(GameObject objectToDisable, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (objectToDisable != null)
            {
                objectToDisable.SetActive(false);
            }
            else
            {
                Debug.LogWarning("The GameObject to disable is null!");
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

            gazeHitPoint = focusInfo.point;
            sxr.ChangeExperimenterTextbox(5, "Gaze Hit Position: " + gazeHitPoint);

            sxr.WriteToTaggedFile("mainFile", gazeHitPoint.ToString());

            // Vector3 screenPoint = Camera.main.WorldToScreenPoint(gazeHitPoint);
            // Debug.Log("Screen coordinates: " + screenPoint);
        }

        void Update()
        {

            if(StartEyeTracker){
            CheckFocus();
            }

            //var gazeInfo = sxr.GetFullGazeInfo();
            //sxr.ChangeExperimenterTextbox(5, "Gaze Info: " + gazeInfo);

           switch (sxr.GetPhase()){
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
                    hasExecuted = true;
                }

                if (sxr.CheckTimer())
                {
                    sxr.HideAllText();
                    sxr.NextPhase();
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
                                        // Move player to CS+ environment
                                        sxr.MoveObjectTo("sXR_prefab", 23.0f, 0f, 0f);
                                        sxr.StartTimer(trialTimeBefore + trialTimeAfter);
                                        hasExecuted = true;
                                    }

                                    if (Mathf.Abs(sxr.TimeRemaining() - Mathf.Abs(timeElapsed)) <= tolerance)
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

                    case 1: // CS-
                            switch (sxr.GetStepInTrial()){
                                case 0:
                                    if (!hasExecuted)
                                    {
                                        // Move player to CS- environment
                                        sxr.MoveObjectTo("sXR_prefab", 61.39f, 0f, 0f);
                                        sxr.StartTimer(trialTimeBefore + trialTimeAfter);
                                        hasExecuted = true;
                                    }
                                    if (Mathf.Abs(sxr.TimeRemaining() - timeElapsed) <= tolerance)
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

                    case 2: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    if (!hasExecuted)
                                    {
                                        // Move player to CS+ environment
                                        sxr.MoveObjectTo("sXR_prefab", 23.0f, 0f, 0f);
                                        sxr.StartTimer(trialTimeBefore + trialTimeAfter);
                                        hasExecuted = true;
                                    }

                                    if (Mathf.Abs(sxr.TimeRemaining() - Mathf.Abs(timeElapsed)) <= tolerance)
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
                    case 3: // CS-
                            switch (sxr.GetStepInTrial()){
                                case 0:
                                    if (!hasExecuted)
                                    {
                                        // Move player to CS- environment
                                        sxr.MoveObjectTo("sXR_prefab", 61.39f, 0f, 0f);
                                        sxr.StartTimer(trialTimeBefore + trialTimeAfter);
                                        hasExecuted = true;
                                    }
                                    if (Mathf.Abs(sxr.TimeRemaining() - timeElapsed) <= tolerance)
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
                    case 4: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    if (!hasExecuted)
                                    {
                                        // Move player to CS+ environment
                                        sxr.MoveObjectTo("sXR_prefab", 23.0f, 0f, 0f);
                                        sxr.StartTimer(trialTimeBefore + trialTimeAfter);
                                        hasExecuted = true;
                                    }

                                    if (Mathf.Abs(sxr.TimeRemaining() - Mathf.Abs(timeElapsed)) <= tolerance)
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
                    }
            break; // End of main case 2


              }

         }
     }
 }

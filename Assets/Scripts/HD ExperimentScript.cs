using UnityEngine;
using ViveSR.anipal.Eye;
using sxr_internal;
using System.Collections; // Required for IEnumerator


namespace SampleExperimentScene
{
    public class HD_ExperimentScript : MonoBehaviour
    {
        [Tooltip("If toggled on program will auto launch Sranipal eye calibration")]
        public bool EyeCalibration; // toggle for eye tracking
        [Tooltip("If toggled on a line will point to where the user is looking")]
        public bool GazeRays; // Set to true to toggle on gaze rays to see in real time where user is looking
        [Tooltip("Drag GameObject Gaze Ray Sample into field")]
        public GameObject GazeRayObject; // drag and drop sranpial Gaze Ray Sample 
        [Tooltip("Enter a buffer time (seconds) you want before the CS is displayed")]
        public float TimeBeforeCS; // Enter time for trial for before the CS
        [Tooltip("Enter a buffer time (seconds) you want after the CS is displayed")]
        public float TimeAfterCS; // Enter time for trial for during and After the CS
        [Tooltip("Drag your CS+ Object into this field the object should be disabled before being dragged in. To disable object when clicked on object (option + shift + a)")]
        public GameObject CS_plus_Object; // drag and drop CS+ object
        [Tooltip("Drag your CS+ sound into this field the object. Audio source with your sound should be attached to object")]
        public GameObject CS_plus_Sound; // drag and drop CS+ sound to get it to play
        [Tooltip("Enter a time (seconds) for once the CS+ is displayed how long you want to wait till the CS+ sound is played. (Note sound delay must be less then object interval)")]
        public float CS_plus_Sound_Delay; // Enter time for sound delay to play after CS+ object is activated.
        [Tooltip("Enter a time (seconds) you want the CS+ object to be displayed. (Note it must be greater then then CS_plus_Sound;)")]
        public float CS_plus_Object_Interval; // Enter time for CS+ object to stay active
        [Tooltip("Drag your CS- Object into this field the object should be disabled before being dragged in. To disable object when clicked on object (option + shift + a)")]
        public GameObject CS_minus_Object; // drag and drop CS- object
        [Tooltip("Drag your CS- sound into this field the object. Audio source with your sound should be attached to object")]
        public GameObject CS_minus_Sound; // drag and drop CS- sound to get it to play
        [Tooltip("Enter a time (seconds) for once the CS- is displayed how long you want to wait till the CS- sound is played. (Note sound delay must be less then object interval)")]
        public float CS_minus_Sound_Delay; // Enter time for sound delay to play after CS- object is activated.
        [Tooltip("Enter a time (seconds) you want the CS- object to be displayed. (Note it must be greater then then CS_minus_Sound;)")]
        public float CS_minus_Object_Interval; // Enter time for CS- object to stay active
        [Tooltip("Toggle on if you want ABA context changing")]
        public bool ABATesting; // Used to determine what context you want Extinction to 


        private string FocusedGameObject = ""; // used for Sranipal
        private Ray testRay; // used for Sranipal
        private FocusInfo focusInfo; // used for Sranipal
        private Vector3 gazeHitPoint; // used in calculating eye tracking data with collisions
        private bool hasExecuted = false; //  used as a way to execute one block of code only once
        private bool hasStartedCS = false; // used to execute the start of the CS+ only once
        private bool StartEyeTracker = false; //used to start the CheckFocus(); function which calculates the eye 
        // tracking data ensuring that all three cameratrack / eyetracker / mainfile are all started at the exact same time
        private string headers = "GazeHitPointX,GazeHitPointY,GazeHitPointZ,GameObjectInFocus"; // used to write headers to the mainfile
        private float TotalTrialTimeCsPlus; //Used to calculate the total time of the trial for CS Plus trial
        private float TotalTrialTimeCsMinus; //Used to calculate the total time of the trial for CS Minus trial
        private float timeUntilCSMinusStarts; // Used to calculate when the when to display CS minus object
        private float timeUntilCSPlusStarts; // Used to calculate when the when to display CS plus object
        private int AnticipatedNumber; // Used for when the user enters if they anticipated US

        // Used to take in a pair of gameobjects and materials in a list
        [System.Serializable]
        public class ObjectMaterialPair
        {
            public GameObject obj;
            public Material newMaterial;
        }

        public ObjectMaterialPair[] objectMaterialPairs; // saves object and material pair into an array

        private Material[] originalMaterials; // saves entered material into an array

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
                StartCoroutine(PlaySoundAfterDelay(CS_Sound, CS_Sound_Delay)); // calls function to play sound with delay
                StartCoroutine(DisableObjects(CS_Object, CS_Object_Interval)); // calls function to deactivate sound with delay
            }

            if (sxr.CheckTimer()) // checks if timer is zero
            {
                sxr.NextStep(); // advances to inter trial interval and sets hasExecuted to false
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

        public void ChangeMaterials() // changes material of object to the one entered
        {
            for (int i = 0; i < objectMaterialPairs.Length; i++)
            {
                Renderer rend = objectMaterialPairs[i].obj.GetComponent<Renderer>();
                if (rend != null)
                    rend.material = objectMaterialPairs[i].newMaterial;
            }
        }

        public void RevertMaterials() // reverts object to the original material 
        {
            for (int i = 0; i < objectMaterialPairs.Length; i++)
            {
                Renderer rend = objectMaterialPairs[i].obj.GetComponent<Renderer>();
                if (rend != null && originalMaterials[i] != null)
                    rend.material = originalMaterials[i];
            }
        }

        void Start()
        {

            if (EyeCalibration) // set to true in the inspector if you would like to auto launch SRanipal eye tracker calibration
            {
                sxr.LaunchEyeCalibration();
            }

            if (GazeRays)
            {
                GazeRayObject.SetActive(true);
            }
            // used to save the original Material of the object before it changes them for the B part of ABA testing
            originalMaterials = new Material[objectMaterialPairs.Length];

            for (int i = 0; i < objectMaterialPairs.Length; i++)
            {
                Renderer rend = objectMaterialPairs[i].obj.GetComponent<Renderer>();
                if (rend != null)
                    originalMaterials[i] = rend.material;
            }



            // error handling
            if (TimeAfterCS < 0 || TimeBeforeCS < 0) // if time before/after is less then or equal to 0 it will throw an error and stop the program
            {
                Debug.LogError("TimeAfterCS must be greater than 0");
                UnityEditor.EditorApplication.isPlaying = false; // stops the editor from playing
            }

            // error handling
            if (CS_minus_Sound_Delay > CS_minus_Object_Interval || CS_plus_Sound_Delay > CS_plus_Object_Interval)
            {
                Debug.LogWarning("CS minus or CS plus sound delay should be less than CS plus object_interval");
            }

            TotalTrialTimeCsPlus = TimeBeforeCS + CS_plus_Object_Interval + TimeAfterCS; // Calculates total trial time for CS plus
            TotalTrialTimeCsMinus = TimeBeforeCS + CS_minus_Object_Interval + TimeAfterCS; // Calculates total trial time for CS minus
            timeUntilCSMinusStarts = CS_minus_Object_Interval + TimeAfterCS; // used for calculating when the CS_Minus_Object appears 
            timeUntilCSPlusStarts = CS_minus_Object_Interval + TimeAfterCS; // used for calculating when the CS_Plus_Object appears 
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
                    if (!hasExecuted)
                    {
                        sxr.WriteHeaderToTaggedFile("mainFile", headers);
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
                    break;

                case 2: // Habituation Phase
                    switch (sxr.GetTrial())
                    {

                        case 0: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
                                    break;

                                case 1: // inter trial interval
                                    if (!hasExecuted)
                                    {
                                        sxr.MoveObjectTo("sXR_prefab", 0.0f, 0f, 0f); // teleports player back to spawn
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

                case 3: // Fear Acquisition training
                    switch (sxr.GetTrial())
                    {
                        case 0: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS-
                                    if (ABATesting)
                                    {
                                        ChangeMaterials(); // changes all entered objects material
                                    }
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_plus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_plus_Object, CS_plus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_plus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_plus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_plus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_plus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
                                    break;

                                case 1: // inter trial interval
                                    if (!hasExecuted)
                                    {
                                        sxr.MoveObjectTo("sXR_prefab", 0.0f, 0f, 0f); // teleports player back to spawn
                                        sxr.StartTimer(5); // // inter trial interval time
                                        hasExecuted = true; // sets has Executed Flag to true so that it only executes once
                                    }

                                    if (sxr.CheckTimer())
                                    {
                                        sxr.NextPhase(); // Goes to the next Phase
                                        hasExecuted = false; // sets has Executed Flag to false for the next trial
                                        if (ABATesting)
                                        {
                                            RevertMaterials(); // Reverts environment back to original color/material
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_minus_Object, CS_minus_Sound, CS_minus_Sound_Delay, CS_minus_Object_Interval, timeUntilCSMinusStarts, TotalTrialTimeCsMinus);
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
                                    StartCS(CS_plus_Object, CS_minus_Sound, CS_plus_Sound_Delay, CS_plus_Object_Interval, timeUntilCSPlusStarts, TotalTrialTimeCsPlus);
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

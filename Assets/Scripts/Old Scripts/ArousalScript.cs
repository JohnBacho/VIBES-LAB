using UnityEngine;
using sxr_internal;
using System.Collections; // Required for IEnumerator
using UnityEditor;


namespace SampleExperimentScene
{
    public class ArousalScript : MonoBehaviour
    {
        public bool EyeCalibration; // toggle for eye tracking
        public float CS_plus_Object_Interval; // Enter time for CS+ object to stay active
        public float CS_minus_Object_Interval; // Enter time for CS- object to stay active
        public GameObject US_Sound; // drag and drop CS+ sound to get it to play
        public float US_Sound_Delay; // Enter time for sound delay to play after CS+ object is activated.

        private Vector3 gazeHitPoint; // used in calculating eye tracking data with collisions
        private bool hasExecuted = false; //  used as a way to execute one block of code only once
        private float TotalTrialTimeCsPlus; //Used to calculate the total time of the trial for CS Plus trial
        private float TotalTrialTimeCsMinus; //Used to calculate the total time of the trial for CS Minus trial
        private float timeUntilCSMinusStarts; // Used to calculate when the when to display CS minus object
        private float timeUntilCSPlusStarts; // Used to calculate when the when to display CS plus object
        private int AnticipatedNumber; // Used for when the user enters if they anticipated US

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
                sxr.HideImagesUI();
                hasExecuted = false; // sets has Executed Flag to false for the next trial
            }
        }

        void Start()
        {
            if (EyeCalibration) // set to true in the inspector if you would like to auto launch SRanipal eye tracker calibration
            {
                sxr.LaunchEyeCalibration();
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

                    if (!hasExecuted)
                    {
                        sxr.StartRecordingCameraPos();
                        sxr.StartRecordingEyeTrackerInfo();
                        sxr.StartTimer(5);
                        hasExecuted = true; // set to true so this block of code only runs once
                    }

                    if (sxr.CheckTimer()) // checks if the timer has reached zero
                    {
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
                                    sxr.DisplayImage("Chipmunk 3");
                                    InterTrial(10f);
                                    break;
                            }
                            break;

                        case 1: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    sxr.DisplayImage("Cotton swabs 3", sxr_internal.UI_Position.FullScreen1);
                                    InterTrial(10f);
                                    break;
                            }
                            break;

                        case 2: // CS+
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    sxr.DisplayImage("Injury 4", sxr_internal.UI_Position.FullScreen1);
                                    InterTrial(10f);
                                    break;
                            }
                            break;
                        case 3: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    sxr.DisplayImage("Picnic 1", sxr_internal.UI_Position.FullScreen1);
                                    InterTrial(10f);
                                    break;
                            }
                            break;
                        case 4: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    sxr.DisplayImage("Fire 11", sxr_internal.UI_Position.FullScreen1);
                                    InterTrial(10f);
                                    break;
                            }
                            break;
                        case 5: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    sxr.DisplayImage("Stingray 2", sxr_internal.UI_Position.FullScreen1);
                                    InterTrial(10f);
                                    break;
                            }
                            break;
                        case 6: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    sxr.DisplayImage("Wall 1", sxr_internal.UI_Position.FullScreen1);
                                    InterTrial(10f);
                                    break;
                            }
                            break;
                        case 7: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    sxr.DisplayImage("Explosion 5", sxr_internal.UI_Position.FullScreen1);
                                    InterTrial(10f);
                                    break;
                            }
                            break;
                        case 8: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    sxr.DisplayImage("Dummy 1", sxr_internal.UI_Position.FullScreen1);
                                    InterTrial(10f);
                                    break;
                            }
                            break;
                        case 9: // CS-
                            switch (sxr.GetStepInTrial())
                            {
                                case 0: // CS+
                                    EditorApplication.isPlaying = false;
                                    break;
                            }
                            break;


                    }
                    break; // End of phase case 2
            }

        }
    }
}
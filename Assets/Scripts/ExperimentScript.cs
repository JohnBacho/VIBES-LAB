using UnityEngine;

namespace SampleExperimentScene
{
    public class ExperimentScript : MonoBehaviour
    {
        private int numHits;
        private int guessedNumber;

        void Update()
        {
            switch (sxr.GetPhase())
            {
                case 0: // Start Screen Phase
                    sxr.StartRecordingCameraPos();
                    sxr.StartRecordingEyeTrackerInfo();
                    break;

                case 1: // Instruction Phase
                    switch (sxr.GetStepInTrial())
                    {
                        case 0:
                            sxr.HideImagesUI();
                          
                            break;

                        case 1:
                            break;
                    }
                    break;

                case 2: // Practice Round
                case 3: // Testing Round
                    switch (sxr.GetStepInTrial())
                    {
                        case 0: // Hit trigger to start
                           
                            if (sxr.GetTrigger())
                            {
                                sxr.HideImagesUI();
                                sxr.NextStep();
                                sxr.StartTimer(1000000);
                                // For testing round, pause camera position recording
                            }
                            break;

                        case 1: // Spawn sphere (or move it back to start)
                            if (!sxr.ObjectExists("Sphere"))
                            {
                                sxr.SpawnObject(PrimitiveType.Sphere, "Sphere", 1, 1.5f, 0);
                                sxr.MakeObjectGrabbable("Sphere");
                                sxr.EnableObjectPhysics("Sphere", false);
                            }
                            else
                            {
                                sxr.MoveObjectTo("Sphere", 1, 1.5f, 0);
                            }
                            sxr.NextStep();
                            break;

                        case 2: // Wait until timer ends, then check collisions
                            if (sxr.CheckTimer())
                            {
                                sxr.NextPhase();
                                sxr.ChangeExperimenterTextbox(4, "Number of goals: " + numHits);
                                if (sxr.GetPhase() == 3)
                                {
                                    sxr.PauseRecordingCameraPos();
                                    sxr.WriteToTaggedFile("mainFile", numHits.ToString());
                                }
                            }

                            if (sxr.CheckCollision("Sphere", "TargetBox"))
                            {
                                numHits++;
                                sxr.PlaySound(sxr_internal.ProvidedSounds.Ding);
                                // Removed the extra parameter here:
                                sxr.MoveObjectTo("Sphere", 1, 1.5f, 0);
                            }
                            break;
                    }
                    break;

                case 4: // Input from user
                    sxr.InputSlider(0, 20, "How many times do you think you hit the goal? [" + guessedNumber + "]", true);
                    if (sxr.ParseInputUI(out guessedNumber))
                    {
                        sxr.NextPhase();
                    }
                    break;

                case 5: // Finished
                    sxr.DisplayImage("finished");
                    break;
            }
        }
    }
}
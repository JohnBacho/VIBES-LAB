using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using sxr_internal;

public class ScriptHandler : MonoBehaviour
{
    public DoorOpener doorOpenerLeft; // Pulls in the door script that enables the door to open and close
    public DoorOpener doorOpenerMiddle; // Pulls in the door script that enables the door to open and close
    public DoorOpener doorOpenerRight; // Pulls in the door script that enables the door to open and close

    public ElevatorDoorController ElevatorLeft; // Pulls in the door script that enables the door to open and close
    public ElevatorDoorController ElevatorMiddle; // Pulls in the door script that enables the door to open and close
    public ElevatorDoorController ElevatorRight; // Pulls in the door script that enables the door to open and close

    public LightingHandler lightHandlerLeft; // handles how the light should work
    public LightingHandler lightHandlerMiddle; // handles how the light should work
    public LightingHandler lightHandlerRight; // handles how the light should work

    private DoorOpener DoorControllerSCRIPT; // Used to dynamically change the door script
    private ElevatorDoorController ElevatorDoorControllerSCRIPT; // Used to dynamically change the elevator script
    private LightingHandler LightSCRIPT; // Used to dynamically change the lighting script
    private int index;
    private string ContextName;

    public void AssignLightingAndDoorControllerForStimulusLocation(StimulusLocation position, ContextType ActiveContext)
    {
        LightingHandler[] lights = { lightHandlerLeft, lightHandlerMiddle, lightHandlerRight };
        DoorOpener[] doors = { doorOpenerLeft, doorOpenerMiddle, doorOpenerRight };
        ElevatorDoorController[] elevators = { ElevatorLeft, ElevatorMiddle, ElevatorRight };

        switch (position)
        {
            case StimulusLocation.Left:
                index = 0;
                break;
            case StimulusLocation.Middle:
                index = 1;
                break;
            case StimulusLocation.Right:
                index = 2;
                break;
            default:
                Debug.LogError("Invalid position");
                break;
        }

        LightSCRIPT = lights[index];

        if (ActiveContext == ContextType.B)
        {
            ElevatorDoorControllerSCRIPT = elevators[index];
        }
        else
        {
            DoorControllerSCRIPT = doors[index];
        }

        for (int i = 0; i < lights.Length; i++)
        {
            if (i != index)
                lights[i].ReduceLightIntensity();
        }
    }

    //Door
    public void ShutDoor()
    {
        DoorControllerSCRIPT.ShutDoor();
    }

    public void OpenDoor()
    {
        DoorControllerSCRIPT.OpenDoor();
    }
    //Elevator
    public void ShutElevator()
    {
        ElevatorDoorControllerSCRIPT.ShutDoors();
    }

    public void OpenElevator()
    {
        ElevatorDoorControllerSCRIPT.OpenDoors();
    }

    public void SetStop()
    {
        LightSCRIPT.Stop = false;
    }

    public void StartLightPattern(Color LightColor)
    {
        StartCoroutine(LightSCRIPT.PatternLight(LightColor));
    }

    public void ChangeLightColor(Color LightColor)
    {
        LightSCRIPT.ChangeLightColor(LightColor);
    }

    public void RestAllLights()
    {
        lightHandlerLeft.ResetLight();
        lightHandlerMiddle.ResetLight();
        lightHandlerRight.ResetLight();
    }

    public void TriggerEntryOpen(ContextType ActiveContext)
    {
        if (ActiveContext == ContextType.B)
        {
            OpenElevator();
        }
        else
        {
            OpenDoor();
        }
    }

    public void TriggerEntryClose(ContextType ActiveContext)
    {
        if (ActiveContext == ContextType.B)
        {
            ShutElevator();
        }
        else
        {
            ShutDoor();
        }
    }

   }

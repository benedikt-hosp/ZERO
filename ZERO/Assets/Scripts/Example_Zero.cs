using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class Example_Zero : MonoBehaviour
{
    /*
     * Public variables
     */
    [Header("Choose VR-SDK")]
    public VRProdivers vrProvider;

    [Header("Choose ET-SDK")]
    public Providers eyeTrackingProvider;

    [Header("Choose Save folder")]
    public string dataFolder;

    // Objects
    ZERO etController;
    GazeWriter gazeWriter;


    // Event to call when you want to drop a message in the gaze file
    public event ET_WriteMessageToGazeFile etMSG2File;


    /* Example method calls:
     * Constructors
     * this.etController = new ZERO(eyeTrackingProvider);                                           // ZERO constructor
     * this.gazeWriter = new GazeWriter(userFolder, this, this.etController.getSetEyeTracker);      // GazeWriter constructor
     * 
     * this.etController.etpc.Calibrate())                                                          // Call to the eye tracker to calibrate
     * this.etController.getSetEyeTracker.ET_NewSampleAvailable_Event += GetCurrentGazeSignal;      // Register a method that is called when the eye tracker has a new sample
     * this.etController.StartET();                                                                 // Start eye tracking
     * this.gazeWriter.startGazeWriting();                                                          // Start writing gaze samples to file
     * this.gazeWriter.stopGazeWriting();                                                           // stop gaze writer and close file.
     * this.etController.CloseET();                                                                 // stop eye tracking thread
     */


    // how to read a new gaze sample
    public void GetCurrentGazeSignal(SampleData sd)
    {
            if(sd != null)
            {
                Ray ray = new Ray(sd.worldGazeOrigin, sd.worldGazeDirection);
                Debug.Log("New sample. Origin:" + ray.origin.ToString() + " direction: " + ray.direction.ToString());
            }
    }


}

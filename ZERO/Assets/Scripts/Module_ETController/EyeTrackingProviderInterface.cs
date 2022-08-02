using System;
using System.Collections;
using System.Collections.Generic;
using Tobii.Research.Unity;
using UnityEngine;


public interface EyeTrackingProviderInterface
{

    // Interface variables
    public static bool IsCalibrating { get { return isCalibrationRunning; } set { isCalibrationRunning = value; } }
    public List<SampleData> getCurrentSamples { get { return gazeSamplesOfCP; } }
    public static Queue<SampleData> gazeQueue;
    public static List<SampleData> gazeSamplesOfCP;
    public static bool isCalibrationRunning = false;
    public static MonoBehaviour _mb;
    public static bool isHaversterThreadRunning = false;
    public static SampleData _sampleData;


    /* Events */
    public event ET_NewSampleAvailable_Event ET_NewSampleAvailable_Event;                                     // Event that provides new samples from the eye tracker thread
    public event ET_Started_Event ET_Started_Event;                                                           // Event that tells that the eye tracker started
    public event ET_Stopped_Event ET_Stopped_Event;                                                           // Event that tells that the eye tracker stopped
    public event ET_SampleHarvesterThread_Started_Event ET_SampleHarvesterThread_Started_Event;               // Event that tells that the eye tracker harvester thread is starting to pull new samples from the devices.
    public event ET_SampleHarvesterThread_Stopped_Event ET_SampleHarvesterThread_Stopped_Event;               // Event that tells that the eye tracker harvester thread has stopped.
    public event ET_Calibration_Started_Event ET_Calibration_Started_Event;                                   // Event that is raised when calibration is started
    public event ET_Calibration_Succeded_Event ET_Calibration_Succeded_Event;                                 // Event that is raised when calibration succeeded
    public event ET_Calibration_Failed_Event ET_Calibration_Failed_Event;                                     // Event that is raised when calibration failed.


    public void ClearQueue();

    public bool InitializeDevice();
    public bool Calibrate();

    public void StartSampleHarvesterThread();

    public void GetGazeQueue();

    public void Close();

    public void StopSampleHarvesterThread();

    public long GetCurrentSystemTimestamp();

    public bool SubscribeToGazeData();


}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tobii.XR;
using UnityEngine;
using Debug = UnityEngine.Debug;
using static EyeTrackingProviderInterface;

public class TobiiXRProvider : EyeTrackingProviderInterface
{
   // Tobii XR sepcific
    bool isTobiiXR;

    public event ET_NewSampleAvailable_Event ET_NewSampleAvailable_Event;
    public event ET_Started_Event ET_Started_Event;
    public event ET_Stopped_Event ET_Stopped_Event;
    public event ET_SampleHarvesterThread_Started_Event ET_SampleHarvesterThread_Started_Event;
    public event ET_SampleHarvesterThread_Stopped_Event ET_SampleHarvesterThread_Stopped_Event;
    public event ET_Calibration_Started_Event ET_Calibration_Started_Event;
    public event ET_Calibration_Succeded_Event ET_Calibration_Succeded_Event;
    public event ET_Calibration_Failed_Event ET_Calibration_Failed_Event;

    public bool InitializeDevice()
    {
        _sampleData = new SampleData();
        gazeQueue = new Queue<SampleData>();
        _mb = GameObject.FindObjectOfType<MonoBehaviour>();


        TobiiXR_Settings settings = new TobiiXR_Settings();
         isTobiiXR = TobiiXR.Start(settings);
        ET_Started_Event?.Invoke();



        Debug.Log("Inside initialize tobiixr");

        if (isTobiiXR == false)
        {
            Debug.LogError("Provider Tobii Pro could not connect to an eye tracker.");

            return false;
        }
        else
        {
            Debug.Log("Provider Tobii Pro found VREyeTracker!!!");

            return true;

        }


    }

    public bool Calibrate()
    {
        bool success = false;
        ET_Calibration_Started_Event?.Invoke();

        // BHO TODO: find a way to call Calibration here
        String filename = @"c:\Program Files(x86)\Tobii\Tobii EyeX Config\Tobii.EyeX.Configuration.exe";
        Process foo = new Process();
        foo.StartInfo.FileName = filename;
        foo.StartInfo.Arguments = "-quick -calibration";
        foo.Start();

    //c:\Program Files(x86)\Tobii\Tobii EyeX Config\Tobii.EyeX.Configuration.exe –quick - calibration
        success = true;

        if (success)
            ET_Calibration_Succeded_Event?.Invoke();
        else
            ET_Calibration_Failed_Event?.Invoke();

        return success;
    }

    IEnumerator GetGaze()
    {
        while (isHaversterThreadRunning)
        {

            //var data = TobiiXR.Advanced.LatestData;                                           // ONLY WORKS WITH OCUMEN LICENSE

            var data_local = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local);           // The local eye tracking space shares origin with the XR camera.
                                                                                                // Data reported in this space is unaffected by head movements and is well
                                                                                                // suited for use cases where you need eye tracking data relative to the head, like avatar eyes.

            var data_world = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);           // 	World space is the main tracking space used by Unity. Eye tracking data in world space uses
                                                                                                // 	the same tracking space as objects in your scene and is useful when computing what object is being focused on by the user.

            _sampleData = new SampleData();
            if (data_world != default(TobiiXR_EyeTrackingData) && data_world != null)
            {
                _sampleData.systemTimeStamp = GetCurrentSystemTimestamp();
                _sampleData.deviceTimestamp = -1;                                               // only available when you have a OCUMEN License. Then use: data_world.deviceTimeStamp;
                _sampleData.worldGazeDistance = 20.0f;

                // TobiiXR only allows developers to access more details (e.g. both eyes separately) if you have an OCUMEN license.
                // This implementation only works with the normal CORE API
                if (data_world.GazeRay.IsValid)
                {
                    _sampleData.whichEye = SampleData.Eye.both;
                    _sampleData.isValid = true;
                    _sampleData.worldGazeOrigin = data_world.GazeRay.IsValid ? data_world.GazeRay.Origin : new Vector3(-1,-1,-1);
                    _sampleData.worldGazeDirection = data_world.GazeRay.IsValid ? data_world.GazeRay.Direction : new Vector3(-1, -1, -1);
                    _sampleData.ConvergenceDistance = data_world.ConvergenceDistanceIsValid  ? data_world.ConvergenceDistance :  -1;
                    _sampleData.isBlinkingLeft = data_world.IsLeftEyeBlinking;
                    _sampleData.isBlinkingRight = data_world.IsRightEyeBlinking;

                    if (data_local.GazeRay.IsValid)
                    {
                        _sampleData.whichEye = SampleData.Eye.both;
                        _sampleData.isValid = true;
                        _sampleData.localGazeOrigin = data_local.GazeRay.IsValid ? data_local.GazeRay.Origin : new Vector3(-1, -1, -1);
                        _sampleData.localGazeDirection = data_local.GazeRay.IsValid ? data_local.GazeRay.Direction : new Vector3(-1, -1, -1);
                        _sampleData.ConvergenceDistance = data_local.ConvergenceDistanceIsValid ? data_local.ConvergenceDistance : -1;
                    }

                }

                ET_NewSampleAvailable_Event?.Invoke(_sampleData);   // Raise event that there is a new sample from the eye tracker available.   // Raise event that there is a new sample from the eye tracker available.
                gazeQueue.Enqueue(_sampleData);         // Add new sample to the current gaze queue.

            }

            yield return null;
        }
    }

    public void GetGazeQueue()
    {
        gazeSamplesOfCP = gazeQueue.ToList();
        ClearQueue();
    }
    public long GetCurrentSystemTimestamp()
    {
        return DateTimeOffset.Now.ToUnixTimeMilliseconds();

    }

    public bool SubscribeToGazeData()
    {
        return true;
    }

    public void ClearQueue()
    {
        // clear queue
        gazeQueue.Clear();
    }

    public void Destroy()
    { }

    public void Close()
    {
        TobiiXR.Stop();
        ET_Stopped_Event?.Invoke();
    }

    public SampleData GetGazeLive()
    {
        SampleData sampleData = gazeQueue.Dequeue();
        return sampleData;
    }

    public void StartSampleHarvesterThread()
    {
        isHaversterThreadRunning = true;
        ET_SampleHarvesterThread_Started_Event?.Invoke();
        _mb.StartCoroutine(GetGaze());

    }

    public void StopSampleHarvesterThread()
    {
        isHaversterThreadRunning = false;
        ET_SampleHarvesterThread_Stopped_Event?.Invoke();
    }
}

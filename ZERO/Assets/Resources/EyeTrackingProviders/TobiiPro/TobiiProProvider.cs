using System;
using UnityEngine;

using Tobii.Research.Unity;
using Tobii.Research;
using System.Collections.Generic;
using System.Linq;

using static EyeTrackingProviderInterface;
using System.Collections;

public class TobiiProProvider : EyeTrackingProviderInterface
{
     // TOBII PRO SPECIFIC
    public VRCalibration calibration = VRCalibration.Instance;
    public VREyeTracker _eyeTracker;



    /* Events */
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
        _mb = GameObject.FindObjectOfType<MonoBehaviour>();

        _sampleData = new SampleData();
        _eyeTracker = VREyeTracker.Instance;
        ET_Started_Event?.Invoke();                         // Raise event                         // Raise event
        gazeQueue = new Queue<SampleData>();


        if (_eyeTracker == null)
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

    public void ClearQueue()
    {
        // clear queue
        gazeQueue.Clear();
    }

    public void GetGazeQueue()
    {
        gazeSamplesOfCP = gazeQueue.ToList();
        ClearQueue();

    }

    public SampleData GetGazeLive()
    {
        SampleData sampleData = gazeQueue.Dequeue();
        return sampleData;
    }

    public bool Calibrate()
    {
        ET_Calibration_Started_Event?.Invoke();
        IsCalibrating = true;
        var calibrationStartResult = calibration.StartCalibration(
                resultCallback: (calibrationResult) =>
                    Debug.Log("Calibration was " + (calibrationResult ? "successful" : "unsuccessful"))
                );

        IsCalibrating = false;
        if (calibrationStartResult)
        {
            ET_Calibration_Succeded_Event?.Invoke();
        }
        else
        {
            ET_Calibration_Failed_Event?.Invoke();
        }

        return calibrationStartResult;

    }

    public void Close()
    {

        EyeTrackingOperations.Terminate();
        _eyeTracker = null;
        calibration = null;
        ET_Stopped_Event?.Invoke();
    }

    IEnumerator GetGaze()
    {
        while (isHaversterThreadRunning)
        {
            // In this script we are collecting the original gaze samples.
            // Tobii allows to access "processed" gaze samples, but do not mention what that means.
            // By using the original gaze we have access to more features.
            // If you need help by switching to the processed data contact me ;)
            //var data = _eyeTracker.LatestGazeData;             
            var data = _eyeTracker.NextData;
            _sampleData = new SampleData();
            
            if(data != default(IVRGazeData) && data != null && data.OriginalGaze != null)
            {
                _sampleData.systemTimeStamp = data.OriginalGaze.SystemTimeStamp;
                _sampleData.deviceTimestamp = data.OriginalGaze.DeviceTimeStamp;

                _sampleData.worldGazeDistance = 20.0f;


                if (data.Right != null && data.Left != null)
                {
                    _sampleData.whichEye = SampleData.Eye.both;
                    _sampleData.isValid = true;
                    _sampleData.worldGazeOrigin = data.CombinedGazeRayWorldValid ? data.CombinedGazeRayWorld.origin : new Vector3(-1, -1, -1);
                    _sampleData.worldGazeDirection = data.CombinedGazeRayWorldValid ? data.CombinedGazeRayWorld.direction : new Vector3(-1, -1, -1);
                    _sampleData.cameraPosition = data.Pose.Position;
                    _sampleData.cameraRotation = data.Pose.Rotation;
                    _sampleData.pupilDiameterLeft = data.Left.PupilDiameterValid ? data.Left.PupilDiameter * 1000 : -1.0d;
                    _sampleData.pupilDiameterRight = data.Right.PupilDiameterValid ? data.Right.PupilDiameter * 1000 : -1.0d;

                }
                else if (data.Right != null && data.Left == null)
                {
                    _sampleData.whichEye = SampleData.Eye.right;
                    _sampleData.isValid = data.Right.GazeRayWorldValid;
                    _sampleData.worldGazeOrigin = data.Right.GazeRayWorld.origin;
                    _sampleData.worldGazeDirection = data.Right.GazeRayWorld.direction;
                    _sampleData.cameraPosition = data.Pose.Position;
                    _sampleData.cameraRotation = data.Pose.Rotation;

                    _sampleData.pupilDiameterLeft = data.Left.PupilDiameterValid? data.Left.PupilDiameter * 1000 : -1.0d;
                    _sampleData.pupilDiameterRight = data.Right.PupilDiameterValid? data.Right.PupilDiameter * 1000 : -1.0d;

                }
                else
                {
                    _sampleData.whichEye = SampleData.Eye.left;
                    _sampleData.isValid = data.Left.GazeRayWorldValid;
                    _sampleData.worldGazeOrigin = data.Left.GazeRayWorld.origin;
                    _sampleData.worldGazeDirection = data.Left.GazeRayWorld.direction;
                    _sampleData.cameraPosition = data.Pose.Position;
                    _sampleData.cameraRotation = data.Pose.Rotation;
                    _sampleData.pupilDiameterLeft = data.Left.PupilDiameterValid ? data.Left.PupilDiameter*1000 : -1.0d;
                    _sampleData.pupilDiameterRight = data.Right.PupilDiameterValid ? data.Right.PupilDiameter*1000 : -1.0d;
                }


                // local space
                if (data.Right != null && data.Left != null)
                {
                    _sampleData.whichEye = SampleData.Eye.both;
                    _sampleData.isValid = true;
                    _sampleData.localGazeOrigin = data.Left.GazeOriginValid && data.Right.GazeOriginValid ? (data.Left.GazeOrigin + data.Right.GazeOrigin) / 2 : new Vector3(-1,-1,-1);
                    _sampleData.localGazeDirection = data.Left.GazeDirectionValid && data.Right.GazeDirectionValid ? (data.Left.GazeDirection + data.Right.GazeDirection) / 2 : new Vector3(-1, -1, -1);

                }
                else if (data.Right != null  && data.Left == null)
                {
                    _sampleData.localGazeOrigin = data.Right.GazeOriginValid ? data.Right.GazeOrigin : new Vector3(-1, -1, -1);
                    _sampleData.localGazeDirection = data.Right.GazeDirectionValid ? data.Right.GazeDirection : new Vector3(-1, -1, -1);

                }
                else
                {
                    _sampleData.whichEye = SampleData.Eye.left;
                    _sampleData.isValid = true;
                    _sampleData.localGazeOrigin = data.Left.GazeOriginValid ? data.Left.GazeOrigin : new Vector3(-1, -1, -1);
                    _sampleData.localGazeDirection = data.Left.GazeDirectionValid ? data.Left.GazeDirection : new Vector3(-1, -1, -1);
                }

                ET_NewSampleAvailable_Event?.Invoke(_sampleData);           // Raise event           // Raise event
                gazeQueue.Enqueue(_sampleData);
                    

            }

            yield return null;
        }
    }

    public bool SubscribeToGazeData()
    {
        bool success = false;
        if (_eyeTracker != null)
        { 
            success = _eyeTracker.SubscribeToGazeData;
            if (success)
                Debug.Log("Subscription successfull!");
            else
                Debug.LogError("Subscription failed!");

        }
        else
        {
            Debug.LogError("There is no eye tracker attached to subscribe gaze from.");
        }
        return success;

    }

    public long GetCurrentSystemTimestamp()
    {
        return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

    }

    public void StartSampleHarvesterThread()
    {
        isHaversterThreadRunning = true;
        ET_SampleHarvesterThread_Started_Event?.Invoke();
        _mb.StartCoroutine(this.GetGaze());
    }

    public void StopSampleHarvesterThread()
    {
        isHaversterThreadRunning = false;
        ET_SampleHarvesterThread_Stopped_Event?.Invoke();
    }
}



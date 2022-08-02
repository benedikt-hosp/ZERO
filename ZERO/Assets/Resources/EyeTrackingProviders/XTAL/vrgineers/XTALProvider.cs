using System;
using UnityEngine;

using System.Collections.Generic;
using System.Linq;

using static EyeTrackingProviderInterface;
using System.Collections;

public class XTALProvider : EyeTrackingProviderInterface
{

    // XTAL SPECIFIC
    VrgHmd _eyeTracker;
    VRgEyeTrackingResult l;
    VRgEyeTrackingResult r;


    //// Interface variables
    //public bool IsCalibrating { get { return isCalibrationRunning; } set { isCalibrationRunning = value; } }
    public List<SampleData> getCurrentSamples { get { return gazeSamplesOfCP; } }


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
        _sampleData = new SampleData();
        gazeQueue = new Queue<SampleData>();
        _eyeTracker = GameObject.Find("Camera").GetComponent<VrgHmd>();
        _mb = GameObject.FindObjectOfType<MonoBehaviour>();

        if (_eyeTracker == null)
        {
            Debug.LogError("Provider XTAL could not connect to an eye tracker.");
            return false;
        }
        ET_Started_Event?.Invoke();
        return true;
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

    public bool Calibrate()
    {
        Debug.Log("XTAL Provider started calibration.");
        IsCalibrating = true;
        _eyeTracker.EyeTrackingCalibrate();
        ET_Calibration_Started_Event?.Invoke();

        // We do not get any feedback from XTAL. We have to assume that it worked.
        // On the display you can see a message. If it says "failed", just recalibrate.
        ET_Calibration_Succeded_Event?.Invoke();
        //ET_Calibration_Failed_Event();

        IsCalibrating = false;
        return true;

    }

    public void Close()
    {
        _eyeTracker = null;
        ET_Stopped_Event?.Invoke();
    }

    IEnumerator GetGaze()
    {
        while (isHaversterThreadRunning)
        {
            _sampleData = new SampleData();

            _eyeTracker.GetEyeTrackingRays(out VRgEyeTrackingResult l, out VRgEyeTrackingResult r);
            _sampleData.systemTimeStamp = (long)((l.TimeS + r.TimeS )*1000 / 2);
            _sampleData.worldGazeDistance = 20.0f;
            

            // IN this script we are assuming, that XTAL is providing the world origin and direction
            // to be sure, we save theses values in local and world space.

            if (r.EyeRay != null && l.EyeRay != null) 
            {
                _sampleData.whichEye = SampleData.Eye.both;
                _sampleData.isValid = true;
                _sampleData.worldGazeOrigin = (l.EyePosition + r.EyePosition) /2;
                _sampleData.worldGazeDirection = l.EyeRay - l.EyePosition + r.EyeRay - r.EyePosition;           // we have to substract the eye position because XTAL is somehow adding it to the vector. But why?
                _sampleData.worldGazeDistance = (l.EyeRay.magnitude + r.EyeRay.magnitude) / 2;

                _sampleData.localGazeOrigin = (l.EyePosition + r.EyePosition) / 2;
                _sampleData.localGazeDirection = l.EyeRay - l.EyePosition + r.EyeRay - r.EyePosition;           // we have to substract the eye position because XTAL is somehow adding it to the vector. But why?
            }
            else if (r.EyeRay != null && l.EyeRay == null)
            {
                _sampleData.whichEye = SampleData.Eye.right;
                _sampleData.isValid = true;
                _sampleData.worldGazeOrigin = r.EyePosition;
                _sampleData.worldGazeDirection = r.EyeRay - r.EyePosition;
                _sampleData.worldGazeDistance = r.EyeRay.magnitude;

                _sampleData.localGazeOrigin = r.EyePosition;
                _sampleData.localGazeDirection = r.EyeRay - r.EyePosition;
            }
            else
            {
                _sampleData.whichEye = SampleData.Eye.left;
                _sampleData.isValid = true;
                _sampleData.worldGazeOrigin = l.EyePosition;
                _sampleData.worldGazeDirection = l.EyeRay - l.EyePosition;
                _sampleData.worldGazeDistance = l.EyeRay.magnitude;

                _sampleData.localGazeOrigin = l.EyePosition;
                _sampleData.localGazeDirection = l.EyeRay - l.EyePosition;
            }
            ET_NewSampleAvailable_Event?.Invoke(_sampleData);


            gazeQueue.Enqueue(_sampleData);
            yield return null;

        }
    }

    public bool SubscribeToGazeData()
    {
        bool success = true;
        _eyeTracker.EnableEyeTracking(true);
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
        if(ET_SampleHarvesterThread_Stopped_Event != null)
            ET_SampleHarvesterThread_Stopped_Event();
    }
}



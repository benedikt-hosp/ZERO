using System;
using UnityEngine;

using System.Collections.Generic;
using System.Linq;

using static EyeTrackingProviderInterface;
using System.Collections;
using System.Collections.Concurrent;

public class XTALProvider : EyeTrackingProviderInterface
{


    public static event OnCalibrationStarted OnCalibrationStartedEvent;
    public static event OnCalibrationSucceeded OnCalibrationSucceededEvent;
    public static event OnCalibrationFailed OnCalibrationFailedEvent;

    public event OnAutoIPDCalibrationStarted OnAutoIPDCalibrationStartedEvent;
    public event OnAutoIPDCalibrationSucceeded OnAutoIPDCalibrationSucceededEvent;
    public event OnAutoIPDCalibrationFailed OnAutoIPDCalibrationFailedEvent;

    public event NewGazeSampleReady NewGazesampleReady;
    public event OnCalibrationStarted OnCalibrationStartedEventObj;
    public event OnCalibrationSucceeded OnCalibrationSucceededEventObj;
    public event OnCalibrationFailed OnCalibrationFailedEventObj;


    // Eye Tracking Interface
    private bool isReady = false;
    private bool isCalibrationRunning = false;
    public bool etIsReady { get { return isReady; } }
    public bool isCalibrating { get { return isCalibrationRunning; } set { this.isCalibrationRunning = value; } }
    public List<SampleData> getCurrentSamples { get { return gazeSamplesOfCP; } }
    ConcurrentQueue<SampleData> gazeQueue;
    public List<SampleData> gazeSamplesOfCP;
    public MonoBehaviour _mb = GameObject.FindObjectOfType<MonoBehaviour>();
    // The surrogate MonoBehaviour that we'll use to manage this coroutine.
    SampleData _sampleData;

    bool queueGazeSignal = false;
    bool isHarvestingGaze = false;


    // XTAL SPECIFIC
    VrgHmd _eyeTracker;
    VRgEyeTrackingResult l;
    VRgEyeTrackingResult r;
   
    public bool initializeDevice()
    {
        _sampleData = new SampleData();
        gazeQueue = new ConcurrentQueue<SampleData>();
        this._mb = GameObject.FindObjectOfType<MonoBehaviour>();

        this.isReady = true;
        this._eyeTracker = GameObject.Find("CameraOrigin").GetComponentInChildren<VrgHmd>();

        if (this._eyeTracker == null)
        {
            Debug.LogError("Provider XTAL could not connect to an eye tracker.");

            return false;
        }
        else
        {
            Debug.Log("Provider XTAL found EyeTracker!!!");

            return true;

        }
       
    }
    public void clearQueue()
    {
        // clear queue
        gazeQueue.Clear();
    }

    public void getGazeQueue()
    {
        this.gazeSamplesOfCP = this.gazeQueue.ToList();
        this.clearQueue();

    }

    public void calibrateET()
    {
        Debug.LogError("XTAL Provider started calibration.");
        OnCalibrationStartedEvent?.Invoke();
        OnCalibrationStartedEventObj?.Invoke();
        isCalibrating = true;
        this._eyeTracker.EyeTrackingCalibrate();       
        isCalibrating = false;
        OnCalibrationSucceededEventObj?.Invoke();
        OnCalibrationSucceededEvent?.Invoke();
    }


    public void close()
    {
        isHarvestingGaze = false;
        this._eyeTracker = null;


    }

    public void stopETThread()
    {
        isHarvestingGaze = false;
    }

    public void startETThread()
    {
        isHarvestingGaze = true;
        this._mb.StartCoroutine(getGaze());
    }

    IEnumerator getGaze()
    {
       

        while (isHarvestingGaze)
        {
            VRgEyeTrackingResult l;
            VRgEyeTrackingResult r;

            this._sampleData = new SampleData();

            this._eyeTracker.GetEyeTrackingRays(out l, out r);
            this._sampleData.timeStamp = (long)((l.TimeS + r.TimeS )*1000 / 2);
            this._sampleData.worldGazeDistance = 20.0f;
            
            // data of both eyes availabe
            if (r.EyeRay != null && l.EyeRay != null) 
            {
                this._sampleData.isValid = true;
                this._sampleData.worldGazeOrigin = l.EyePosition + r.EyePosition;
                this._sampleData.worldGazeOrigin_R = r.EyePosition;
                this._sampleData.worldGazeOrigin_L = l.EyePosition;

                this._sampleData.worldGazeDirection = l.EyeRay + r.EyeRay;
                this._sampleData.worldGazeDirection_L = l.EyeRay;
                this._sampleData.worldGazeDirection_R = r.EyeRay;

                this._sampleData.worldGazeDistance = (l.EyeRay.magnitude + r.EyeRay.magnitude) / 2;

                this._sampleData.vergenceAngle_R = r.EyeRay;
                this._sampleData.vergenceAngle_L = l.EyeRay;

                this._sampleData.ipd = Math.Sqrt((Math.Pow(r.EyePosition.x - l.EyePosition.x, 2) + Math.Pow(r.EyePosition.y - l.EyePosition.y, 2) + Math.Pow(r.EyePosition.z - l.EyePosition.z, 2)));



            } // only right eye data available
            else if (r.EyeRay != null && l.EyeRay == null)
            {
                this._sampleData.isValid = true;
                this._sampleData.vergenceAngle_R = r.EyeRay;
                this._sampleData.worldGazeDirection = r.EyeRay;
                this._sampleData.worldGazeDirection_R = r.EyeRay;

                this._sampleData.worldGazeOrigin = r.EyePosition;
                this._sampleData.worldGazeOrigin_R = r.EyePosition;

                this._sampleData.worldGazeDistance = r.EyeRay.magnitude;
            } // only left eye data available
            else
            {
                this._sampleData.isValid = true;
                this._sampleData.vergenceAngle_L = l.EyeRay;
                this._sampleData.worldGazeOrigin = l.EyePosition;
                this._sampleData.worldGazeOrigin_L = l.EyePosition;

                this._sampleData.worldGazeDirection = l.EyeRay;
                this._sampleData.worldGazeDirection_L = l.EyeRay;
                this._sampleData.worldGazeDistance = l.EyeRay.magnitude;

            }
            
            NewGazesampleReady?.Invoke(this._sampleData);

            if(queueGazeSignal)
                gazeQueue.Enqueue(this._sampleData);
            yield return null;

            if (!isHarvestingGaze)
                break;
        }
    }


    public bool subscribeToGazeData()
    {
        bool success = true;
        if(this._eyeTracker != null)
            this._eyeTracker.EnableEyeTracking(true);
        else
            success = false;

        return success;

    }

    public long getCurrentSystemTimestamp()
    {
        return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

    }

    public void calibratePositionAndIPD()
    {
        OnAutoIPDCalibrationStartedEvent?.Invoke();
        this._eyeTracker.RunAutoInterpupillaryDistance();
        OnAutoIPDCalibrationSucceededEvent?.Invoke();
        


    }

}



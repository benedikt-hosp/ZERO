using System;
using UnityEngine;

using Tobii.Research.Unity;
using Tobii.Research;
using System.Collections.Generic;
using System.Linq;

using static EyeTrackingProviderInterface;
using System.Collections;
using System.Collections.Concurrent;
using ViveSR.anipal.Eye;

public class TobiiProProvider : EyeTrackingProviderInterface
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


    // TOBII PRO SPECIFIC
    public VRCalibration calibration = VRCalibration.Instance;
    public VREyeTracker _eyeTracker;

    public bool initializeDevice()
    {
        this._mb = GameObject.FindObjectOfType<MonoBehaviour>();

        _sampleData = new SampleData();
        this._eyeTracker = VREyeTracker.Instance;
        gazeQueue = new ConcurrentQueue<SampleData>();
        this.isReady = true;


        if (this._eyeTracker == null)
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

        bool calibrated = false;

        Debug.Log("TobiiProProvider started calibration.");
        isCalibrating = true;
        OnCalibrationStartedEvent?.Invoke();
        OnCalibrationStartedEventObj?.Invoke();
        isCalibrating = true;
        var calibrationStartResult = calibration.StartCalibration(
                resultCallback: (calibrationResult) =>
                calibrated = calibrationResult
                );


        Debug.Log("Calibration was " + (calibrated ? "successful" : "unsuccessful"));

        if (calibrated)
        { 
            OnCalibrationSucceededEventObj?.Invoke();
            OnCalibrationSucceededEvent?.Invoke();
        }
        else
        {
            OnCalibrationFailedEventObj?.Invoke();
            OnCalibrationFailedEvent?.Invoke();
        }
        isCalibrating = false;
        


    }

    public void close()
    {
        isHarvestingGaze = false;
        this._eyeTracker = null;
        EyeTrackingOperations.Terminate();
        this.calibration = null;
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
       
            var data = this._eyeTracker.NextData;             
            this._sampleData = new SampleData();
            
            if(data != default(IVRGazeData) && data != null)
            {
                _sampleData.timeStamp = data.TimeStamp;                 // long to float

                if(data.Right.GazeRayWorldValid && data.Left.GazeRayWorldValid)
                {
                    _sampleData.isValid = data.CombinedGazeRayWorldValid;
                    _sampleData.worldGazeOrigin = data.CombinedGazeRayWorld.origin;
                    _sampleData.worldGazeDirection = data.CombinedGazeRayWorld.direction;
                }
                else if (data.Right.GazeRayWorldValid && !data.Left.GazeRayWorldValid)
                {
                    _sampleData.isValid = data.Right.GazeRayWorldValid;
                    _sampleData.worldGazeOrigin = data.Right.GazeRayWorld.origin;
                    _sampleData.worldGazeDirection = data.Right.GazeRayWorld.direction;
                }
                else
                {
                    _sampleData.isValid = data.Left.GazeRayWorldValid;
                    _sampleData.worldGazeOrigin = data.Left.GazeRayWorld.origin;
                    _sampleData.worldGazeDirection = data.Left.GazeRayWorld.direction;
                }


                _sampleData.worldGazeDistance = 20.0f;
                NewGazesampleReady?.Invoke(this._sampleData);

                if (queueGazeSignal)
                    gazeQueue.Enqueue(this._sampleData);

            }

           
            yield return null;

            if (!isHarvestingGaze)
                break;
        }
    }

    public bool subscribeToGazeData()
    {
        bool success = false;
        if (this._eyeTracker != null)
        { 
            success = this._eyeTracker.SubscribeToGazeData;
            if (success)
                Debug.Log("Subscription successfull!");
            else
                Debug.LogError("Subscription failed!");

            success = this._eyeTracker.SubscribeToGazeData;
        }
        else
        {
            Debug.LogError("There is no eye tracker attached to subscribe gaze from.");
        }
        return success;

    }

    public long getCurrentSystemTimestamp()
    {
        return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

    }
    public void calibratePositionAndIPD()
    {
        // As far as i know this is not possible with this headset.
    }

}



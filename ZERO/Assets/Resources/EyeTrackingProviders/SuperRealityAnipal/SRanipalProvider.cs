using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


using ViveSR.anipal.Eye;
using System.Collections;
using System.Collections.Concurrent;

public class SRanipalProvider : EyeTrackingProviderInterface
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


    // SRanipal Specific
    private GameObject _sranipalGameObject;
    private SRanipal_Eye_Framework sranipal;
    private EyeData_v2 eyeData = new EyeData_v2();



    public bool initializeDevice()
    {
        _sampleData = new SampleData();
        gazeQueue = new ConcurrentQueue<SampleData>();
        this._mb = GameObject.FindObjectOfType<MonoBehaviour>();

        this.isReady = true;

        // SRanipal specific
        this._sranipalGameObject = new GameObject("EyeFramework");
        this.sranipal = _sranipalGameObject.AddComponent<SRanipal_Eye_Framework>();

        if (!SRanipal_Eye_API.IsViveProEye()) return false;
        this.sranipal.StartFramework();


        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING)
        {
            this.sranipal.StartFramework();
        }

        return (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING);                
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
        while(isHarvestingGaze)
        {
            this._sampleData = new SampleData();
            bool success = SRanipal_Eye_v2.GetVerboseData(out eyeData.verbose_data);


            if (success)
            {
                if (!this.eyeData.Equals(default(EyeData_v2)))
                {
                    Vector3 origin;
                    Vector3 direction;
                    this._sampleData.timeStamp = getCurrentSystemTimestamp();

                    if (!this.eyeData.verbose_data.right.Equals(default(SingleEyeData)) && !this.eyeData.verbose_data.right.Equals(default(SingleEyeData)))
                    {

                        this._sampleData.isValid = SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out origin, out direction);
                        this._sampleData.worldGazeOrigin = origin;
                        this._sampleData.worldGazeDirection = direction;
                        this._sampleData.worldGazeDistance = eyeData.verbose_data.combined.convergence_distance_mm / 1000;

                    }
                    else if (!this.eyeData.verbose_data.right.Equals(default(SingleEyeData)) && this.eyeData.verbose_data.right.Equals(default(SingleEyeData)))
                    {
                        this._sampleData.isValid = SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out origin, out direction);
                        this._sampleData.worldGazeOrigin = origin;
                        this._sampleData.worldGazeDirection = direction;
                        this._sampleData.worldGazeDistance = eyeData.verbose_data.combined.convergence_distance_mm / 1000;

                    }
                    else
                    {
                        this._sampleData.isValid = SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out origin, out direction);
                        this._sampleData.worldGazeOrigin = origin;
                        this._sampleData.worldGazeDirection = direction;
                        this._sampleData.worldGazeDistance = eyeData.verbose_data.combined.convergence_distance_mm / 1000;
                    }
                    NewGazesampleReady?.Invoke(this._sampleData);

                    if (queueGazeSignal)
                        gazeQueue.Enqueue(this._sampleData);
                  
                }        
            }
            yield return null;

            if (!isHarvestingGaze)
                break;

        }
    }

    public void calibrateET()
    {
        Debug.LogError("SRanipal Provider started calibration.");
        OnCalibrationStartedEvent?.Invoke();
        OnCalibrationStartedEventObj?.Invoke();
        isCalibrating = true;

        if (SRanipal_Eye_v2.LaunchEyeCalibration())
            OnCalibrationSucceededEventObj?.Invoke();
        else
            OnCalibrationSucceededEvent?.Invoke();

        isCalibrating = false;

    }

    public void close()
    {
        isHarvestingGaze = false;
        this.sranipal = null;


    }

    public bool subscribeToGazeData()
    {      
        return SRanipal_Eye_Framework.Instance.enabled = true;
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


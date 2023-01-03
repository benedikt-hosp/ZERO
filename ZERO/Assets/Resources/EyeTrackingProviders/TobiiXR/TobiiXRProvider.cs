using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tobii.XR;
using UnityEngine;
using Debug = UnityEngine.Debug;
using static EyeTrackingProviderInterface;
using System.Collections.Concurrent;

public class TobiiXRProvider : EyeTrackingProviderInterface
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





    // Tobii XR sepcific
    bool isTobiiXR;


    public bool initializeDevice()
    {
        _sampleData = new SampleData();
        gazeQueue = new ConcurrentQueue<SampleData>();
        _mb = GameObject.FindObjectOfType<MonoBehaviour>();
        TobiiXR_Settings settings = new TobiiXR_Settings();
        isTobiiXR = TobiiXR.Start(settings);

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
        // BHO TODO: find a way to call Calibration here
        String filename = @"c:\Program Files(x86)\Tobii\Tobii EyeX Config\Tobii.EyeX.Configuration.exe";
        Process foo = new Process();
        foo.StartInfo.FileName = filename;
        foo.StartInfo.Arguments = "-quick -calibration";
        foo.Start();

        //c:\Program File(x86)\Tobii\Tobii EyeX Config\Tobii.EyeX.Configuration.exe –quick - calibration

        isCalibrating = false;
        OnCalibrationSucceededEventObj?.Invoke();
        OnCalibrationSucceededEvent?.Invoke();

    }

    IEnumerator getGaze()
    {
        while (isHarvestingGaze)
        {

            //var data = TobiiXR.Advanced.LatestData;                                           // ONLY WORKS WITH OCUMEN LICENSE

            var data_local = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local);           // The local eye tracking space shares origin with the XR camera.
                                                                                                // Data reported in this space is unaffected by head movements and is well
                                                                                                // suited for use cases where you need eye tracking data relative to the head, like avatar eyes.

            var data_world = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);           // 	World space is the main tracking space used by Unity. Eye tracking data in world space uses
                                                                                                // 	the same tracking space as objects in your scene and is useful when computing what object is being focused on by the user.

            //_sampleData = new SampleData();
            //if (data_world != default(TobiiXR_EyeTrackingData) && data_world != null)
            //{
            //    _sampleData.timeStamp = GetCurrentSystemTimestamp();
            //    //_sampleData.deviceTimestamp = -1;                                               // only available when you have a OCUMEN License. Then use: data_world.deviceTimeStamp;
            //    _sampleData.worldGazeDistance = 20.0f;

            //    // TobiiXR only allows developers to access more details (e.g. both eyes separately) if you have an OCUMEN license.
            //    // This implementation only works with the normal CORE API
            //    if (data_world.GazeRay.IsValid)
            //    {
            //        _sampleData.whichEye = SampleData.Eye.both;
            //        _sampleData.isValid = true;
            //        _sampleData.worldGazeOrigin = data_world.GazeRay.IsValid ? data_world.GazeRay.Origin : new Vector3(-1,-1,-1);
            //        _sampleData.worldGazeDirection = data_world.GazeRay.IsValid ? data_world.GazeRay.Direction : new Vector3(-1, -1, -1);
            //        _sampleData.ConvergenceDistance = data_world.ConvergenceDistanceIsValid  ? data_world.ConvergenceDistance :  -1;
            //        _sampleData.isBlinkingLeft = data_world.IsLeftEyeBlinking;
            //        _sampleData.isBlinkingRight = data_world.IsRightEyeBlinking;

            //        if (data_local.GazeRay.IsValid)
            //        {
            //            _sampleData.whichEye = SampleData.Eye.both;
            //            _sampleData.isValid = true;
            //            _sampleData.localGazeOrigin = data_local.GazeRay.IsValid ? data_local.GazeRay.Origin : new Vector3(-1, -1, -1);
            //            _sampleData.localGazeDirection = data_local.GazeRay.IsValid ? data_local.GazeRay.Direction : new Vector3(-1, -1, -1);
            //            _sampleData.ConvergenceDistance = data_local.ConvergenceDistanceIsValid ? data_local.ConvergenceDistance : -1;
            //        }

            //    }


                //NewGazesampleReady?.Invoke(this._sampleData);

                //if (queueGazeSignal)
                //    gazeQueue.Enqueue(this._sampleData);
            //}


            yield return null;

            if (!isHarvestingGaze)
                break;
        }
    }

    public long GetCurrentSystemTimestamp()
    {
        return DateTimeOffset.Now.ToUnixTimeMilliseconds();

    }

    public bool subscribeToGazeData()
    {
        return true;
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

    public void close()
    {
        TobiiXR.Stop();
        isHarvestingGaze = false;
    }


    public void calibratePositionAndIPD()
    {
        throw new NotImplementedException();
    }

}

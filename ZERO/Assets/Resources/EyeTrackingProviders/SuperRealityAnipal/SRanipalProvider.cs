using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static EyeTrackingProviderInterface;
using ViveSR.anipal.Eye;
using System.Collections;

public class SRanipalProvider : EyeTrackingProviderInterface
{

    // SRanipal Specific
    public GameObject _sranipalGameObject;
    public SRanipal_Eye_Framework sranipal;
    public EyeData_v2 eyeData = new EyeData_v2();

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
        gazeQueue = new Queue<SampleData>();


        _sranipalGameObject = new GameObject("EyeFramework");
        sranipal = _sranipalGameObject.AddComponent<SRanipal_Eye_Framework>();

        if (!SRanipal_Eye_API.IsViveProEye()) return false;
        sranipal.StartFramework();


        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING)
        {
             sranipal.StartFramework();
        }

        if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING)
        { 
            ET_Started_Event?.Invoke();
            return true;
        }

        return false;                
    }

    IEnumerator GetGaze()
    { 
        while(isHaversterThreadRunning)
        {
            _sampleData = new SampleData();
            bool success = SRanipal_Eye_v2.GetVerboseData(out eyeData.verbose_data);

            if (success)
            {
                if (!eyeData.Equals(default(EyeData_v2)))
                {
                    Vector3 origin;
                    Vector3 direction;

                    _sampleData.systemTimeStamp = GetCurrentSystemTimestamp();
                    _sampleData.deviceTimestamp = eyeData.timestamp;
                    _sampleData.worldGazeDistance = 20.0f;

                    if (!eyeData.verbose_data.right.Equals(default(SingleEyeData)) && !eyeData.verbose_data.right.Equals(default(SingleEyeData)))
                    {
                        _sampleData.whichEye = SampleData.Eye.both;
                        _sampleData.isValid = true;

                        _sampleData.isValid = SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out origin, out direction);
                        _sampleData.worldGazeOrigin = origin;
                        _sampleData.worldGazeDirection = direction;

                        _sampleData.localGazeOrigin = origin;
                        _sampleData.localGazeDirection = direction;

                        _sampleData.ConvergenceDistance = eyeData.verbose_data.combined.convergence_distance_validity ? eyeData.verbose_data.combined.convergence_distance_mm : 20.0f;
                        _sampleData.pupilDiameterLeft = eyeData.verbose_data.left.pupil_diameter_mm;
                        _sampleData.pupilDiameterRight = eyeData.verbose_data.right.pupil_diameter_mm;

                    }
                    else if (!eyeData.verbose_data.right.Equals(default(SingleEyeData)) && eyeData.verbose_data.right.Equals(default(SingleEyeData)))
                    {
                        _sampleData.whichEye = SampleData.Eye.right;
                        _sampleData.isValid = SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out origin, out direction);
                        _sampleData.worldGazeOrigin = origin;
                        _sampleData.worldGazeDirection = direction;
                        
                        _sampleData.localGazeOrigin = origin;
                        _sampleData.localGazeDirection = direction;

                        _sampleData.pupilDiameterLeft = -1.0f;
                        _sampleData.pupilDiameterRight = eyeData.verbose_data.right.pupil_diameter_mm;

                    }
                    else
                    {
                        _sampleData.whichEye = SampleData.Eye.left;
                        _sampleData.isValid = SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out origin, out direction);
                        _sampleData.worldGazeOrigin = origin;
                        _sampleData.worldGazeDirection = direction;

                        _sampleData.localGazeOrigin = origin;
                        _sampleData.localGazeDirection = direction;

                        _sampleData.pupilDiameterRight = -1.0f;
                        _sampleData.pupilDiameterLeft = eyeData.verbose_data.left.pupil_diameter_mm;
                    }


                    ET_NewSampleAvailable_Event?.Invoke(_sampleData);
                    gazeQueue.Enqueue(_sampleData);

                }        
            }
            yield return null;

        }
    }


    public void ClearQueue()
    {
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

    public void Destroy()
    {
        sranipal = null;
            
    }

    public bool Calibrate()
    {
        isCalibrationRunning = true;
        ET_Calibration_Started_Event?.Invoke(); 
        bool success = SRanipal_Eye_v2.LaunchEyeCalibration();
        isCalibrationRunning = false;
        if (success)
            ET_Calibration_Succeded_Event?.Invoke();
        else
            ET_Calibration_Failed_Event?.Invoke();

        return success;
        
    }

    public void Close()
    {
        sranipal = null;
        ET_Stopped_Event?.Invoke();


    }

    public bool SubscribeToGazeData()
    {
         if (!SRanipal_Eye_Framework.Instance.enabled)
         {
            SRanipal_Eye_Framework.Instance.enabled = true;
         }
        return SRanipal_Eye_Framework.Instance.enabled;

    }

    public long GetCurrentSystemTimestamp()
    {
        return DateTimeOffset.Now.ToUnixTimeMilliseconds();

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


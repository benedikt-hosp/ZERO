using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
//using ViveSR.anipal.Eye;



[System.Serializable]
public class ZERO
{
    public Camera fieldCam;
    // EyeTracking Controller
    public bool calibrated = false;
    public EyeTrackingProviderController etpc;
    private Providers eyeTrackingProvider;
    Camera[] cams;
    private string userFolder;
    GazeTracker gazeTracker;


    public EyeTrackingProviderController getSetEyetrackingProvider { get { return this.etpc; } }

    public ZERO(Providers eyeTrackingProvider, Zero exZero)
    {

        this.eyeTrackingProvider = eyeTrackingProvider;
        this.loadETGameObjects();
        this.etpc = new EyeTrackingProviderController(this.eyeTrackingProvider);
        if (exZero.writeGazeToFile)                                                 // if wanted, create gazeWriter
            this.gazeTracker = new GazeTracker(this.userFolder, this.etpc.eyeTrackingProviderInterface, exZero);




    }

    public bool isCalibrated()
    {
        return this.calibrated;
    }

    private void loadETGameObjects()
    {
        GameObject _gameobject;
        switch (this.eyeTrackingProvider)
        {
            case Providers.XTAL:
                _gameobject = loadGameobject("EyeTrackingProviders/XTAL/vrgineers/", "CameraOrigin");

                this.cams = _gameobject.GetComponentsInChildren<Camera>();

                if (this.cams.Length >= 0)
                {
                    this.cams[1].tag = "MainCamera";
                    this.cams[1].enabled = true;

                    Debug.LogWarning("Main Camera is set");
                }
                else
                {
                    Debug.LogError("There are multiple cameras. Could not decide which to set to main camera.");
                }


                // 2
                _gameobject = loadGameobject("EyeTrackingProviders/XTAL/vrgineers/", "Controllers");

                break;
            default:
                break;

        }
    }


    public GameObject loadGameobject(string path, string name)
    {
        GameObject instance = null;
        if (!GameObject.Find(name))
        {
            instance = GameObject.Instantiate(Resources.Load(path + name, typeof(GameObject))) as GameObject;
            instance.name = name;
            UnityEngine.Object.DontDestroyOnLoad(instance);
        }




        return instance;
    }


    public void startET()
    {
        this.etpc.SubscribeToGaze();
        this.etpc.startETThread();      // writes into a Queue

    }

    public bool setPositionAndIPD()
    {
        this.etpc.CalibratePositionAndIPD();
        return true;
    }

    public void stop()
    {
        this.etpc.stop();
    }

    public void close()
    {

        this.etpc.Close();

    }



}

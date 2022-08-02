using UnityEngine;
using ViveSR.anipal.Eye;


[System.Serializable]
public class ETController
{

    // EyeTracking Controller
    public bool calibrated = false;
    public EyeTrackingProviderController etpc;
    public GazeWriter eventTracker;

    public EyeTrackingProviderInterface getSetEyeTracker { get { return this.etpc.eyeTrackingProviderInterface; } }

    public ETController(Providers eyeTrackingProvider)
    {
        this.loadETGameObjects(eyeTrackingProvider);
        this.etpc = new EyeTrackingProviderController(eyeTrackingProvider);
    }

    public bool isCalibrated()
    {
        return this.calibrated;
    }

    private void loadETGameObjects(Providers eyeTrackingProvider)
    {
        GameObject _gameobject;
        Camera[] cams;
        switch (eyeTrackingProvider)
        {
            case Providers.HTCViveSranipal:

                // 1
                _gameobject = loadGameobject("SteamVR/Prefabs/", "[CameraRig]");
                cams = _gameobject.GetComponentsInChildren<Camera>();
                if (cams.Length > 0)
                {
                    cams[0].tag = "MainCamera";
                    Debug.Log("Main Camera is set");
                }
                else
                {
                    Debug.LogError("There are multiple cameras. Could not decide which to set to main camera.");
                }

                // 2
                _gameobject = loadGameobject("EyeTrackingProviders/SuperRealityAnipal/ViveSR/Prefabs/", "SRanipal Eye Framework");
                _gameobject.GetComponent<SRanipal_Eye_Framework>().EnableEye = true;
                _gameobject.GetComponent<SRanipal_Eye_Framework>().EnableEyeVersion = SRanipal_Eye_Framework.SupportedEyeVersion.version2;

                _gameobject = null;
                break;


            case Providers.PupiLabs:
                break;
            case Providers.TobiiXR:
                break;
            case Providers.TobiiPro:
                // 1
                _gameobject = loadGameobject("SteamVR/Prefabs/", "[CameraRig]");

                cams = _gameobject.GetComponentsInChildren<Camera>();
                if (cams.Length > 0)
                {
                    cams[0].tag = "MainCamera";
                    Debug.Log("Main Camera is set");
                }
                else
                {
                    Debug.LogError("There are multiple cameras. Could not decide which to set to main camera.");
                }

                // 2
                _gameobject = loadGameobject("EyeTrackingProviders/TobiiPro/VR/Prefabs/", "[VRCalibration]");
                _gameobject = loadGameobject("EyeTrackingProviders/TobiiPro/VR/Prefabs/", "[VREyeTracker]");
                _gameobject = loadGameobject("EyeTrackingProviders/TobiiPro/VR/Prefabs/", "VRCalibrationPoint");
                _gameobject = null;
                break;
            


            case Providers.XTAL:
                // 1
                
                _gameobject = loadGameobject("EyeTrackingProviders/XTAL/vrgineers/", "CameraOrigin");
                cams = _gameobject.GetComponentsInChildren<Camera>();
                if (cams.Length > 0)
                {
                    cams[0].tag = "MainCamera";
                    Debug.Log("Main Camera is set");
                }
                else
                {
                    Debug.LogError("There are multiple cameras. Could not decide which to set to main camera.");
                }


                // 2
                //_gameobject = loadGameobject("EyeTrackingProviders/XTAL/vrgineers/", "Controllers");
                //_gameobject = null;

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
        }
            

        
        
        return instance;
    }


    public void startET()
    {
        this.etpc.SubscribeToGaze();        
    }

  
    public void startSampleHarvester()
    {
        this.etpc.startETThread();
    }

    public void stopSampleHarvester()
    {
        this.etpc.stopETThread();
    }


    public void close()
    {
       this.etpc.close();
    }



}
        
using System;
using UnityEngine;


public class EyeTrackingProviderController
{
    public EyeTrackingProviderInterface eyeTrackingProviderInterface;

    private const string SranipalProviderName = "SRanipalProvider";
    private const string TobiiXRProviderName = "TobiiXRProvider";
    private const string PupilProviderName = "PupilProvider";
    private const string TobiiProProviderName = "TobiiProProvider"; // BHO
    private const string XTALProviderName = "XTALProvider"; // BHO
    public bool ETReady = false;

    // default value
    private string _currentProviderName = TobiiProProviderName;
    private Providers providerSDK;

    public EyeTrackingProviderInterface getSetETProvider { get { return eyeTrackingProviderInterface; } }


    public EyeTrackingProviderController(Providers providerSDK)
    {
        this.providerSDK = providerSDK;
        InitGazeProvider();
        
        
    }

    private void InitGazeProvider()
    {
        if (eyeTrackingProviderInterface != null) return;
        Debug.Log("Initializing provider: " + this.providerSDK);
        UpdateCurrentProvider();

        eyeTrackingProviderInterface = GetProvider();



        if (eyeTrackingProviderInterface != null)
        {
            bool success = eyeTrackingProviderInterface.InitializeDevice();
            if (success)
            {
                Debug.Log("Initialized device!");
                ETReady = true;
            }
            else
                Debug.Log("Cannot initialize device");

        }
        else
        {
            Debug.LogError("ETPC: does not work!");
        }

    }

    private void UpdateCurrentProvider()
    {

        switch (this.providerSDK)
        {
            case Providers.HTCViveSranipal:
                _currentProviderName = SranipalProviderName;
                break;
            case Providers.PupiLabs:
                _currentProviderName = PupilProviderName;
                break;
            case Providers.TobiiXR:
                _currentProviderName = TobiiXRProviderName;
                break;
            case Providers.TobiiPro:                                        // BHO
                _currentProviderName = TobiiProProviderName;                    // BHO
                break;
            case Providers.XTAL:
                _currentProviderName = XTALProviderName;
                break;
            default:
                return;
        }

    }

    private EyeTrackingProviderInterface GetProvider()
    {
        return GetProviderFromName(_currentProviderName);
    }

    /* Searches for the choosen Implementation file of the EyeTrackingProviderInterface
   * and activates it. So we use the correct file which corresponds to the currently choosen Eye-Tracker
   */
    public EyeTrackingProviderInterface GetProviderFromName(string ProviderName)
    {
        Type providerType = Type.GetType(ProviderName);
        if (providerType == null)
        {
            Debug.Log("provider type not found");
            return null;
        }
        else
        {
            Debug.Log("Found provider " + providerType.FullName + " going to load it...");
        }

        try
        {
            var tmp = Activator.CreateInstance(providerType) as EyeTrackingProviderInterface;
            if (tmp != null)
            {
                Debug.Log("Activated instance of provider" + tmp.ToString());
            }
            return tmp;
        }
        catch (Exception)
        {
            Debug.LogError("There was an error instantiating the gaze provider: " + ProviderName);
        }
        return null;
    }

    // ======================================== Calibration
    public bool Calibrate()
    {
        bool success = false;
        Debug.Log("ETController: started calibration");
        if (eyeTrackingProviderInterface != null)
        {
            success = eyeTrackingProviderInterface.Calibrate();
        }

        return success;


    }

    public void startETThread()
    {
        eyeTrackingProviderInterface.StartSampleHarvesterThread();
    }

    public void stopETThread()
    {
        eyeTrackingProviderInterface.StopSampleHarvesterThread();
    }

    public void close()
    {
        eyeTrackingProviderInterface.Close();
    }

    public bool SubscribeToGaze()
    {
        Debug.Log("Subscribed to gaze signal.");

        bool registrered = eyeTrackingProviderInterface.SubscribeToGazeData();
        if (!registrered)
            Debug.LogWarning("Could not subscribe to gaze");
        return registrered;
            
    }

    
}
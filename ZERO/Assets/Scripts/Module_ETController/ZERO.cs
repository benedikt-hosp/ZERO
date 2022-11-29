using UnityEngine;
using ViveSR.anipal.Eye;


[System.Serializable]
public class ZERO
{

    // EyeTracking Controller
    public bool calibrated = false;
    public EyeTrackingProviderController etpc;
    public GazeWriter eventTracker;

    public EyeTrackingProviderInterface getSetEyeTracker { get { return this.etpc.eyeTrackingProviderInterface; } }

    public ZERO(Providers eyeTrackingProvider)
    {
        this.etpc = new EyeTrackingProviderController(eyeTrackingProvider);
    }

    public bool isCalibrated()
    {
        return this.calibrated;
    }

    public void StartET()
    {
        this.etpc.StartET();
    }

    public void CloseET()
    {
        this.etpc.CloseET();
    }



}
        
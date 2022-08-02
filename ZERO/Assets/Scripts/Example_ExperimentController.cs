using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Example_ExperimentController : MonoBehaviour
{
    /*
     * Public variables
     */
    [Header("Choose VR-SDK")]
    public VRProdivers vrProvider;

    [Header("Choose ET-SDK")]
    public Providers eyeTrackingProvider;

    [Header("Choose Save folder")]
    public string dataFolder;

    // Objects
    ETController etController;
    GazeWriter gazeWriter;

    /*
     * Private variables
     */
    private string userFolder;
    private int fileCounter = 1;

    void OnEnable()
    {
        userFolder = createUserFolder();


        // Activate gaze objects
            this.etController = new ETController(eyeTrackingProvider);


        // add GazeWriter Object
        this.gazeWriter = new GazeWriter(userFolder, this.etController.getSetEyeTracker);

    }

    // Start is called before the first frame update
    void Start()
    {
        // Register listener to the event of the eye tracker when there is a new sample.
        this.etController.getSetEyeTracker.ET_NewSampleAvailable_Event += GetCurrentGazeSignal;

        // Start eye tracking
        this.etController.startET();
        this.startWritingGaze();


    }

    private string createUserFolder()
    {
        string oldName;
        if (dataFolder == null)
        {
            userFolder = "C:\\ET-Output-Test\\Userfolder"; 
        }
        else
        {
            userFolder = dataFolder;
        }

        oldName = userFolder;

        while (Directory.Exists(userFolder))
        {
            userFolder = oldName + fileCounter.ToString();
            fileCounter += 1;
        }

        Directory.CreateDirectory(userFolder);
        return userFolder;  
    }

    public void GetCurrentGazeSignal(SampleData sd)
    {
            if(sd != null)
            {
                Ray ray = new Ray(sd.worldGazeOrigin, sd.worldGazeDirection);
                Debug.Log("New sample. Origin:" + ray.origin.ToString() + " direction: " + ray.direction.ToString());
            }
    }

    // Update is called once per frame
    void Update()
    {
        // start eye tracking calibration
        if (Input.GetKeyUp(KeyCode.C))
        {
            if (this.etController.etpc.Calibrate())
            {
                this.etController.calibrated = true;
            }

            this.etController.calibrated = true;
        }

        // wait for Space input key down to continue
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if(this.etController.calibrated)
            {

                this.etController.startSampleHarvester();
                // DO whatever you want to do in the update loop.


            }
        }



        // leave application
        if (!Input.GetKeyUp(KeyCode.Escape))
        {


        }
        else
        {
            Debug.Log("Eye Tracker not found or Finished showing all conditions.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
             Application.OpenURL(webplayerQuitURL);
#else
             Application.Quit();
#endif
        }

    }
    public void startWritingGaze()
    {
        // writes into a file
        this.gazeWriter.startGazeWriting();
    }

    public void stopWritingGaze()
    {
        // stop writing
        this.gazeWriter.stopGazeWriting();
    }

    private void OnDisable()
    {
        this.stopWritingGaze();

        this.etController.stopSampleHarvester();

        etController.close();
        this.gazeWriter.Close();


    }
}

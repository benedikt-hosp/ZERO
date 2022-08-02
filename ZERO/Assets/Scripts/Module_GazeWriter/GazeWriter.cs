using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Tobii.Research.Unity;
using UnityEngine;

public class GazeWriter    
{

    EyeTrackingProviderInterface m_EyeTrackingProvider;
    StreamWriter gazeStreamWriter;
    List<SampleData> currentSamples;
    string gazeFile;
    public MonoBehaviour _mb;
    private int saveGazePeriod = 2;                                         // define the period, the script should save gaze samples to file    


    string header = "System Timestamp\t Device Timestamp \t Camera Position X\t Camera Position y\t Camera position z\t" +
                    "Camera Rotation x\t Camera Rotation y\t Camera Rotation z\t" +
                    "Eye\t Valid \t isBlinkLeft \t isBlinkRight\t " +
                    "Local Gaze Origin X\t" + "Local Gaze Origin Y\t" + "Local Gaze Origin Z\t" +
                    "Local Gaze Direction X\t" + "Local Gaze Direction Y\t" + "Local Gaze Direction Z\t" +
                    "World Gaze Origin X\t" + "World Gaze Origin Y\t" + "World Gaze Origin Z\t" +
                    "World Gaze Direction X\t" + "World Gaze Direction Y\t" + "World Gaze Direction Z\t" +
                    "World Gaze Distance\t" + "Convergence Distance m\t" +
                    "World Gaze Point X\t" + "World Gaze Point Y\t" + "World Gaze Point Z\t" +
                    "Pupil Diameter Right mm\t" + "Pupil Diameter Left mm\t" +
                    "Accuracy \t" + "Precision";


    private bool isWritingGazeFile;

    public GazeWriter(string userFolder, EyeTrackingProviderInterface eyeTrackerObject)
    {
        _mb = GameObject.FindObjectOfType<MonoBehaviour>();

        m_EyeTrackingProvider = eyeTrackerObject;
        gazeFile = Path.Combine(userFolder, "gaze.csv");
        
        Debug.Log("Saving event file to " + gazeFile);
        this.gazeStreamWriter = new StreamWriter(gazeFile);
        this.gazeStreamWriter.WriteLine(header);
        this.gazeStreamWriter.Flush();

    }


    public void startGazeWriting()
    {
        isWritingGazeFile = true;
        this._mb.StartCoroutine(writeGazePeriodically());
    }

    public void stopGazeWriting()
    {
        isWritingGazeFile = false;
    }

    IEnumerator writeGazePeriodically()
    {

        while(isWritingGazeFile)
        {
            m_EyeTrackingProvider.GetGazeQueue();
            currentSamples = m_EyeTrackingProvider.getCurrentSamples;


            if (currentSamples != null)
            {
                WriteGazeToFile(currentSamples);

            }
            yield return new WaitForSeconds(this.saveGazePeriod);

        }
    }

    public long getCurrentSystemTimestamp()
    {
        return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

    }

    private void WriteGazeToFile(List<SampleData> currentSamples)
    {

        for (int i = 0; i < currentSamples.Count; i++)
        {
            this.WriteGazeData(currentSamples[i]);
        }
    }

    private void WriteGazeData(SampleData gazeData)
    {
            string sampleLine = gazeData.systemTimeStamp.ToString() + "\t" + gazeData.deviceTimestamp.ToString() + "\t" +
                gazeData.cameraPosition.x.ToString() + "\t" + gazeData.cameraPosition.y.ToString() + "\t" + gazeData.cameraPosition.z.ToString() + "\t" +
                gazeData.cameraRotation.x.ToString() + "\t" + gazeData.cameraRotation.y.ToString() + "\t" + gazeData.cameraRotation.z.ToString() + "\t" +

                gazeData.whichEye.ToString() + "\t" + gazeData.isValid.ToString() + "\t" + gazeData.isBlinkingLeft.ToString() + "\t" + gazeData.isBlinkingRight.ToString() + "\t" +

                gazeData.localGazeOrigin.x.ToString() + "\t" + gazeData.localGazeOrigin.y.ToString() + "\t" + gazeData.localGazeOrigin.y.ToString() + "\t" +
                gazeData.localGazeDirection.x.ToString() + "\t" + gazeData.localGazeDirection.y.ToString() + "\t" + gazeData.localGazeDirection.z.ToString() + "\t" +

                gazeData.worldGazeOrigin.x.ToString() + "\t" + gazeData.worldGazeOrigin.y.ToString() + "\t" + gazeData.worldGazeOrigin.y.ToString() + "\t" +
                gazeData.worldGazeDirection.x.ToString() + "\t" + gazeData.worldGazeDirection.y.ToString() + "\t" + gazeData.worldGazeDirection.z.ToString() + "\t" +

                gazeData.worldGazeDistance.ToString() + "\t" + gazeData.ConvergenceDistance.ToString() + "\t" +
                gazeData.worldGazePoint.x.ToString() + "\t" + gazeData.worldGazePoint.y.ToString() + "\t" + gazeData.worldGazePoint.z.ToString() + "\t" +

                gazeData.pupilDiameterRight.ToString() + "\t" + gazeData.pupilDiameterLeft.ToString() + "\t" + gazeData.accuracy.ToString() +"\t" + gazeData.precision.ToString();

            this.gazeStreamWriter.WriteLine(sampleLine);
            this.gazeStreamWriter.Flush();       

    }
     

    internal void Close()
    {
        // write remaining samples to file before deleting the objects.
        m_EyeTrackingProvider.GetGazeQueue();
        currentSamples = m_EyeTrackingProvider.getCurrentSamples;


        if (currentSamples != null)
        {
            WriteGazeToFile(currentSamples);

        }
        this.gazeStreamWriter.Close();
    }
}


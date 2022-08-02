using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
public class SampleData
{

    public enum Eye
    {
        left,
        right,
        both
    }

    // General
    public long systemTimeStamp { get; set; }
    public long deviceTimestamp { get; set; }
    public Vector3 cameraPosition { get; set; }
    public Quaternion cameraRotation { get; set; }
    public Eye whichEye { get; set; }
    public bool isValid { get; set; }


    // Blinks?
    public bool isBlinkingLeft { get; set; }
    public bool isBlinkingRight { get; set; }
  
    
   
    // Gaze features
    public Vector3 localGazeOrigin { get; set; }
    public Vector3 localGazeDirection { get; set; }
    public Vector3 worldGazeOrigin { get; set; }
    public Vector3 worldGazeDirection { get; set; }
    public float worldGazeDistance { get; set; }
    public float ConvergenceDistance { get; set; }
    public Vector3 worldGazePoint { get; set; }

    // Pupil diameter
    public double pupilDiameterRight { get; set; }
    public double pupilDiameterLeft { get; set; }


    // Results
    public float accuracy { get; set; }
    public float precision { get; set; }

   /* UNDER CONSTRUCTION
    * public bool checkValidity()
    {
        if (!float.IsNaN(this.systemTimeStamp) && !float.IsNaN(this.worldGazeOrigin.x) && !float.IsNaN(this.worldGazeOrigin.y) && !float.IsNaN(this.worldGazeOrigin.z) && !float.IsNaN(this.worldGazeDirection.x) && !float.IsNaN(this.worldGazeDirection.y) && !float.IsNaN(this.worldGazeDirection.z) && !float.IsNaN(this.worldGazeDistance))
            this.isValid = true;
        else
            this.isValid = false;


        return this.isValid;


    }
   */
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
public class SampleData
{
    internal Vector3 vergenceAngle_R;
    internal Vector3 vergenceAngle_L;

    public long timeStamp { get; set; }
    public bool isValid { get; set; }
    public bool exclude { get; set; }
    public float targetId { get; set; }

    public double ipd { get; set; }
    public Vector3 cameraPosition { get; set; }
    public Vector3 localGazeOrigin { get; set; }
    public Vector3 localGazeDirection { get; set; }
    public Vector3 worldGazeOrigin { get; set; }
    public Vector3 worldGazeDirection { get; set; }
    public Vector3 worldGazeOrigin_R { get; set; }
    public Vector3 worldGazeOrigin_L { get; set; }
    public Vector3 worldGazeDirection_R { get; set; }
    public Vector3 worldGazeDirection_L { get; set; }
    public float worldGazeDistance { get; set; }
    public Vector3 worldGazePoint { get; set; }
    public Vector3 localMarkerPosition { get; set; }
    public Vector3 worldMarkerPosition { get; set; }
    public float OffsetAngle { get; set; }
    public float interSampleAngle { get; set; }



    internal bool checkValidity()
    {
        if (!float.IsNaN(this.timeStamp) && !float.IsNaN(this.worldGazeOrigin.x) && !float.IsNaN(this.worldGazeOrigin.y) && !float.IsNaN(this.worldGazeOrigin.z) && !float.IsNaN(this.worldGazeDirection.x) && !float.IsNaN(this.worldGazeDirection.y) && !float.IsNaN(this.worldGazeDirection.z) && !float.IsNaN(this.worldGazeDistance))
            this.isValid = true;
        else
            this.isValid = false;


        return this.isValid;


    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSpline : BezierSpline
{
    public TombiCharacterController charScript;
    public PlayerMovementRail rail=null;
    private int stepsPerCurve = 10;
    // Start is called before the first frame update
    void Start()
    {
        charScript.transform.position = this.GetPoint(0f);
        charScript.playerRail = this.GetDirection(0f);

        //if (rail != null)
        //{
        //    Vector3 point = GetPoint(0f);
        //    int steps = stepsPerCurve * CurveCount;
        //    for (int i = 1; i <= steps; i++)
        //    {
        //        point = GetPoint(i / (float)steps);

        //        rail.AddPoint(point);
        //        //Handles.DrawLine(point, point + GetDirection(i / (float)steps) * directionScale);
        //    }
        //}

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

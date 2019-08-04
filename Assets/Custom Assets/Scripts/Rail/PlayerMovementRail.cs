﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]

#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
public class PlayerMovementRail : SerializableMonoBehaviour
{
    public override string FileExtension { get => ".smmr"; protected set => base.FileExtension = value; }
    public override string FileExtensionName { get => "SMMR"; protected set => base.FileExtensionName = value; }

    [HideInInspector]
    public List<PlayerMovementRail> otherRails;
    public GameObject[] points;
    public AdvancedUtilities.Cameras.BasicCameraController camScript;
    public GameObject camObj;
    public GameObject target;
    public bool loop = false;

    //[HideInInspector]
    public List<PlayerMovementRailConnection> connections = new List<PlayerMovementRailConnection>();

    public Vector3 railSwitchColliderSize = Vector3.one / 2f;

    [HideInInspector]
    public float currentCamAngle;

    [HideInInspector]
    public float distance;

    [HideInInspector]
    public Vector3 playerVectorPosition;
    TombiCharacterController charScript;
    public bool moveTargetOnPoint = false;
    public GameObject pointToMoveTarget;

    [HideInInspector]
    public bool editorSelected = false;

    [HideInInspector]
    public bool editorCollapsed = false;

    [HideInInspector]
    public bool editorConnectionsCollapsed = false;

    [HideInInspector] 
    public bool isEditing = false;

    [HideInInspector]
    public Color railGizmoColor = Color.green;

    public ForkDirection generatedForkDirection = ForkDirection.Unknown;

    // Start is called before the first frame update
    void Awake()
    {
#if UNITY_EDITOR
        if(UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Initialize();
        }
        else
        {
            otherRails = new List<PlayerMovementRail>(FindObjectsOfType<PlayerMovementRail>());
            otherRails.Remove(this);

        }
#else
        Initialize();
#endif
    }

    public enum ForkDirection
    {
        Unknown = -1,
        Top,
        Bottom,
        Left,
        Right
    }

    public void Initialize()
    {
        if (camObj == null)
        {
            if(Camera.main != null)
            {
                camObj = Camera.main.gameObject;

            }
        }
        if (target == null)
        {
            TombiCharacterController charScript = FindObjectOfType<TombiCharacterController>();

            if (charScript != null && charScript.gameObject != null)
            {
                target = charScript.gameObject;

            }
        }

        if (camScript == null)
        {
            camScript = FindObjectOfType<AdvancedUtilities.Cameras.BasicCameraController>();
        }

        if (target != null)
        {
            charScript = target.GetComponent<TombiCharacterController>();

            if (moveTargetOnPoint)
            {
                RaycastHit hit;

                if (Physics.Raycast(pointToMoveTarget.transform.position, -pointToMoveTarget.transform.up, out hit, 100f))
                {
                    target.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    target.GetComponent<Rigidbody>().isKinematic = true;

                    target.transform.position = hit.point;
                    target.GetComponent<Rigidbody>().isKinematic = false;

                }
                else
                {
                    Debug.LogWarning("Impossible to move player on rail point!");
                    return;
                }
                Vector3 tempRail = (pointToMoveTarget.transform.position - pointToMoveTarget.GetComponent<PlayerMovementRailPoint>().nextPoint.transform.position);
                UpdatePlayerRail(tempRail, pointToMoveTarget.transform.position, pointToMoveTarget.GetComponent<PlayerMovementRailPoint>().nextPoint.transform.position);
                charScript.RotateTowardsMovementDir(tempRail * -1, true);
                return;
            }
        }
        else
        {
            Debug.Log("No target assigned to railcontroller!");
        }
    }

    public void AddConnection()
    {
        PlayerMovementRailConnection conn = new PlayerMovementRailConnection();
        conn.name = conn.name + " " + connections.Count;
        connections.Add(conn);
    }

    public void RemoveConnection(int index)
    {
        connections.RemoveAt(index);
    }

    public void AddPoint()
    {
        System.Array.Resize<GameObject>(ref this.points, this.points.Length + 1);

        GameObject obj = Resources.Load("Prefabs/RailPoint", typeof(GameObject)) as GameObject;

        GameObject newObj = Instantiate(obj, this.gameObject.transform);

        newObj.name = "RailPoint" + points.Length.ToString();

        //CustomAssets.Utilities.Design.IconDrawer.DrawDotBigIcon(newObj, 1);

        PlayerMovementRailPoint p = newObj.GetComponent<PlayerMovementRailPoint>();

        if(this.points.Length >= 2)
        {
            PlayerMovementRailPoint prevP = this.points[this.points.Length - 2].GetComponent<PlayerMovementRailPoint>();

            p.stopPoint = true;

            if (prevP.stopPoint)
            {
                prevP.stopPoint = false;
                if(prevP.stopCollider != null && prevP.stopCollider.gameObject != null)
                {
                    DestroyImmediate(prevP.stopCollider.gameObject);

                }
                prevP.stopCollider = null;

            }
            prevP.nextPoint = p.gameObject;
            p.prevPoint = prevP.gameObject;
            p.transform.position = prevP.transform.position + Vector3.forward;
        }
        else
        {
            p.transform.position = this.transform.position + Vector3.forward;
        }
        

        this.points[this.points.Length - 1] = newObj;
    }

    public void AddPoint(Vector3 point)
    {
        System.Array.Resize<GameObject>(ref this.points, this.points.Length + 1);

        GameObject obj = Resources.Load("Prefabs/RailPoint", typeof(GameObject)) as GameObject;

        GameObject newObj = Instantiate(obj, this.gameObject.transform);

        newObj.name = "RailPoint";

        //CustomAssets.Utilities.Design.IconDrawer.DrawDotBigIcon(newObj, 1);

        PlayerMovementRailPoint p = newObj.GetComponent<PlayerMovementRailPoint>();
        PlayerMovementRailPoint prevP = this.points[this.points.Length - 2].GetComponent<PlayerMovementRailPoint>();
        p.stopPoint = true;


        p.prevPoint = prevP.gameObject;

        p.transform.position = prevP.transform.position;

        this.points[this.points.Length - 1] = newObj;
    }
    

    public void RemovePoint()
    {
        DestroyImmediate(this.points[this.points.Length - 1].gameObject);
        System.Array.Resize<GameObject>(ref this.points, this.points.Length - 1);

        PlayerMovementRailPoint p = this.points[this.points.Length - 1].GetComponent<PlayerMovementRailPoint>();
        p.stopPoint = true;
        p.GenerateStopCollider();
    }
    /// <summary>
    /// Calculate angle between two Vector3 using dot product.
    /// </summary>
    /// <param name="vect1">First vector</param>
    /// <param name="vect2">Second vector</param>
    /// <returns>Angle in degrees between the two vectors</returns>
    private float DegAgleBetweenVectors3(Vector3 vect1, Vector3 vect2)
    {
        float cosAngle = Vector3.Dot(vect1.normalized, vect2.normalized);
        float degAngle = Mathf.Acos(cosAngle) * Mathf.Rad2Deg;
        return degAngle;
    }

    /// <summary>
    /// Calculate radians angle between two Vector3
    /// </summary>
    /// <param name="vect1">First vector</param>
    /// <param name="vect2">Second vector</param>
    /// <returns>Angle in radians between the two vectors</returns>
    private float RadAgleBetweenVectors3(Vector3 vect1, Vector3 vect2)
    {
       return DegAgleBetweenVectors3(vect1, vect2) * Mathf.Deg2Rad;
    }

// Distance to point (p) from line segment (end points a b)
float DistanceLineSegmentPoint( Vector3 a, Vector3 b, Vector3 p )
{
    // If a == b line segment is a point and will cause a divide by zero in the line segment test.
    // Instead return distance from a
    if (a == b)
        return Vector3.Distance(a, p);
     
    // Line segment to point distance equation
    Vector3 ba = b - a;
    Vector3 pa = a - p;
    return (pa - ba * (Vector3.Dot(pa, ba) / Vector3.Dot(ba, ba))).magnitude;
}


    public void Update()
    {
        int i = 0;

        GameObject previousPoint = null;
        GameObject firstPoint = null;

        if(points != null)
        {
            foreach (GameObject p in points)
            {
                if (p != null)
                {
                    
                    PlayerMovementRailPoint point = p.GetComponent<PlayerMovementRailPoint>();


                    /* FORKING HANDLING */
                    if (point != null)
                    {
                        if (point.forkSettings == null)
                        {
                            point.forkSettings = new PlayerMovementRailPoint.Forking();
                        }

                        PlayerMovementRail[] availableForksForPoint = point.GetAvailableForks();

                        if (availableForksForPoint.Length > 0)
                        {
                            point.forkSettings.forkedNode = false;

                            for (int c = 0; c < availableForksForPoint.Length; c++)
                            {

                                if (availableForksForPoint[c] != null)
                                {
                                    if (availableForksForPoint[c].points.Length > 0)
                                    {
                                        if (availableForksForPoint[c].points[0] != null)
                                        {
                                            point.forkSettings.forkedNode = true;
                                            point.forkSettings.parentNode = null;
                                            point.RemoveStopCollider();

                                            availableForksForPoint[c].points[0].transform.position = point.transform.position;
                                            availableForksForPoint[c].points[0].transform.LookAt(availableForksForPoint[c].points[1].transform);
                                            availableForksForPoint[c].points[0].GetComponent<PlayerMovementRailPoint>().RemoveStopCollider();
                                            availableForksForPoint[c].points[0].GetComponent<PlayerMovementRailPoint>().forkSettings.forkedNode = true;
                                            availableForksForPoint[c].points[0].GetComponent<PlayerMovementRailPoint>().forkSettings.parentNode = point.gameObject;
                                        }
                                    }
                                }
                            }
                        }
                    }


                    if (i == 0)
                    {
                        firstPoint = p;
                    }

                    if (previousPoint != null)
                    {
                        if (point.prevPoint != null)
                        {
                            //point.prevPoint.transform.LookAt(point.transform);
                            point.prevPoint.GetComponent<PlayerMovementRailPoint>().nextPoint = p;
                        }

                        if (point.nextPoint == null)
                        {
                            if (loop && firstPoint != null)
                            {
                                point.transform.position = firstPoint.transform.position;

                                point.RemoveStopCollider();
                                //point.gameObject.SetActive(false);
                                //point.gameObject.hideFlags = HideFlags.HideInHierarchy;
                                firstPoint.GetComponent<PlayerMovementRailPoint>().RemoveStopCollider();
                                firstPoint.GetComponent<PlayerMovementRailPoint>().prevPoint = point.gameObject;

                                point.nextPoint = firstPoint;
                            }
                            else 
                            {
                                //point.transform.LookAt(point.prevPoint.transform);
                                point.gameObject.SetActive(true);
                                point.gameObject.hideFlags = HideFlags.None;
                                point.GenerateStopCollider();
                                firstPoint.GetComponent<PlayerMovementRailPoint>().GenerateStopCollider();

                                if (firstPoint != null)
                                {

                                }

                            }

                        }

                        if ((point.prevPoint != null) && (point.nextPoint != null))
                        {
                            float angle = DegAgleBetweenVectors3(point.prevPoint.transform.position - point.transform.position, point.transform.position - point.nextPoint.transform.position);
                            point.angleBetweenPrevAndNext = angle;
                        }
                        point.prevPoint = previousPoint;

                    }
                    else
                    {
                        if (!loop)
                        {
                            point.prevPoint = null;

                            if(!point.forkSettings.forkedNode)
                            {
                                point.stopPoint = true;

                            }
                            else
                            {
                                point.stopPoint = false;

                            }
                        }

                    }
                    previousPoint = p;

                    if (target != null)
                    {
                        Vector3 vectorToTarget = target.transform.position - p.transform.position;
                        vectorToTarget.y = 0;
                        float distanceToTarget = vectorToTarget.magnitude;

                        if (distanceToTarget < 0.2f && !point.activeForRotation)
                        {
                            SetActiveForRotation(point);

                            if (!point.forkSettings.forkedNode)
                            {
                                charScript.stateController.onRailFork = false;

                                UpdateLastPlayerInput();
                                RotateTargetAtNode(point);
                            }
                            else
                            {
                                if(!charScript.stateController.onRailFork)
                                {
                                    charScript.rb.velocity = Vector3.zero;
                                    //charScript.transform.position = new Vector3(point.transform.position.x, charScript.transform.position.y, point.transform.position.z);
                                    charScript.stateController.mustChooseFork = true;
                                }
                                charScript.stateController.onRailFork = true;
                                charScript.currentForkedPoint = point;

                            }                       
                        }
                        else if (distanceToTarget > 0.2f && !charScript.stateController.mustChooseFork)
                        {
                            point.activeForRotation = false;
                            charScript.stateController.onRailFork = false;

                        }
                    }
                }
                i++;
            }
            if(mustUpdateCamera && cameraUpdatePoint != null)
            {
                StartCoroutine(SuperUpdate(cameraUpdatePoint));

            }
        }
    }

    public void ChangeFork(PlayerMovementRailPoint point, ForkDirection direction)
    {
        bool disableCurrentRail = false;

        switch (direction)
        {
            case ForkDirection.Unknown:
                break;

            case ForkDirection.Top:
                if (point.forkSettings.forkTop != null)
                {
                    point.forkSettings.forkTop.gameObject.SetActive(true);
                    point.forkSettings.forkTop.lastPlayerInput = this.lastPlayerInput;
                    point.forkSettings.forkTop.SetActiveForRotation(point.forkSettings.forkTop.points[0].GetComponent<PlayerMovementRailPoint>());
                    point.forkSettings.forkTop.RotateTargetAtNode(point.forkSettings.forkTop.points[0].GetComponent<PlayerMovementRailPoint>());
                    disableCurrentRail = true;
                }
                break;

            case ForkDirection.Bottom:
                if (point.forkSettings.forkBottom != null)
                {
                    point.forkSettings.forkBottom.gameObject.SetActive(true);
                    point.forkSettings.forkBottom.lastPlayerInput = eInput.RIGHT;
                    point.forkSettings.forkBottom.SetActiveForRotation(point.forkSettings.forkBottom.points[0].GetComponent<PlayerMovementRailPoint>());
                    point.forkSettings.forkBottom.RotateTargetAtNode(point.forkSettings.forkBottom.points[0].GetComponent<PlayerMovementRailPoint>());
                    disableCurrentRail = true;
                }
                break;
            case ForkDirection.Left:
                if (point.forkSettings.forkLeft != null)
                {
                    point.forkSettings.forkLeft.gameObject.SetActive(true);
                    disableCurrentRail = true;
                }
                break;

            case ForkDirection.Right:
                if(point.forkSettings.forkRight != null)
                {
                    point.forkSettings.forkRight.gameObject.SetActive(true);
                    disableCurrentRail = true;
                }
                break;

            default:
                break;

        }
        //point.forkSettings.forkBottom.StartCoroutine(point.forkSettings.forkBottom.SuperUpdate(point.forkSettings.forkBottom.points[0].GetComponent<PlayerMovementRailPoint>()));
        if(disableCurrentRail)
        {
            this.gameObject.SetActive(false);

        }
    }

    void ClearAllRotationActiveFlags(PlayerMovementRailPoint point)
    {
        if (point.prevPoint != null)
        {
            point.prevPoint.GetComponent<PlayerMovementRailPoint>().activeForRotation = false;

        }

        if (point.nextPoint != null)
        {
            point.nextPoint.GetComponent<PlayerMovementRailPoint>().activeForRotation = false;

        }
    }

    void SetActiveForRotation(PlayerMovementRailPoint point)
    {
        ClearAllRotationActiveFlags(point);
        point.activeForRotation = true;
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        int c = 0;

        if (points != null && points.Length > 0)
        {
            foreach (GameObject p in points)
            {
                if (p != null)
                {
                    PlayerMovementRailPoint point = p.GetComponent<PlayerMovementRailPoint>();

                    if (point.prevPoint != null)
                    {
                        Gizmos.color = railGizmoColor;
                        Gizmos.DrawLine(point.prevPoint.transform.position, p.transform.position);

                        if ((point.prevPoint != null) && (point.nextPoint != null))
                        {
                            float angle = DegAgleBetweenVectors3(point.prevPoint.transform.position - point.transform.position, point.transform.position - point.nextPoint.transform.position);
                            Handles.Label(new Vector3(p.transform.position.x, p.transform.position.y + 1f, p.transform.position.z), angle.ToString("00.00") + "°");

                        }
                    }
                    else //This is the first element, draw label START
                    {
                        Handles.Label(new Vector3(p.transform.position.x, p.transform.position.y + 1f, p.transform.position.z), "Start");
                    }

                    if (point.nextPoint != null)
                    {
                        if (point.forkSettings.forkedNode && this.generatedForkDirection != ForkDirection.Unknown)
                        {
                            if (point.forkSettings.parentNode != null)
                            {
                                Handles.Label((point.transform.position + point.nextPoint.transform.position) / 2, this.generatedForkDirection.ToString() + " Forked: " + point.forkSettings.parentNode.name);

                            }
                        }
                        else
                        {
                            Handles.Label((point.transform.position + point.nextPoint.transform.position) / 2, this.name);

                        }
                    }

                    if (point.enableRailJoin)
                    {
                        if (point.railPointToJoin != null)
                        {
                            Matrix4x4 oldMatrix = Gizmos.matrix;
                            Gizmos.color = Color.magenta;
                            Gizmos.DrawLine(point.transform.position, point.railPointToJoin.transform.position);

                            if (point.switchRailCollider == null)
                            {
                                Gizmos.matrix = Matrix4x4.TRS(point.transform.position, Quaternion.Euler(point.transform.rotation.eulerAngles.x, point.transform.rotation.eulerAngles.y, point.transform.rotation.eulerAngles.z), Vector3.one);
                                Gizmos.DrawWireCube(Vector3.zero, point.joinPointArea);
                            }

                            if (point.railPointToJoin.GetComponent<PlayerMovementRailPoint>().switchRailCollider == null)
                            {
                                Gizmos.matrix = Matrix4x4.TRS(point.railPointToJoin.transform.position, Quaternion.Euler(point.railPointToJoin.transform.rotation.eulerAngles.x, point.railPointToJoin.transform.rotation.eulerAngles.y, point.railPointToJoin.transform.rotation.eulerAngles.z), Vector3.one);
                                Gizmos.DrawWireCube(Vector3.zero, point.railPointToJoin.GetComponent<PlayerMovementRailPoint>().joinPointArea);
                            }
                            Gizmos.matrix = oldMatrix;

                        }
                    }

                    if (point.activeForRotation)
                    {
                        if (point.stopPoint)
                        {
                            Gizmos.color = Color.red;
                        }
                        else
                        {
                            Gizmos.color = Color.yellow;
                        }
                    }
                    else
                    {
                        Gizmos.color = Color.blue;
                    }
                    Gizmos.DrawWireCube(p.transform.position, new Vector3(0.3f, 0.3f, 0.3f));

                    if (target != null)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawWireCube(target.transform.position, new Vector3(0.5f, 0.5f, 0.5f));
                    }

                    if (!loop)
                    {
                        if (c == points.Length - 1)
                        {
                            Handles.Label(new Vector3(p.transform.position.x, p.transform.position.y + 1f, p.transform.position.z), "End");
                        }
                    }

                    c++;
                }
            }
        }
    }
#endif


    [HideInInspector]
    public bool isRotatingCamera = false;

    //[HideInInspector]
    public eInput lastPlayerInput = eInput.NONE;

    public enum eInput
    {
        NONE = 0,
        RIGHT = 1,
        LEFT = 2
    }

    eInput GetInputAsEnum()
    {
        if (!isRotatingCamera)
        {
            if (Input.GetButton("PS4_DPAD_RIGHT"))
            {
                return eInput.RIGHT;
            }
            else if (Input.GetButton("PS4_DPAD_LEFT"))
            {
                return eInput.LEFT;
            }
            return lastPlayerInput;

        }
        else
        {
            return lastPlayerInput;
        }
    }

    public void UpdateLastPlayerInput()
    {
        lastPlayerInput = GetInputAsEnum();
    }

    public bool canUpdateCameraRotation = true;

    void RotateTargetAtNode(PlayerMovementRailPoint point)
    {
        int directionSign = 1;

        float lerpTime;
        float camAngle;
        float updatedCamAngle;
        Vector3 vectDirection = Vector3.zero;
        camScript.Rotation.AutoRotation.Enabled = true;

        if (point.activeForRotation && (!point.stopPoint || loop))
        {
            //This is the moment when we move the camera according to dotproduct of forward camera vector and spline vector.
            if (target != null && camObj != null)
            {
                if (point.nextPoint != null)
                {
                    mustUpdateCamera = true;
                    cameraUpdatePoint = point;
                    //if (lastPlayerInput == eInput.RIGHT)
                    //{
                    //    vectDirection = point.transform.position - point.nextPoint.transform.position;
                    //    directionSign = 1;
                    //}
                    //else if (lastPlayerInput == eInput.LEFT)
                    //{
                    //    vectDirection = point.transform.position - point.prevPoint.transform.position;
                    //    directionSign = -1;
                    //}
                    //camAngle = DegAgleBetweenVectors3(vectDirection * directionSign, camObj.transform.forward);
                    //updatedCamAngle = CalculateUpdatedCameraAngle(camAngle);

                    ////Check if rotation lerp time is set
                    //if (point.rotationLerpTime > 0)
                    //{
                    //    //Use set rotation lerp time
                    //    lerpTime = point.rotationLerpTime;
                    //}
                    //else
                    //{
                    //    //Use standard smooth lerp time
                    //    lerpTime = Time.deltaTime * 20f;
                    //}

                    //if (point.stopWhileRotating)
                    //{
                    //    StartCoroutine(charScript.LockMovement(0, lerpTime + 0.2f));
                    //    StartCoroutine(LockWhileRotating(0, lerpTime + 0.2f));

                    //    if (directionSign > 0)
                    //    {
                    //        UpdatePlayerRail(vectDirection * directionSign, point.transform.position, point.nextPoint.transform.position);
                    //    }
                    //    else if (directionSign < 0)
                    //    {
                    //        UpdatePlayerRail(vectDirection * directionSign, point.transform.position, point.prevPoint.transform.position);
                    //    }
                    //    camScript.Rotation.AutoRotateBy(updatedCamAngle, 0, lerpTime);
                    //    point.activeForRotation = false;
                    //    return;
                    //}


                    ////Check if we are moving
                    //if (vectDirection != Vector3.zero)
                    //{
                    //    if (canUpdateCameraRotation)
                    //    {
                    //        //Rotate camera as long as we are moving
                    //        camScript.Rotation.AutoRotateBy(updatedCamAngle, 0, lerpTime);
                    //    }

                    //    if (directionSign > 0)
                    //    {
                    //        UpdatePlayerRail(vectDirection * directionSign, point.transform.position, point.nextPoint.transform.position);

                    //    }
                    //    else if (directionSign < 0)
                    //    {
                    //        UpdatePlayerRail(vectDirection * directionSign, point.transform.position, point.prevPoint.transform.position);

                    //    }
                    //}
                }
            }
            else
            {
                Debug.Log("Porcoddio");
            }
            //Deactivate point for rotation
            point.activeForRotation = false;
        }
    }

    IEnumerator SuperUpdate(PlayerMovementRailPoint point)
    {
        int directionSign = 1;

        float lerpTime;
        float camAngle;
        float updatedCamAngle = -1;
        float invert = -1;
        Vector3 vectDirection = Vector3.zero;


            if (lastPlayerInput == eInput.RIGHT)
            {
                vectDirection = point.transform.position - point.nextPoint.transform.position;
                directionSign = 1;
            }
            else if (lastPlayerInput == eInput.LEFT)
            {
                vectDirection = point.transform.position - point.prevPoint.transform.position;
                directionSign = -1;
            }
            camAngle = DegAgleBetweenVectors3(vectDirection * directionSign, camObj.transform.forward);
            updatedCamAngle = CalculateUpdatedCameraAngle(camAngle);

            //Check if rotation lerp time is set
            if (point.rotationLerpTime > 0)
            {
                //Use set rotation lerp time
                lerpTime = point.rotationLerpTime;
            }
            else
            {
                //Use standard smooth lerp time
                lerpTime = Time.deltaTime * 20f;
            }

            //if (point.stopWhileRotating)
            //{
            //    StartCoroutine(charScript.LockMovement(0, lerpTime + 0.2f));
            //    StartCoroutine(LockWhileRotating(0, lerpTime + 0.2f));

            //    if (directionSign > 0)
            //    {
            //        UpdatePlayerRail(vectDirection * directionSign, point.transform.position, point.nextPoint.transform.position);
            //    }
            //    else if (directionSign < 0)
            //    {
            //        UpdatePlayerRail(vectDirection * directionSign, point.transform.position, point.prevPoint.transform.position);
            //    }
            //    camScript.Rotation.AutoRotateBy(updatedCamAngle, 0, lerpTime);
            //    point.activeForRotation = false;
            //}

            //Check if we are moving
            if (vectDirection != Vector3.zero)
            {
                if (canUpdateCameraRotation)
                {
                    //Rotate camera as long as we are moving
                    camScript.Rotation.AutoRotateBy(updatedCamAngle, 0, lerpTime);
                }

                if (directionSign > 0)
                {
                    UpdatePlayerRail(vectDirection * directionSign, point.transform.position, point.nextPoint.transform.position);

                }
                else if (directionSign < 0)
                {
                    UpdatePlayerRail(vectDirection * directionSign, point.transform.position, point.prevPoint.transform.position);

                }
            }
        

        if (Mathf.Abs(updatedCamAngle) < 0.5f)
        {
            mustUpdateCamera = false;
        }

        yield return 0;
    }

    public bool mustUpdateCamera = false;
    public PlayerMovementRailPoint cameraUpdatePoint = null;
    float CalculateUpdatedCameraAngle(float actCamAngle)
    {
        float angleDiff;
        int angleSign;

        //Calculate angle where camera has to rotate
        if (actCamAngle > 90f)
        {
            angleDiff = actCamAngle - 90f;
            angleSign = -1;
        }
        else
        {
            angleDiff = 90f - actCamAngle;
            angleSign = 1;
        }
        return angleSign * angleDiff;
    }
   
    void UpdatePlayerRail(Vector3 rail, Vector3 firstRailPoint, Vector3 lastRailPoint)
    {
        charScript.railFirstPoint = firstRailPoint;
        charScript.railLastPoint = lastRailPoint;
        charScript.playerRail = rail * -1;        
    }

    public IEnumerator LockWhileRotating(float delayTime, float lockTime)
    {
        yield return new WaitForSeconds(delayTime);
        isRotatingCamera = true;
        yield return new WaitForSeconds(lockTime);
        isRotatingCamera = false;
    }

    public override void Save(string path)
    {
        throw new System.NotImplementedException();
    }

    public override void Load()
    {
        throw new System.NotImplementedException();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]

#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
public class PMRail : SerializableMonoBehaviour
{
    public override string FileExtension { get => ".smmr"; protected set => base.FileExtension = value; }
    public override string FileExtensionName { get => "SMMR"; protected set => base.FileExtensionName = value; }

    [HideInInspector]
    public List<PMRail> otherRails;
    public GameObject[] points;
    public Typo.Utilities.Cameras.BasicCameraController camScript;
    public GameObject camObj;
    public GameObject target;
    public GameObject forkArrows;
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
            otherRails = new List<PMRail>(FindObjectsOfType<PMRail>());
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
            camScript = FindObjectOfType<Typo.Utilities.Cameras.BasicCameraController>();
        }

        if (target != null)
        {
            charScript = target.GetComponent<TombiCharacterController>();

            if(forkArrows != null)
            {
                forkArrows.transform.SetParent(target.transform);
                forkArrows.transform.position = target.transform.position;
            }

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
                Vector3 tempRail = (pointToMoveTarget.transform.position - pointToMoveTarget.GetComponent<PMRailPoint>().nextPoint.transform.position);
                UpdatePlayerRail(tempRail, pointToMoveTarget.transform.position, pointToMoveTarget.GetComponent<PMRailPoint>().nextPoint.transform.position);
                charScript.RotateTowardsMovementDir(tempRail * -1, true);
                return;
            }
        }
        else
        {
            Debug.Log("No target assigned to railcontroller!");
        }
    }

    public void OnEnable()
    {
        //Update all event handlers for every point
        UpdatePointsEventHandlers();
    }

    public void ChangeFork()
    {

    }

    public void UpdatePointsEventHandlers()
    {
        foreach(GameObject p in points)
        {
            if(p != null)
            {
                PMRailPoint point = p.GetComponent<PMRailPoint>();

                point.OnPointWarmupEnter -= HandlePointWarmupEnter;
                point.OnPointWarmupEnter += HandlePointWarmupEnter;
                point.OnPointWarmupStay -= HandlePointWarmupStay;
                point.OnPointWarmupStay += HandlePointWarmupStay;
                point.OnPointWarmupExit -= HandlePointWarmupExit;
                point.OnPointWarmupExit += HandlePointWarmupExit;

                point.OnPointActiveEnter -= HandlePointActiveEnter;
                point.OnPointActiveEnter += HandlePointActiveEnter;
                point.OnPointActiveStay -= HandlePointActiveStay;
                point.OnPointActiveStay += HandlePointActiveStay;
                point.OnPointActiveExit -= HandlePointActiveExit;
                point.OnPointActiveExit += HandlePointActiveExit;

            }
            else
            {
                Debug.Log(string.Format("Point {0} is null, skipping handlers assignment.", p.name));
            }
        }
    }

    public void ActivateForkArrows(PMRailPoint point)
    {
        ForkArrowsManager fArrowsScript = forkArrows.GetComponent<ForkArrowsManager>();

        if (point.forkSettings.forkBottom != null)
        {
            fArrowsScript.forkBottom.SetActive(true);
        }
        else
        {
            fArrowsScript.forkBottom.SetActive(false);
        }

        if ((point.forkSettings.forkTop != null))
        {
            fArrowsScript.forkTop.SetActive(true);
        }
        else
        {
            fArrowsScript.forkTop.SetActive(false);
        }

        if ((point.forkSettings.forkRight != null) || (point.forkSettings.forkLeft != null) || (point != points[points.Length - 1]))
        {
            fArrowsScript.forkForward.SetActive(true);

            if ((this.generatedForkDirection == ForkDirection.Bottom) || (this.generatedForkDirection == ForkDirection.Top))
            {
                if ((point.forkSettings.parentPoint != points[points.Length - 1]) && (point.forkSettings.parentPoint != points[0]))
                {
                    fArrowsScript.forkBack.SetActive(true);
                }
                else
                {
                    fArrowsScript.forkBack.SetActive(false);
                }
            }
            else
            {
                fArrowsScript.forkBack.SetActive(false);
            }
        }
        else
        {
            fArrowsScript.forkForward.SetActive(false);
        }
    }

    public void HandlePointActiveEnter(PMRailPoint point)
    {
        //Debug.Log("Enter" + point.name + "_" + point.transform.position);
        lastPlayerInput = oldlastPlayerInput;

        if (point.playerForkInput != eInput.NONE)
        {
            Debug.Log("Active Input " + point.name + "_" + point.playerForkInput.ToString());

            point.playerForkInput = eInput.NONE;
        }
    }

    public void HandlePointActiveStay(PMRailPoint point)
    {
        //Debug.Log("Stay" + point.name + "_" + point.transform.position);

        UpdateLastPlayerInput();
        point.stateController.IsActiveForRotation = true;
        RotateTargetAtNode(point);
    }

    public void HandlePointActiveExit(PMRailPoint point)
    {

        //Debug.Log("Exit" + point.name + "_" + point.transform.position);
    }

    public void HandlePointWarmupEnter(PMRailPoint point)
    {
        //Debug.Log("Enter" + point.name + "_" + point.transform.position);

        if(point.forkSettings.forkedNode)
        {
            ActivateForkArrows(point);
            forkArrows.SetActive(true);
        }
    }

    public void HandlePointWarmupStay(PMRailPoint point)
    {
        //Debug.Log("Stay" + point.name + "_" + point.transform.position);

        if(point.playerForkInput != eInput.NONE)
        {
            Debug.Log("Warmup Input " + point.name + "_" + point.playerForkInput.ToString());

            point.playerForkInput = eInput.NONE;
        }
    }

    public void HandlePointWarmupExit(PMRailPoint point)
    {
        //Debug.Log("Exit" + point.name + "_" + point.transform.position);
        if (point.forkSettings.forkedNode)
        {
            forkArrows.SetActive(false);
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

        PMRailPoint p = newObj.GetComponent<PMRailPoint>();

        if(this.points.Length >= 2)
        {
            PMRailPoint prevP = this.points[this.points.Length - 2].GetComponent<PMRailPoint>();

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

        newObj.name = "RailPoint" + points.Length.ToString();

        //CustomAssets.Utilities.Design.IconDrawer.DrawDotBigIcon(newObj, 1);

        PMRailPoint p = newObj.GetComponent<PMRailPoint>();

        if (this.points.Length >= 2)
        {
            PMRailPoint prevP = this.points[this.points.Length - 2].GetComponent<PMRailPoint>();

            p.stopPoint = true;

            if (prevP.stopPoint)
            {
                prevP.stopPoint = false;

                if (prevP.stopCollider != null && prevP.stopCollider.gameObject != null)
                {
                    DestroyImmediate(prevP.stopCollider.gameObject);
                }
                prevP.stopCollider = null;

            }
            prevP.nextPoint = p.gameObject;
            p.prevPoint = prevP.gameObject;

        }
        p.transform.position = point;
        this.points[this.points.Length - 1] = newObj;

    }
    
    public void RemovePoint()
    {
        DestroyImmediate(this.points[this.points.Length - 1].gameObject);
        System.Array.Resize<GameObject>(ref this.points, this.points.Length - 1);

        PMRailPoint p = this.points[this.points.Length - 1].GetComponent<PMRailPoint>();
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


    public void UpdatePointsMasterRail()
    {
        foreach (GameObject p in points)
        {
            if (p != null)
            {
                PMRailPoint point = p.GetComponent<PMRailPoint>();

                point.rail = this;
            }
        }
    }
    public Vector3 playerPositionWhileChoosing = Vector3.zero;
    public bool mustResetForkArrows = false;

    public void HandleForkArrowsGUI()
    {
        if(forkArrows != null)
        {
            Vector3 offset = new Vector3(0f, 1f, 0f);
            bool mustReset = true;

            foreach (GameObject p in points)
            {
                if (p != null)
                {

                    PMRailPoint point = p.GetComponent<PMRailPoint>();

                    if (point.stateController.IsWarmingUp)
                    {
                        mustReset = false;
                    }
                }
            }

            if (mustReset)
            {
                forkArrows.SetActive(false);
            }
            else
            {
                //Update fork arrows position
                forkArrows.transform.position = target.transform.position + offset;
            }
        }
    }

    public void Update()
    {
        UpdatePointsMasterRail();
        //HandleForkArrowsGUI();

        if (points != null)
        {
            oldlastPlayerInput = GetOldInputAsEnum();

            if (refreshTargetAndCamera && refreshPoint != null)
            {
                StartCoroutine(UpdateAtActivePoint(refreshPoint));
            }
        }
    }

    public bool mustReset = false;
    public bool mustResetForkArrowsAfterChoice = false;

    public void ChangeFork(PMRailPoint point, ForkDirection direction)
    {
        //bool disableCurrentRail = false;

        //if(point.forkSettings.parentNode == null)
        //{/*
        //    switch (direction)
        //    {
        //        case ForkDirection.Unknown:
        //            break;

        //        case ForkDirection.Top:
        //            if (point.forkSettings.forkTop != null)
        //            {
        //                point.forkSettings.forkTop.gameObject.SetActive(true);
        //                point.forkSettings.forkTop.lastPlayerInput = eInput.LEFT;
        //                point.forkSettings.forkTop.SetActiveForRotation(point.forkSettings.forkTop.points[0].GetComponent<PMRailPoint>());
        //                point.forkSettings.forkTop.RotateTargetAtNode(point.forkSettings.forkTop.points[0].GetComponent<PMRailPoint>());
        //                point.forkSettings.forkTop.mustReset = true;
        //                point.forkSettings.forkTop.mustResetForkArrowsAfterChoice = true;
        //                disableCurrentRail = true;

        //            }
        //            break;

        //        case ForkDirection.Bottom:
        //            if (point.forkSettings.forkBottom != null)
        //            {
        //                point.forkSettings.forkBottom.gameObject.SetActive(true);
        //                point.forkSettings.forkBottom.lastPlayerInput = eInput.RIGHT;
        //                point.forkSettings.forkBottom.SetActiveForRotation(point.forkSettings.forkBottom.points[0].GetComponent<PMRailPoint>());
        //                point.forkSettings.forkBottom.RotateTargetAtNode(point.forkSettings.forkBottom.points[0].GetComponent<PMRailPoint>());
        //                point.forkSettings.forkBottom.mustReset = true;
        //                point.forkSettings.forkBottom.mustResetForkArrowsAfterChoice = true;
        //                disableCurrentRail = true;
        //            }
        //            break;

        //        case ForkDirection.Left:
        //            if (point.forkSettings.forkLeft != null)
        //            {
        //                point.forkSettings.forkLeft.gameObject.SetActive(true);
        //                point.forkSettings.forkLeft.lastPlayerInput = GetInputAsEnum();
        //                point.forkSettings.forkLeft.SetActiveForRotation(point.forkSettings.forkLeft.points[0].GetComponent<PMRailPoint>());
        //                point.forkSettings.forkLeft.RotateTargetAtNode(point.forkSettings.forkLeft.points[0].GetComponent<PMRailPoint>());
        //                point.forkSettings.forkLeft.mustReset = true;
        //                point.forkSettings.forkLeft.mustResetForkArrowsAfterChoice = true;
        //                disableCurrentRail = true;
        //            }
        //            break;

        //        case ForkDirection.Right:
        //            if (point.forkSettings.forkRight != null)
        //            {
        //                point.forkSettings.forkRight.gameObject.SetActive(true);
        //                point.forkSettings.forkRight.lastPlayerInput = GetInputAsEnum();
        //                point.forkSettings.forkRight.SetActiveForRotation(point.forkSettings.forkRight.points[0].GetComponent<PMRailPoint>());
        //                point.forkSettings.forkRight.RotateTargetAtNode(point.forkSettings.forkRight.points[0].GetComponent<PMRailPoint>());
        //                point.forkSettings.forkRight.mustReset = true;
        //                point.forkSettings.forkRight.mustResetForkArrowsAfterChoice = true;
        //                disableCurrentRail = true;
        //            }
        //            break;

        //        default:
        //            break;

        //    }
        //    */
        //}
        //else
        //{
        //    PMRail parentRail = point.forkSettings.parentNode.GetComponent<PMRailPoint>().rail;
        //    parentRail.gameObject.SetActive(true);
        //    parentRail.lastPlayerInput = GetInputAsEnum();
        //    parentRail.SetActiveForRotation(point.forkSettings.parentNode.GetComponent<PMRailPoint>());
        //    parentRail.RotateTargetAtNode(point.forkSettings.parentNode.GetComponent<PMRailPoint>());
        //    parentRail.mustReset = true;
        //    parentRail.mustResetForkArrowsAfterChoice= true;
        //    disableCurrentRail = true;

        //}

        //if (disableCurrentRail)
        //{

        //    this.gameObject.SetActive(false);
        //}
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
                    PMRailPoint point = p.GetComponent<PMRailPoint>();

                    if (point.prevPoint != null)
                    {
                        Gizmos.color = railGizmoColor;
                        Gizmos.DrawLine(point.prevPoint.transform.position, p.transform.position);

                        if ((point.prevPoint != null) && (point.nextPoint != null))
                        {
                            float angle = DegAgleBetweenVectors3(point.prevPoint.transform.position - point.transform.position, point.transform.position - point.nextPoint.transform.position);
                           // Handles.Label(new Vector3(p.transform.position.x, p.transform.position.y + 1f, p.transform.position.z), angle.ToString("00.00") + "°");

                        }
                    }
                    else //This is the first element, draw label START
                    {
                        //Handles.Label(new Vector3(p.transform.position.x, p.transform.position.y + 1f, p.transform.position.z), "Start");
                    }

                    if (point.nextPoint != null)
                    {
                        if (point.forkSettings.forkedNode && this.generatedForkDirection != ForkDirection.Unknown)
                        {
                            //if (point.forkSettings.parentNode != null)
                            //{
                            //    Handles.Label((point.transform.position + point.nextPoint.transform.position) / 2, this.generatedForkDirection.ToString() + " Forked: " + point.forkSettings.parentNode.name);

                            //}
                        }
                        else
                        {
                            //Handles.Label((point.transform.position + point.nextPoint.transform.position) / 2, this.name);

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

                            if (point.railPointToJoin.GetComponent<PMRailPoint>().switchRailCollider == null)
                            {
                                Gizmos.matrix = Matrix4x4.TRS(point.railPointToJoin.transform.position, Quaternion.Euler(point.railPointToJoin.transform.rotation.eulerAngles.x, point.railPointToJoin.transform.rotation.eulerAngles.y, point.railPointToJoin.transform.rotation.eulerAngles.z), Vector3.one);
                                Gizmos.DrawWireCube(Vector3.zero, point.railPointToJoin.GetComponent<PMRailPoint>().joinPointArea);
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
                    //Gizmos.DrawWireCube(p.transform.position, new Vector3(0.3f, 0.3f, 0.3f));

                    if (target != null)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawWireCube(target.transform.position, new Vector3(0.5f, 0.5f, 0.5f));
                    }

                    if (!loop)
                    {
                        if (c == points.Length - 1)
                        {
                            //Handles.Label(new Vector3(p.transform.position.x, p.transform.position.y + 1f, p.transform.position.z), "End");
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
    public eInput oldlastPlayerInput = eInput.NONE;

    public enum eInput
    {
        NONE = 0,
        LEFT,
        RIGHT,
        UP,
        DOWN
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

    eInput GetOldInputAsEnum()
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
            return oldlastPlayerInput;

        }
        else
        {
            return oldlastPlayerInput;
        }
    }

    public void UpdateLastPlayerInput()
    {
        lastPlayerInput = GetInputAsEnum();
    }

    void RotateTargetAtNode(PMRailPoint point)
    {
        Vector3 vectDirection = Vector3.zero;
        camScript.Rotation.AutoRotation.Enabled = true;

        if (point.stateController.IsActiveForRotation && (!point.stopPoint || loop))
        {
            if (target != null && camObj != null)
            {
                if (point.nextPoint != null)
                {
                    refreshTargetAndCamera = true;
                    refreshPoint = point;
                }
            }

            //Deactivate point for rotation
            point.stateController.IsActiveForRotation = false;
        }
    }

    IEnumerator UpdateAtActivePoint(PMRailPoint point)
    {
        int directionSign = 1;
        float lerpTime;
        float verticalLerpTime;

        float camAngle;
        float updatedCamAngle = -1;
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
        updatedCamAngle = CalculateUpdatedCameraAngle(camAngle) + (point.pointSettings.rotationAngleOffset * directionSign);

        //Check if rotation lerp time is set
        if (point.pointSettings.rotationLerpTime > 0)
        {
            //Use set rotation lerp time
            lerpTime = Time.deltaTime * point.pointSettings.rotationLerpTime;
        }
        else
        {
            //Use standard smooth lerp time
            lerpTime = Time.deltaTime * 20f;
        }

        //Check if rotation lerp time is set
        if (point.pointSettings.verticalRotationLerpTime > 0)
        {
            //Use set rotation lerp time
            verticalLerpTime = Time.deltaTime * point.pointSettings.verticalRotationLerpTime;
        }
        else
        {
            //Use standard smooth lerp time
            verticalLerpTime = Time.deltaTime * 20f;
        }

        //Check if we are moving
        if (vectDirection != Vector3.zero)
        {
            Vector3 pointToRotateAt = Vector3.zero;

            if (point.pointSettings.updateCameraRotation)
            {
                //camScript.Rotation.RotateAroundTarget(0f, Mathf.LerpAngle(camScript.Rotation.VerticalRotation, camScript.Rotation.VerticalRotation + (point.pointSettings.verticalRotationAngle * directionSign), verticalLerpTime) - camScript.Rotation.VerticalRotation, target.transform);
                camScript.Rotation.AutoRotateBy(updatedCamAngle, 0f, Time.deltaTime * Mathf.Abs(updatedCamAngle) * lerpTime);
                
                //camScript.Rotation.AutoRotateByVertical(point.pointSettings.verticalRotationAngle * directionSign, verticalLerpTime);

                //camScript.Rotation.AutoRotateBy(updatedCamAngle, point.pointSettings.verticalRotationAngle * directionSign, lerpTime);

            }

            if (directionSign > 0)
            {
                pointToRotateAt = point.nextPoint.transform.position;
            }
            else if (directionSign < 0)
            {
                pointToRotateAt = point.prevPoint.transform.position;
            }

            if (pointToRotateAt != Vector3.zero)
            {
                UpdatePlayerRail(vectDirection * directionSign, point.transform.position, pointToRotateAt);
            }
        }

        if (Mathf.Abs(updatedCamAngle) < 0.5f)
        {
            refreshTargetAndCamera = false;
        }

        yield return 0;
    }

    public bool refreshTargetAndCamera = false;
    public PMRailPoint refreshPoint = null;

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
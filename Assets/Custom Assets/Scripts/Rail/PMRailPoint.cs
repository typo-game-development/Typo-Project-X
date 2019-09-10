using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[System.Serializable]
[ExecuteInEditMode]
public class PMRailPoint : MonoBehaviour
{
    [System.Serializable]
    public class PMRailPointSettings
    {
        [EditorRename("Warm-up radius")]
        public float warmUpRadius = 0.5f;        
        public readonly float activationRadius = 0.1f; //Lowest stable value to avoid floating point error with fast moving objects is 0.1f

        public bool checkForYPosition = false;

        [Header("Camera")]
        [Tooltip("The offset for point rotation in degrees.")]
        public float rotationAngleOffset = 0f;
        public float rotationLerpTime = 0f;

        public float verticalRotationLerpTime = 0f;
        public float verticalRotationAngle = 0f;

        public bool stopWhileRotating = false;
        public bool updateCameraRotation = true;

    }

    [System.Serializable]
    public class PMRailPointDebugSettings
    {
        [EditorRename("Warm-up radius")]
        public Color warmUpRadiusColor = Color.cyan;
        public Color warmUpRadiusColorIfForked = Color.green;

        public Color activationRadiusColor = new Color(255f, 215f, 0f);
        public Color activationRadiusColorIfForked = Color.blue;
    }

    [System.Serializable]
    public class PMRailPointStateController
    {
        private PMRailPoint point = null;

        [ShowOnly] public bool IsWarmingUp = false;
        [ShowOnly] public bool IsActive = false;
        [ShowOnly] public bool MustResetActive = false;

        [ShowOnly] [SerializeField] private bool m_IsActiveForRotation = false;

        public PMRailPointStateController(PMRailPoint point)
        {
            try
            {
                this.point = point ?? throw new ArgumentNullException(nameof(point));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public bool IsActiveForRotation
        {
            get
            {
                return m_IsActiveForRotation;
            }
            set
            {
                if(value)
                {
                    point.ClearAllRotationActiveFlags();
                }
                m_IsActiveForRotation = value;
            }
        }
    }

    [Header("General")]
    public PMRailPointSettings pointSettings = new PMRailPointSettings();

    [Header("Forking")]
    public PMRailPointForking forkSettings = new PMRailPointForking();

    [Header("States")]
    public PMRailPointStateController stateController;

    [Header("Debug")]
    public PMRailPointDebugSettings debugSettings = new PMRailPointDebugSettings();

    public GameObject prevPoint;
    public GameObject nextPoint;

    [HideInInspector]
    public float angleBetweenPrevAndNext = 0;

    public BoxCollider stopCollider;
    public BoxCollider switchRailCollider;

    public bool stopPoint = false;

    [HideInInspector]
    public bool activeForRotation = false;

    [HideInInspector]
    public PMRail rail = null;

    public bool enableRailJoin = false;

    [HideInInspector]
    public GameObject railPointToJoin = null;

    [HideInInspector]
    public Vector3 joinPointArea = Vector3.one;


    public float startCameraVerticalRotation;
    public bool isRotatedDown = false;
    public bool isRotatedUp = false;


    void OnValidate()
    {
        forkSettings.forkTop = forkSettings.m_forkTop;
        forkSettings.forkBottom = forkSettings.m_forkBottom;
        forkSettings.forkLeft = forkSettings.m_forkLeft;
        forkSettings.forkRight = forkSettings.m_forkRight;

    }

    [System.Serializable]
    public class PMRailPointForking
    {
        [Header("Input")]
        public string upForkChooseInputName = "Up";
        public string downForkChooseInputName = "Down";
        public string leftForkChooseInputName = "Left";
        public string rightForkChooseInputName = "Right";

        [Header("Fork Points")]
        [SerializeField] public PMRailPoint m_forkTop;
        [SerializeField] public PMRailPoint m_forkBottom;
        [SerializeField] public PMRailPoint m_forkLeft;
        [SerializeField] public PMRailPoint m_forkRight;

        public PMRailPoint forkTop
        {
            get
            {
                return m_forkTop;
            }
            set
            {
                m_forkTop = value;
            }
        }
        public PMRailPoint forkBottom
        {
            get
            {
                return m_forkBottom;
            }
            set
            {
                m_forkBottom = value;
            }
        }
        public PMRailPoint forkLeft
        {
            get
            {
                return m_forkLeft;
            }
            set
            {
                m_forkLeft = value;
            }
        }
        public PMRailPoint forkRight
        {
            get
            {
                return m_forkRight;
            }
            set
            {
                m_forkRight = value;
            }
        }

        public bool forkedNode;

        public PMRailPoint parentPoint;

    }

    /// <summary>
    /// Retuns an array with all the available fork points
    /// </summary>
    /// <returns></returns>
    public PMRailPoint[] GetAvailableForks()
    {
        List<PMRailPoint> forks = new List<PMRailPoint>();
        if (forkSettings.forkTop != null)
        {
            forks.Add(forkSettings.forkTop);
        }

        if (forkSettings.forkBottom != null)
        {
            forks.Add(forkSettings.forkBottom);
        }

        if (forkSettings.forkLeft != null)
        {
            forks.Add(forkSettings.forkLeft);
        }

        if (forkSettings.forkRight != null)
        {
            forks.Add(forkSettings.forkRight);
        }
        return forks.ToArray();
    }

    public void ClearAllRotationActiveFlags()
    {
        if (this.prevPoint != null)
        {
            this.prevPoint.GetComponent<PMRailPoint>().stateController.IsActiveForRotation = false;

        }

        if (this.nextPoint != null)
        {
            this.nextPoint.GetComponent<PMRailPoint>().stateController.IsActiveForRotation = false;

        }
    }

    /// <summary>
    /// Updates fork points settings 
    /// </summary>
    /// <param name="forkPoints">fork points array</param>
    public void UpdateForkPointSettings(PMRailPoint[] forkPoints)
    {
        if (forkPoints.Length > 0)
        {
            this.forkSettings.forkedNode = true;

            for (int i = 0; i < forkPoints.Length; i++)
            {
                forkPoints[i].gameObject.transform.position = this.transform.position;

                if(forkPoints[i].forkSettings == null)
                {
                    forkPoints[i].forkSettings = new PMRailPointForking();
                }
                forkPoints[i].forkSettings.forkedNode = true;
                forkPoints[i].forkSettings.parentPoint = this;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
         stateController = new PMRailPointStateController(this);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateForkPointSettings(GetAvailableForks());

        HandleWarmup();

        HandleActive();

        //if (stopCollider != null)
        //{
        //    stopCollider.transform.rotation = this.transform.rotation;//}
        //    stopCollider.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z) - this.transform.forward * 0.5f;//new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        //}

        //this.transform.position = new Vector3(this.transform.position.x, this.GetComponentInParent<PlayerMovementRail>().transform.position.y, this.transform.position.z);

        //if (this.prevPoint != null && this.nextPoint != null)
        //{
        //    this.gameObject.transform.LookAt(this.nextPoint.transform);
        //}
        //else if (this.prevPoint != null && this.nextPoint == null)
        //{
        //    this.gameObject.transform.LookAt(this.prevPoint.transform);
        //}
    }

    public void HandleWarmup()
    {
        if(rail != null && rail.target != null)
        {
            Vector3 railPosition = new Vector3(rail.target.transform.position.x, 0, rail.target.transform.position.z);
            Vector3 thisPosition = new Vector3(this.transform.position.x, 0, this.transform.position.z);

            //Check if player is inside the warmup area and outside the activation area.
            if ((Vector3.Distance(railPosition, thisPosition) < this.pointSettings.warmUpRadius) &&
                    (Vector3.Distance(railPosition, thisPosition) > this.pointSettings.activationRadius) && !stateController.IsActive)
            {
                this.stateController.IsActive = false;

                if (!this.stateController.IsWarmingUp && OnPointWarmupEnter != null && forkSettings.parentPoint == null)
                {
                    this.OnPointWarmupEnter(this);
                }
                this.stateController.IsWarmingUp = true;

                if (OnPointWarmupStay != null && forkSettings.parentPoint == null)
                {
                    WarmupInputListener();
                    this.OnPointWarmupStay(this);
                }
            }
            else
            {
                if (this.stateController.IsWarmingUp && OnPointWarmupExit != null && forkSettings.parentPoint == null)
                {
                    playerForkInput = PMRail.eInput.NONE;
                    this.OnPointWarmupExit(this);
                }
                this.stateController.IsWarmingUp = false;
            }
        }
    }

    public void HandleActive()
    {
        if (rail != null && rail.target != null)
        {
            Vector3 railPosition = new Vector3(rail.target.transform.position.x, 0, rail.target.transform.position.z);
            Vector3 thisPosition = new Vector3(this.transform.position.x, 0, this.transform.position.z);

            //Check if player is inside the activation area.
            if ((Vector3.Distance(railPosition, thisPosition) <= this.pointSettings.activationRadius) && !stateController.IsWarmingUp)
            {
                if (!this.stateController.IsActive && OnPointActiveEnter != null && forkSettings.parentPoint == null)
                {
                    ActivationInputListener();
                    this.OnPointActiveEnter(this);
                }
                this.stateController.IsActive = true;

                if (OnPointActiveStay != null && forkSettings.parentPoint == null)
                {
                    this.OnPointActiveStay(this);
                }
            }
            else
            {
                if (this.stateController.IsActive && OnPointActiveExit != null && forkSettings.parentPoint == null)
                {
                    this.OnPointActiveExit(this);
                }
                this.stateController.IsActive = false;
            }
        }
    }

    public PMRail.eInput playerForkInput = PMRail.eInput.NONE;

    /// <summary>
    /// Handles logic for inputs while in warm-up area
    /// </summary>
    public void WarmupInputListener()
    {
        if(Input.GetButtonDown("PS4_DPAD_UP") && playerForkInput == PMRail.eInput.NONE)
        {
            playerForkInput = PMRail.eInput.UP;
        }

        if (Input.GetButtonDown("PS4_DPAD_DOWN") && playerForkInput == PMRail.eInput.NONE)
        {
            playerForkInput = PMRail.eInput.DOWN;
        }

        if (Input.GetButtonDown("PS4_DPAD_LEFT") && playerForkInput == PMRail.eInput.NONE)
        {
            playerForkInput = PMRail.eInput.LEFT;
        }

        if (Input.GetButtonDown("PS4_DPAD_RIGHT") && playerForkInput == PMRail.eInput.NONE)
        {
            playerForkInput = PMRail.eInput.RIGHT;
        }
    }

    /// <summary>
    /// Handles logic for inputs while in activation area
    /// </summary>
    public void ActivationInputListener()
    {
        if (Input.GetButton("PS4_DPAD_UP"))
        {
            playerForkInput = PMRail.eInput.UP;
        }

        if (Input.GetButton("PS4_DPAD_DOWN"))
        {
            playerForkInput = PMRail.eInput.DOWN;
        }

        if (Input.GetButton("PS4_DPAD_LEFT"))
        {
            playerForkInput = PMRail.eInput.LEFT;
        }

        if (Input.GetButton("PS4_DPAD_RIGHT"))
        {
            playerForkInput = PMRail.eInput.RIGHT;
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(this.forkSettings != null && this.forkSettings.parentPoint == null && this.forkSettings.forkedNode)
        {
            //Draw warm-up radius
            if (this.forkSettings.forkedNode)
            {
                Gizmos.color = new Color(255f, 215f, 0f);
                Handles.color = new Color(255f, 215f, 0f,0.2f);
            }
            else
            {
                Gizmos.color = Color.cyan;
                Handles.color = new Color(0f, 255f, 255f, 0.2f);

            }

            if (this.stopPoint)
            {
                Gizmos.color = Color.red;
                Handles.color = new Color(1, 0, 0, 0.2f);

                DrawCircle(this.transform.position, pointSettings.warmUpRadius);

            }
            else
            {
                DrawCircle(this.transform.position, pointSettings.warmUpRadius);

                //Draw activation radius
                Gizmos.color = Color.green;
                DrawCircle(this.transform.position, pointSettings.activationRadius);
            }
            
            if(this.stateController.IsActive)
            {
                Handles.Label(transform.position + Vector3.up * 2,
                                     transform.position.ToString() + "\nActive");
            }
            else if(this.stateController.IsWarmingUp)
            {
                Handles.Label(transform.position + Vector3.up * 2,
                     transform.position.ToString() + "\nWarming up");
            }


            Handles.DrawSolidDisc(transform.position, transform.up, pointSettings.warmUpRadius);

            Handles.color = Color.blue;
            pointSettings.warmUpRadius = Handles.ScaleValueHandle(pointSettings.warmUpRadius,
                            transform.position,
                            transform.rotation, 5, Handles.ArrowHandleCap, 1);
        }
    }
#endif
    public delegate void PMRailPointEventHandler(PMRailPoint point);

    public event PMRailPointEventHandler OnPointActiveEnter;
    public event PMRailPointEventHandler OnPointActiveStay;
    public event PMRailPointEventHandler OnPointActiveExit;

    public event PMRailPointEventHandler OnPointWarmupEnter;
    public event PMRailPointEventHandler OnPointWarmupStay;
    public event PMRailPointEventHandler OnPointWarmupExit;

    /// <summary>
    /// Draws gizmo circle for warmup area
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    private void DrawCircle(Vector3 center, float radius)
    {
        float theta = 0;
        float x = radius * Mathf.Cos(theta);
        float y = radius * Mathf.Sin(theta);
        Vector3 pos = transform.position + new Vector3(x, 0, y);
        Vector3 newPos = pos;
        Vector3 lastPos = pos;

        for (theta = 0.1f; theta < Mathf.PI * 2; theta += 0.1f)
        {
            x = radius * Mathf.Cos(theta);
            y = radius * Mathf.Sin(theta);
            newPos = transform.position + new Vector3(x, 0, y);
            Gizmos.DrawLine(pos, newPos);
            pos = newPos;
        }
        Gizmos.DrawLine(pos, lastPos);
    }

    //public IEnumerator AutoRotateByVertical(float degrees, float time)
    //{
    //    camScript.Rotation.AutoRotateBy(0f, degrees, time);
    //    yield return new WaitForSeconds(0);
    //}

    //public IEnumerator AutoRotateToVertical(float degrees, float time)
    //{
    //    camScript.Rotation.AutoRotateTo(0f, degrees, time);
    //    yield return new WaitForSeconds(0);
    //}

    public void GenerateStopCollider()
    {
        if(stopCollider == null)
        {
            GameObject colliderObj = new GameObject();
            colliderObj.name = "StopCollider";

            stopCollider = colliderObj.AddComponent<BoxCollider>();
            colliderObj.transform.parent = this.transform;
            stopCollider.isTrigger = false;
            stopCollider.size = new Vector3(16, 16, 0.5f);
        }
    }

    public void GenerateSwitchCollider()
    {
        if(stopPoint)
        {
            GameObject colliderObj = new GameObject();
            colliderObj.name = "SwitchCollider";
            colliderObj.tag = "SwitchCollider";

            switchRailCollider = colliderObj.AddComponent<BoxCollider>();
            colliderObj.transform.parent = this.transform;

            switchRailCollider.isTrigger = true;
            switchRailCollider.size = joinPointArea;
        }
    }

    public void RemoveStopCollider()
    {
        if(stopCollider != null && stopCollider.gameObject != null)
        {
            DestroyImmediate(stopCollider.gameObject);
        }
    }

    public void RemoveSwitchRailCollider()
    {

    }

    private void OnDestroy()
    {
        DestroyImmediate(switchRailCollider);
        DestroyImmediate(stopCollider);
    }
}
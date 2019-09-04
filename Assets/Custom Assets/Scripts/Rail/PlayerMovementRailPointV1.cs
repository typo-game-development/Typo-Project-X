using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[RequireComponent(typeof(BoxCollider))]
[ExecuteInEditMode]
public class PlayerMovementRailPointV1 : MonoBehaviour
{
    public GameObject prevPoint;
    public GameObject nextPoint;

    [HideInInspector]
    public float angleBetweenPrevAndNext = 0;
     

    public float rotationLerpTime = 0f;
    public BoxCollider stopCollider;
    public BoxCollider switchRailCollider;

    public bool stopPoint = false;
    public bool stopWhileRotating = false;

    [HideInInspector]
    public bool activeForRotation = false;

    [HideInInspector]
    public PlayerMovementRailV1 masterRail = null;

    public bool enableRailJoin = false;

    [HideInInspector]
    public GameObject railPointToJoin = null;

    [HideInInspector]
    public Vector3 joinPointArea = Vector3.one;

    
    public float startCameraVerticalRotation;
    public bool isRotatedDown = false;
    public bool isRotatedUp = false;


    [Header("Forking")]
    public Forking forkSettings = new Forking();

    public class TwoLineReorderableListElement : UnityEngine.PropertyAttribute { }

    void OnValidate()
    {
        forkSettings.forkTop = forkSettings.m_forkTop;
        forkSettings.forkBottom = forkSettings.m_forkBottom;
        forkSettings.forkLeft = forkSettings.m_forkLeft;
        forkSettings.forkRight = forkSettings.m_forkRight;

    }

    [System.Serializable]
    public class Forking
    {
        [SerializeField] public PlayerMovementRailV1 m_forkTop;
        [SerializeField] public PlayerMovementRailV1 m_forkBottom;
        [SerializeField] public PlayerMovementRailV1 m_forkLeft;
        [SerializeField] public PlayerMovementRailV1 m_forkRight;

        public PlayerMovementRailV1 forkTop
        {
            get
            {
                return m_forkTop;
            }
            set
            {
                if (value != null)
                {
                    m_forkTop.generatedForkDirection = PlayerMovementRailV1.ForkDirection.Top;

                }
                m_forkTop = value;
            }
        }
        public PlayerMovementRailV1 forkBottom
        {
            get
            {
                return m_forkBottom;
            }
            set
            {
                if (value != null)
                {
                    m_forkBottom.generatedForkDirection = PlayerMovementRailV1.ForkDirection.Bottom;

                }
                m_forkBottom = value;
            }
        }
        public PlayerMovementRailV1 forkLeft
        {
            get
            {
                return m_forkLeft;
            }
            set
            {
                if (value != null)
                {
                    m_forkLeft.generatedForkDirection = PlayerMovementRailV1.ForkDirection.Left;

                }
                m_forkLeft = value;
            }
        }
        public PlayerMovementRailV1 forkRight
        {
            get
            {
                return m_forkRight;
            }
            set
            {
                if (value != null)
                {
                    m_forkRight.generatedForkDirection = PlayerMovementRailV1.ForkDirection.Right;

                }
                m_forkRight = value;
            }
        }

        public bool forkedNode;
        
        public GameObject parentNode;

    }

    public PlayerMovementRailV1[] GetAvailableForks()
    {
        List<PlayerMovementRailV1> forks = new List<PlayerMovementRailV1>();
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

    private void Awake()
    {
     if (forkSettings == null)
        {
                 forkSettings = new Forking();

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        masterRail = this.gameObject.GetComponentInParent<PlayerMovementRailV1>();

        if (stopPoint && !masterRail.loop)
        {
            if (stopCollider == null)
            {
                GenerateStopCollider();
            }
        }

        if (enableRailJoin)
        {
            if (switchRailCollider == null)
            {
                GenerateSwitchCollider();
            }
        }
        else
        {
            if (switchRailCollider != null)
            {
                DestroyImmediate(switchRailCollider);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (stopCollider != null)
        {
            stopCollider.transform.rotation = this.transform.rotation;//}
            stopCollider.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z) - this.transform.forward * 0.5f;//new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        }

        if (switchRailCollider != null)
        {
            switchRailCollider.gameObject.transform.rotation = this.transform.rotation;//}
            switchRailCollider.gameObject.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);

            TombiCharacterController charScript;

            charScript = FindObjectOfType<TombiCharacterController>();

            RailingSystem.SwitchColliderTrigger trg = switchRailCollider.GetComponent<RailingSystem.SwitchColliderTrigger>();

        }
        this.transform.position = new Vector3(this.transform.position.x, this.GetComponentInParent<PlayerMovementRailV1>().transform.position.y, this.transform.position.z);

        if (this.prevPoint != null && this.nextPoint != null)
        {
            this.gameObject.transform.LookAt(this.nextPoint.transform);
        }
        else if (this.prevPoint != null && this.nextPoint == null)
        {
            this.gameObject.transform.LookAt(this.prevPoint.transform);
        }
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
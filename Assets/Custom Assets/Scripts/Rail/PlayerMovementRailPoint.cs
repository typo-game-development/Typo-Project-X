using System.Collections;
using UnityEngine;

[System.Serializable]
[RequireComponent(typeof(BoxCollider))]
[ExecuteInEditMode]
public class PlayerMovementRailPoint : MonoBehaviour
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
    private PlayerMovementRail m_rail = null;

    public bool enableRailJoin = false;

    [HideInInspector]
    public GameObject railPointToJoin = null;

    [HideInInspector]
    public Vector3 joinPointArea = Vector3.one;
    // Start is called before the first frame update
    void Start()
    {
        m_rail = this.gameObject.GetComponentInParent<PlayerMovementRail>();
        //if (camScript != null)
        //{
        //    startCameraVerticalRotation = camScript.Rotation.VerticalRotation;

        //}
        if (stopPoint && !m_rail.loop)
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

    //public AdvancedUtilities.Cameras.BasicCameraController camScript;

    // Update is called once per frame
    void Update()
    {
        if (stopCollider != null)
        {
            stopCollider.transform.rotation = this.transform.rotation;//}
            stopCollider.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);//new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        }

        if (switchRailCollider != null)
        {
            switchRailCollider.gameObject.transform.rotation = this.transform.rotation;//}
            switchRailCollider.gameObject.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);

            TombiCharacterController charScript;

            charScript = FindObjectOfType<TombiCharacterController>();

            RailingSystem.SwitchColliderTrigger trg = switchRailCollider.GetComponent<RailingSystem.SwitchColliderTrigger>();

            if (trg != null && trg.stay)
            {
                //if (Input.GetButton("PS4_DPAD_UP"))
                //{
                //    if (!isRotatedUp)
                //    {
                //        isRotatedDown = false;
                //        isRotatedUp = true;
                //        //charScript.RotateTowardsMovementDir(charScript.sceneCamera.gameObject.transform.forward);
                //        StartCoroutine(AutoRotateByVertical(15f, 0.2f));
                //    }
                //}

                //if (Input.GetButton("PS4_DPAD_DOWN"))
                //{
                //    if (isRotatedUp)
                //    {
                //        StartCoroutine(AutoRotateToVertical(startCameraVerticalRotation, 0.2f));
                //        isRotatedUp = false;

                //    }

                //    if (!isRotatedDown)
                //    {
                //        isRotatedDown = true;
                //        isRotatedUp = false;

                //        StartCoroutine(AutoRotateByVertical(-15f, 0.2f));
                //    }
                //    //charScript.RotateTowardsMovementDir(-charScript.sceneCamera.gameObject.transform.forward);

                //}
            }
        }
        this.transform.position = new Vector3(this.transform.position.x, this.GetComponentInParent<PlayerMovementRail>().transform.position.y, this.transform.position.z);

        if (this.prevPoint != null && this.nextPoint != null)
        {
            this.gameObject.transform.LookAt(this.nextPoint.transform);
        }
        else if (this.prevPoint != null && this.nextPoint == null)
        {
            this.gameObject.transform.LookAt(this.prevPoint.transform);
        }
    }

    public float startCameraVerticalRotation;
    public bool isRotatedDown = false;
    public bool isRotatedUp = false;

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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CameraSpline : BezierSpline
{
    [Range(-1f,1f)]
    public float compensationConstant = 0f;
    public bool manualInputBounds;

    public CameraSpline(float compensationConstant, bool manualInputBounds, Bounds cameraBoundings)
    {
        this.compensationConstant = compensationConstant;
        this.manualInputBounds = manualInputBounds;
        CameraBoundings = cameraBoundings;

        showDirections = false;
    }

    public Bounds CameraBoundings
    {
        get
        {
            if(this.GetComponent<BoxCollider>() != null)
            {
                return this.GetComponent<BoxCollider>().bounds;

            }
            else
            {
                return new Bounds();
            }
        }
        set
        {
            this.GetComponent<BoxCollider>().size = value.size;
            this.GetComponent<BoxCollider>().center = value.center;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Typo.Utilities.Cameras.BasicCameraController camScript = FindObjectOfType<Typo.Utilities.Cameras.BasicCameraController>();
        if (camScript != null)
        {
            camScript.splineFollow.targetSpline = this;
        }
    }
#if UNITY_EDITOR
    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (!UnityEditor.Selection.Contains(this.gameObject))
        {
            Color oldColor = Gizmos.color;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(CameraBoundings.center, CameraBoundings.size);
            Gizmos.color = oldColor;
        }
    }
#endif

    public void SetBoxColliderBounds()
    {
        BoxCollider cubeCollider = this.GetComponent<BoxCollider>();

        if(cubeCollider != null)
        {
            Bounds b = new Bounds(this.transform.InverseTransformPoint(GetPoint(0.5f)), Vector3.zero);

            for (int i = 0; i < points.Length; i += 3)
            {
                if (points[i] != this.transform.position)
                {
                    b.Encapsulate(points[i]);

                }
            }
            cubeCollider.center = b.center;
            cubeCollider.size = b.size;
        }
    }
}

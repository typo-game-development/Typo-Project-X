using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeLineRenderer : MonoBehaviour
{
    LineRenderer lineRenderer;

    public GameObject[] planks;
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = lineRenderer.endWidth = 0.1f;

    }

    // Update is called once per frame
    void Update()
    {
        int i = 0;

        lineRenderer.positionCount = planks.Length;

        foreach(GameObject plank in planks)
        {
            if (plank != null)
            {
                if(plank.GetComponent<MeshRenderer>())
                {
                    plank.GetComponent<MeshRenderer>().enabled = false;

                }

                lineRenderer.SetPosition(i, plank.transform.position);

            }
            i++;
        }

    }
}

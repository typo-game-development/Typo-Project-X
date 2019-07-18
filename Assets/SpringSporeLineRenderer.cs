using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SpringSporeLineRenderer : MonoBehaviour
{
    LineRenderer lineRenderer;

    public GameObject anchor;
    public GameObject spore;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = lineRenderer.endWidth = 0.1f;

    }

    // Update is called once per frame
    void Update()
    {

        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, anchor.transform.position);

        lineRenderer.SetPosition(1, spore.transform.position);

    }
}

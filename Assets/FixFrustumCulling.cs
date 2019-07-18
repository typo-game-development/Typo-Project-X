using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixFrustumCulling : MonoBehaviour
{
    void Awake()
    {
        MeshFilter mFilter = GetComponent<MeshFilter>();
        if (mFilter != null)
        {
            Mesh mesh = mFilter.mesh;
            mesh.bounds = new Bounds(Vector3.zero, 10000f * Vector3.one);
        }
    }
}

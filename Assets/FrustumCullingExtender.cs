using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrustumCullingExtender : MonoBehaviour
{
    //private Camera cam;

    //private void Awake()
    //{
    //    //foreach (GameObject g in FindGameObjectsWithLayer(18))
    //    //{
    //    //    if (g != null)
    //    //    {
    //    //        g.AddComponent<FixFrustumCulling>();

    //    //    }
    //    //}
    //}
    //void Start()
    //{
    //    cam = this.GetComponent<Camera>();


    //}

    //void OnPreCull()
    //{
    //    cam.cullingMatrix = Matrix4x4.Ortho(int.MinValue, int.MaxValue, int.MinValue, int.MaxValue, 0.001f, int.MaxValue) *
    //                        Matrix4x4.Translate(Vector3.forward * int.MinValue / 2f) *
    //                        cam.worldToCameraMatrix;
    //}

    //void OnDisable()
    //{
    //    cam.ResetCullingMatrix();
    //}

    //private GameObject[] FindGameObjectsWithLayer(int layer)
    //{
    //    GameObject[] goArray = FindObjectsOfType(typeof(GameObject)) as GameObject[];
    //    List<GameObject> goList = new List<GameObject>();
    //    for (int i = 0; i < goArray.Length; i++)
    //    {
    //        if (goArray[i].layer == layer)
    //        {
    //            goList.Add(goArray[i]);
    //        }
    //    }
    //    if (goList.Count == 0)
    //    {
    //        return null;
    //    }
    //    return goList.ToArray();
    //}
}

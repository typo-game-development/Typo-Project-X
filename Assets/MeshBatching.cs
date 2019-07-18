using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Copy meshes from children into the parent's Mesh.
// CombineInstance stores the list of meshes.  These are combined
// and assigned to the attached Mesh.

//[RequireComponent(typeof(MeshFilter))]
//[RequireComponent(typeof(MeshRenderer))]
public class MeshBatching : MonoBehaviour
{
    public Transform combinedMesh;

    void Start()
    {
        GameObject[] meshFilters = FindGameObjectsWithLayer(19);
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            MeshFilter mFilter = meshFilters[i].GetComponent<MeshFilter>();

            if(mFilter != null)
            {
                combine[i].mesh = mFilter.sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                meshFilters[i].gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                //meshFilters[i].gameObject.AddComponent<DisableFrustumCulling>();
            }


            i++;
        }
        combinedMesh.GetComponent<MeshFilter>().mesh = new Mesh();
        combinedMesh.GetComponent<MeshFilter>().mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true);
        //transform.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        combinedMesh.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

        combinedMesh.gameObject.SetActive(true);
        combinedMesh.gameObject.transform.position = Vector3.zero;
    }

    private GameObject[] FindGameObjectsWithLayer(int layer)
    {
        GameObject[] goArray = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        List<GameObject> goList = new List<GameObject>();
        for (int i = 0; i < goArray.Length; i++)
        {
            if (goArray[i].layer == layer)
            {
                goList.Add(goArray[i]);
            }
        }
        if (goList.Count == 0)
        {
            return null;
        }
        return goList.ToArray();
    }
}
/*
    Author: Francesco Podda
    Date: 28/05/2019
*/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class LedgeController : MonoBehaviour
{
    [System.Serializable]
    public class CustomMeshData
    {
        [EditorRename("Mesh Object")] public GameObject meshObj;

        public List<Helpers.CustomEdgeData> edges = new List<Helpers.CustomEdgeData>();
        public Vector3[] vertices = new Vector3[0];

        public CustomMeshData(GameObject meshObj)
        {
            this.meshObj = meshObj;
        }

        public Mesh GetMesh()
        {
            /* Get shared mesh instead of .mesh to avoid
               errors while executing code in edit mode */

            if (meshObj.GetComponent<MeshFilter>() != null)
            {
                return meshObj.GetComponent<MeshFilter>().sharedMesh;
            }
            else
            {
                return null;
            }
        }
    }

    [HideInInspector] public Collider[] colliders = new Collider[0];
    [HideInInspector] public CustomMeshData[] meshDatas;
    [HideInInspector] public bool editMode = false;
    [HideInInspector] public bool drawLedges = false;
    [HideInInspector] public bool drawSelectedOnly = false;
    [HideInInspector] public bool calculatingEdges = false;
    [Range(-10f, 10f)]
    public float edgeLengthThreshold = 0.5f;
    public float colliderSize = 0.1f;
    public int climbLayerID;

    public void UpdateMeshData()
    {
        int i = 0;
        GameObject[] meshObjs = FindGameObjectsWithLayer(climbLayerID);
        System.Array.Resize(ref meshDatas, meshObjs.Length);

        foreach (GameObject meshObj in meshObjs)
        {
            Mesh mesh;
            CustomMeshData mData = new CustomMeshData(meshObj);
            Helpers.CustomEdgeData lastEdge = null;
            
            mesh = mData.GetMesh();

            if (mesh != null)
            {
                mData.vertices = mesh.vertices;
                mData.edges = Helpers.GetEdges(mesh.triangles).FindBoundary().SortEdges();
                int i2 = mesh.triangles.Length;

                foreach (Helpers.CustomEdgeData edge in mData.edges.ToList())
                {
                    if (lastEdge != null && lastEdge.triangleIndex == edge.triangleIndex)
                    {
                        //Remove inner edges
                        //if((lastEdge.v1 < edge.v2) || (lastEdge.v2 > edge.v1))
                        //{                        
                        //    mData.edges.Remove(edge);
                        //}
                        //if ((lastEdge.v1 < edge.v2) && (lastEdge.v2 > edge.v1))
                        //{
                        //    mData.edges.Remove(edge);
                        //}
                    }
                    else
                    {
                        lastEdge = edge;
                    }
                }
                meshDatas[i] = mData;
            }

            i++;
        }
        calculatingEdges = false;
    }

    public void GenerateColliderData()
    {
        foreach (Collider coll in colliders)
        {
            if(coll.gameObject != null)
            {
                DestroyImmediate(coll.gameObject);
            }
        }
        System.Array.Resize(ref colliders, 0);

        foreach (CustomMeshData meshData in meshDatas)
        {
            foreach (Helpers.CustomEdgeData edge in meshData.edges.ToList())
            {
                if(edge.selected)
                {
                    Vector3 vert1 = meshData.meshObj.transform.TransformPoint(meshData.vertices[edge.v1]);
                    Vector3 vert2 = meshData.meshObj.transform.TransformPoint(meshData.vertices[edge.v2]);

                    AddColliderBetweenPoints(meshData.meshObj,vert1, vert2, colliderSize);
                }
            }
        }
    }

    private void AddColliderBetweenPoints(GameObject obj,  Vector3 startPos, Vector3 endPos, float colliderSize)
    {
        BoxCollider col = new GameObject("Ledge Collider").AddComponent<BoxCollider>();
        Rigidbody rb = col.gameObject.AddComponent<Rigidbody>();

        col.transform.parent = obj.transform;
        float lineLength = Vector3.Distance(startPos, endPos);
        col.size = new Vector3(colliderSize, colliderSize, lineLength);
        Vector3 midPoint = (startPos + endPos) / 2;
        col.transform.position = midPoint;
        col.isTrigger = true;
        col.gameObject.layer = 12;

        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        //CustomAssets.Utilities.Design.IconDrawer.DrawDotBigIcon(col.gameObject, 3);

        System.Array.Resize(ref colliders, colliders.Length + 1);

        colliders[colliders.Length - 1] = col;

        float angle = Vector3.SignedAngle(startPos, endPos, -Vector3.up); //Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x) * Mathf.Rad2Deg;

        //col.transform.Rotate(0, angle, 0);

        //// Following lines calculate the angle between startPos and endPos
        //float angle = (Mathf.Abs(startPos.y - endPos.y) / Mathf.Abs(startPos.x - endPos.x));
        //if ((startPos.y < endPos.y && startPos.x > endPos.x) || (endPos.y < startPos.y && endPos.x > startPos.x))
        //{
        //    angle *= -1;
        //}
        //angle = Mathf.Abs( Mathf.Rad2Deg * Mathf.Atan(angle));

        //if(angle != float.NaN && angle > 0)
        //{
        //    col.transform.Rotate(0, 0, angle);

        //}
    }

    public static void DrawLine(Vector3 p1, Vector3 p2, float width)
    {
        int count = 1 + Mathf.CeilToInt(width); // how many lines are needed.
        if (count == 1)
        {
            Gizmos.DrawLine(p1, p2);
        }
        else
        {
            Camera c = Camera.current;
            if (c == null)
            {
                Debug.LogError("Camera.current is null");
                return;
            }
            var scp1 = c.WorldToScreenPoint(p1);
            var scp2 = c.WorldToScreenPoint(p2);

            Vector3 v1 = (scp2 - scp1).normalized; // line direction
            Vector3 n = Vector3.Cross(v1, Vector3.forward); // normal vector

            for (int i = 0; i < count; i++)
            {
                Vector3 o = 0.99f * n * width * ((float)i / (count - 1) - 0.5f);
                Vector3 origin = c.ScreenToWorldPoint(scp1 + o);
                Vector3 destiny = c.ScreenToWorldPoint(scp2 + o);
                Gizmos.DrawLine(origin, destiny);
            }
        }
    }

    private GameObject[] FindGameObjectsWithLayer(int layer) {
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
public static class Helpers
{
    [System.Serializable]
    public class CustomEdgeData
    {
        public int v1;
        public int v2;
        public int triangleIndex;
        public bool selected;

        public CustomEdgeData(int aV1, int aV2, int aIndex)
        {
            v1 = aV1;
            v2 = aV2;
            triangleIndex = aIndex;
        }
    }

    public static List<CustomEdgeData> GetEdges(int[] aIndices)
    {
        List<CustomEdgeData> result = new List<CustomEdgeData>();
        for (int i = 0; i < aIndices.Length; i += 3)
        {
            int v1 = aIndices[i];
            int v2 = aIndices[i + 1];
            int v3 = aIndices[i + 2];
            result.Add(new CustomEdgeData(v1, v2, i));
            result.Add(new CustomEdgeData(v2, v3, i));
            result.Add(new CustomEdgeData(v3, v1, i));
        }
        return result;
    }

    public static List<CustomEdgeData> FindBoundary(this List<CustomEdgeData> aEdges)
    {
        List<CustomEdgeData> result = new List<CustomEdgeData>(aEdges);
        for (int i = result.Count - 1; i > 0; i--)
        {
            for (int n = i - 1; n >= 0; n--)
            {
                if (result[i].v1 == result[n].v2 && result[i].v2 == result[n].v1)
                {
                    // shared edge so remove both
                    result.RemoveAt(i);
                    result.RemoveAt(n);
                    i--;
                    break;
                }
            }
        }
        return result;
    }
    public static List<CustomEdgeData> SortEdges(this List<CustomEdgeData> aEdges)
    {
        List<CustomEdgeData> result = new List<CustomEdgeData>(aEdges);
        for (int i = 0; i < result.Count - 2; i++)
        {
            CustomEdgeData E = result[i];
            for (int n = i + 1; n < result.Count; n++)
            {
                CustomEdgeData a = result[n];
                if (E.v2 == a.v1)
                {
                    // in this case they are already in order so just continoue with the next one
                    if (n == i + 1)
                        break;
                    // if we found a match, swap them with the next one after "i"
                    result[n] = result[i + 1];
                    result[i + 1] = a;
                    break;
                }
            }
        }
        return result;
    }
}
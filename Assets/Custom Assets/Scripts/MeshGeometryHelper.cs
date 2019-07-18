using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class MeshGeometryHelper : MonoBehaviour
{
    public Mesh mesh;
    public Vector3[] vertices;
    public List<Helpers.CustomEdgeData> edges;

    void Start()
    {

            mesh = GetComponent<MeshFilter>().sharedMesh;
            vertices = mesh.vertices;
            edges = Helpers.GetEdges(mesh.triangles).FindBoundary().SortEdges();

    }

    void Update()
    {
        if (mesh != null)
        {

            //for (var i = 0; i < vertices.Length; i++)
            //{
            //    vertices[i] += Vector3.up * Time.deltaTime;
            //}

            //// assign the local vertices array into the vertices array of the Mesh.
            //mesh.vertices = vertices;
            //mesh.RecalculateBounds();
        }

    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < vertices.Length - 1; i++)
        {
            //Handles.FreeMoveHandle(transform.TransformPoint(vertices[i]), this.transform.rotation, 0.1f, Vector3.zero,  Handles.DotHandleCap);
        }

        for(int i = 0; i< edges.Count - 1; i++)
        {
            Gizmos.color = Color.red;
            Vector3 vert1 = transform.TransformPoint(vertices[edges[i].v1]);
            Vector3 vert2 = transform.TransformPoint(vertices[edges[i].v2]);

            DrawLine(vert1, vert2, 1f);
        }
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
}

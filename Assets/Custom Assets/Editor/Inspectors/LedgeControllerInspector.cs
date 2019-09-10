/*
    Author: Francesco Podda
    Date: 28/05/2019
*/

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LedgeController))]
public class LedgeControllerInspector : Editor
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnInspectorGUI()
    {
        LedgeController t = (LedgeController)target;
        base.OnInspectorGUI();



        t.editMode = EditorGUILayout.Toggle("Edit Mode", t.editMode);
        t.drawLedges = EditorGUILayout.Toggle("Show Ledges", t.drawLedges);

        if(t.drawLedges)
        {
            t.drawSelectedOnly = EditorGUILayout.Toggle("Show Only Selected Ledges", t.drawSelectedOnly);
        }

        EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
        if (GUILayout.Button("Update Mesh List"))
        {
            calculating = true;
            t.UpdateMeshData();
            calculating = false;
        }


        if (GUILayout.Button("Populate Colliders"))
        {
            t.GenerateColliderData();
        }
        EditorGUI.EndDisabledGroup();

    }

    bool calculating = false;

    public void OnSceneGUI()
    {
        LedgeController t = (LedgeController)target;

        DrawHandles(t);
    }

    public void DrawHandles(LedgeController lController)
    {
        if(!calculating)
        {
            if (lController.meshDatas != null && lController.meshDatas.Length > 0)
            {
                foreach (LedgeController.CustomMeshData meshData in lController.meshDatas)
                {
                    for (int i = 0; i < meshData.edges.Count - 1; i++)
                    {
                        //World position of vertex 1
                        Vector3 v1 = meshData.meshObj.transform.TransformPoint(meshData.vertices[meshData.edges[i].v1]);

                        //World position of vertex 2
                        Vector3 v2 = meshData.meshObj.transform.TransformPoint(meshData.vertices[meshData.edges[i].v2]);

                        //Vector3 v1Prev = Vector3.zero;
                        //Vector3 v2Prev = Vector3.zero;

                        //Vector3 v1Next = Vector3.zero;
                        //Vector3 v2Next = Vector3.zero;

                        //if(i > 0)
                        //{
                        //    v1Prev = meshData.meshObj.transform.TransformPoint(meshData.vertices[meshData.edges[i - 1].v1]);
                        //    v2Prev = meshData.meshObj.transform.TransformPoint(meshData.vertices[meshData.edges[i - 1].v2]);
                        //}

                        //if(i + 1 < meshData.edges.Count)
                        //{
                        //    v1Next = meshData.meshObj.transform.TransformPoint(meshData.vertices[meshData.edges[i + 1].v1]);
                        //    v2Next = meshData.meshObj.transform.TransformPoint(meshData.vertices[meshData.edges[i + 1].v2]);
                        //}
                        if ((Mathf.Abs((v1 - v2).z) >= lController.edgeLengthThreshold) && (Mathf.Abs((v1 - v2).y) <= 0.025f) && (Mathf.Abs((v1 - v2).x) <= 0.1f))
                        {
                            //if (Mathf.Abs((v1 - v2).y) <= 0.1f)
                            {
                                //World halfway position of current edge
                                Vector3 midEdgePos = (v1 + v2) / 2;

                                if (!Physics.Linecast(new Vector3(midEdgePos.x - 0.1f, midEdgePos.y + 0.2f, midEdgePos.z), new Vector3(midEdgePos.x + 0.1f, midEdgePos.y + 0.2f, midEdgePos.z)) &&
                                    !Physics.Linecast(new Vector3(midEdgePos.x + 0.1f, midEdgePos.y + 0.2f, midEdgePos.z), new Vector3(midEdgePos.x - 0.1f, midEdgePos.y + 0.2f, midEdgePos.z)))
                                {
                                    if ((!Physics.Linecast(new Vector3(midEdgePos.x - 0.2f, midEdgePos.y + 0.3f, midEdgePos.z), new Vector3(midEdgePos.x - 0.2f, midEdgePos.y - 0.3f, midEdgePos.z)) ||
                                        !Physics.Linecast(new Vector3(midEdgePos.x + 0.2f, midEdgePos.y + 0.3f, midEdgePos.z), new Vector3(midEdgePos.x + 0.2f, midEdgePos.y - 0.3f, midEdgePos.z))) &&
                                        (!Physics.Linecast(new Vector3(midEdgePos.x - 0.2f, midEdgePos.y - 0.3f, midEdgePos.z), new Vector3(midEdgePos.x - 0.2f, midEdgePos.y + 0.3f, midEdgePos.z)) ||
                                        !Physics.Linecast(new Vector3(midEdgePos.x + 0.2f, midEdgePos.y - 0.3f, midEdgePos.z), new Vector3(midEdgePos.x + 0.2f, midEdgePos.y + 0.3f, midEdgePos.z))))
                                    {
                                        bool drawEdge = false;

                                        if (meshData.edges[i].selected)
                                        {
                                            Handles.color = Color.green;
                                        }
                                        else
                                        {
                                            Handles.color = Color.blue;
                                        }

                                        if (lController.drawLedges)
                                        {
                                            if (lController.drawSelectedOnly)
                                            {
                                                if (meshData.edges[i].selected)
                                                {
                                                    Handles.DrawLine(v1, v2);
                                                }
                                            }
                                            else
                                            {
                                                Handles.DrawLine(v1, v2);
                                            }
                                        }

                                        if (lController.editMode)
                                        {
                                            if (lController.drawSelectedOnly)
                                            {
                                                if (meshData.edges[i].selected)
                                                {
                                                    drawEdge = true;
                                                }
                                            }
                                            else
                                            {
                                                drawEdge = true;
                                            }

                                            if (drawEdge)
                                            {
                                                if (Handles.Button((v1 + v2) / 2, new Quaternion(0, 0, 0, 0), 0.05f, 0.05f, Handles.DotHandleCap))
                                                {
                                                    meshData.edges[i].selected = !meshData.edges[i].selected;
                                                }
                                            }
                                        }
                                    }
                                }
                            }                            
                        }
                    }
                }
            }
        }
    }
}

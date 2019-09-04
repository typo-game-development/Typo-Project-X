using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor class for PlayerMovementRail.
/// </summary>
[CustomEditor(typeof(PlayerMovementRailV1))]
public class PlayerMovementRailEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();


        PlayerMovementRailV1 t = (PlayerMovementRailV1)target;
        test = t;

        if (GUILayout.Button("Add Point"))
        {
            t.AddPoint();

        }

        if (GUILayout.Button("Remove Point"))
        {
            t.RemovePoint();

        }
        GUILayout.Space(20f);

    }

    //[InitializeOnLoadMethod]
    //static void Init()
    //{
    //    //horizontalLine = new GUIStyle();
    //    //horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
    //    //horizontalLine.margin = new RectOffset(0, 0, 4, 4);
    //    //horizontalLine.fixedHeight = 1;

    //    SceneView.duringSceneGui -= OnSceneGUI;
    //    SceneView.duringSceneGui += OnSceneGUI;
    //}

    public static bool flag = false;
    public static Vector3 firstSwitchPoint = Vector3.zero;
    public static bool getFirstSwitchPoint;

    //private static void OnSceneGUI(SceneView sceneView)
    //{
    //    if (flag)
    //    {
    //        if (UnityEngine.Event.current.type == EventType.Layout)
    //        {
    //            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(UnityEngine.Event.current.GetHashCode(), FocusType.Passive));
    //        }
            
    //    }

    //    Handles.BeginGUI();
    //    {
    //        if (EditorWindow.mouseOverWindow is SceneView)
    //        {
    //            if (flag)
    //            {
    //                DrawCircleBrush(Color.red, 10f);
    //            }

    //            if (UnityEngine.Event.current.type == EventType.KeyDown)
    //            {
    //                if (UnityEngine.Event.current.keyCode == KeyCode.LeftControl)
    //                {
    //                    flag = true;
    //                    DrawCircleBrush(Color.red, 10f);

    //                }
    //            }
    //            if (UnityEngine.Event.current.type == EventType.KeyUp)
    //            {
    //                if (UnityEngine.Event.current.keyCode == KeyCode.LeftControl)
    //                {
    //                    flag = false;
    //                }
    //            }

    //            //if(UnityEngine.Event.current.type == EventType.MouseDown)
    //            //{
    //            //    if (UnityEngine.Event.current.button == 0)
    //            //    {
    //            //        Ray ray;
    //            //        RaycastHit hit;

    //            //        ray = HandleUtility.GUIPointToWorldRay(UnityEngine.Event.current.mousePosition);

    //            //        if (Physics.Raycast(ray, out hit, 100f))
    //            //        {
    //            //            Vector3 v1 = new Vector3(test.points[0].transform.position.x, test.points[0].transform.position.y, test.points[0].transform.position.z);
    //            //            Vector3 v2 = new Vector3(0, 0, hit.point.z).normalized;

    //            //            Vector3 AB = test.points[test.points.Length - 1].transform.transform.position - test.points[0].transform.position;
    //            //            Vector3 AC = hit.point - test.points[0].transform.position;
    //            //            Vector3 AX = Vector3.Project(AC, AB);
    //            //            Vector3 X = AX + test.points[0].transform.position;

    //            //            RaycastHit hit2;

    //            //            if (Physics.Raycast((X + Vector3.up * 100f), (X - Vector3.up * 100f) - (X + Vector3.up * 100f), out hit2))
    //            //            {
    //            //                firstSwitchPoint = new Vector3(X.x, hit2.point.y + 0.5f, X.z);
    //            //                Handles.Label(X, " ");

    //            //            }
    //            //        }
    //            //    }                    
    //            //}
    //        }
    //    }
    //    Handles.color = Color.magenta;
    //    Handles.Label(firstSwitchPoint, " ");
    //    Handles.DrawWireCube(firstSwitchPoint, new Vector3(1f, 1f, 1f));

    //    Handles.EndGUI();

    //    //UnityEngine.Event e = UnityEngine.Event.current;
    //    //int controlID = GUIUtility.GetControlID(FocusType.Passive);

    //    //switch (e.type)
    //    //{
    //    //    case EventType.MouseDown:
    //    //        break;

    //    //    case EventType.MouseUp:
    //    //        break;

    //    //    case EventType.MouseDrag:
    //    //        break;

    //    //    case EventType.KeyDown:

    //    //        if (e.keyCode == KeyCode.Space)
    //    //        {
    //    //            // Do something on pressing Spcae
    //    //        }
    //    //        if (e.keyCode == KeyCode.S)
    //    //        {
    //    //            // Do something on pressing S
    //    //        }
    //    //        break;
    //    //}
    //}

    public static Vector3 size;
    public static PlayerMovementRailV1 test;

    private static void DrawCircleBrush(Color _color, float _size)
    {
        //// Circle
        //Handles.CircleHandleCap(0, UnityEngine.Event.current.mousePosition, Quaternion.identity, _size, EventType.Repaint);
        //// Cross Center
        //Handles.DrawLine(UnityEngine.Event.current.mousePosition + Vector2.left, UnityEngine.Event.current.mousePosition + Vector2.right);
        //Handles.DrawLine(UnityEngine.Event.current.mousePosition + Vector2.up, UnityEngine.Event.current.mousePosition + Vector2.down);
        //if(firstSwitchPoint != Vector3.zero)
        //{
        //    Handles.color = Color.magenta;
        //    Handles.DrawWireCube(firstSwitchPoint, new Vector3(1f, 1f, 1f));
        //    Debug.Log("Daje");
        //}
        Handles.color = _color;

        if (test != null)
        {
            Ray ray;
            RaycastHit hit;

            //Vector3 point Camera.current.ScreenToWorldPoint(UnityEngine.Event.current.mousePosition);

            ray = HandleUtility.GUIPointToWorldRay(UnityEngine.Event.current.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 v1 = new Vector3(test.points[0].transform.position.x, test.points[0].transform.position.y, test.points[0].transform.position.z);
                Vector3 v2 = new Vector3(0, 0, hit.point.z).normalized;

                Vector3 AB = test.points[test.points.Length - 1].transform.transform.position - test.points[0].transform.position;
                Vector3 AC = hit.point - test.points[0].transform.position;
                Vector3 AX = Vector3.Project(AC, AB);
                Vector3 X = AX + test.points[0].transform.position;

                RaycastHit hit2;

                if (Physics.Raycast((X + Vector3.up * 100f), (X - Vector3.up * 100f) - (X + Vector3.up * 100f), out hit2))
                {
                    Vector3 pos2 = new Vector3(X.x, hit2.point.y + 0.5f, X.z);
                    Handles.Label(X, " ");

                    Handles.DrawWireCube(pos2, new Vector3(1f, 1f, 1f));

                }
            }

        }
    }

    public static Vector3 LerpByDistance(Vector3 A, Vector3 B, float x)
    {
        Vector3 P = x * Vector3.Normalize(B - A) + A;
        return P;
    }
}

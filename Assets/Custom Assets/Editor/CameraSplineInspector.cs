using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

[System.Serializable]
[CustomEditor(typeof(CameraSpline))]
public class CameraSplineInspector : BezierSplineInspector
{
    CameraSpline cameraSpline;
    UnityEditor.IMGUI.Controls.BoxBoundsHandle m_BoxBoundsHandle = new UnityEditor.IMGUI.Controls.BoxBoundsHandle();

    private void Awake()
    {

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (Tools.current > Tool.Move)
        {
            Hidden = true;
        }
        else
        {
            Hidden = false;
        }
        cameraSpline = target as CameraSpline;

        if(cameraSpline != null)
        {
            cameraSpline.transform.rotation = Quaternion.Euler(0,0,0);
            cameraSpline.transform.localScale = Vector3.one;

        }
        cameraSpline.GetComponent<BoxCollider>().material = null;
        cameraSpline.GetComponent<BoxCollider>().isTrigger = true;
        cameraSpline.GetComponent<BoxCollider>().hideFlags = HideFlags.NotEditable;

        // Additional code for the derived class...
        m_BoxBoundsHandle.center = cameraSpline.CameraBoundings.center;
        m_BoxBoundsHandle.size = cameraSpline.CameraBoundings.size;

        cameraSpline.manualInputBounds = EditorGUILayout.Toggle("Manual Input Bounds", cameraSpline.manualInputBounds);


        if (!cameraSpline.manualInputBounds)
        {
            cameraSpline.SetBoxColliderBounds();

        }
    }
    public static bool Hidden
    {
        get
        {
            Type type = typeof(Tools);
            FieldInfo field = type.GetField("s_Hidden", BindingFlags.NonPublic | BindingFlags.Static);
            return ((bool)field.GetValue(null));
        }
        set
        {
            Type type = typeof(Tools);
            FieldInfo field = type.GetField("s_Hidden", BindingFlags.NonPublic | BindingFlags.Static);
            field.SetValue(null, value);
        }
    }
    public override void OnSceneGUI()
    {
        base.OnSceneGUI();

        if(cameraSpline != null)
        {
            if (cameraSpline.manualInputBounds)
            {
                Handles.color = Color.cyan;
                m_BoxBoundsHandle.DrawHandle();

            }
            cameraSpline.CameraBoundings = new Bounds(cameraSpline.transform.InverseTransformPoint(m_BoxBoundsHandle.center), m_BoxBoundsHandle.size);
        }

    }
}

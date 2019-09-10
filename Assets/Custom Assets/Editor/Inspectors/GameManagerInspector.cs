using UnityEditor;
using UnityEngine;
using Tomba;
using System.Collections.Generic;

[CustomEditor(typeof(Tomba.GameManager))]
public class GameManagerInspector : Editor
{
    bool collapseSaveStates = true;

    public override void OnInspectorGUI()
    {
        GameManager t = (GameManager)target;
        //DrawDefaultInspector();
        EditorGUILayout.Space();
        EditorGUI.indentLevel = 1;

        collapseSaveStates = EditorGUILayout.Foldout(collapseSaveStates, "Available Save States");

        if (collapseSaveStates)
        {
            EditorGUI.indentLevel += 1;
            for (int i = 0; i < t.saveStates.Length; i++)
            {
                SerializedObject so = new SerializedObject(t);

                if (t.saveStates[i] != null)
                {
                    t.saveStates[i].ID = i;
                    EditorGUILayout.PropertyField(so.FindProperty("saveStates").GetArrayElementAtIndex(i));

                }
                //EditorGUI.PrefixLabel(new Rect(25, 45, 100, 15), 0, new GUIContent("Thumbnail:"));


            }
        }

        //GUILayout.EndVertical();
    }
}
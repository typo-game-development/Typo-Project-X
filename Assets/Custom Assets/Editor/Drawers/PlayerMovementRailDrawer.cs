using UnityEngine;
using UnityEditor;

//[CustomPropertyDrawer(typeof(PlayerMovementRail))]
public class PlayerMovementRailDrawer : PropertyDrawer
{
    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        SerializedProperty connections = prop.serializedObject.FindProperty("connections");

        if(connections != null)
        {
            for (int i = 0; i < connections.arraySize; i++)
            {
                EditorGUILayout.LabelField("Test");
                EditorGUILayout.PropertyField(connections.GetArrayElementAtIndex(i));
            }
        }

    }
}


using UnityEditor;
using UnityEngine;
[CustomPropertyDrawer(typeof(EditorRenameAttribute))]
public class EditorNameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, new GUIContent((attribute as EditorRenameAttribute).NewName));
    }
}
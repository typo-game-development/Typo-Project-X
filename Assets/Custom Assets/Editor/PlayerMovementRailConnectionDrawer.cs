using UnityEngine;
using UnityEditor;

//[CustomPropertyDrawer(typeof(PlayerMovementRailConnection))]
public class PlayerMovementRailConnectionDrawer : PropertyDrawer
{
     int curveWidth = 50;
     float min = 0;
     float max = 1;

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        //PlayerMovementRailConnection railConnection = (PlayerMovementRailConnection)prop.objectReferenceValue;

    }
}

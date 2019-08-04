using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerMovementRailPoint))]
[CanEditMultipleObjects]
public class PlayerMovementRailPointEditor : Editor
{
    SerializedProperty railPointToJoinSer;
    SerializedProperty joinPointAreaSer;
    private void OnEnable()
    {
        railPointToJoinSer = serializedObject.FindProperty("railPointToJoin");
        joinPointAreaSer = serializedObject.FindProperty("joinPointArea");

    }


    public override void OnInspectorGUI()
    {
        PlayerMovementRailPoint t = (PlayerMovementRailPoint)target;
        DrawDefaultInspector();


        if (t.enableRailJoin)
        {
            railPointToJoinSer.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Join Point", "Rail point to join with."), railPointToJoinSer.objectReferenceValue, typeof(GameObject), true);

            joinPointAreaSer.vector3Value = EditorGUILayout.Vector3Field("Join Point Area", joinPointAreaSer.vector3Value);

            serializedObject.ApplyModifiedProperties();

        }




    }
}

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PMRailPoint))]
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
        PMRailPoint t = (PMRailPoint)target;
        DrawDefaultInspector();



        if (t.enableRailJoin)
        {
            railPointToJoinSer.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Join Point", "Rail point to join with."), railPointToJoinSer.objectReferenceValue, typeof(GameObject), true);

            joinPointAreaSer.vector3Value = EditorGUILayout.Vector3Field("Join Point Area", joinPointAreaSer.vector3Value);

            serializedObject.ApplyModifiedProperties();

        }

        if (GUILayout.Button("Generate Stop Collider"))
        {
            t.GenerateStopCollider();

        }

        if (GUILayout.Button("Remove Stop Collider"))
        {
            t.RemoveStopCollider();

        }

    }
}

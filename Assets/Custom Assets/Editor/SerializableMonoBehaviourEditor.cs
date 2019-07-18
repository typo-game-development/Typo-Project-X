using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SerializableMonoBehaviour), true)]
public class SerializableMonoBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SerializableMonoBehaviour t = (SerializableMonoBehaviour)target;
        GUILayout.Space(10);

        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if (GUILayout.Button("Import " + t.GetType().Name + " (" + t.FileExtension + ")"))
            {
                Debug.Log(EditorUtility.OpenFilePanelWithFilters("Select Events File", "", new string[] {t.FileExtensionName, t.FileExtension}));

            }

            if (GUILayout.Button("Export " + t.GetType().Name + " (" + t.FileExtension + ")"))
            {
                Debug.Log(EditorUtility.OpenFolderPanel("Select Export Folder", "", ""));

            }
        }
    }
}

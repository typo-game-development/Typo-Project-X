using System;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(PlayerMovementRailController))]
public class PlayerMovementRailControllerEditor : Editor
{
    [Flags]
    public enum EditorListOption
    {
        None = 0,
        ListSize = 1,
        ListLabel = 2,
        ElementLabels = 4,
        Buttons = 8,
        Default = ListSize | ListLabel | ElementLabels,
        NoElementLabels = ListSize | ListLabel,
        All = Default | Buttons
    }

    private static GUIStyle horizontalLine;
    private static GUILayoutOption[] miniButtonWidth = new GUILayoutOption[4] { GUILayout.MaxWidth(50), GUILayout.MaxHeight(30), GUILayout.MinWidth(50), GUILayout.MinHeight(30) };
    private static GUIStyle ToggleButtonStyleNormal = null;
    private static GUIStyle ToggleButtonStyleToggled = null;
    private static GUIContent
                    moveButtonContent = new GUIContent("\u21b4", "move down"),
                    duplicateButtonContent = new GUIContent("+", "duplicate"),
                    deleteButtonContent = new GUIContent("-", "delete"),
                    addButtonContent = new GUIContent("+", "add element");


    [InitializeOnLoadMethod]
    static void Init()
    {
        horizontalLine = new GUIStyle();
        horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
        horizontalLine.margin = new RectOffset(0, 0, 4, 4);
        horizontalLine.fixedHeight = 1;
    }

    public static PlayerMovementRailController targetScript;
    bool railFold = false;

    bool moveTargetOnPointToggle = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //Get currrent editor target
        targetScript = (PlayerMovementRailController)target;

        //Update toggled button style if null
        if (ToggleButtonStyleNormal == null)
        {
            ToggleButtonStyleNormal = "Button";
            ToggleButtonStyleToggled = new GUIStyle(ToggleButtonStyleNormal);
            ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;
        }




        if (!targetScript.isEditing)
        {
            ////Add rail
            //if (GUILayout.Button("Attach Rail"))
            //{
            //    //targetScript.AddRail();
            //}

            //Add rail
            if (GUILayout.Button("Add Rail"))
            {
                targetScript.AddRail();
            }

            ////Remove rail
            //if (GUILayout.Button("Remove Rail"))
            //{
            //    targetScript.RemoveRail();
            //}

            ////Remove rail
            //if (GUILayout.Button("Refresh"))
            //{
            //    targetScript.RefreshRails();
            //}



            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Available Rail Controllers", EditorStyles.boldLabel);

            railFold = EditorGUILayout.Foldout(railFold, (railFold ? "Collapse" : "Expand"), true);
            EditorGUI.indentLevel++;
            if (railFold)
            {
                serializedObject.Update();
                Show(serializedObject.FindProperty("rails"), ToggleButtonStyleToggled, ToggleButtonStyleNormal, EditorListOption.Buttons);
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.indentLevel--;


        }
        else
        {
            GUIStyle s = new GUIStyle(EditorStyles.boldLabel);
            s.normal.textColor = Color.red;

            EditorGUILayout.LabelField("Rail Edit Mode Active", s);

            serializedObject.Update();
            Show(serializedObject.FindProperty("rails"), ToggleButtonStyleToggled, ToggleButtonStyleNormal, EditorListOption.Buttons);
            serializedObject.ApplyModifiedProperties();

        }
        moveTargetOnPointToggle = EditorGUILayout.BeginToggleGroup("Move Target On Point", moveTargetOnPointToggle);

        EditorGUILayout.ObjectField("Target Object", (GameObject)null, typeof(GameObject), true);

        string[] railPointOptions = new string[0];
        int i = 0;

        foreach(PlayerMovementRailV1 rail in targetScript.rails)
        {
            if(rail.points != null && rail.points.Length > 0)
            {
                foreach (GameObject point in rail.points)
                {
                    System.Array.Resize(ref railPointOptions, railPointOptions.Length + 1);
                    railPointOptions[i] = "[" + i + "] " + rail.name + "_" + point.name;
                    i++;
                }
            }
        }

        selIndex = EditorGUILayout.Popup("",selIndex, railPointOptions);

        EditorGUILayout.EndToggleGroup();
    }
    public int selIndex = 0;

    public static void Show(SerializedProperty list, GUIStyle toggled, GUIStyle untoggled, EditorListOption options = EditorListOption.Default)
    {
        if (!list.isArray)
        {
            EditorGUILayout.HelpBox(list.name + " is neither an array nor a list!", MessageType.Error);
            return;
        }

        bool
            showListLabel = (options & EditorListOption.ListLabel) != 0,
            showListSize = (options & EditorListOption.ListSize) != 0;

        if (showListLabel)
        {
            EditorGUILayout.PropertyField(list);
            EditorGUI.indentLevel += 1;
        }
        if (!showListLabel || list.isExpanded)
        {
            SerializedProperty size = list.FindPropertyRelative("Array.size");
            if (showListSize)
            {
                EditorGUILayout.PropertyField(size);
            }
            if (size.hasMultipleDifferentValues)
            {
                EditorGUILayout.HelpBox("Not showing lists with different sizes.", MessageType.Info);
            }
            else
            {
                ShowElements(list, toggled, untoggled, options);
            }
        }
        if (showListLabel)
        {
            EditorGUI.indentLevel -= 1;
        }
    }

    private static void ShowElements(SerializedProperty list, GUIStyle toggled, GUIStyle untoggled, EditorListOption options)
    {
        bool
            showElementLabels = (options & EditorListOption.ElementLabels) != 0,
            showButtons = (options & EditorListOption.Buttons) != 0;

        for (int i = 0; i < list.arraySize; i++)
        {
            if(list.GetArrayElementAtIndex(i).objectReferenceValue != null)
            {
                PlayerMovementRailV1 rail = (PlayerMovementRailV1)list.GetArrayElementAtIndex(i).objectReferenceValue;

                if (targetScript.isEditing == true)
                {
                    if (rail.isEditing)
                    {
                        HorizontalLine(Color.red);
                        GUIStyle s = new GUIStyle(EditorStyles.boldLabel);
                        EditorGUILayout.LabelField("Rail Name: " + rail.name + "",s);

                        if (showButtons)
                        {
                            EditorGUILayout.BeginHorizontal();
                        }
                        if (showElementLabels)
                        {
                            EditorGUILayout.ObjectField("Rail Object", list.GetArrayElementAtIndex(i).objectReferenceValue, typeof(GameObject), true);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.ObjectField("Rail Object", list.GetArrayElementAtIndex(i).objectReferenceValue, typeof(GameObject), true);
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUI.BeginDisabledGroup(false);
                        rail.railSwitchColliderSize = EditorGUILayout.Vector3Field("Switch collider size:", rail.railSwitchColliderSize);
                        DrawFields(list.GetArrayElementAtIndex(i));
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.BeginHorizontal();
                        if (showButtons)
                        {
                            ShowButtons(list, toggled, untoggled, i);
                            EditorGUILayout.EndHorizontal();
                            GUILayout.Space(10f);
                        }
                        HorizontalLine(Color.red);
                    }
                }
                else
                {
                    HorizontalLine(Color.grey);
                    GUIStyle s = new GUIStyle(EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Rail Name: " + rail.name + "",s);
                    rail.editorCollapsed = EditorGUILayout.Foldout(rail.editorCollapsed, "Parameters", true);


                    if(rail.editorCollapsed)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        if (showButtons)
                        {
                            EditorGUILayout.BeginHorizontal();
                        }
                        if (showElementLabels)
                        {
                            EditorGUILayout.ObjectField("Rail Object", list.GetArrayElementAtIndex(i).objectReferenceValue, typeof(GameObject), true);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.ObjectField("Rail Object", list.GetArrayElementAtIndex(i).objectReferenceValue, typeof(GameObject), true);
                            EditorGUILayout.EndHorizontal();
                        }
                        DrawFields(list.GetArrayElementAtIndex(i));

                        EditorGUI.EndDisabledGroup();
                    }
                    
                    EditorGUILayout.BeginHorizontal();
                    if (showButtons)
                    {
                        ShowButtons(list, toggled, untoggled, i);
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(10f);
                    }
                }

            }
        }
        //if (showButtons && list.arraySize == 0 && GUILayout.Button(addButtonContent, EditorStyles.miniButton))
        //{
        //    list.arraySize += 1;
        //}
    }

    static void DrawFields(SerializedProperty rail)
    {
        PlayerMovementRailV1 r = (PlayerMovementRailV1)rail.objectReferenceValue;

        EditorGUILayout.Vector3Field("Switch collider size:", r.railSwitchColliderSize);

        if(r.connections != null && r.connections.Count > 0)
        {
            EditorGUI.indentLevel++;

            r.editorConnectionsCollapsed = EditorGUILayout.Foldout(r.editorConnectionsCollapsed, "Rail Connections", true);

            if(r.editorConnectionsCollapsed)
            {
                int i = 0;

                HorizontalLine(Color.grey);

                foreach (PlayerMovementRailConnection conn in r.connections.ToArray())
                {
                    if (conn != null)
                    {
                        GUIStyle s = new GUIStyle(EditorStyles.boldLabel);
                        GUIStyle b = new GUIStyle(EditorStyles.miniButton);
                        b.normal.textColor = Color.red;

                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField(conn.name,s);
                        if(targetScript.isEditing)
                        {
                            if (GUILayout.Button("Remove", b, new GUILayoutOption[4] { GUILayout.MaxWidth(50), GUILayout.MaxHeight(20), GUILayout.MinWidth(50), GUILayout.MinHeight(20) }))
                            {
                                r.RemoveConnection(i);
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                        conn.name = EditorGUILayout.TextField("Name:", conn.name);

                        string[] railPointOptions = new string[0];
                        int i2 = 0;

                        foreach (PlayerMovementRailV1 rail1 in targetScript.rails)
                        {
                            if (rail1.points != null && rail1.points.Length > 0)
                            {
                                foreach (GameObject point in rail1.points)
                                {
                                    System.Array.Resize(ref railPointOptions, railPointOptions.Length + 1);
                                    railPointOptions[i2] = "[" + i2 + "] " + rail1.name + "_" + point.name;
                                    i2++;
                                }
                            }
                        }
                        conn.point1Options = railPointOptions;
                        conn.point2Options = railPointOptions;

                        conn.point1SelectedOptionIndex = EditorGUILayout.Popup("Point 1", conn.point1SelectedOptionIndex, conn.point1Options);
                        conn.point2SelectedOptionIndex = EditorGUILayout.Popup("Point 2", conn.point2SelectedOptionIndex, conn.point2Options);

                        conn.direction = (PlayerMovementRailConnection.eConnectionDirection)EditorGUILayout.EnumPopup("Direction", conn.direction);

                        HorizontalLine(Color.grey);

                    }
                    i++;
                }

            }
            EditorGUI.indentLevel--;

        }
        else
        {
            EditorGUILayout.HelpBox("No connections available for this rail."+ Environment.NewLine + "To add new connections enter edit mode and click on 'Add Connections' button.", MessageType.Warning);
        }
    }

    // utility method
    static void HorizontalLine(Color color)
    {
        var c = GUI.color;
        GUI.color = color;
        GUILayout.Box(GUIContent.none, horizontalLine);
        GUI.color = c;
    }

    private static void ShowButtons(SerializedProperty list, GUIStyle toggled, GUIStyle untoggled, int index)
    {
        PlayerMovementRailV1 rail = (PlayerMovementRailV1)list.GetArrayElementAtIndex(index).objectReferenceValue;

        if(list.GetArrayElementAtIndex(index).objectReferenceValue != null)
        {
            if (GUILayout.Button((rail.editorSelected ? "Exit" : "Edit"), (rail.editorSelected ? toggled : untoggled), miniButtonWidth))
            {
                if (!rail.editorSelected && EditorUtility.DisplayDialog("Edit START", 
                    "Do you want to edit rail '" + rail.name + "'?" + Environment.NewLine + Environment.NewLine +
                    "This will cancel every ongoing editing operations for other rails." + Environment.NewLine + Environment.NewLine + "The operation can't be reversed.", 
                    "Yes", "No"))
                {
                    for (int i = 0; i < list.arraySize; i++)
                    {
                        PlayerMovementRailV1 temprail = (PlayerMovementRailV1)list.GetArrayElementAtIndex(i).objectReferenceValue;
                        temprail.editorSelected = false;
                    }
                    rail.editorSelected = true;
                    rail.railGizmoColor = Color.red;

                    rail.isEditing = true;
                    targetScript.isEditing = true;

                }
                else if (rail.editorSelected && EditorUtility.DisplayDialog("Edit END", "Do you want to save and exit edit mode?", "Yes", "No"))
                {
                    rail.editorSelected = false;
                    rail.railGizmoColor = Color.green;

                    rail.isEditing = false;
                    targetScript.isEditing = false;

                }
            }
            GUIStyle s = new GUIStyle(GUI.skin.button);
            s.normal.textColor = Color.red;

            if(!rail.isEditing)
            {
                if (GUILayout.Button("Delete", s, miniButtonWidth))
                {
                    if (EditorUtility.DisplayDialog("Rail Remove", "Do you want to remove rail '" + rail.name + "'?" + Environment.NewLine + "The operation can't be reversed.", "Yes", "No"))
                    {
                        targetScript.RemoveRail(rail);
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Add Connection", new GUILayoutOption[4] { GUILayout.MaxWidth(100), GUILayout.MaxHeight(30), GUILayout.MinWidth(100), GUILayout.MinHeight(30) }))
                {
                    //if (EditorUtility.DisplayDialog("Add Connection", "Do you want to add a new connection?", "Yes", "No"))
                    {
                        rail.AddConnection();
                    }
                }
            }

            if (UnityEngine.Event.current.type == EventType.KeyDown)
            {
                if (UnityEngine.Event.current.keyCode == KeyCode.Escape)
                {
                    rail.editorSelected = false;
                }
            }
        }
    }
}

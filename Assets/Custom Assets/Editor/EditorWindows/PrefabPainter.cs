using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PrefabPainter
{
    public class PrefabPainter : EditorWindow
    {
        Vector2 scrollPos;
        Rect windowBounds = new Rect(0, 0, 0, 0);
        public bool showDropArea = false;

        // Layout Processing
        Event currentEvent;

        // Palettes
        private Texture2D deleteIcon;
        private Texture2D saveIcon;

        [SerializeField]
        private PaintPalette activePalette;
        private List<PaintPalette> palettes = new List<PaintPalette>();
        private bool creatingNewPalette = false;
        public float SizeInterval = 0.10f;
        public bool displayOverrideWarning = true;

        // Prefab List
        PaintObject selectedPrefab;
        float prefabListHeight;
        private Texture2D blueTexture;
        private Texture2D greyTexture;
        private bool displayPrefabSettings;

        // Brush Gizmo
        private Color activeOuterColor = new Color(0.15f, 0.75f, 1f);
        private Color passiveOuterColor = Color.blue;
        private Color innerColor = new Color(0.15f, 0.75f, 1f, 0.1f);

        // Mouse Information
        private Vector3 currentMousePosition = Vector3.zero;
        private Vector3 previousMousePosition = Vector3.zero;
        private RaycastHit mouseHitPoint;
        private const string MouseLocationName = "Mouse Location";
        private Transform MouseLocation;

        // Brush Settings
        public bool displayDebugInfo = true;
        public float brushSize = 4f;
        public int brushDensity = 2;
        public LayerMask paintMask; 
        public float maxYPosition = 400;

        // Paint Objects
        private GameObject paintGroup;
        private string paintGroupName = "[PPBatch]";
        public int listSize = 0;
        public List<PaintObject> paintObjects;
        private bool isPainting;
        private List<string> layerNames;

        private int selectedTab = 0;


        //Window properties
        private static int width = 440;
        private static int height = 600;
        static PrefabPainter window;

        [MenuItem("Tools/Prefab Painter")]
        static void Init()
        {
            window = (PrefabPainter)GetWindow(typeof(PrefabPainter));
            window.titleContent = new GUIContent("Prefab Painter");

            window.autoRepaintOnSceneChange = true;
            
            //Fixed size
            window.maxSize = new Vector2(width, height);
            window.minSize = new Vector2(250, 300);

            window.Show();
            window.Focus();
            window.Repaint();
        }

        void OnEnable()
        {
            listSize = 0;

            Camera.onPreCull -= DrawPreviewMeshWithCamera;
            Camera.onPreCull += DrawPreviewMeshWithCamera;

            SceneView.duringSceneGui += SceneGUI;
            EditorApplication.hierarchyChanged += HierarchyChanged;
            if (paintObjects == null) paintObjects = new List<PaintObject>();

            layerNames = new List<string>();

            for (int i = 0; i <= 31; i++)
            {
                if(LayerMask.LayerToName(i) != "")
                {
                    layerNames.Add(LayerMask.LayerToName(i));
                }
            }

            deleteIcon = EditorGUIUtility.Load("icons/d_TreeEditor.Trash.png") as Texture2D;
            saveIcon = EditorGUIUtility.Load("icons/SaveActive.png") as Texture2D;

            blueTexture = new Texture2D(64, 64);
            greyTexture = new Texture2D(64, 64);

            for (int y = 0; y < blueTexture.height; y++)
            {
                for (int x = 0; x < blueTexture.width; x++)
                {
                    blueTexture.SetPixel(x, y, new Color(0.25f, 0.42f, 0.66f));
                    greyTexture.SetPixel(x, y, new Color(0.9f, 0.9f, 0.9f));
                }
            }
            blueTexture.Apply();
            greyTexture.Apply();

            LoadPalette(activePalette);
        }

        void OnDisable()
        {
            Camera.onPreCull -= DrawPreviewMeshWithCamera;
            SceneView.duringSceneGui -= SceneGUI;
            EditorApplication.hierarchyChanged -= HierarchyChanged;
            if (MouseLocation) DestroyImmediate(MouseLocation.gameObject);
            if (GameObject.Find(MouseLocationName) != null)
                DestroyImmediate(GameObject.Find(MouseLocationName));
        }

        void SceneGUI(SceneView sceneView)
        {
            windowBounds.width = Screen.width;
            windowBounds.height = Screen.height;

            currentEvent = Event.current;
            UpdateMousePosition(sceneView);

            DrawBrushGizmo();
            SceneInput();
       

            if (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag)
                SceneView.RepaintAll();
        }

        static GUIStyle _foldoutStyle;

        static GUIStyle FoldoutStyle
        {
            get
            {
                if (_foldoutStyle == null)
                {
                    _foldoutStyle = new GUIStyle(EditorStyles.foldout)
                    {
                        font = EditorStyles.boldFont
                    };
                }

                return _foldoutStyle;
            }
        }

        static GUIStyle _boxStyle;

        public static GUIStyle BoxStyle
        {
            get
            {
                if (_boxStyle == null)
                {
                    _boxStyle = new GUIStyle(EditorStyles.helpBox);
                }

                return _boxStyle;
            }
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Load", "ToolbarButton"))
            {
                string path = EditorUtility.OpenFilePanel("Select Palette", "", "asset");
                path = path.Replace(Application.dataPath, "Assets");

                if (path.Length != 0)
                {
                    activePalette = (PaintPalette)AssetDatabase.LoadAssetAtPath(path, typeof(PaintPalette));
                    LoadPalette(activePalette);
                    if (!palettes.Contains(activePalette))
                    {
                        palettes.Add(activePalette);
                    }
                    Debug.Log("<color=cyan>[Prefab Painter] </color>Palette loaded.");
                }
            }

            if (activePalette != null)
            {
                GUILayout.Label(new GUIContent("Active: " + activePalette.name), "ToolbarButton");
            }
            else
            {
                GUILayout.Label(new GUIContent("Active: none"), "ToolbarButton");
            }
            GUILayout.Space(5f);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent("", saveIcon, "Save active prefabs as palette."), "ToolbarButton"))
            {
                if (activePalette != null && palettes.Contains(activePalette))
                {
                    if (displayOverrideWarning)
                    {
                        switch (EditorUtility.DisplayDialogComplex("Override palette", "Saving the current palette will override the currently active palette which is '" + activePalette.name + "'.", "Okay", "Cancel", "Okay and do not show again"))
                        {
                            case 0:
                                OverridePalette(activePalette);
                                break;

                            case 1:
                                break;

                            case 2:
                                OverridePalette(activePalette);
                                displayOverrideWarning = false;
                                break;

                            default:
                                break;
                        }
                    }
                    else
                    {
                        OverridePalette(activePalette);
                    }

                }
                else
                {
                    CreateNewPalette();
                }
            }
            if (GUILayout.Button(new GUIContent("", deleteIcon, "Remove currently loaded palette."), "ToolbarButton"))
            {
                Debug.Log("Remove Palette");
            }

            if (GUILayout.Button(new GUIContent("Palettes", "Load in a palette."), "ToolbarPopup"))
            {
                GenericMenu menu = new GenericMenu();
                if (palettes.Count > 0)
                {
                    for (int i = 0; i < palettes.Count; i++)
                    {
                        if (palettes[i] != null) AddMenuItemForPalette(menu, palettes[i].name, palettes[i]);
                    }
                }
                menu.AddItem(new GUIContent("New Palette"), creatingNewPalette, OnNewPaletteSelected);
                menu.AddItem(new GUIContent("Clear List"), false, OnClearList);
                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();
            selectedTab = GUILayout.Toolbar(selectedTab, new string[] { "Painting", "Palettes", "Settings" });

            switch (selectedTab)
            {
                case 0:
                    DrawPaintingTab();
                    break;

                default:
                    break;
            }
        }

        void DrawPaintingTab()
        {
            EditorGUILayout.Space();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

            float tempSize = brushSize;
            int tempDensity = brushDensity;

            Rect r = EditorGUILayout.GetControlRect();
            r.height = 100;

            EditorGUILayout.BeginVertical(PrefabPainter.BoxStyle);

            GUILayout.Space(r.height);
            r.height = 120;

            GUI.Box(r, string.Empty);

            r.width -= 20;
            r.x += 10;
            GUI.DrawTexture(r, (Texture)Resources.Load("Icons/PrefabPlacerLogo_Light_v3"), ScaleMode.ScaleToFit);

            GUILayout.Label("Brush Settings", EditorStyles.boldLabel);
            paintMask = EditorGUILayout.MaskField(new GUIContent("Paint Layer", "On which layer the tool will paint."), paintMask, layerNames.ToArray());
            brushSize = EditorGUILayout.FloatField("Brush Size", brushSize);
            brushDensity = EditorGUILayout.IntField("Brush Density", brushDensity);
            paintGroupName = EditorGUILayout.TextField("Paint Group Name", paintGroupName);

            GUILayout.Space(3);
            EditorGUILayout.EndVertical();
            GUILayout.Space(0);

            listSize = Mathf.Max(0, listSize);

            if (Event.current.type == EventType.Layout && selectedPrefab != null)
            {
                displayPrefabSettings = true;
            }

            if (displayPrefabSettings)
            {
                selectedPrefab.DisplaySettings();
            }

            DisplayPrefabs(paintObjects);

            EditorGUILayout.Space();
            GUILayout.Space(100f + prefabListHeight);
            CheckForChanges(tempSize, tempDensity);
            EditorGUILayout.EndScrollView();
        }

        #region Palette

        void OverridePalette(PaintPalette palette)
        {
            palette.palette = paintObjects;
        }

        void LoadPalette(PaintPalette palette)
        {
            listSize = palette.palette.Count;
            paintObjects = palette.palette;
            bool missingPrefabs = false;

            for (int i = 0; i < listSize; i++)
            {
                if (paintObjects[i].GetGameObject() == null) missingPrefabs = true;
            }

            if (missingPrefabs) Debug.Log("<color=cyan>[Prefab Painter] </color>One or more prefabs could not be loaded.");
        }

        void LoadEmptyPalette()
        {
            listSize = 0;
            paintObjects = new List<PaintObject>();
        }

        void CreateNewPalette()
        {
            string relativePath = "";
            string absolutePath = EditorUtility.SaveFilePanel("Save Palette", "Assets/", "New Palette", "asset");

            if (absolutePath.StartsWith(Application.dataPath))
            {
                relativePath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
            }

            if (relativePath.Length != 0)
            {
                PaintPalette asset = ScriptableObject.CreateInstance<PaintPalette>();
                asset.palette = paintObjects;
                AssetDatabase.CreateAsset(asset, relativePath);
                AssetDatabase.SaveAssets();
                palettes.Add(asset);
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
                activePalette = asset;
                creatingNewPalette = false;

                Debug.Log("<color=cyan>[Prefab Painter] </color>Palette saved.");
            }

            else Debug.Log("<color=cyan>[Prefab Painter] </color>Selected path is invalid.");
        }

        #endregion

        void AddMenuItemForPalette(GenericMenu menu, string menuPath, PaintPalette palette)
        {
            bool paletteSelected;
            if (activePalette == null) paletteSelected = false;
            else
            {
                if (activePalette.Equals(palette)) paletteSelected = true;
                else paletteSelected = false;
            }

            if (creatingNewPalette) paletteSelected = false;
            menu.AddItem(new GUIContent(menuPath), paletteSelected, OnPaletteSelected, palette);
        }

        void OnPaletteSelected(object palette)
        {
            LoadPalette((PaintPalette)palette);
            activePalette = (PaintPalette)palette;
            creatingNewPalette = false;
        }

        void OnClearList()
        {
            palettes.Clear();
        }

        void OnNewPaletteSelected()
        {
            creatingNewPalette = true;
            activePalette = null;
            LoadEmptyPalette();
        }

        void CheckForChanges(float tempSize, int tempDensity)
        {
            if (tempSize != brushSize)
            {
                brushSize = Mathf.Max(brushSize, 1);
                SceneView.RepaintAll();
            }

            else if (brushDensity != tempDensity)
            {
                brushDensity = Mathf.Clamp(brushDensity, 1, 100);
                SceneView.RepaintAll();
            }

            else if (paintObjects != null && listSize != paintObjects.Count)
            {
                List<PaintObject> tempObj = new List<PaintObject>(listSize);
                for (int i = 0; i < listSize; i++)
                {
                    if (paintObjects.Count > i) tempObj.Add(paintObjects[i]);
                    else tempObj.Add(new PaintObject(null));
                }

                paintObjects = new List<PaintObject>(tempObj);
            }
        }

        void DrawBrushGizmo()
        {
            if (isPainting) Handles.color = activeOuterColor;
            else Handles.color = passiveOuterColor;

            if (mouseHitPoint.transform)
            {
                innerColor.a = brushDensity * 0.01f;

                if (GameObject.Find(MouseLocationName) == null)
                {
                    MouseLocation = new GameObject(MouseLocationName).transform;

                }
                else
                {
                    MouseLocation = GameObject.Find(MouseLocationName).transform;
                }

                MouseLocation.rotation = mouseHitPoint.transform.rotation;
                MouseLocation.forward = mouseHitPoint.normal;

                Handles.CircleHandleCap(2, currentMousePosition, MouseLocation.rotation, brushSize, EventType.Repaint); 
                Handles.color = innerColor;
                Handles.DrawSolidDisc(currentMousePosition, mouseHitPoint.normal, brushSize);
                MouseLocation.up = mouseHitPoint.normal;
            }

            Handles.BeginGUI();
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;

            GUILayout.BeginArea(new Rect(currentEvent.mousePosition.x + 10, currentEvent.mousePosition.y + 10, 250, 100));

            if (displayDebugInfo)
            {
                if(selectedPrefab != null && paintObjects.IndexOf(selectedPrefab) > -1)
                {
                    GUILayout.TextField("Selected Prefab: " + selectedPrefab.GetName(), style);
                }
                else
                {
                    GUILayout.TextField("Prefab Name: NONE", style);
                }
                GUILayout.TextField("Size: " + System.Math.Round(brushSize, 2), style);
                GUILayout.TextField("Density: " + System.Math.Round((double)brushDensity, 2), style);
                GUILayout.TextField("Surface Name: " + (mouseHitPoint.collider ? mouseHitPoint.collider.name : "none"), style);
            }



            GUILayout.EndArea();
            Handles.EndGUI();
        }

        private void DrawPreviewMeshWithCamera(Camera camera)
        {
            if (camera)
            {
                DrawPreviewMesh(camera);
            }
        }

        private void DrawPreviewMesh(Camera camera)
        {
            if (selectedPrefab != null && paintObjects.IndexOf(selectedPrefab) > -1)
            {
                GameObject selectedPrefabObject = selectedPrefab.GetGameObject();

                if(selectedPrefabObject != null)
                {
                    MeshRenderer meshRenderer = selectedPrefabObject.GetComponentInChildren<MeshRenderer>(true);
                    MeshFilter meshFilter = selectedPrefabObject.GetComponentInChildren<MeshFilter>(true);
                    Mesh mesh = meshFilter.sharedMesh;
                    Material[] mats = meshRenderer.sharedMaterials;

                    Vector3 position = mouseHitPoint.point;
                    Vector3 scale = new Vector3(selectedPrefab.GetSize().y, selectedPrefab.GetSize().y, selectedPrefab.GetSize().y);
                    Quaternion rotation = Quaternion.FromToRotation(Vector3.up, mouseHitPoint.normal);

                    Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);

                    for (int i = meshRenderer.subMeshStartIndex; i < mesh.subMeshCount; i++)
                    {
                        Graphics.DrawMesh(mesh, matrix, mats[i], selectedPrefab.GetGameObject().layer, camera, i, null, meshRenderer.shadowCastingMode);
                    }
                }
                else
                {
                    Debug.Log("[Prefab Painter] Selected prefab GameObject is missing.");
                }
            }
        }

        void UpdateMousePosition(SceneView sceneView)
        {
            RaycastHit hit;

            if (currentEvent.control)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
  
            Vector3 mousePos = currentEvent.mousePosition;
            float ppp = EditorGUIUtility.pixelsPerPoint;
            mousePos.y = sceneView.camera.pixelHeight - mousePos.y * ppp;
            mousePos.x *= ppp;

            Ray ray = sceneView.camera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out hit, 1000, paintMask))
            {
                currentMousePosition = hit.point;
                mouseHitPoint = hit;
            }
            else
            {
                mouseHitPoint = new RaycastHit();
            }
        }

        public bool PreventCustomUserHotkey(EventType type, EventModifiers codeModifier, KeyCode hotkey)
        {
            Event currentevent = Event.current;

            if (currentevent.type == type && currentevent.modifiers == codeModifier && currentevent.keyCode == hotkey)
            {
                currentevent.Use();
                return true;
            }
            return false;
        }

        void SceneInput()
        {
            if (PreventCustomUserHotkey(EventType.ScrollWheel, EventModifiers.Control, KeyCode.None))
            {
                if (currentEvent.delta.y > 0)
                {
                    brushSize = brushSize + SizeInterval;
                }
                else
                {
                    brushSize = brushSize - SizeInterval;
                    brushSize = Mathf.Max(SizeInterval, brushSize);
                }
                Repaint();

            }

            else if (PreventCustomUserHotkey(EventType.ScrollWheel, EventModifiers.Alt, KeyCode.None))
            {
                if (currentEvent.delta.y > 0)
                {
                    brushDensity++;
                }
                else
                {
                    brushDensity--;
                }
                brushDensity = Mathf.Clamp(brushDensity, 1, 100);
                Repaint();
            }

            else if (currentEvent.control && (currentEvent.button == 0 && currentEvent.type == EventType.MouseDown))
            {
                isPainting = true;
                Painting();
            }

            else if (isPainting && !currentEvent.control || (currentEvent.button != 0 || currentEvent.type == EventType.MouseUp))
            {
                previousMousePosition = Vector3.zero;
                isPainting = false;
            }

            else if (isPainting && (currentEvent.type == EventType.MouseDrag))
            {
                Painting();
            }
        }

        void Painting()
        {
            if (paintObjects != null && paintObjects.Count > 0)
            {
                if (Vector3.Distance(previousMousePosition, currentMousePosition) > brushSize)
                {
                    previousMousePosition = currentMousePosition;
                    DrawPaint();
                }
            }
            else Debug.LogWarning("[Prefab Painter] Prefab list is empty!");
        }

        void DrawPaint()
        {
            if (paintGroup == null)
            {
                if (GameObject.Find(paintGroupName)) paintGroup = GameObject.Find(paintGroupName);
                else paintGroup = new GameObject(paintGroupName);
            }

            int localDensity = brushDensity;
            Vector3 dir = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up) * Vector3.right;
            Vector3[] spawnPoint = new Vector3[localDensity];

            for (int i = 0; i < localDensity; i++)
            {
                dir = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.up) * Vector3.right;
                Vector3 spawnPos = (dir * brushSize * Random.Range(0.1f, 1.1f)) + currentMousePosition;

                if (spawnPos != Vector3.zero)
                {
                    spawnPoint[i] = spawnPos;
                    PrefabSpawn(spawnPoint[i]);
                }
            }
        }

        //public void PrefabInstantiate(int index)
        //{
        //    RaycastHit hit;
        //    GameObject instanceOf = PrefabUtility.InstantiatePrefab(prefab.GetArrayElementAtIndex(index).objectReferenceValue) as GameObject;

        //    Vector3 radiusAdjust = Random.insideUnitSphere * radius.floatValue / 2;
        //    float prefabSize = size.floatValue;

        //    if (hideInHierarchy.boolValue)
        //        instanceOf.hideFlags = HideFlags.HideInHierarchy;

        //    instanceOf.transform.localScale = new Vector3(prefabSize, prefabSize, prefabSize);
        //    instanceOf.transform.position = new Vector3(mousePos.x, mousePos.y, mousePos.z);
        //    instanceOf.transform.rotation = new Quaternion(0, 0, 0, 0);

        //    if (canAling.boolValue)
        //        instanceOf.transform.rotation = mouseRot;
        //    else
        //        instanceOf.transform.rotation = new Quaternion(0, 0, 0, 0);


        //    instanceOf.transform.Translate(radiusAdjust.x, positionOffset.floatValue, radiusAdjust.y);

        //    if (Physics.Raycast(instanceOf.transform.position, -instanceOf.transform.up, out hit))
        //    {
        //        if (!canPlaceOver.boolValue && hit.collider.tag == instanceOf.tag)
        //        {
        //            DestroyImmediate(instanceOf);
        //            return;
        //        }

        //        instanceOf.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
        //        instanceOf.transform.parent = hit.collider.gameObject.transform;

        //        if (canAling.boolValue)
        //            instanceOf.transform.up = hit.normal;
        //    }
        //    else
        //    {
        //        DestroyImmediate(instanceOf);
        //        return;
        //    }

        //    if (isRandomR.boolValue)
        //        instanceOf.transform.Rotate(0, Random.Range(0, 180) * 45, 0, Space.Self);

        //    Undo.RegisterCreatedObjectUndo(instanceOf, "Instantiate");
        //}

        private void PrefabRemove()
        {
            GameObject[] prefabsInRadius;

            prefabsInRadius = GameObject.FindGameObjectsWithTag("Untagged");

            foreach (GameObject p in prefabsInRadius)
            {
                float dist = Vector3.Distance(currentMousePosition, p.transform.position);

                if (dist <= brushSize)
                    if (p != null)
                        Undo.DestroyObjectImmediate(p);
            }
        }

        GameObject PrefabSpawn(Vector3 pos)
        {
            int rndIndex = paintObjects.IndexOf(selectedPrefab);

            if(paintObjects.Count >= rndIndex && rndIndex > -1)
            {
                GameObject prefabObj = paintObjects[rndIndex].GetGameObject();
                GameObject go = null;

                if (prefabObj != null)
                {
                    go = (GameObject)PrefabUtility.InstantiatePrefab(prefabObj);
                    Undo.RegisterCreatedObjectUndo(go, "Prefab Paint");

                    if (MouseLocation)
                    {
                        go.transform.rotation = MouseLocation.rotation;
                        go.transform.up = MouseLocation.up;
                    }

                    else
                    {
                        go.transform.rotation = Quaternion.identity;
                    }

                    bool randomRotationX = paintObjects[rndIndex].GetRandomRotationX();
                    bool randomRotationY = paintObjects[rndIndex].GetRandomRotationY();
                    bool randomRotationZ = paintObjects[rndIndex].GetRandomRotationZ();

                    if (randomRotationX) go.transform.Rotate(Vector3.right, Random.Range(0, 360));
                    if (randomRotationY) go.transform.Rotate(Vector3.up, Random.Range(0, 360));
                    if (randomRotationZ) go.transform.Rotate(Vector3.forward, Random.Range(0, 360));

                    Vector2 scale = paintObjects[rndIndex].GetSize();
                    if (scale != Vector2.one && scale != Vector2.zero)
                    {
                        go.transform.localScale *= Random.Range(scale.x, scale.y);
                    }

                    go.transform.position = pos;
                    //DoubleRayCast(go, rndIndex);
                    if (go != null)
                    {
                        AddObjectToGroup(go, rndIndex);
                    }
                }
                return go;
            }
            else
            {
                return null;
            }
        }

        void AddObjectToGroup(GameObject obj, int index)
        {
            Transform parent = GameObject.Find(paintGroupName).transform;
            if (parent == null) parent = new GameObject(paintGroupName).transform;
            obj.transform.SetParent(parent);
        }

        public bool LayerContain(LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }

        void DoubleRayCast(GameObject obj, int index)
        {
            Vector3 position = obj.transform.position + obj.transform.up * maxYPosition;
            obj.transform.position = position;
            obj.SetActive(false);
            RaycastHit groundHit;

            if (Physics.Raycast(position, -obj.transform.up, out groundHit, Mathf.Infinity, paintMask))
            {
                RaycastHit objectHit;
                if (LayerContain(paintMask, groundHit.collider.gameObject.layer))
                {
                    obj.SetActive(true);

                    int objMask = 1 << obj.layer;

                    if (Physics.Raycast(groundHit.point, obj.transform.up, out objectHit, Mathf.Infinity, objMask))
                    {
                        Vector3 newPos;
                        float differencialDistance = Vector3.Distance(objectHit.point, obj.transform.position);
                        newPos = groundHit.point + (obj.transform.up * differencialDistance);
                        obj.transform.position = newPos;
                        return;
                    }
                }
            }
            DestroyImmediate(obj);
        }

        void Update()
        {
            SceneView.RepaintAll();
        }

        private void HierarchyChanged()
        {
            Repaint();
        }

        public static bool BeginFold(string foldName, bool foldState)
        {
            EditorGUILayout.BeginVertical(BoxStyle);
            GUILayout.Space(3);
            foldState = EditorGUI.Foldout(EditorGUILayout.GetControlRect(),
                foldState, foldName, true, FoldoutStyle);
            if (foldState) GUILayout.Space(3);
            return foldState;
        }

        public static void EndVertical()
        {

        }

        public void DisplayPrefabs(List<PaintObject> prefabs)
        {
            int numberOfPrefabs = prefabs.Count;
            int windowWidth = (int)EditorGUIUtility.currentViewWidth;
            int prefabBoxSize = 100;
            int y;

            if (selectedPrefab != null)
            {
                y = 215;
            }
            else
            {
                y = 110;
            }
            
            //Skip logo
            y += 120;

            int c = Mathf.FloorToInt(windowWidth / (prefabBoxSize + 20) + 1);

            Rect r2 = EditorGUILayout.GetControlRect();

            int columns = Mathf.FloorToInt(windowWidth / (prefabBoxSize + 38) + 1);
            int rows = Mathf.FloorToInt(numberOfPrefabs / columns);
            prefabListHeight = rows * prefabBoxSize;

            for (int i = 0; i < numberOfPrefabs; i++)
            {
                var e = Event.current;
                GameObject go = prefabs[i].GetGameObject();

                columns = Mathf.FloorToInt(windowWidth / (prefabBoxSize + 38) + 1);
                rows = Mathf.FloorToInt(numberOfPrefabs / columns);
                prefabListHeight = rows * prefabBoxSize;

                int posX = 5 + prefabBoxSize * (i - (Mathf.FloorToInt(i / columns)) * columns);
                int posY = y + prefabBoxSize * Mathf.FloorToInt(i / columns);

                Rect r = new Rect(posX, posY, prefabBoxSize, prefabBoxSize);
                Rect border = new Rect(r.x + 2, r.y + 6, r.width - 4, r.height - 4);

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    if (r.Contains(Event.current.mousePosition))
                    {
                        selectedPrefab = prefabs[i];
                        Repaint();
                    }
                }

                GUIStyle style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter};

                if (prefabs[i] == selectedPrefab && selectedPrefab != null)
                {
                    EditorGUI.DrawPreviewTexture(border, blueTexture, null, ScaleMode.ScaleToFit, 0f);
                }
                else
                {
                    EditorGUI.DrawPreviewTexture(border, greyTexture, null, ScaleMode.ScaleToFit, 0f);

                }
                EditorGUI.LabelField(border, "No Preview", style);

                border.x += 2;
                border.y += 2;
                border.width -= 4;
                border.height -= 4;

                Texture2D preview = AssetPreview.GetAssetPreview(go);

                if (preview != null)
                {
                    EditorGUI.DrawPreviewTexture(border, preview, null, ScaleMode.ScaleToFit, 0f);
                }

                float closeButtonSize = 20;

                GUIStyle style1 = new GUIStyle(GUI.skin.button);
                style1.normal.textColor = Color.red;
                style1.alignment = TextAnchor.MiddleCenter;
                style1.contentOffset = new Vector2(0.5f, 0);

                border.x = r.x + (border.width - (closeButtonSize - 4));
                border.width = closeButtonSize;
                border.height = closeButtonSize;

                if (GUI.Button(border, "X", style1))
                {
                    prefabs.Remove(prefabs[i]);
                    LoadPalette(activePalette);
                }           
            }

            r2.y = y;
            r2.height = prefabListHeight + prefabBoxSize + 7;

            //Handle and show drag and drop area
            HandleDragAndDropArea(r2);
        }

        public void HandleDragAndDropArea(Rect r)
        {
            Event e = Event.current;
            Rect dropArea = r;

            if(showDropArea)
            {
                GUI.Box(dropArea, string.Empty, EditorStyles.helpBox);                
                GUI.Label(dropArea, "DROP IT LIKE IT'S HOT!", EditorStyles.centeredGreyMiniLabel);
            }

            switch (e.type)
            {
                case EventType.DragUpdated:

                case EventType.DragPerform:
                    showDropArea = true;

                    if (!dropArea.Contains(e.mousePosition))
                    {
                        return;
                    }
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;

                    if (e.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        showDropArea = false;

                        foreach (Object dragged_object in DragAndDrop.objectReferences)
                        {
                            if (dragged_object is GameObject)
                            {
                                GameObject go = (GameObject)dragged_object;
                                PaintObject po = new PaintObject(go);
                                po.SetName(dragged_object.name);
                                paintObjects.Add(po);
                                listSize++;
                            }
                        }
                    }
                    break;

                case EventType.DragExited:
                    showDropArea = false;
                    break;
            }
        }
    }
}
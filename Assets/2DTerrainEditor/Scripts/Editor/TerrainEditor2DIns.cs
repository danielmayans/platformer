using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditorInternal;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainEditor2D))]
public class TerrainEditor2DIns: Editor
{
    private TerrainEditor2D _myTerrainEditor2D;

    public float BrushSize = 5;
    public float BrushHardness = 0.25f;
    public float BrushNoise = 0;

    private bool _showRndSettings;
    private bool _showCapSettings;
    private bool _showAddSettings;
    private bool _showSettings = true;

    private bool _editMode;
    private bool _startEdit;
    private bool _digMode;
    private bool _brushSizeMode;

    private bool _playMode;

    private Tool _previousUsingTool;

    private List<RecordedTerrainVerts> _recordedTerrainVerts = new List<RecordedTerrainVerts>();
    
    void OnEnable()
    {
        if (EditorPrefs.HasKey("2DTE_BrushSize"))
            BrushSize = float.Parse(EditorPrefs.GetString("2DTE_BrushSize"));
        if (EditorPrefs.HasKey("2DTE_BrushHardness"))
            BrushHardness = float.Parse(EditorPrefs.GetString("2DTE_BrushHardness"));
        if (EditorPrefs.HasKey("2DTE_BrushNoise"))
            BrushNoise = float.Parse(EditorPrefs.GetString("2DTE_BrushNoise"));
        if (EditorPrefs.HasKey("2DTE_ShowCapSettings"))
            _showCapSettings = EditorPrefs.GetBool("2DTE_ShowCapSettings");
        if (EditorPrefs.HasKey("2DTE_ShowSettings"))
            _showSettings = EditorPrefs.GetBool("2DTE_ShowSettings");
        if (EditorPrefs.HasKey("2DTE_ShowRndSettings"))
            _showRndSettings = EditorPrefs.GetBool("2DTE_ShowRndSettings");

        _myTerrainEditor2D = target as TerrainEditor2D;

        if (_myTerrainEditor2D == null)
            return;

        _myTerrainEditor2D.GetComponent<PolygonCollider2D>().enabled = false;

        if (_myTerrainEditor2D.Create3DCollider)
            _myTerrainEditor2D.Collider3DObj.GetComponent<MeshCollider>().enabled = false;

        _myTerrainEditor2D.enabled = true;

        if (PrefabUtility.GetPrefabType(_myTerrainEditor2D.gameObject) != PrefabType.Prefab)
            Undo.RegisterCreatedObjectUndo(_myTerrainEditor2D.gameObject, "Terrain 2D");
        _recordedTerrainVerts.Add(new RecordedTerrainVerts(_myTerrainEditor2D.GetVertsPos()));
    }

    public override void OnInspectorGUI()
    {
        BrushSize = EditorGUILayout.Slider("Brush size", BrushSize, 0.1f, 50);
        BrushHardness = EditorGUILayout.Slider("Brush hardness", BrushHardness, 0.01f, 1);
        BrushNoise = EditorGUILayout.Slider("Brush noise", BrushNoise, 0, 1);

        if (_myTerrainEditor2D == null)
        {
            EditorGUILayout.HelpBox("Missing object reference", MessageType.Warning);
            return;
        }

        GUIStyle s = new GUIStyle(GUI.skin.button);
        if (_editMode)
            s.normal.background = s.onActive.background;

        if (PrefabUtility.GetPrefabType(_myTerrainEditor2D.gameObject) == PrefabType.Prefab)
        {
            EditorGUILayout.HelpBox("Terrain settings is not avaliable in project view. Please, place terrain prefab to the scene.", MessageType.None);
            return;
        }
        
        if (GUILayout.Button("EDIT MODE", s, GUILayout.Height(40)))
        {
            SwitchEditMode();
        }

        if (_editMode)
            EditorGUILayout.HelpBox("Hold [LMB] - raise terrain \nHold [D + LMB] - lower terrain \nHold [ALT+Mouse Wheel] - change brush size", MessageType.None);
        else
        {

            if (GetSceneViewCamera() != null)
            {
                if (!GetSceneViewCamera().orthographic)
                    EditorGUILayout.HelpBox("Scene view camera is not orthographic. Set scene view camera mode to perspective or 2D to enter edit mode.", MessageType.Warning);
                else EditorGUILayout.HelpBox("Use [Shift+E] to enter Edit Mode", MessageType.None);
            }
        }

        EditorGUILayout.Space();
        EditorGUI.BeginChangeCheck();

        _myTerrainEditor2D.MainMaterial = (Material)EditorGUILayout.ObjectField("Main material", _myTerrainEditor2D.MainMaterial, typeof(Material), false);
        _myTerrainEditor2D.TextureSize = EditorGUILayout.IntField("Texture size", _myTerrainEditor2D.TextureSize);
        _myTerrainEditor2D.SortingLayerName = EditorGUILayout.TextField("Layer Name", _myTerrainEditor2D.SortingLayerName);
        _myTerrainEditor2D.OrderInLayer = EditorGUILayout.IntField("Order in layer", _myTerrainEditor2D.OrderInLayer);

        EditorGUILayout.Space();

        _showRndSettings = EditorGUILayout.Foldout(_showRndSettings, "Randomizer");
        if (_showRndSettings)
        {
            EditorGUI.indentLevel = 1;

            _myTerrainEditor2D.RndHeight = EditorGUILayout.Slider("Height", _myTerrainEditor2D.RndHeight, 0.1f, _myTerrainEditor2D.Height);
            _myTerrainEditor2D.RndHillsCount = EditorGUILayout.IntSlider("Hills count", _myTerrainEditor2D.RndHillsCount, 1, _myTerrainEditor2D.Width / 2);
            _myTerrainEditor2D.RndAmplitude = EditorGUILayout.Slider("Amplitude", _myTerrainEditor2D.RndAmplitude, 0.1f, _myTerrainEditor2D.Height / 2f);

            if (GUILayout.Button("Randomize"))
            {
                _myTerrainEditor2D.RandomizeTerrain();
            }

            EditorGUI.indentLevel = 0;
        }

        _showCapSettings = EditorGUILayout.Foldout(_showCapSettings, "Cap Settings");
        if (_showCapSettings)
        {
            EditorGUI.indentLevel = 1;
            
            _myTerrainEditor2D.CreateCap = EditorGUILayout.Toggle("Create cap", _myTerrainEditor2D.CreateCap);
            EditorGUI.BeginDisabledGroup(!_myTerrainEditor2D.CreateCap);
            _myTerrainEditor2D.CapHeight = EditorGUILayout.FloatField("Height", _myTerrainEditor2D.CapHeight);
            _myTerrainEditor2D.CapOffset = EditorGUILayout.FloatField("Offset", _myTerrainEditor2D.CapOffset);
            _myTerrainEditor2D.CapMaterial = (Material)EditorGUILayout.ObjectField("Material", _myTerrainEditor2D.CapMaterial, typeof(Material), false);
            _myTerrainEditor2D.CapTextureTiling = EditorGUILayout.IntField("Texture tiling", _myTerrainEditor2D.CapTextureTiling);
            EditorGUI.EndDisabledGroup();

            EditorGUI.indentLevel = 0;
        }

        _showAddSettings = EditorGUILayout.Foldout(_showAddSettings, "Additionally");
        if (_showAddSettings)
        {
            EditorGUI.indentLevel = 1;

            _myTerrainEditor2D.FixSides = EditorGUILayout.Toggle("Fix sides", _myTerrainEditor2D.FixSides);
            EditorGUI.BeginDisabledGroup(!_myTerrainEditor2D.FixSides);
            _myTerrainEditor2D.LeftFixedPoint = EditorGUILayout.FloatField("Left fixed point", _myTerrainEditor2D.LeftFixedPoint);
            _myTerrainEditor2D.RightFixedPoint = EditorGUILayout.FloatField("Right fixed point", _myTerrainEditor2D.RightFixedPoint);
            EditorGUI.EndDisabledGroup();

            _myTerrainEditor2D.Create3DCollider = EditorGUILayout.Toggle("3D collider", _myTerrainEditor2D.Create3DCollider);
            EditorGUI.BeginDisabledGroup(!_myTerrainEditor2D.Create3DCollider);
            _myTerrainEditor2D.Collider3DWidth = EditorGUILayout.FloatField("3D collider width", _myTerrainEditor2D.Collider3DWidth);
            EditorGUI.EndDisabledGroup();

            EditorGUI.indentLevel = 0;
        }

        if (EditorGUI.EndChangeCheck())
        {
            CheckValues();
            _myTerrainEditor2D.EditMesh(_myTerrainEditor2D.GetVertsPos());
        }

        _showSettings = EditorGUILayout.Foldout(_showSettings, "Terrain Settings");
        if (_showSettings)
        {
            EditorGUI.indentLevel = 1;

            _myTerrainEditor2D.Width = EditorGUILayout.IntField("Width", _myTerrainEditor2D.Width);
            _myTerrainEditor2D.Height = EditorGUILayout.IntField("Height", _myTerrainEditor2D.Height);
            _myTerrainEditor2D.Resolution = EditorGUILayout.IntField("Resolution", _myTerrainEditor2D.Resolution);

            if (_myTerrainEditor2D.Resolution > 8)
                EditorGUILayout.HelpBox("Resolution is responsible for the number of polygons. High resolution value can affect performance.", MessageType.Warning);
            
            if (GUILayout.Button("Apply"))
            {
                _myTerrainEditor2D.CreateTerrain();
            }

            EditorGUI.indentLevel = 0;
        }

        if (EditorApplication.isPlaying && !_playMode)
        {
            _myTerrainEditor2D.GetComponent<PolygonCollider2D>().enabled = true;
            _myTerrainEditor2D.UpdateCollider();

            if (_myTerrainEditor2D.Create3DCollider)
            {
                _myTerrainEditor2D.Collider3DObj.GetComponent<MeshCollider>().enabled = true;
                _myTerrainEditor2D.UpdateCollider3D();
            }

            _playMode = true;
        }
        else _playMode = false;
        
        if (GUI.changed)
        {
            _myTerrainEditor2D.SetSrotingLayerAndOrder();
            CheckValues();
            Undo.RegisterCompleteObjectUndo(_myTerrainEditor2D, "Terrain 2D Inspector");
            Undo.RegisterCompleteObjectUndo(this, "Terrain 2D Inspector");
            EditorUtility.SetDirty(target);
        }
    }

    string[] GetSortingLayerNames()
    {
        Type internalEditorUtilityType = typeof(InternalEditorUtility);
        PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
        return (string[])sortingLayersProperty.GetValue(null, new object[0]);
    }

    void CheckValues()
    {
        if (_myTerrainEditor2D.Width < 10)
            _myTerrainEditor2D.Width = 10;
        if (_myTerrainEditor2D.Height < 10)
            _myTerrainEditor2D.Height = 10;
        if (_myTerrainEditor2D.Resolution < 1)
            _myTerrainEditor2D.Resolution = 1;
        if (_myTerrainEditor2D.CapHeight < 0.1f)
            _myTerrainEditor2D.CapHeight = 0.1f;
        if (_myTerrainEditor2D.LeftFixedPoint < 0)
            _myTerrainEditor2D.LeftFixedPoint = 0;
        if (_myTerrainEditor2D.LeftFixedPoint > _myTerrainEditor2D.Height)
            _myTerrainEditor2D.LeftFixedPoint = _myTerrainEditor2D.Height;
        if (_myTerrainEditor2D.RightFixedPoint < 0)
            _myTerrainEditor2D.RightFixedPoint = 0;
        if (_myTerrainEditor2D.RightFixedPoint > _myTerrainEditor2D.Height)
            _myTerrainEditor2D.RightFixedPoint = _myTerrainEditor2D.Height;
        

        _myTerrainEditor2D.gameObject.GetComponent<Renderer>().material = _myTerrainEditor2D.MainMaterial;
    }

    void SwitchEditMode()
    {
        if (GetSceneViewCamera() != null)
        {
            if (!_editMode && !GetSceneViewCamera().orthographic)
                return;
        }

        _editMode = !_editMode;

        if (_editMode)
        {
            _previousUsingTool = Tools.current;
            Tools.current = Tool.None;
        }
        else Tools.current = _previousUsingTool;
    }

    private bool _hotkeyEnteredInEditMode;
    private Vector2 _mousePos;
    private Vector2 _handleLocalPos;
    void OnSceneGUI()
    {
        if (PrefabUtility.GetPrefabType(_myTerrainEditor2D.gameObject) == PrefabType.Prefab || _myTerrainEditor2D == null)
            return;

        Repaint();

        if (Event.current.Equals(Event.KeyboardEvent("#" + KeyCode.E)))
            SwitchEditMode();

        if (!_editMode)
            return;

        if (GetSceneViewCamera() != null)
        {
            _mousePos = Camera.current.ScreenToWorldPoint(new Vector2(Event.current.mousePosition.x, (Camera.current.pixelHeight - Event.current.mousePosition.y)));
            _handleLocalPos = _mousePos - new Vector2(_myTerrainEditor2D.transform.position.x, _myTerrainEditor2D.transform.position.y);

            Handles.color = Color.green;
            Handles.CircleCap(0, _mousePos, Quaternion.identity, BrushSize);
        }
            

        #region DrawTerrainBorders
        Vector3 terrainPos = _myTerrainEditor2D.transform.position;
        Handles.color = new Color(1,1,1, 0.5f);
        Handles.DrawLine(terrainPos, terrainPos + new Vector3(0, _myTerrainEditor2D.Height));
        Handles.DrawLine(terrainPos, terrainPos + new Vector3(_myTerrainEditor2D.Width, 0));
        Handles.DrawLine(terrainPos + new Vector3(0, _myTerrainEditor2D.Height), terrainPos + new Vector3(0, _myTerrainEditor2D.Height) + new Vector3(_myTerrainEditor2D.Width, 0));
        Handles.DrawLine(terrainPos + new Vector3(_myTerrainEditor2D.Width, 0), terrainPos + new Vector3(0, _myTerrainEditor2D.Height) + new Vector3(_myTerrainEditor2D.Width, 0));

        if (_myTerrainEditor2D.FixSides)
        {
            Vector3 leftFixedPoint = terrainPos + new Vector3(0, _myTerrainEditor2D.LeftFixedPoint);
            Vector3 rightFixedPoint = terrainPos + new Vector3(_myTerrainEditor2D.Width, _myTerrainEditor2D.RightFixedPoint);
            Handles.DrawLine(leftFixedPoint, leftFixedPoint - new Vector3(1, 0));
            Handles.DrawLine(rightFixedPoint, rightFixedPoint + new Vector3(1, 0));
        }
        
        #endregion

        #region Events
        //Mouse
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            _myTerrainEditor2D.gameObject.GetComponent<PolygonCollider2D>().enabled = false;

            if (_myTerrainEditor2D.Create3DCollider)
                _myTerrainEditor2D.Collider3DObj.GetComponent<MeshCollider>().enabled = false;

            _startEdit = true;
        }

        if (Event.current.type == EventType.MouseUp)
        {
            _startEdit = false;

            if (_recordedTerrainVerts.Count > 30)
                _recordedTerrainVerts.RemoveAt(0);
            _recordedTerrainVerts.Add(new RecordedTerrainVerts(_myTerrainEditor2D.GetVertsPos()));
            Undo.RegisterCompleteObjectUndo(this, "Terrain2D edit");
        }

        //Hotkeys
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.D)
            _digMode = true;
        if (Event.current.type == EventType.keyUp && Event.current.keyCode == KeyCode.D)
            _digMode = false;
        if (Event.current.alt)
        {
            if (Event.current.type == EventType.ScrollWheel && Event.current.delta.y > 0)
                BrushSize -= 0.2f;
            if (Event.current.type == EventType.ScrollWheel && Event.current.delta.y < 0)
                BrushSize += 0.2f;
            if (BrushSize <= 0.1f)
                BrushSize = 0.1f;

            Event.current.Use();
        }

        //Undo
        if (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed")
        {
            if (_recordedTerrainVerts.Count > 1)
            {
                _myTerrainEditor2D.EditMesh(_recordedTerrainVerts[_recordedTerrainVerts.Count - 2].TerrainVerts);
                _recordedTerrainVerts.RemoveAt(_recordedTerrainVerts.Count - 1);
            }
        }
        
        #endregion

        #region StartEditMesh
        if (_startEdit)
        {
            Vector3[] vertsPos = _myTerrainEditor2D.GetVertsPos();

            for (int i = 0; i < vertsPos.Length; i += 2)
            {
                if (Vector2.Distance(vertsPos[i], _handleLocalPos) <= BrushSize)
                {
                    float vertOffset = BrushSize - Vector2.Distance(vertsPos[i], _handleLocalPos);

                    if (_digMode)
                        vertsPos[i] -= new Vector3(0, vertOffset * (BrushHardness * 0.1f));
                    else vertsPos[i] += new Vector3(0, vertOffset * (BrushHardness * 0.1f));

                    if (BrushNoise > 0f)
                        vertsPos[i] += new Vector3(0, UnityEngine.Random.Range(-BrushNoise * 0.25f, BrushNoise * 0.25f));

                    if (vertsPos[i].y < 0.1f)
                        vertsPos[i].y = 0.1f;
                    if (vertsPos[i].y > _myTerrainEditor2D.Height)
                        vertsPos[i].y = _myTerrainEditor2D.Height;
                        
                }
            }
            _myTerrainEditor2D.EditMesh(vertsPos);

            Selection.activeGameObject = _myTerrainEditor2D.gameObject;
        }
        #endregion

        #region ConfigureHandles

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        if (_myTerrainEditor2D.GetComponent<Renderer>() != null)
            EditorUtility.SetSelectedWireframeHidden(_myTerrainEditor2D.GetComponent<Renderer>(), true);
        if (_myTerrainEditor2D.CapObj != null)
        {
            if (_myTerrainEditor2D.CapObj.GetComponent<Renderer>() != null)
                EditorUtility.SetSelectedWireframeHidden(_myTerrainEditor2D.CapObj.GetComponent<Renderer>(), true);
        }
        
        #endregion

        SceneView.RepaintAll();
        EditorUtility.SetDirty(_myTerrainEditor2D);
        
    }

    public void SaveMesh(GameObject obj, Mesh mesh)
    {
        if (PrefabUtility.GetPrefabType(_myTerrainEditor2D.gameObject) == PrefabType.Prefab)
            return;

        string path = GetSavedMeshesFolderPath();

        if (String.IsNullOrEmpty(GetSavedMeshesFolderPath()))
        {
            Debug.LogError("'SavedTerrain2DMeshes' folder is missing. Please, create folder with this name to save 2D terrain mesh.");
            return;
        }

        AssetDatabase.Refresh();

        if (!AssetDatabase.Contains(mesh))
        {
            AssetDatabase.CreateAsset(mesh, path + mesh.name + ".asset");
            AssetDatabase.SaveAssets();
        }
        else
        {
            AssetDatabase.SaveAssets();
        }
    }

    private string GetSavedMeshesFolderPath()
    {
        string[] targetDirs = Directory.GetDirectories(Application.dataPath, "SavedMeshes", SearchOption.AllDirectories);
        foreach (var savedMeshesPath in targetDirs)
        {
            return savedMeshesPath.Remove(0, savedMeshesPath.IndexOf("/Assets", System.StringComparison.Ordinal) + 1) + "\\";
        }

        return null;
    }

    private Camera GetSceneViewCamera()
    {
        if (SceneView.lastActiveSceneView != null)
            if (SceneView.lastActiveSceneView.camera != null)
                return SceneView.lastActiveSceneView.camera;

        return null;
    }

    void OnDisable()
    {
        EditorPrefs.SetString("2DTE_BrushSize", BrushSize.ToString());
        EditorPrefs.SetString("2DTE_BrushHardness", BrushHardness.ToString());
        EditorPrefs.SetString("2DTE_BrushNoise", BrushNoise.ToString());
        EditorPrefs.SetBool("2DTE_ShowCapSettings", _showCapSettings);
        EditorPrefs.SetBool("2DTE_ShowSettings", _showSettings);
        EditorPrefs.SetBool("2DTE_ShowRndSettings", _showRndSettings);

        if (_myTerrainEditor2D == null)
            return;

        if (PrefabUtility.GetPrefabType(_myTerrainEditor2D.gameObject) == PrefabType.Prefab)
            return;

        SaveMesh(_myTerrainEditor2D.gameObject, _myTerrainEditor2D.GetComponent<MeshFilter>().sharedMesh);
        if (_myTerrainEditor2D.CreateCap)
            SaveMesh(_myTerrainEditor2D.CapObj, _myTerrainEditor2D.CapObj.GetComponent<MeshFilter>().sharedMesh);
        if (_myTerrainEditor2D.Create3DCollider)
            SaveMesh(_myTerrainEditor2D.Collider3DObj, _myTerrainEditor2D.Collider3DObj.GetComponent<MeshFilter>().sharedMesh);

        _myTerrainEditor2D.GetComponent<PolygonCollider2D>().enabled = true;
        _myTerrainEditor2D.UpdateCollider();

        if (_myTerrainEditor2D.Create3DCollider)
        {
            _myTerrainEditor2D.Collider3DObj.GetComponent<MeshCollider>().enabled = true;
            _myTerrainEditor2D.UpdateCollider3D();
        }


        if (_editMode)
            SwitchEditMode();
    }

    
}

class RecordedTerrainVerts
{
    public Vector3[] TerrainVerts;

    public RecordedTerrainVerts(Vector3[] terrainVerts)
    {
        TerrainVerts = terrainVerts;
    }
}

public static class Terrain2DCreator
{
    [MenuItem("GameObject/Create Other/Terrain2D")]
    static void CreateTerrain2D()
    {
        GameObject terrain2D = new GameObject("New Terrain2D");
        terrain2D.AddComponent<TerrainEditor2D>();

        if (!EditorPrefs.HasKey("LastMeshId"))
            EditorPrefs.SetInt("LastMeshId", 0);
        int lastMeshId = EditorPrefs.GetInt("LastMeshId");
        terrain2D.GetComponent<TerrainEditor2D>().MeshId = lastMeshId;
        lastMeshId++;
        EditorPrefs.SetInt("LastMeshId", lastMeshId);

        terrain2D.GetComponent<TerrainEditor2D>().CreateTerrain();
        Selection.activeGameObject = terrain2D.gameObject;
    }
}

using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(GridItemSO), true)]
public class GridItemEditor : Editor, IEditorGridProvider
{
    private const int GridSize = 5;
    private const int CenterIndex = 2;
    
    private GridItemSO data;
    private bool gridChanged = false;

    public override void OnInspectorGUI()
    {
        data = (GridItemSO)target;

        EditorGUILayout.LabelField("Base Settings", EditorStyles.boldLabel);
        data.id = EditorGUILayout.TextField("Item ID", data.id);
        
        EditorGUILayout.Space();
        GUILayout.Label("Prefab Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        data.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", data.prefab, typeof(GameObject), false);
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Or Generate From Unit Prefab:", EditorStyles.miniLabel);
        data.unitPrefab = (GameObject)EditorGUILayout.ObjectField("Unit Prefab (1x1)", data.unitPrefab, typeof(GameObject), false);
        
        Color defaultColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Generate & Save Prefab", GUILayout.Height(30)))
        {
            CreateAndSavePrefab(data);
        }
        GUI.backgroundColor = defaultColor;
        
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Shape Designer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Grid Mapping:\nUp Arrow = Forward (+Z)\nRight Arrow = Right (+X)", MessageType.Info);

        if (data.layoutGrid == null || data.layoutGrid.Length != GridSize * GridSize)
        {
            data.layoutGrid = new bool[GridSize * GridSize];
        }

        gridChanged = false;

        GUILayout.BeginVertical(EditorStyles.helpBox);
        
        DrawDirectionIndicator(true);
        EditorGridDrawer.DrawGrid(this);
        DrawDirectionIndicator(false);
        
        GUILayout.EndVertical();

        if (gridChanged)
        {
            RecalculateOffsets(data);
            EditorUtility.SetDirty(data);
        }

        EditorGUILayout.Space();
        GUI.enabled = false;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseOffsets"));
        GUI.enabled = true;

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawDirectionIndicator(bool isTop)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Space(80);
        GUI.color = isTop ? Color.blue : Color.gray;
        GUILayout.Label(isTop ? "▲ (+Z)" : "▼ (-Z)", 
            isTop ? EditorStyles.boldLabel : EditorStyles.miniLabel, 
            GUILayout.Width(50));
        GUI.color = Color.white;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    #region IEditorGridProvider Implementation

    public int GetGridWidth() => GridSize;
    public int GetGridHeight() => GridSize;
    public float GetCellSize() => 30f;
    public void OnCellHover(int x, int y) { }

    public EditorGridCellData GetCellData(int x, int y)
    {
        int index = y * GridSize + x;
        bool isActive = data.layoutGrid[index];
        
        Color bgColor;
        if (isActive)
        {
            bgColor = Color.black;
        }
        else if (x == CenterIndex && y == CenterIndex)
        {
            bgColor = Color.cyan;
        }
        else if (x == CenterIndex)
        {
            bgColor = new Color(0.8f, 0.8f, 1f);
        }
        else if (y == CenterIndex)
        {
            bgColor = new Color(1f, 0.8f, 0.8f);
        }
        else
        {
            bgColor = Color.white;
        }

        return new EditorGridCellData
        {
            BackgroundColor = bgColor,
            Label = "",
            IsVisible = true,
            IsClickable = true
        };
    }

    public void OnCellClicked(int x, int y)
    {
        int index = y * GridSize + x;
        data.layoutGrid[index] = !data.layoutGrid[index];
        gridChanged = true;
    }

    #endregion

    private void RecalculateOffsets(GridItemSO data)
    {
        data.baseOffsets.Clear();
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                int index = y * GridSize + x;
                if (data.layoutGrid[index])
                {
                    data.baseOffsets.Add(new Vector2Int(x - CenterIndex, y - CenterIndex));
                }
            }
        }
    }

    private void CreateAndSavePrefab(GridItemSO data)
    {
        if (data.unitPrefab == null)
        {
            Debug.LogError("Lütfen önce bir Unit Prefab (1x1) atayın!");
            return;
        }

        if (string.IsNullOrEmpty(data.id))
        {
            Debug.LogError("Lütfen önce bir ID belirleyin!");
            return;
        }

        string folderPath = "Assets/Prefabs/BlockPrefabs";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        GameObject root = new GameObject(data.id);

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                int index = y * GridSize + x;
                if (data.layoutGrid[index])
                {
                    GameObject unit = (GameObject)PrefabUtility.InstantiatePrefab(data.unitPrefab);
                    unit.transform.SetParent(root.transform);
                    
                    int offsetX = x - CenterIndex;
                    int offsetZ = y - CenterIndex;
                    
                    unit.transform.position = new Vector3(offsetX, 0, offsetZ);
                }
            }
        }

        string localPath = folderPath + "/" + data.id + ".prefab";
        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, localPath);
        
        data.prefab = prefabAsset;
        DestroyImmediate(root);

        Debug.Log($"Prefab başarıyla oluşturuldu: {localPath}");
        EditorUtility.SetDirty(data);
    }
}
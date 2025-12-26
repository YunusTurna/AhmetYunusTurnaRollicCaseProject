using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LevelEditorWindow : EditorWindow, IEditorGridProvider
{
    public LevelData activeLevel;

    private BlockSO selectedBlockSO;
    private ColorGateSO selectedGateSO;

    private GridItemColor selectedColor = GridItemColor.Red;
    private GridItemRotation selectedRotation = GridItemRotation.Deg0;
    private GridItemMirror selectedMirror = GridItemMirror.None;
    private MoveDirection selectedMoveDirection = MoveDirection.Horizontal;

    private BlockEffectType selectedEffectType = BlockEffectType.None;
    private float selectedBombTimer = 10f;
    private int selectedIceBlocksToDestroy = 3;

    private int selectedTool = 0;
    private string[] toolNames = { "Block", "Obstacle", "Color Gate", "Eraser", "Void (No Grid)" };

    private Vector2Int currentMouseGridPos = new Vector2Int(-1, -1);
    private Vector2 scrollPosition;

    [MenuItem("CBJ Level Editor/Level Editor")]
    public static void ShowWindow() => GetWindow<LevelEditorWindow>("Level Editor");

    private void OnEnable() => wantsMouseMove = true;

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Level Editor Setup", EditorStyles.boldLabel);

        activeLevel = (LevelData)EditorGUILayout.ObjectField("Target Level Data", activeLevel, typeof(LevelData), false);

        EditorGUILayout.Space();

        if (activeLevel == null)
        {
            EditorGUILayout.HelpBox("Editöre başlamak için yukarıdaki alana bir Level Data dosyası sürükleyin.", MessageType.Warning);
            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawLevelInfo();
        EditorGUILayout.Space();
        DrawClearButton();
        DrawGridSettings();
        EditorGUILayout.Space();
        DrawToolbar();
        EditorGUILayout.Space();
        EditorGridDrawer.DrawGrid(this);

        EditorGUILayout.EndScrollView();

        if (Event.current.type == EventType.MouseMove) Repaint();
        if (GUI.changed && activeLevel != null) EditorUtility.SetDirty(activeLevel);
    }

    void DrawLevelInfo()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Level Info", EditorStyles.miniBoldLabel);
        activeLevel.EditorLevelIndex = EditorGUILayout.IntField("Level Index", activeLevel.EditorLevelIndex);
        activeLevel.EditorLevelName = EditorGUILayout.TextField("Level Name", activeLevel.EditorLevelName);
        activeLevel.EditorStatus = (LevelStatus)EditorGUILayout.EnumPopup("Status", activeLevel.EditorStatus);
        activeLevel.EditorDifficulty = (LevelDifficulty)EditorGUILayout.EnumPopup("Difficulty", activeLevel.EditorDifficulty);
        EditorGUILayout.EndVertical();
    }

    void DrawClearButton()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        Color defaultColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
        if (GUILayout.Button("Clear Level", GUILayout.Height(25), GUILayout.Width(120)))
        {
            if (EditorUtility.DisplayDialog("Clear Level", "Tüm objeleri silmek istediğinize emin misiniz?", "Evet", "İptal"))
            {
                ClearLevel();
            }
        }
        GUI.backgroundColor = defaultColor;
        GUILayout.EndHorizontal();
    }

    void DrawGridSettings()
    {
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        activeLevel.EditorWidth = EditorGUILayout.IntField("Width", activeLevel.EditorWidth);
        activeLevel.EditorHeight = EditorGUILayout.IntField("Height", activeLevel.EditorHeight);
        GUILayout.EndHorizontal();
        activeLevel.EditorTimeLimit = EditorGUILayout.FloatField("Time Limit", activeLevel.EditorTimeLimit);
    }

    void DrawToolbar()
    {
        GUILayout.Label("Tools", EditorStyles.boldLabel);

        int oldTool = selectedTool;
        selectedTool = GUILayout.Toolbar(selectedTool, toolNames);
        if (oldTool != selectedTool) selectedRotation = GridItemRotation.Deg0;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        switch (selectedTool)
        {
            case 0: DrawBlockTool(); break;
            case 1: EditorGUILayout.HelpBox("Engel yerleştirmek için tıklayın.", MessageType.None); break;
            case 2: DrawGateTool(); break;
            case 3: EditorGUILayout.HelpBox("Silmek için tıklayın.", MessageType.None); break;
            case 4: EditorGUILayout.HelpBox("Void alanı için tıklayın.", MessageType.Info); break;
        }

        EditorGUILayout.EndVertical();
    }

    void DrawBlockTool()
    {
        selectedBlockSO = (BlockSO)EditorGUILayout.ObjectField("Block SO", selectedBlockSO, typeof(BlockSO), false);
        selectedRotation = (GridItemRotation)EditorGUILayout.EnumPopup("Rotation", selectedRotation);
        selectedMirror = (GridItemMirror)EditorGUILayout.EnumPopup("Mirror", selectedMirror);
        selectedColor = (GridItemColor)EditorGUILayout.EnumPopup("Color", selectedColor);

        GUILayout.Space(5);
        GUILayout.Label("Movement", EditorStyles.miniBoldLabel);
        selectedMoveDirection = (MoveDirection)EditorGUILayout.EnumPopup("Direction", selectedMoveDirection);

        GUILayout.Space(5);
        GUILayout.Label("Effect", EditorStyles.miniBoldLabel);
        selectedEffectType = (BlockEffectType)EditorGUILayout.EnumPopup("Type", selectedEffectType);

        if (selectedEffectType == BlockEffectType.Bomb)
            selectedBombTimer = EditorGUILayout.FloatField("Timer (sec)", selectedBombTimer);
        else if (selectedEffectType == BlockEffectType.Ice)
            selectedIceBlocksToDestroy = EditorGUILayout.IntField("Blocks to Destroy", selectedIceBlocksToDestroy);
    }

    void DrawGateTool()
    {
        selectedGateSO = (ColorGateSO)EditorGUILayout.ObjectField("Gate SO", selectedGateSO, typeof(ColorGateSO), false);
        selectedRotation = (GridItemRotation)EditorGUILayout.EnumPopup("Rotation", selectedRotation);
        selectedMirror = (GridItemMirror)EditorGUILayout.EnumPopup("Mirror", selectedMirror);
        selectedColor = (GridItemColor)EditorGUILayout.EnumPopup("Color", selectedColor);
    }

    void ClearLevel()
    {
        activeLevel.EditorBlocks.Clear();
        activeLevel.EditorObstacles.Clear();
        activeLevel.EditorColorGates.Clear();
        activeLevel.EditorVoidCells.Clear();
        EditorUtility.SetDirty(activeLevel);
    }

    #region IEditorGridProvider

    public int GetGridWidth() => activeLevel.EditorWidth;
    public int GetGridHeight() => activeLevel.EditorHeight;
    public float GetCellSize() => 30f;

    public void OnCellHover(int x, int y)
    {
        if (currentMouseGridPos.x != x || currentMouseGridPos.y != y)
        {
            currentMouseGridPos = new Vector2Int(x, y);
            Repaint();
        }
    }

    public EditorGridCellData GetCellData(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);
        bool isVoid = activeLevel.EditorVoidCells.Contains(pos);

        if (isVoid && selectedTool != 4)
            return EditorGridCellData.Invisible;

        Color visualColor = Color.white;
        string label = "";

        if (isVoid)
        {
            visualColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            label = "Ø";
        }
        else if (activeLevel.EditorObstacles.Contains(pos))
        {
            visualColor = Color.black;
            label = "X";
        }
        else
        {
            var gate = GetGateAt(x, y);
            var block = GetBlockAt(x, y);

            if (gate != null)
            {
                visualColor = GridUtils.GetUnityColor(gate.itemColor);
                if (gate.position.x == x && gate.position.y == y)
                    label = "G";
            }
            else if (block != null)
            {
                visualColor = GridUtils.GetUnityColor(block.itemColor);

                if (block.position.x == x && block.position.y == y)
                {
                    string moveLabel = block.moveDirection switch
                    {
                        MoveDirection.Horizontal => "↔",
                        MoveDirection.Vertical => "↕",
                        MoveDirection.All => "✥",
                        _ => "★"
                    };

                    string effectLabel = block.effect?.type switch
                    {
                        BlockEffectType.Bomb => "B",
                        BlockEffectType.Ice => "❄",
                        _ => ""
                    };

                    label = string.IsNullOrEmpty(effectLabel) ? moveLabel : $"{effectLabel}{moveLabel}";
                }
            }
            else if (IsPreviewCell(x, y))
            {
                Color previewColor = GridUtils.GetUnityColor(selectedColor);
                previewColor.a = 0.5f;
                visualColor = previewColor;

                if (x == currentMouseGridPos.x && y == currentMouseGridPos.y)
                {
                    label = selectedTool switch
                    {
                        0 => GetMoveDirectionLabel(selectedMoveDirection),
                        2 => "G",
                        _ => "+"
                    };
                }
            }
        }

        return new EditorGridCellData
        {
            BackgroundColor = visualColor,
            Label = label,
            IsVisible = true,
            IsClickable = true
        };
    }

    string GetMoveDirectionLabel(MoveDirection dir) => dir switch
    {
        MoveDirection.Horizontal => "↔",
        MoveDirection.Vertical => "↕",
        MoveDirection.All => "✥",
        _ => "★"
    };

    public void OnCellClicked(int x, int y)
    {
        currentMouseGridPos = new Vector2Int(x, y);

        switch (selectedTool)
        {
            case 0: TryPlaceBlock(x, y); break;
            case 1: ToggleObstacle(x, y); break;
            case 2: PlaceColorGate(x, y); break;
            case 3: EraseCell(x, y); break;
            case 4: ToggleVoid(x, y); break;
        }
    }

    #endregion

    #region Level Editor Logic

    bool IsPreviewCell(int x, int y)
    {
        if (currentMouseGridPos.x == -1) return false;

        if (selectedTool == 1 || selectedTool == 3 || selectedTool == 4)
            return x == currentMouseGridPos.x && y == currentMouseGridPos.y;

        GridItemSO shape = selectedTool == 0 ? selectedBlockSO : selectedTool == 2 ? selectedGateSO : null;
        if (shape == null) return false;

        var occupied = CalculateOccupiedCells(currentMouseGridPos.x, currentMouseGridPos.y, shape, selectedRotation, selectedMirror);
        return occupied.Contains(new Vector2Int(x, y));
    }

    void ToggleVoid(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);
        EraseCell(x, y);

        if (activeLevel.EditorVoidCells.Contains(pos))
            activeLevel.EditorVoidCells.Remove(pos);
        else
            activeLevel.EditorVoidCells.Add(pos);
    }

    void TryPlaceBlock(int x, int y)
    {
        if (selectedBlockSO == null) return;
        if (!CanPlace(x, y, selectedBlockSO, selectedRotation, selectedMirror)) return;

        EraseCell(x, y);

        var newBlock = new BlockData
        {
            position = new Vector2Int(x, y),
            itemColor = selectedColor,
            rotation = selectedRotation,
            mirror = selectedMirror,
            shapeData = selectedBlockSO,
            moveDirection = selectedMoveDirection,
            effect = new BlockEffectData
            {
                type = selectedEffectType,
                bombTimer = selectedBombTimer,
                iceBlocksToDestroy = selectedIceBlocksToDestroy
            }
        };
        activeLevel.EditorBlocks.Add(newBlock);
    }

    void PlaceColorGate(int x, int y)
    {
        if (selectedGateSO == null) return;
        if (!CanPlace(x, y, selectedGateSO, selectedRotation, selectedMirror)) return;

        EraseCell(x, y);

        var newGate = new ColorGateData
        {
            position = new Vector2Int(x, y),
            itemColor = selectedColor,
            rotation = selectedRotation,
            mirror = selectedMirror,
            gateData = selectedGateSO
        };
        activeLevel.EditorColorGates.Add(newGate);
    }

    bool CanPlace(int x, int y, GridItemSO shape, GridItemRotation rot, GridItemMirror mirror)
    {
        var cells = CalculateOccupiedCells(x, y, shape, rot, mirror);

        foreach (var cell in cells)
        {
            if (IsOutOfBounds(cell)) return false;
            if (activeLevel.EditorVoidCells.Contains(cell)) return false;
            if (activeLevel.EditorObstacles.Contains(cell)) return false;
            if (GetGateAt(cell.x, cell.y) != null) return false;
            if (GetBlockAt(cell.x, cell.y) != null) return false;
        }
        return true;
    }

    void ToggleObstacle(int x, int y)
    {
        EraseCell(x, y);
        activeLevel.EditorObstacles.Add(new Vector2Int(x, y));
    }

    void EraseCell(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);

        activeLevel.EditorObstacles.Remove(pos);

        var gate = GetGateAt(x, y);
        if (gate != null) activeLevel.EditorColorGates.Remove(gate);

        var block = GetBlockAt(x, y);
        if (block != null) activeLevel.EditorBlocks.Remove(block);
    }

    ColorGateData GetGateAt(int x, int y)
    {
        foreach (var gate in activeLevel.EditorColorGates)
        {
            if (gate.gateData == null) continue;
            var cells = CalculateOccupiedCells(gate.position.x, gate.position.y, gate.gateData, gate.rotation, gate.mirror);
            if (cells.Contains(new Vector2Int(x, y))) return gate;
        }
        return null;
    }

    BlockData GetBlockAt(int x, int y)
    {
        foreach (var block in activeLevel.EditorBlocks)
        {
            if (block.shapeData == null) continue;
            var cells = CalculateOccupiedCells(block.position.x, block.position.y, block.shapeData, block.rotation, block.mirror);
            if (cells.Contains(new Vector2Int(x, y))) return block;
        }
        return null;
    }

    List<Vector2Int> CalculateOccupiedCells(int cx, int cy, GridItemSO shape, GridItemRotation rot, GridItemMirror mirror = GridItemMirror.None)
    {
        var cells = new List<Vector2Int> { new Vector2Int(cx, cy) };
        if (shape == null) return cells;

        foreach (var offset in shape.baseOffsets)
        {
            var transformed = GridUtils.TransformVector(offset, rot, mirror);
            cells.Add(new Vector2Int(cx + transformed.x, cy + transformed.y));
        }
        return cells;
    }

    bool IsOutOfBounds(Vector2Int cell)
    {
        return cell.x < 0 || cell.x >= activeLevel.EditorWidth || cell.y < 0 || cell.y >= activeLevel.EditorHeight;
    }

    #endregion
}
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor, IEditorGridProvider
{
    private bool showGrid = true;
    private GridManager grid;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        grid = (GridManager)target;

        GUILayout.Space(10);
        GUILayout.Label("--- LOGICAL GRID MAP ---", EditorStyles.boldLabel);

        showGrid = EditorGUILayout.Foldout(showGrid, "Show Logical Grid");

        if (showGrid && grid.width > 0 && grid.height > 0)
        {
            EditorGridDrawer.DrawGrid(this);
        }

        GUILayout.Space(5);
        EditorGridDrawer.DrawLegend(
            (new Color(0.8f, 0.2f, 0.2f), "Obstacle (Engel)"),
            (new Color(0.2f, 0.4f, 0.8f), "Blok (Dolu)"),
            (new Color(0.6f, 0.2f, 0.8f), "Kapı (Gate)"),
            (Color.white * 0.8f, "Boş Hücre")
        );

        if (!Application.isPlaying)
        {
            GUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "Blok ve Kapı verileri sadece Play Mode'da görünür (Runtime Data). " +
                "Editör modunda sadece Void ve Obstacle'ları düzenleyebilirsiniz.",
                MessageType.Warning);
        }
    }

    #region IEditorGridProvider Implementation

    public int GetGridWidth() => grid.width;
    public int GetGridHeight() => grid.height;
    public void OnCellHover(int x, int y) { }

    public float GetCellSize()
    {
        float containerWidth = EditorGUIUtility.currentViewWidth - 40;
        float cellSize = containerWidth / grid.width;
        return Mathf.Clamp(cellSize, 20f, 40f);
    }

    public EditorGridCellData GetCellData(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);

        Color cellColor = Color.white * 0.8f;
        string cellText = $"{x},{y}";

        bool isObs = grid.editorObstacles.Contains(pos);

        if (isObs)
        {
            cellColor = new Color(0.8f, 0.2f, 0.2f);
            cellText = "OBS";
        }
        else if (Application.isPlaying)
        {
            BlockData block = grid.GetBlockAt(pos);
            ColorGateData gate = grid.GetGateAt(pos);

            if (block != null)
            {
                cellColor = new Color(0.2f, 0.4f, 0.8f);

                string dirArrow = "";
                switch (block.moveDirection)
                {
                    case MoveDirection.Horizontal: dirArrow = "↔"; break;
                    case MoveDirection.Vertical: dirArrow = "↕"; break;
                    default: dirArrow = "✥"; break;
                }

                cellText = $"{block.itemColor.ToString().Substring(0, 1)}\n{dirArrow}";
            }
            else if (gate != null)
            {
                cellColor = new Color(0.6f, 0.2f, 0.8f);
                cellText = "GATE";
            }
        }

        return new EditorGridCellData
        {
            BackgroundColor = cellColor,
            Label = cellText,
            IsVisible = true,
            IsClickable = true
        };
    }

    public void OnCellClicked(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);

        if (Application.isPlaying)
        {
            var b = grid.GetBlockAt(pos);
            if (b != null)
                Debug.Log($"Seçilen Hücre {pos}: Blok Var -> {b.itemColor}");
            else
                Debug.Log($"Seçilen Hücre {pos}: Boş");
        }
        else
        {
            if (grid.editorObstacles.Contains(pos))
                grid.editorObstacles.Remove(pos);
            else
                grid.editorObstacles.Add(pos);

            EditorUtility.SetDirty(grid);
        }
    }

    #endregion
}
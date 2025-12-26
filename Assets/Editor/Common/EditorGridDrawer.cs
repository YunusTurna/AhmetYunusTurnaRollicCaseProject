using UnityEngine;
using UnityEditor;

public static class EditorGridDrawer
{
    public static void DrawGrid(IEditorGridProvider provider, float cellSizeOverride = 0)
    {
        if (provider == null) return;

        int width = provider.GetGridWidth();
        int height = provider.GetGridHeight();
        float cellSize = cellSizeOverride > 0 ? cellSizeOverride : provider.GetCellSize();

        if (width <= 0 || height <= 0) return;

        GUIStyle cellStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 10,
            fontStyle = FontStyle.Bold
        };
        cellStyle.normal.textColor = Color.white;

        GUILayout.BeginVertical();

        for (int y = height - 1; y >= 0; y--)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            for (int x = 0; x < width; x++)
            {
                DrawCell(provider, x, y, cellSize, cellStyle);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }

    private static void DrawCell(IEditorGridProvider provider, int x, int y, float cellSize, GUIStyle style)
    {
        EditorGridCellData cellData = provider.GetCellData(x, y);

        if (!cellData.IsVisible)
        {
            GUILayout.Space(cellSize + 4);
            return;
        }

        Color originalBgColor = GUI.backgroundColor;
        GUI.backgroundColor = cellData.BackgroundColor;

        if (cellData.IsClickable)
        {
            if (GUILayout.Button(cellData.Label, style, GUILayout.Width(cellSize), GUILayout.Height(cellSize)))
            {
                provider.OnCellClicked(x, y);
            }
        }
        else
        {
            GUILayout.Box(cellData.Label, style, GUILayout.Width(cellSize), GUILayout.Height(cellSize));
        }

        if (Event.current.type == EventType.Repaint)
        {
            Rect lastRect = GUILayoutUtility.GetLastRect();
            if (lastRect.Contains(Event.current.mousePosition))
            {
                provider.OnCellHover(x, y);
            }
        }

        GUI.backgroundColor = originalBgColor;
    }

    public static void DrawLegend(params (Color color, string description)[] legendItems)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Legend", EditorStyles.boldLabel);
        
        foreach (var item in legendItems)
        {
            EditorGUILayout.BeginHorizontal();
            
            Color originalBg = GUI.backgroundColor;
            GUI.backgroundColor = item.color;
            GUILayout.Box("", GUILayout.Width(20), GUILayout.Height(15));
            GUI.backgroundColor = originalBg;
            
            EditorGUILayout.LabelField(item.description);
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();
    }
}

using UnityEngine;

public struct EditorGridCellData
{
    public Color BackgroundColor;
    
    public string Label;
    
    public bool IsVisible;
    public bool IsClickable;

    public static EditorGridCellData Default => new EditorGridCellData
    {
        BackgroundColor = Color.white,
        Label = "",
        IsVisible = true,
        IsClickable = true
    };

    public static EditorGridCellData Invisible => new EditorGridCellData
    {
        BackgroundColor = Color.clear,
        Label = "",
        IsVisible = false,
        IsClickable = false
    };
}

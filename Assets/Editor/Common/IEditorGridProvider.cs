using UnityEngine;

public interface IEditorGridProvider
{
    int GetGridWidth();
    int GetGridHeight();
    EditorGridCellData GetCellData(int x, int y);
    void OnCellClicked(int x, int y);
    void OnCellHover(int x, int y);
    float GetCellSize();
}

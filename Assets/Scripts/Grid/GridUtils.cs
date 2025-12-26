using UnityEngine;
using System.Collections.Generic;
public static class GridUtils
{
    public static Vector2Int TransformVector(Vector2Int v, GridItemRotation rotation, GridItemMirror mirror)
    {
        Vector2Int result = v;
        
        // First apply mirror
        switch (mirror)
        {
            case GridItemMirror.MirrorX:
                result = new Vector2Int(-result.x, result.y);
                break;
            case GridItemMirror.MirrorZ:
                result = new Vector2Int(result.x, -result.y);
                break;
        }
        
        // Then apply rotation
        Quaternion q = GetRotationQuaternion(rotation);
        Vector3 rotated = q * new Vector3(result.x, 0, result.y);
        return new Vector2Int(Mathf.RoundToInt(rotated.x), Mathf.RoundToInt(rotated.z));
    }

    public static Color GetUnityColor(GridItemColor color)
    {
        switch (color)
        {
            case GridItemColor.Red: return Color.red;
            case GridItemColor.Blue: return Color.blue;
            case GridItemColor.Green: return Color.green;
            case GridItemColor.Yellow: return Color.yellow;
            case GridItemColor.Purple: return new Color(0.6f, 0f, 0.8f);
            case GridItemColor.Orange: return new Color(1f, 0.6f, 0f);
            case GridItemColor.Cyan: return Color.cyan;
            case GridItemColor.Pink: return new Color(1f, 0.4f, 0.7f);
            default: return Color.white;
        }
    }

    public static Quaternion GetRotationQuaternion(GridItemRotation rot)
    {
        switch (rot)
        {
            case GridItemRotation.Deg90: return Quaternion.Euler(0, 90, 0);
            case GridItemRotation.Deg180: return Quaternion.Euler(0, 180, 0);
            case GridItemRotation.Deg270: return Quaternion.Euler(0, 270, 0);
            default: return Quaternion.identity;
        }
    }

    public static Vector3 GetMirrorScale(GridItemMirror mirror)
    {
        switch (mirror)
        {
            case GridItemMirror.MirrorX: return new Vector3(-1, 1, 1);
            case GridItemMirror.MirrorZ: return new Vector3(1, 1, -1);
            default: return Vector3.one;
        }
    }

    public static List<Vector2Int> CalculateOccupiedCells(Vector2Int center, GridItemSO shape, GridItemRotation rot, GridItemMirror mirror = GridItemMirror.None)
    {
        List<Vector2Int> cells = new List<Vector2Int> { center };
        if (shape != null)
        {
            foreach (var offset in shape.baseOffsets)
                cells.Add(center + TransformVector(offset, rot, mirror));
        }
        return cells;
    }
}

public enum MoveDirection
{
    None,
    Horizontal,
    Vertical,
    All
}
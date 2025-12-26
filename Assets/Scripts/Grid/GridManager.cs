using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [Range(1, 20)] public int width = 5;
    [Range(1, 20)] public int height = 5;
    public float cellSize = 1.0f;

    [Header("Editor Data")]
    public List<Vector2Int> editorObstacles = new();

    private readonly Dictionary<Vector2Int, BlockData> blockMap = new();
    private readonly Dictionary<Vector2Int, ColorGateData> gateMap = new();
    private readonly Dictionary<BlockData, Transform> blockVisuals = new();

    private Vector3 centeringOffset;

    private HashSet<Vector2Int> obstacles = new();

    private void OnValidate()
    {
        CalculateCenteringOffset();
    }

    private void Awake()
    {
        Initialize(width, height, cellSize, editorObstacles);
    }

    public void Initialize(int w, int h, float size, List<Vector2Int> obs)
    {
        width = w; height = h; cellSize = size;

        obstacles = new HashSet<Vector2Int>(obs ?? new List<Vector2Int>());

        editorObstacles = new List<Vector2Int>(obstacles);

        CalculateCenteringOffset();
        ClearMaps();
    }

    private void CalculateCenteringOffset()
    {
        float gridW = width * cellSize;
        float gridH = height * cellSize;
        centeringOffset = new Vector3(-(gridW / 2f) + (cellSize / 2f), 0, -(gridH / 2f) + (cellSize / 2f));
    }

    public void ClearMaps()
    {
        blockMap.Clear();
        gateMap.Clear();
        blockVisuals.Clear();
    }

    private void RegisterMapItem<T>(Dictionary<Vector2Int, T> map, Vector2Int pos, GridItemSO shape, GridItemRotation rot, GridItemMirror mirror, T data)
    {
        List<Vector2Int> occupied = CalculateOccupiedCells(pos, shape, rot, mirror);
        foreach (var cell in occupied)
        {
            if (!map.ContainsKey(cell)) map.Add(cell, data);
        }
    }

    public void RegisterBlock(BlockData data, Transform visual)
    {
        blockVisuals[data] = visual;
        RegisterMapItem(blockMap, data.position, data.shapeData, data.rotation, data.mirror, data);
    }

    public void RegisterGate(ColorGateData data)
    {
        RegisterMapItem(gateMap, data.position, data.gateData, data.rotation, data.mirror, data);
    }

    public void UnregisterBlock(BlockData data)
    {
        List<Vector2Int> occupied = CalculateOccupiedCells(data.position, data.shapeData, data.rotation, data.mirror);
        foreach (var cell in occupied)
        {
            if (blockMap.ContainsKey(cell) && blockMap[cell] == data)
                blockMap.Remove(cell);
        }
        blockVisuals.Remove(data);
    }

    public void UpdateBlockPosition(BlockData block, Vector2Int newPos)
    {
        Transform visual = GetBlockTransform(block);
        UnregisterBlock(block);
        block.position = newPos;
        RegisterBlock(block, visual);
    }

    public bool IsMoveValid(BlockData blockToMove, Vector2Int targetOriginPos)
    {
        List<Vector2Int> targetCells = CalculateOccupiedCells(targetOriginPos, blockToMove.shapeData, blockToMove.rotation, blockToMove.mirror);

        foreach (var checkPos in targetCells)
        {
            if (IsObstacle(checkPos)) return false;
            if (gateMap.ContainsKey(checkPos)) return false;
            if (blockMap.TryGetValue(checkPos, out BlockData otherBlock)) { if (otherBlock != blockToMove) return false; }
        }
        return true;
    }

    public List<Vector2Int> CalculateOccupiedCells(Vector2Int center, GridItemSO shape, GridItemRotation rot, GridItemMirror mirror = GridItemMirror.None)
    {
        return GridUtils.CalculateOccupiedCells(center, shape, rot, mirror);
    }

    public BlockData GetBlockAt(Vector2Int pos) => blockMap.TryGetValue(pos, out var val) ? val : null;
    public ColorGateData GetGateAt(Vector2Int pos) => gateMap.TryGetValue(pos, out var val) ? val : null;
    public Transform GetBlockTransform(BlockData data) => blockVisuals.TryGetValue(data, out var val) ? val : null;
    public bool IsObstacle(Vector2Int pos) => obstacles.Contains(pos);
    public float GetCellSize() => cellSize;
    public int GetBlockCount() { return blockMap.Values.Count; }

    public BlockData GetRandomBlock()
    {
        if (blockVisuals.Count == 0) return null;
        
        var blocks = new List<BlockData>(blockVisuals.Keys);
        int randomIndex = Random.Range(0, blocks.Count);
        return blocks[randomIndex];
    }

    public Vector3 GridToWorld(Vector2Int pos)
        => new Vector3(pos.x * cellSize, 0, pos.y * cellSize) + centeringOffset;

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        if (centeringOffset == Vector3.zero && width > 0) CalculateCenteringOffset();

        return new Vector2Int(
            Mathf.RoundToInt((worldPos.x - centeringOffset.x) / cellSize),
            Mathf.RoundToInt((worldPos.z - centeringOffset.z) / cellSize)
        );
    }
}
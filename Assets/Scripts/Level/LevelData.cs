using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "RollicCase/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    [SerializeField] private int levelIndex;
    [SerializeField] private string levelName;
    [SerializeField] private LevelStatus status = LevelStatus.Locked;
    [SerializeField] private LevelDifficulty difficulty = LevelDifficulty.Easy;
    [SerializeField] private int goldAward = 100;

    [Header("Grid Settings")]
    [SerializeField] private int width = 6;
    [SerializeField] private int height = 8;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float timeLimit = 60f;

    [Header("Level Objects")]
    [SerializeField] private List<BlockData> blocks = new();
    [SerializeField] private List<ColorGateData> colorGates = new();
    [SerializeField] private List<Vector2Int> obstacles = new();
    [SerializeField] private List<Vector2Int> voidCells = new();

    // Read-only properties for runtime access
    public int LevelIndex => levelIndex;
    public string LevelName => levelName;
    public LevelStatus Status => status;
    public LevelDifficulty Difficulty => difficulty;
    public int GoldAward => goldAward;
    public int Width => width;
    public int Height => height;
    public float TimeLimit => timeLimit;
    public float CellSize => cellSize;
    public IReadOnlyList<BlockData> Blocks => blocks;
    public IReadOnlyList<ColorGateData> ColorGates => colorGates;
    public IReadOnlyList<Vector2Int> Obstacles => obstacles;
    public IReadOnlyList<Vector2Int> VoidCells => voidCells;

    // Editor access - allows modification in Editor scripts
#if UNITY_EDITOR
    public int EditorLevelIndex { get => levelIndex; set => levelIndex = value; }
    public string EditorLevelName { get => levelName; set => levelName = value; }
    public LevelStatus EditorStatus { get => status; set => status = value; }
    public LevelDifficulty EditorDifficulty { get => difficulty; set => difficulty = value; }
    public int EditorWidth { get => width; set => width = value; }
    public int EditorHeight { get => height; set => height = value; }
    public float EditorTimeLimit { get => timeLimit; set => timeLimit = value; }
    public List<BlockData> EditorBlocks => blocks;
    public List<ColorGateData> EditorColorGates => colorGates;
    public List<Vector2Int> EditorObstacles => obstacles;
    public List<Vector2Int> EditorVoidCells => voidCells;
#endif

    // Runtime method to update status (if needed at runtime)
    public void SetStatus(LevelStatus newStatus) => status = newStatus;
}

public enum LevelStatus
{
    Locked,
    Unlocked,
    Completed
}

public enum LevelDifficulty
{
    Easy,
    Medium,
    Hard,
    Extreme
}

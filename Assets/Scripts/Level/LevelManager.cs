using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Data")]
    public List<LevelData> allLevels;
    public LevelData currentLevelData;

    [Header("Modules")]
    public GridManager Grid;
    public LevelGenerator Generator;

    [Header("Scene References")]
    public Transform gridRoot;

    [Header("Unity Events")]
    public UnityEvent OnLevelCompleted;
    public UnityEvent OnLevelFailed;
    public UnityEvent OnStartLevel;

    [Header("Pool Settings")]
    [SerializeField] private int obstaclePoolSize = 30;
    [SerializeField] private int blockPoolSize = 20;
    [SerializeField] private int gatePoolSize = 10;

    void Awake()
    {
        Instance = this;
        PreWarmPools();
    }

    private void PreWarmPools()
    {
        if (Generator.obstaclePrefab != null)
            ObjectPoolManager.Instance.PreWarmPool(Generator.obstaclePrefab, obstaclePoolSize);

        HashSet<GameObject> blockPrefabs = new();
        HashSet<GameObject> gatePrefabs = new();
        
        foreach (var level in allLevels)
        {
            foreach (var block in level.Blocks)
                if (block.shapeData?.prefab != null) blockPrefabs.Add(block.shapeData.prefab);
            foreach (var gate in level.ColorGates)
                if (gate.gateData?.prefab != null) gatePrefabs.Add(gate.gateData.prefab);
        }

        foreach (var prefab in blockPrefabs)
            ObjectPoolManager.Instance.PreWarmPool(prefab, blockPoolSize);
        
        foreach (var prefab in gatePrefabs)
            ObjectPoolManager.Instance.PreWarmPool(prefab, gatePoolSize);
    }

    public void TryStartLevel()
    {
        if (!HealthResourceManager.Instance.HasEnough(1))
        {
            HealthResourceManager.Instance.OnInsufficientAmount?.Invoke(1);
            Debug.Log("Not enough hearts to start the level.");
            return;
        }
        
        SaveManager.Instance?.StartLevelSession(currentLevelData.LevelIndex, currentLevelData.LevelName);
        
        Generator.GenerateLevel(currentLevelData, Grid);
        OnStartLevel?.Invoke();
    }

    public void SetNextLevel()
    {
        var nextLevel = allLevels.FirstOrDefault(l => l.LevelIndex == currentLevelData.LevelIndex + 1);
        currentLevelData = nextLevel != null ? nextLevel : currentLevelData;
        SaveManager.Instance?.SetCurrentLevel(currentLevelData.LevelIndex);
    }

    public void CompleteLevel()
    {
        currentLevelData.SetStatus(LevelStatus.Completed);
        if (GoldResourceManager.Instance != null)
            GoldResourceManager.Instance.Add(currentLevelData.GoldAward);
        SaveManager.Instance?.EndLevelSession(true);
        
        SetNextLevel();
    }

    public void CheckLevelCompletion()
    {
        if (Grid.GetBlockCount() == 0)
        {
            OnLevelCompleted?.Invoke();
        }
    }

    public void FailLevel()
    {
        HealthResourceManager.Instance?.Spend(1);
    
        SaveManager.Instance?.EndLevelSession(false);
        
        OnLevelFailed?.Invoke();
        Debug.Log("Level Failed!");
    }
}
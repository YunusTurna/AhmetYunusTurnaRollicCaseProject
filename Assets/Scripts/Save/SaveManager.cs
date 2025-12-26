using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Save Manager - Oyun verilerini JSON formatında kaydeder ve yükler.
/// 
/// NOT: Bu save sistemi, case incelemesi ve değerlendirme sürecinde
/// kayıt dosyalarının kolayca erişilebilir olması amacıyla Assets klasörüne
/// kayıt yapmaktadır. Production ortamında Application.persistentDataPath
/// kullanılması önerilir.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string saveFolderPath;
    private string sessionsFolderPath;
    private string playerDataPath;

    private PlayerSaveData playerData;
    private LevelSessionData currentSession;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Case incelemesi için Assets klasörüne kaydedilmektedir.
        // Production için: Application.persistentDataPath kullanılmalıdır.
#if UNITY_EDITOR
        saveFolderPath = Path.Combine(Application.dataPath, "SaveData");
#else
        saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");
#endif
        sessionsFolderPath = Path.Combine(saveFolderPath, "Sessions");
        playerDataPath = Path.Combine(saveFolderPath, "player_data.json");

        EnsureDirectoriesExist();
        LoadPlayerData();
    }

    void EnsureDirectoriesExist()
    {
        if (!Directory.Exists(saveFolderPath))
            Directory.CreateDirectory(saveFolderPath);
        if (!Directory.Exists(sessionsFolderPath))
            Directory.CreateDirectory(sessionsFolderPath);
    }

    #region Player Data

    public void LoadPlayerData()
    {
        if (File.Exists(playerDataPath))
        {
            string json = File.ReadAllText(playerDataPath);
            playerData = JsonUtility.FromJson<PlayerSaveData>(json);
        }
        else
        {
            playerData = new PlayerSaveData
            {
                currentLevelIndex = 0,
                gold = 0,
                health = 5,
                timeExtensionCount = 0,
                blockDestroyerCount = 0
            };
            SavePlayerData();
        }
    }

    public void SavePlayerData()
    {
        string json = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(playerDataPath, json);
    }

    public PlayerSaveData GetPlayerData() => playerData;

    public void SetCurrentLevel(int levelIndex)
    {
        playerData.currentLevelIndex = levelIndex;
        SavePlayerData();
    }

    public void SetGold(int amount)
    {
        playerData.gold = amount;
        SavePlayerData();
    }

    public void SetHealth(int amount)
    {
        playerData.health = amount;
        SavePlayerData();
    }

    public void SetPowerUpCount(PowerUpType type, int count)
    {
        switch (type)
        {
            case PowerUpType.TimeExtension:
                playerData.timeExtensionCount = count;
                break;
            case PowerUpType.BlockDestroyer:
                playerData.blockDestroyerCount = count;
                break;
        }
        SavePlayerData();
    }

    public int GetPowerUpCount(PowerUpType type)
    {
        return type switch
        {
            PowerUpType.TimeExtension => playerData.timeExtensionCount,
            PowerUpType.BlockDestroyer => playerData.blockDestroyerCount,
            _ => 0
        };
    }

    #endregion

    #region Level Session

    public void StartLevelSession(int levelIndex, string levelName)
    {
        currentSession = new LevelSessionData
        {
            levelIndex = levelIndex,
            levelName = levelName,
            timeSpent = 0,
            timeExtensionsUsed = 0,
            blockDestroyersUsed = 0,
            completed = false,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")
        };
    }

    public void UpdateSessionTime(float deltaTime)
    {
        if (currentSession != null)
            currentSession.timeSpent += deltaTime;
    }

    public void RecordPowerUpUsed(PowerUpType type)
    {
        if (currentSession == null) return;

        switch (type)
        {
            case PowerUpType.TimeExtension:
                currentSession.timeExtensionsUsed++;
                break;
            case PowerUpType.BlockDestroyer:
                currentSession.blockDestroyersUsed++;
                break;
        }
    }

    public void EndLevelSession(bool completed)
    {
        if (currentSession == null) return;

        currentSession.completed = completed;
        SaveLevelSession();
        currentSession = null;
    }

    void SaveLevelSession()
    {
        if (currentSession == null) return;

        // Create level-specific folder
        string levelFolderPath = Path.Combine(sessionsFolderPath, $"Level_{currentSession.levelIndex}");
        if (!Directory.Exists(levelFolderPath))
            Directory.CreateDirectory(levelFolderPath);

        string fileName = $"session_{currentSession.timestamp}.json";
        string filePath = Path.Combine(levelFolderPath, fileName);
        string json = JsonUtility.ToJson(currentSession, true);
        File.WriteAllText(filePath, json);
    }

    public List<LevelSessionData> GetAllSessions()
    {
        var sessions = new List<LevelSessionData>();

        if (!Directory.Exists(sessionsFolderPath))
            return sessions;

        string[] files = Directory.GetFiles(sessionsFolderPath, "*.json");
        foreach (string file in files)
        {
            string json = File.ReadAllText(file);
            var session = JsonUtility.FromJson<LevelSessionData>(json);
            sessions.Add(session);
        }

        return sessions;
    }

    #endregion

    [ContextMenu("Reset Save Data")]
    public void ResetSaveData()
    {
        playerData = new PlayerSaveData
        {
            currentLevelIndex = 0,
            gold = 0,
            health = 5,
            timeExtensionCount = 0,
            blockDestroyerCount = 0
        };
        SavePlayerData();
        Debug.Log("Save data reset!");
    }

    [ContextMenu("Open Save Folder")]
    public void OpenSaveFolder()
    {
        Application.OpenURL(saveFolderPath);
    }
}

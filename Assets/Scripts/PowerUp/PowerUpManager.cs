using UnityEngine;
using UnityEngine.Events;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    [Header("Time Extension")]
    [SerializeField] private Sprite timeExtensionIcon;
    [SerializeField] private int timeExtensionCount;

    [Header("Block Destroyer")]
    [SerializeField] private Sprite blockDestroyerIcon;
    [SerializeField] private int blockDestroyerCount;

    public UnityEvent<PowerUpType, int> OnCountChanged;
    public UnityEvent<PowerUpType> OnPurchaseSuccess;
    public UnityEvent<PowerUpType> OnPurchaseFailed;

    private const string TimeExtensionKey = "PowerUp_TimeExtension";
    private const string BlockDestroyerKey = "PowerUp_BlockDestroyer";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void Start()
    {
        OnCountChanged?.Invoke(PowerUpType.TimeExtension, timeExtensionCount);
        OnCountChanged?.Invoke(PowerUpType.BlockDestroyer, blockDestroyerCount);
    }

    public int GetCount(PowerUpType type)
    {
        return type switch
        {
            PowerUpType.TimeExtension => timeExtensionCount,
            PowerUpType.BlockDestroyer => blockDestroyerCount,
            _ => 0
        };
    }

    public Sprite GetIcon(PowerUpType type)
    {
        return type switch
        {
            PowerUpType.TimeExtension => timeExtensionIcon,
            PowerUpType.BlockDestroyer => blockDestroyerIcon,
            _ => null
        };
    }

    public bool HasAvailable(PowerUpType type)
    {
        return GetCount(type) > 0;
    }

    public void TryUse(PowerUpType type)
    {
        if (!HasAvailable(type)) return;

        switch (type)
        {
            case PowerUpType.TimeExtension:
                timeExtensionCount--;
                break;
            case PowerUpType.BlockDestroyer:
                blockDestroyerCount--;
                break;
        }
        SaveManager.Instance?.RecordPowerUpUsed(type);
        Save();
        OnCountChanged?.Invoke(type, GetCount(type));
    }
    public void AddPowerUp(PowerUpType type, int amount)
    {
        switch (type)
        {
            case PowerUpType.TimeExtension:
                timeExtensionCount += amount;
                break;
            case PowerUpType.BlockDestroyer:
                blockDestroyerCount += amount;
                break;
        }
        Save();
        OnCountChanged?.Invoke(type, GetCount(type));
    }

    private void Load()
    {
        if (SaveManager.Instance != null)
        {
            var data = SaveManager.Instance.GetPlayerData();
            timeExtensionCount = data.timeExtensionCount;
            blockDestroyerCount = data.blockDestroyerCount;
        }
    }

    private void Save()
    {
        SaveManager.Instance?.SetPowerUpCount(PowerUpType.TimeExtension, timeExtensionCount);
        SaveManager.Instance?.SetPowerUpCount(PowerUpType.BlockDestroyer, blockDestroyerCount);
    }

    [ContextMenu("Add 5 Time Extension")]
    private void DebugAddTimeExtension() => AddPowerUp(PowerUpType.TimeExtension, 5);

    [ContextMenu("Add 5 Block Destroyer")]
    private void DebugAddBlockDestroyer() => AddPowerUp(PowerUpType.BlockDestroyer, 5);
}

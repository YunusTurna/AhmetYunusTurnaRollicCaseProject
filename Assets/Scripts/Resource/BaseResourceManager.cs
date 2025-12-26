using UnityEngine;
using UnityEngine.Events;

public abstract class BaseResourceManager<T> : MonoBehaviour, IResourceProvider, IResourceManager where T : BaseResourceManager<T>
{
    public static T Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private Sprite icon;
    
    [Header("Current Value")]
    [SerializeField] protected int currentAmount;

    [Header("Events")]
    public UnityEvent<int> OnAmountChanged;
    public UnityEvent<int> OnInsufficientAmount;

    public Sprite Icon => icon;
    public int Amount => currentAmount;

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = (T)this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    protected virtual void Start()
    {
        OnAmountChanged?.Invoke(currentAmount);
    }

    public virtual void Add(int amount)
    {
        currentAmount += amount;
        Save();
        OnAmountChanged?.Invoke(currentAmount);
    }

    public virtual void Spend(int amount)
    {
        if (currentAmount < amount) return;

        currentAmount -= amount;
        Save();
        OnAmountChanged?.Invoke(currentAmount);
    }

    public bool HasEnough(int amount) => currentAmount >= amount;
    protected virtual void Load() { }
    protected virtual void Save() { }
}


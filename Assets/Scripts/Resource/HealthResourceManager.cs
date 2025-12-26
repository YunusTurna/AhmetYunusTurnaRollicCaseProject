using UnityEngine;
using UnityEngine.Events;
using System;

public class HealthResourceManager : BaseResourceManager<HealthResourceManager>
{
    [Header("Health Settings")]
    [SerializeField] private int maxHearts = 5;
    [SerializeField] private float heartRegenMinutes = 30f;

    [Header("Timer Event")]
    public UnityEvent<float> OnHeartTimerUpdated;

    private DateTime lastHeartRegenTime;
    private float HeartRegenSeconds => heartRegenMinutes * 60f;

    public int MaxHearts => maxHearts;

    protected override void Load()
    {
        if (SaveManager.Instance != null)
            currentAmount = SaveManager.Instance.GetPlayerData().health;
        else
            currentAmount = maxHearts;
    }

    protected override void Save()
    {
        SaveManager.Instance?.SetHealth(currentAmount);
    }

    protected override void Awake()
    {
        base.Awake();
        lastHeartRegenTime = DateTime.Now; // For now, reset timer on start
    }

    protected override void Start()
    {
        ProcessOfflineRegen();
        base.Start();
    }

    private void Update()
    {
        if (currentAmount < maxHearts)
            ProcessRegen();
    }

    public void FillHealthWithGolds(int amount)
    {
        if (GoldResourceManager.Instance.HasEnough(amount))
        {
            GoldResourceManager.Instance.Spend(amount);
            FillToMax();
        }
        else
        {
            GoldResourceManager.Instance.OnInsufficientAmount?.Invoke(amount);
        }
    }

    public override void Add(int amount)
    {
        currentAmount = Mathf.Min(currentAmount + amount, maxHearts);
        Save();
        OnAmountChanged?.Invoke(currentAmount);

        if (currentAmount == maxHearts)
            OnHeartTimerUpdated?.Invoke(-1f);
    }

    public void FillToMax()
    {
        currentAmount = maxHearts;
        Save();
        OnAmountChanged?.Invoke(currentAmount);
        OnHeartTimerUpdated?.Invoke(-1f);
    }

    public override void Spend(int amount)
    {
        if (currentAmount < amount) return;

        currentAmount -= amount;
        Save();
        OnAmountChanged?.Invoke(currentAmount);
    }

    private void ProcessRegen()
    {
        TimeSpan elapsed = DateTime.Now - lastHeartRegenTime;
        float secondsRemaining = HeartRegenSeconds - (float)elapsed.TotalSeconds;

        if (secondsRemaining <= 0)
        {
            Add(1);
            lastHeartRegenTime = DateTime.Now;
        }
        else
        {
            OnHeartTimerUpdated?.Invoke(secondsRemaining);
        }
    }

    private void ProcessOfflineRegen()
    {
        if (currentAmount >= maxHearts) return;
        // Simplified: no offline regen for now (can add later with SaveManager)
    }

    [ContextMenu("Add 1 Heart")]
    private void DebugAdd() => Add(1);

    [ContextMenu("Spend 1 Heart")]
    private void DebugSpend() => Spend(1);

    [ContextMenu("Spend All Hearts")]
    private void DebugSpendAll() => Spend(currentAmount);

    [ContextMenu("Refill Hearts")]
    private void DebugRefill() => Add(maxHearts);

    [ContextMenu("Reset Hearts")]
    private void DebugReset()
    {
        currentAmount = maxHearts;
        lastHeartRegenTime = DateTime.Now;
        Save();
        OnAmountChanged?.Invoke(currentAmount);
    }
}

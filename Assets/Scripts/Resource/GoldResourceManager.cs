using UnityEngine;

public class GoldResourceManager : BaseResourceManager<GoldResourceManager>
{
    protected override void Load()
    {
        if (SaveManager.Instance != null)
            currentAmount = SaveManager.Instance.GetPlayerData().gold;
        else
            currentAmount = 0;
    }

    protected override void Save()
    {
        SaveManager.Instance?.SetGold(currentAmount);
    }

    public override void Spend(int amount)
    {
        if (currentAmount < amount)
        {
            OnInsufficientAmount?.Invoke(amount);
            return;
        }

        currentAmount -= amount;
        Save();
        OnAmountChanged?.Invoke(currentAmount);
    }

    [ContextMenu("Add 100 Gold")]
    private void DebugAdd() => Add(100);

    [ContextMenu("Spend 100 Gold")]
    private void DebugSpend() => Spend(100);

    [ContextMenu("Reset Gold")]
    private void DebugReset()
    {
        currentAmount = 0;
        Save();
        OnAmountChanged?.Invoke(currentAmount);
    }
}

using UnityEngine;

public class GoldUI : BaseResourceUI<GoldResourceManager>
{
    protected override GoldResourceManager Manager => GoldResourceManager.Instance;

    protected override void Subscribe()
    {
        if (Manager == null || isSubscribed) return;

        Manager.OnAmountChanged.AddListener(UpdateDisplay);
        isSubscribed = true;
        UpdateDisplay(Manager.Amount);
    }

    protected override void Unsubscribe()
    {
        if (Manager == null || !isSubscribed) return;

        Manager.OnAmountChanged.RemoveListener(UpdateDisplay);
        isSubscribed = false;
    }
}


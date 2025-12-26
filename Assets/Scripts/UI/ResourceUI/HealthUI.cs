using UnityEngine;
using TMPro;

public class HealthUI : BaseResourceUI<HealthResourceManager>
{
    [SerializeField] private TMP_Text timerText;

    protected override HealthResourceManager Manager => HealthResourceManager.Instance;

    protected override void Subscribe()
    {
        if (Manager == null || isSubscribed) return;

        Manager.OnAmountChanged.AddListener(UpdateDisplay);
        Manager.OnHeartTimerUpdated.AddListener(UpdateTimerDisplay);
        isSubscribed = true;
        UpdateDisplay(Manager.Amount);
    }

    protected override void Unsubscribe()
    {
        if (Manager == null || !isSubscribed) return;

        Manager.OnAmountChanged.RemoveListener(UpdateDisplay);
        Manager.OnHeartTimerUpdated.RemoveListener(UpdateTimerDisplay);
        isSubscribed = false;
    }

    private void UpdateTimerDisplay(float secondsRemaining)
    {
        if (timerText == null) return;

        if (secondsRemaining < 0)
        {
            timerText.text = "FULL";
            return;
        }

        int minutes = Mathf.FloorToInt(secondsRemaining / 60f);
        int seconds = Mathf.FloorToInt(secondsRemaining % 60f);
        timerText.text = $"{minutes:D2}:{seconds:D2}";
    }
}


using UnityEngine;

public class TimeExtensionPowerUp : BasePowerUp
{
    [Header("Time Extension Settings")]
    [SerializeField] private float additionalSeconds = 5f;
    [SerializeField] private InGameUI inGameUI;

    public override PowerUpType Type => PowerUpType.TimeExtension;

    protected override void Execute()
    {
        if (inGameUI != null)
        {
            inGameUI.AddTime(additionalSeconds);
            Debug.Log($"Time extended by {additionalSeconds} seconds! Remaining: {CurrentCount}");
        }
        else
        {
            Debug.LogWarning("InGameUI not found!");
        }
    }
}

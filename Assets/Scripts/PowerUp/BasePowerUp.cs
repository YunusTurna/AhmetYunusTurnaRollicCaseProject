using UnityEngine;
using UnityEngine.Events;

public abstract class BasePowerUp : MonoBehaviour
{
    public UnityEvent OnPowerUpUsed;
    public UnityEvent OnPowerUpFailed;
    public UnityEvent OnNoPowerUpsAvailable;

    public abstract PowerUpType Type { get; }

    public int CurrentCount => PowerUpManager.Instance != null 
        ? PowerUpManager.Instance.GetCount(Type) 
        : 0;

    public virtual bool CanUse()
    {
        if (PowerUpManager.Instance == null) return false;
        return PowerUpManager.Instance.HasAvailable(Type);
    }

    public void TryUse()
    {
        if (PowerUpManager.Instance == null)
        {
            OnPowerUpFailed?.Invoke();
            return;
        }

        if (!PowerUpManager.Instance.HasAvailable(Type))
        {
            OnNoPowerUpsAvailable?.Invoke();
            return;
        }

        PowerUpManager.Instance.TryUse(Type);

        Execute();
        OnPowerUpUsed?.Invoke();
        return;
    }
    protected abstract void Execute();
}

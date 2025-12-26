using UnityEngine;
using TMPro;
using System.Collections.Generic;

public abstract class BaseResourceUI<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] protected List<TMP_Text> amountText;

    protected bool isSubscribed;

    protected abstract T Manager { get; }

    protected virtual void Start() => Subscribe();
    protected virtual void OnEnable() { if (!isSubscribed) Subscribe(); }
    protected virtual void OnDisable() => Unsubscribe();

    protected abstract void Subscribe();
    protected abstract void Unsubscribe();

    protected virtual void UpdateDisplay(int amount)
    {
        if (amountText == null) return;

        foreach (var text in amountText)
        {
            if (text != null)
                text.SetText(amount.ToString());
        }
    }
    public virtual void ForceUpdateDisplay()
    {
        if (Manager != null)
            UpdateDisplay((Manager as IResourceProvider).Amount);
    }
}

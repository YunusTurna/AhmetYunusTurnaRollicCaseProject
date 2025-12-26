using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PurchaseButtonUI : MonoBehaviour
{
    [Header("Cost")]
    [SerializeField] private PurchaseCostType costType;
    [SerializeField] private ResourceType costResourceType;
    [SerializeField] private PowerUpType costPowerUpType;
    [SerializeField] private int costAmount;
    [SerializeField] private int realMoneyPriceCents = 99;
    [SerializeField] private Sprite realMoneyIcon;

    [Header("Reward")]
    [SerializeField] private PurchaseRewardType rewardType;
    [SerializeField] private ResourceType rewardResourceType;
    [SerializeField] private PowerUpType rewardPowerUpType;
    [SerializeField] private int rewardAmount;

    [Header("UI")]
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image costImage;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private Image rewardImage;
    [SerializeField] private Button purchaseButton;

    [Header("Events")]
    public UnityEvent OnPurchaseSuccess;
    public UnityEvent OnPurchaseFailed;
    public UnityEvent<int> OnRealMoneyPurchaseRequested;

    void Start()
    {
        UpdateUI();
        purchaseButton.onClick.AddListener(TryPurchase);
    }

    void UpdateUI()
    {
        if (costText != null)
            costText.SetText(costType == PurchaseCostType.RealMoney ? $"${realMoneyPriceCents / 100f:F2}" : $"{costAmount}");
        
        if (costImage != null)
            costImage.sprite = GetCostIcon();
        
        if (rewardText != null)
            rewardText.SetText($"{rewardAmount}");
        
        if (rewardImage != null)
            rewardImage.sprite = GetRewardIcon();
    }

    Sprite GetCostIcon() => costType switch
    {
        PurchaseCostType.Resource => GetResourceManager(costResourceType)?.Icon,
        PurchaseCostType.PowerUp => PowerUpManager.Instance?.GetIcon(costPowerUpType),
        PurchaseCostType.RealMoney => realMoneyIcon,
        _ => null
    };

    Sprite GetRewardIcon() => rewardType switch
    {
        PurchaseRewardType.Resource => GetResourceManager(rewardResourceType)?.Icon,
        PurchaseRewardType.PowerUp => PowerUpManager.Instance?.GetIcon(rewardPowerUpType),
        PurchaseRewardType.HealthFill => HealthResourceManager.Instance?.Icon,
        _ => null
    };

    void TryPurchase()
    {
        if (!CanAfford()) { OnPurchaseFailed?.Invoke(); return; }

        SpendCost();
        GiveReward();
        OnPurchaseSuccess?.Invoke();
    }

    bool CanAfford() => costType switch
    {
        PurchaseCostType.Free => true,
        PurchaseCostType.RealMoney => true,
        PurchaseCostType.Resource => GetResourceManager(costResourceType)?.HasEnough(costAmount) ?? false,
        PurchaseCostType.PowerUp => PowerUpManager.Instance?.GetCount(costPowerUpType) >= costAmount,
        _ => false
    };

    void SpendCost()
    {
        switch (costType)
        {
            case PurchaseCostType.Resource:
                GetResourceManager(costResourceType)?.Spend(costAmount);
                break;
            case PurchaseCostType.PowerUp:
                for (int i = 0; i < costAmount; i++)
                    PowerUpManager.Instance?.TryUse(costPowerUpType);
                break;
            case PurchaseCostType.RealMoney:
                OnRealMoneyPurchaseRequested?.Invoke(realMoneyPriceCents);
                break;
        }
    }

    void GiveReward()
    {
        switch (rewardType)
        {
            case PurchaseRewardType.Resource:
                GetResourceManager(rewardResourceType)?.Add(rewardAmount);
                break;
            case PurchaseRewardType.PowerUp:
                PowerUpManager.Instance?.AddPowerUp(rewardPowerUpType, rewardAmount);
                break;
            case PurchaseRewardType.HealthFill:
                HealthResourceManager.Instance?.FillToMax();
                break;
        }
    }

    IResourceManager GetResourceManager(ResourceType type) => type switch
    {
        ResourceType.Gold => GoldResourceManager.Instance,
        ResourceType.Health => HealthResourceManager.Instance,
        _ => null
    };
}

public enum PurchaseCostType { Free, Resource, PowerUp, RealMoney }
public enum PurchaseRewardType { Resource, PowerUp, HealthFill }

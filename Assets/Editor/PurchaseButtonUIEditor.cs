using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PurchaseButtonUI))]
[CanEditMultipleObjects]
public class PurchaseButtonUIEditor : Editor
{
    SerializedProperty costType;
    SerializedProperty costResourceType;
    SerializedProperty costPowerUpType;
    SerializedProperty costAmount;
    SerializedProperty realMoneyPriceCents;
    SerializedProperty realMoneyIcon;

    SerializedProperty rewardType;
    SerializedProperty rewardResourceType;
    SerializedProperty rewardPowerUpType;
    SerializedProperty rewardAmount;

    SerializedProperty costText;
    SerializedProperty costImage;
    SerializedProperty rewardText;
    SerializedProperty rewardImage;
    SerializedProperty purchaseButton;

    SerializedProperty onPurchaseSuccess;
    SerializedProperty onPurchaseFailed;
    SerializedProperty onRealMoneyPurchaseRequested;

    void OnEnable()
    {
        costType = serializedObject.FindProperty("costType");
        costResourceType = serializedObject.FindProperty("costResourceType");
        costPowerUpType = serializedObject.FindProperty("costPowerUpType");
        costAmount = serializedObject.FindProperty("costAmount");
        realMoneyPriceCents = serializedObject.FindProperty("realMoneyPriceCents");
        realMoneyIcon = serializedObject.FindProperty("realMoneyIcon");

        rewardType = serializedObject.FindProperty("rewardType");
        rewardResourceType = serializedObject.FindProperty("rewardResourceType");
        rewardPowerUpType = serializedObject.FindProperty("rewardPowerUpType");
        rewardAmount = serializedObject.FindProperty("rewardAmount");

        costText = serializedObject.FindProperty("costText");
        costImage = serializedObject.FindProperty("costImage");
        rewardText = serializedObject.FindProperty("rewardText");
        rewardImage = serializedObject.FindProperty("rewardImage");
        purchaseButton = serializedObject.FindProperty("purchaseButton");

        onPurchaseSuccess = serializedObject.FindProperty("OnPurchaseSuccess");
        onPurchaseFailed = serializedObject.FindProperty("OnPurchaseFailed");
        onRealMoneyPurchaseRequested = serializedObject.FindProperty("OnRealMoneyPurchaseRequested");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Cost Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(costType);

        PurchaseCostType selectedCostType = (PurchaseCostType)costType.enumValueIndex;
        
        switch (selectedCostType)
        {
            case PurchaseCostType.Resource:
                EditorGUILayout.PropertyField(costResourceType);
                EditorGUILayout.PropertyField(costAmount);
                break;
            case PurchaseCostType.PowerUp:
                EditorGUILayout.PropertyField(costPowerUpType);
                EditorGUILayout.PropertyField(costAmount);
                break;
            case PurchaseCostType.RealMoney:
                EditorGUILayout.PropertyField(realMoneyPriceCents, new GUIContent("Price (Cents)"));
                EditorGUILayout.PropertyField(realMoneyIcon);
                
                // Preview price
                float dollars = realMoneyPriceCents.intValue / 100f;
                EditorGUILayout.HelpBox($"Display: ${dollars:F2}", MessageType.Info);
                break;
            case PurchaseCostType.Free:
                EditorGUILayout.HelpBox("No cost required", MessageType.Info);
                break;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Reward Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(rewardType);

        PurchaseRewardType selectedRewardType = (PurchaseRewardType)rewardType.enumValueIndex;
        
        switch (selectedRewardType)
        {
            case PurchaseRewardType.Resource:
                EditorGUILayout.PropertyField(rewardResourceType);
                EditorGUILayout.PropertyField(rewardAmount);
                break;
            case PurchaseRewardType.PowerUp:
                EditorGUILayout.PropertyField(rewardPowerUpType);
                EditorGUILayout.PropertyField(rewardAmount);
                break;
            case PurchaseRewardType.HealthFill:
                EditorGUILayout.HelpBox("Fills health to maximum", MessageType.Info);
                break;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("UI References", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(costText);
        EditorGUILayout.PropertyField(costImage);
        EditorGUILayout.PropertyField(rewardText);
        EditorGUILayout.PropertyField(rewardImage);
        EditorGUILayout.PropertyField(purchaseButton);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(onPurchaseSuccess);
        EditorGUILayout.PropertyField(onPurchaseFailed);
        
        if (selectedCostType == PurchaseCostType.RealMoney)
            EditorGUILayout.PropertyField(onRealMoneyPurchaseRequested);

        serializedObject.ApplyModifiedProperties();
    }
}

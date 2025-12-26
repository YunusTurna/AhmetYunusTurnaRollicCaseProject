using UnityEngine;
using TMPro;

public class BlockVisualController : MonoBehaviour
{
    [Header("Effect Display")]
    [SerializeField] private TMP_Text effectText;
    
    [Header("Movement Indicator")]
    [SerializeField] private TMP_Text movementIndicatorText;

    private BlockData blockData;

    public void Initialize(BlockData data)
    {
        blockData = data;
        UpdateDirectionVisuals();
    }

    public void UpdateDirectionVisuals()
    {
        if (blockData == null || movementIndicatorText == null) return;

        bool showArrows = blockData.moveDirection != MoveDirection.None && blockData.moveDirection != MoveDirection.All;
        movementIndicatorText.gameObject.SetActive(showArrows);
        
        if (showArrows)
        {
            string symbol = blockData.moveDirection == MoveDirection.Horizontal ? "H" : "V";
            movementIndicatorText.SetText(symbol);
        }
    }



    public void SetEffectText(string text)
    {
        if (effectText != null)
        {
            effectText.gameObject.SetActive(true);
            effectText.SetText(text);
        }
    }

    public void HideEffectText()
    {
        if (effectText != null)
            effectText.gameObject.SetActive(false);
    }

    public void SetMoveDirection(MoveDirection direction)
    {
        if (blockData != null)
            blockData.moveDirection = direction;
        
        UpdateDirectionVisuals();
    }

    public void ResetController()
    {
        HideEffectText();
        if (movementIndicatorText != null)
            movementIndicatorText.gameObject.SetActive(false);
        blockData = null;
    }
}

using UnityEngine;
using TMPro;

public class LevelNodeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text levelIndexText;
    [SerializeField] private GameObject lockIcon;

    public void SetLevel(int levelIndex, bool isLocked)
    {
        if (levelIndexText != null)
            levelIndexText.text = levelIndex.ToString();

        if (lockIcon != null)
            lockIcon.SetActive(isLocked);
    }
}

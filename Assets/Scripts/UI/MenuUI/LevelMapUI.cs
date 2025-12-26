using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class LevelMapUI : MonoBehaviour
{
    [Header("Node References")]
    [SerializeField] private List<LevelNodeUI> levelNodes = new();
    [SerializeField] private TMP_Text levelTitleText;
    [SerializeField] private TMP_Text levelDifficultyText;
    [SerializeField] private TMP_Text levelPopPanelText;

    [Header("Settings")]
    [SerializeField] private int currentNodeIndex = 0;

    private void Start()
    {
        UpdateAllNodes();
    }

    public void UpdateAllNodes()
    {
        if (LevelManager.Instance == null || LevelManager.Instance.allLevels == null) return;

        var allLevels = LevelManager.Instance.allLevels;
        int currentLevelIndex = LevelManager.Instance.currentLevelData != null 
            ? LevelManager.Instance.currentLevelData.LevelIndex 
            : 1;

        int startLevel = currentLevelIndex - currentNodeIndex;

        for (int i = 0; i < levelNodes.Count; i++)
        {
            int levelIndex = startLevel + i;
            var levelData = allLevels.Find(l => l.LevelIndex == levelIndex);

            if (levelData != null)
            {
                levelNodes[i].gameObject.SetActive(true);
                levelNodes[i].SetLevel(levelIndex, levelData.Status == LevelStatus.Locked);
            }
            else
            {
                levelNodes[i].gameObject.SetActive(false);
            }
        }

        UpdateLevelInfo();
    }

    private void UpdateLevelInfo()
    {
        if (LevelManager.Instance == null || LevelManager.Instance.currentLevelData == null) return;

        var data = LevelManager.Instance.currentLevelData;

        if (levelTitleText != null)
            levelTitleText.SetText($"Level {data.LevelIndex}");

        if (levelDifficultyText != null)
            levelDifficultyText.SetText(data.Difficulty.ToString().ToUpper());

        if (levelPopPanelText != null)
            levelPopPanelText.SetText($"Level {data.LevelIndex}");
    }

    public void RefreshNodes()
    {
        UpdateAllNodes();
    }
}

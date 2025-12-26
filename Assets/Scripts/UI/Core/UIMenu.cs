using System.Collections.Generic;
using UnityEngine;

public class UIMenu : MonoBehaviour
{
    [Header("Menu Settings")]
    [SerializeField] private string menuId;
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private bool useAnimation = true;

    [Header("Panels")]
    [SerializeField] private List<UIPanel> panels = new();
    [SerializeField] private UIPanel defaultPanel;

    private UIPanel currentPanel;
    private Stack<UIPanel> panelHistory = new();

    public string MenuId => menuId;
    public bool IsVisible => menuRoot != null && menuRoot.activeSelf;
    public UIPanel CurrentPanel => currentPanel;

    public void Show()
    {
        if (menuRoot != null)
            menuRoot.SetActive(true);

        panelHistory.Clear();
        
        if (defaultPanel != null)
            ShowPanel(defaultPanel.PanelId);
    }

    public void Hide()
    {
        CloseAllPanels();
        panelHistory.Clear();

        if (menuRoot != null)
            menuRoot.SetActive(false);
    }

    public void ShowPanel(string panelId)
    {
        var panel = GetPanel(panelId);
        if (panel == null) return;

        if (currentPanel != null && currentPanel != panel)
        {
            panelHistory.Push(currentPanel);
        }

        currentPanel = panel;
        currentPanel.Show(!useAnimation);
    }

    public void ShowPanelAdditive(string panelId)
    {
        var panel = GetPanel(panelId);
        if (panel == null) return;

        if (currentPanel != null)
            panelHistory.Push(currentPanel);

        currentPanel = panel;
        currentPanel.Show(!useAnimation);
    }

    public void ClosePanel(string panelId)
    {
        var panel = GetPanel(panelId);
        if (panel == null) return;

        panel.Hide(!useAnimation);

        if (currentPanel == panel)
        {
            currentPanel = panelHistory.Count > 0 ? panelHistory.Pop() : null;
        }
    }

    public void GoBack()
    {
        if (panelHistory.Count == 0) return;

        currentPanel?.Hide(!useAnimation);
        currentPanel = panelHistory.Pop();
        currentPanel?.Show(!useAnimation);
    }

    public void CloseAllPanels()
    {
        foreach (var panel in panels)
        {
            if (panel != null)
                panel.Hide(!useAnimation);
        }
        currentPanel = null;
    }

    public UIPanel GetPanel(string panelId)
    {
        return panels.Find(p => p.PanelId == panelId);
    }

    public bool HasPanel(string panelId)
    {
        return GetPanel(panelId) != null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(menuId))
            menuId = gameObject.name;
    }
#endif
}

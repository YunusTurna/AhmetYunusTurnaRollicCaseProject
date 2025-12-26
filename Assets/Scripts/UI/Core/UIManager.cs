using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Menus")]
    [SerializeField] private List<UIMenu> menus = new();
    [SerializeField] private UIMenu defaultMenu;

    private UIMenu currentMenu;

    public UIMenu CurrentMenu => currentMenu;
    public UnityEvent<string> OnMenuChanged = new();
    public UnityEvent<string> OnPanelChanged = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (defaultMenu != null)
            ShowMenu(defaultMenu.MenuId);
    }

    public void ShowMenu(string menuId)
    {
        var menu = GetMenu(menuId);
        if (menu == null) return;

        if (currentMenu != null && currentMenu != menu)
            currentMenu.Hide();

        currentMenu = menu;
        currentMenu.Show();

        OnMenuChanged?.Invoke(menuId);
    }

    public void ShowPanel(string panelId)
    {
        currentMenu?.ShowPanel(panelId);
        OnPanelChanged?.Invoke(panelId);
    }

    public void ShowPanelAdditive(string panelId)
    {
        currentMenu?.ShowPanelAdditive(panelId);
        OnPanelChanged?.Invoke(panelId);
    }

    public void ClosePanel(string panelId)
    {
        currentMenu?.ClosePanel(panelId);
    }

    public void GoBack()
    {
        currentMenu?.GoBack();
    }

    public UIMenu GetMenu(string menuId)
    {
        return menus.Find(m => m.MenuId == menuId);
    }

    public UIPanel GetPanel(string panelId)
    {
        foreach (var menu in menus)
        {
            var panel = menu.GetPanel(panelId);
            if (panel != null)
                return panel;
        }
        return null;
    }

}

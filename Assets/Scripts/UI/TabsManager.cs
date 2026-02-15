using System.Collections.Generic;
using UnityEngine;

public class TabsManager : MonoBehaviour
{
    [SerializeField] private List<TabPanel> panels = new List<TabPanel>();

    private TabPanel currentOpenTab;

    /// <summary>
    /// Open a tab panel. Automatically closes any other open panel.
    /// </summary>
    public void OpenTab(TabPanel tab)
    {
        if (currentOpenTab == tab)
        {
            currentOpenTab.Close();
            return;
        }

        // Close previous
        if (currentOpenTab != null)
        {
            currentOpenTab.Close();
        }

        // Open new
        currentOpenTab = tab;

        if (currentOpenTab != null)
        {
            currentOpenTab.Open();
        }
    }

    /// <summary>
    /// Close the currently open tab
    /// </summary>
    public void CloseCurrentTab()
    {
        if (currentOpenTab != null)
        {
            currentOpenTab.Close();
            currentOpenTab = null;
        }
    }

    public void CloseAllTabs()
    {
        foreach(TabPanel tab in panels)
        {
            tab.Close();
        }
    }

    public void OpenDefualtTab(TabPanel tab)
    {
        if (currentOpenTab == tab)
        {
            return;
        }

        // Close previous
        if (currentOpenTab != null)
        {
            currentOpenTab.Close();
        }

        // Open new
        currentOpenTab = tab;

        if (currentOpenTab != null)
        {
            currentOpenTab.Open();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    public static UIManager Instance;
    [SerializeField] private TabsManager _TabsManager;

    private void OnEnable()
    {
        HumanSimulationManager.StartSimulation += StartSimulation;
    }
    private void OnDisable()
    {
        HumanSimulationManager.StartSimulation -= StartSimulation;
    }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        _TabsManager.gameObject.SetActive(true);
    }

    private void StartSimulation()
    {
        _TabsManager.CloseAllTabs();
        _TabsManager.gameObject.SetActive(false);
    }

    public void OpenTab(TabPanel tab)
    {
        _TabsManager.OpenDefualtTab(tab);
    }

}

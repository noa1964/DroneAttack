using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private TabsManager _TabsManager;
    [SerializeField] private List<SimulationTab> _SimulationTabs;

    [Header("Pause Button")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Image pauseButtonImage;
    [SerializeField] private Sprite playSprite;
    [SerializeField] private Sprite pauseSprite;

    private bool isPaused = false;

    private void OnEnable()
    {
        HumanSimulationManager.StartSimulation += StartSimulation;
    }

    private void OnDisable()
    {
        HumanSimulationManager.StartSimulation -= StartSimulation;
    }

    void Start()
    {
        Instance = this;
        _TabsManager.gameObject.SetActive(true);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);

        // Set initial button sprite
        if (pauseButtonImage != null)
            pauseButtonImage.sprite = pauseSprite;
    }

    private void StartSimulation()
    {
        _TabsManager.CloseAllTabs();

        foreach (SimulationTab simulationTab in _SimulationTabs)
            simulationTab.tab.gameObject.SetActive(simulationTab.isInSimulation);
    }

    public void OpenTab(TabPanel tab)
    {
        _TabsManager.OpenDefualtTab(tab);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        TimeManager.Instance.SetPause(isPaused);

        if (pauseButtonImage != null)
            pauseButtonImage.sprite = isPaused ? playSprite : pauseSprite;
    }
}

[System.Serializable]
public class SimulationTab
{
    public TabPanel tab;
    public bool isInSimulation;
}
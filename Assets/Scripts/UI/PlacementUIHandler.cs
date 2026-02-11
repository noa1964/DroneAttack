using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlacementUIHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _PanleRef;
    [SerializeField] private TMP_Dropdown _FloorDropdown;
    [SerializeField] private TMP_Dropdown _EnemyTypeDropdown;

    [Header("Enemy Types (Enum)")]
    [SerializeField]
    private List<EnemyType> enemyTypes =
        new List<EnemyType>() { EnemyType.EnemyA, EnemyType.EnemyB, EnemyType.EnemyC };

    private Building _selectedBuilding;
    private List<Floor> _floors = new List<Floor>();

    private EnemyType _selectedEnemy;

    #region Unity

    private void OnEnable()
    {
        PlacementManager.SelectedBuildingUpdate += UpdateFloors;
    }

    private void OnDisable()
    {
        PlacementManager.SelectedBuildingUpdate -= UpdateFloors;
    }

    private void Start()
    {
        SetupEnemyDropdown();
        SetupFloorDropdown();
    }

    #endregion

    #region Setup

    private void SetupEnemyDropdown()
    {
        _EnemyTypeDropdown.ClearOptions();

        List<string> options = new List<string>();
        foreach (var type in enemyTypes)
            options.Add(type.ToString());

        _EnemyTypeDropdown.AddOptions(options);
        _EnemyTypeDropdown.onValueChanged.AddListener(OnEnemyTypeChanged);

        // Default selection
        if (enemyTypes.Count > 0)
            PlacementManager.Instance.selectedEnemyType = enemyTypes[0];

        OnEnemyTypeChanged(0);
    }

    private void SetupFloorDropdown()
    {
        _FloorDropdown.ClearOptions();
        _FloorDropdown.interactable = false;
        _FloorDropdown.onValueChanged.AddListener(OnFloorSelected);
    }

    #endregion

    #region Building Update

    private void UpdateFloors(Building building)
    {
        if (building == null)
        {
            _FloorDropdown.ClearOptions();
            _FloorDropdown.interactable = false;
            return;
        }

        _selectedBuilding = building;
        _floors = building._Floors;

        _FloorDropdown.ClearOptions();

        List<string> floorOptions = new List<string>();
        for (int i = 0; i < _floors.Count; i++)
        {
            floorOptions.Add("Floor " + (i + 1));
        }

        _FloorDropdown.AddOptions(floorOptions);
        _FloorDropdown.interactable = _floors.Count > 0;

        if (_floors.Count > 0)
        {
            _FloorDropdown.value = 0;
            _FloorDropdown.RefreshShownValue();
            OnFloorSelected(0);
        }
    }


    public void ConfirmBtn()
    {
        _PanleRef.SetActive(false);
        Debug.Log("start simulation");
        HumanSimulationManager.StartSimulation?.Invoke();
    }

    public EnemyType GetSelectedType()
    {
        return _selectedEnemy;
    }

    #endregion

    #region Dropdown Events

    private void OnFloorSelected(int index)
    {
        if (_selectedBuilding == null) return;
        if (index < 0 || index >= _floors.Count) return;

        _selectedBuilding.EnableOnlyFloor(index);

        PlacementManager.Instance.SetActiveFloor(_floors[index]._Ground);
    }

    private void OnEnemyTypeChanged(int index)
    {
        if (index < 0 || index >= enemyTypes.Count) return;

        PlacementManager.Instance.selectedEnemyType = enemyTypes[index];
        Debug.Log("Selected enemy: " + enemyTypes[index]);
        _selectedEnemy = enemyTypes[index];
    }

    #endregion

}

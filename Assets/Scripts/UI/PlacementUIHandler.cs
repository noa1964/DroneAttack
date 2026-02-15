using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;

public class PlacementUIHandler : MonoBehaviour
{
    
    [Header("UI References")]
    [SerializeField] private GameObject _PanleRef;
    [SerializeField] private TMP_Dropdown _FloorDropdown;
    [SerializeField] private TMP_Dropdown _EnemyTypeDropdown;
    [SerializeField] private TMP_Dropdown _WeaponTypeDropdown;
    [SerializeField] private GameObject _BlockPanel;
    [Header("Display")]
    [SerializeField] private List<EnemyInfo> _Enemies;
    [SerializeField] private List<Weapon> _Weapons;

    private Building _selectedBuilding;
    private List<Floor> _floors = new List<Floor>();

    private EnemyType _selectedEnemy;
    private WeaponType _selectedWeapon;

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
        SetupWeaponDropdown();
        SetupFloorDropdown();
        _BlockPanel.SetActive(true);
       // _PanleRef.SetActive(true);
    }

    #endregion

    #region Setup

    private void SetupEnemyDropdown()
    {
        _EnemyTypeDropdown.ClearOptions();

        var enumNames = Enum.GetNames(typeof(EnemyType)).ToList();

        _EnemyTypeDropdown.AddOptions(enumNames);
        _EnemyTypeDropdown.onValueChanged.AddListener(OnEnemyTypeChanged);


        OnEnemyTypeChanged(0);
    }

    private void SetupWeaponDropdown()
    {
        _WeaponTypeDropdown.ClearOptions();

        var enumNames = Enum.GetNames(typeof(WeaponType)).ToList();

        _WeaponTypeDropdown.AddOptions(enumNames);
        _WeaponTypeDropdown.onValueChanged.AddListener(OnWeaponTypeChanged);


        OnWeaponTypeChanged(0);
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
        UIManager.Instance.OpenTab(GetComponent<TabPanel>());
        _BlockPanel.SetActive(false);
        _PanleRef.SetActive(true);
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

    public EnemyType GetSelectedEnemyType()
    {
        return _selectedEnemy;
    }

    public WeaponType GetSelectedWeaponType()
    {
        return _selectedWeapon;
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
        PlacementManager.Instance.selectedEnemyType = (EnemyType)index;
        Debug.Log("Selected enemy: " + (EnemyType)index);
        _selectedEnemy = (EnemyType)index;
        SetActiveEnemyDisplay(_selectedEnemy);

    }

    private void OnWeaponTypeChanged(int index)
    {
        PlacementManager.Instance.selectedWeaponType = (WeaponType)index;
        Debug.Log("Selected enemy: " + (WeaponType)index);
        _selectedWeapon = (WeaponType)index;
        SetActiveWeaponDisplay(_selectedWeapon);
    }

    public void SetActiveEnemyDisplay(EnemyType typeToActivate)
    {
        foreach (var enemy in _Enemies)
        {
            if (enemy.enemyObj != null)
            {
                enemy.enemyObj.gameObject.SetActive(enemy.type == typeToActivate);
            }
        }
    }

    public void SetActiveWeaponDisplay(WeaponType typeToActivate)
    {
        foreach (var weapon in _Weapons)
        {
            if (weapon.weaponObj != null)
            {
                weapon.weaponObj.SetActive(weapon.type == typeToActivate);
            }
        }
    }

    #endregion

}

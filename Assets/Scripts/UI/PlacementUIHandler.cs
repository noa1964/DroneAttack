using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlacementUIHandler : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown floorDropdown;
    public TMP_Dropdown enemyTypeDropdown;

    [Header("Data")]
    public List<EnemyType> enemyTypes = new List<EnemyType>() { EnemyType.EnemyA, EnemyType.EnemyB, EnemyType.EnemyC };

    private Building _selectedBuilding;

    private void Start()
    {
        // Populate enemy type dropdown
        enemyTypeDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (var type in enemyTypes)
            options.Add(type.ToString());

        enemyTypeDropdown.AddOptions(options);
        enemyTypeDropdown.onValueChanged.AddListener(OnEnemyTypeChanged);

        floorDropdown.onValueChanged.AddListener(OnFloorSelected);
    }

    public void SetSelectedBuilding(Building building)
    {
        _selectedBuilding = building;

        floorDropdown.ClearOptions();
        List<string> floorOptions = new List<string>();
        for (int i = 0; i < building._Floors.Count; i++)
            floorOptions.Add("Floor " + (i + 1));

        floorDropdown.AddOptions(floorOptions);
        floorDropdown.value = 0;
        OnFloorSelected(0);
    }

    private void OnFloorSelected(int index)
    {
        if (_selectedBuilding == null) return;

        _selectedBuilding.EnableOnlyFloor(index);
        PlacementManager.Instance.SetActiveFloor(_selectedBuilding._Floors[index]._Ground);
    }

    private void OnEnemyTypeChanged(int index)
    {
        PlacementManager.Instance.selectedEnemyType = enemyTypes[index];
        Debug.Log("Selected enemy: " + enemyTypes[index]);
    }
}

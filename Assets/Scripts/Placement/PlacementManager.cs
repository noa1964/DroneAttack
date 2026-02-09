using System;
using System.Collections.Generic;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;
    public GameObject dotPrefab; // prefab of your dot
    public int maxDotsPerFloor = 3;

    [HideInInspector] public EnemyType selectedEnemyType; // Set from UI

    private Ground _activeFloor;
    private Building _selectedBuilding;

    [SerializeField] private Camera _PlacementCamera;
    // Track dots per floor
    private Dictionary<Ground, List<GameObject>> _dotsPerFloor = new Dictionary<Ground, List<GameObject>>();


    public static Action<Building> SelectedBuildingUpdate;

    
    private void Awake()
    {
        Instance = this;
    }

    public void SetSelectedBuilding(Building building, Ground ground)
    {
        _selectedBuilding = building;
        SetActiveFloor(ground);
        SelectedBuildingUpdate?.Invoke(building);
    }

    public void SetActiveFloor(Ground ground)
    {
        if (_selectedBuilding == null || !_selectedBuilding.IsFloorOnTheBuilding(ground))
            return;

        _activeFloor = ground;
    }

    public void TryPlaceOnFloor(Ground ground, Vector3 worldPosition)
    {
        Debug.Log("click on   " + (_selectedBuilding == null) + "  " + (_activeFloor == null) + "  " + (ground != _activeFloor));
        if (_selectedBuilding == null || _activeFloor == null || ground != _activeFloor)
            return;

        if (!_dotsPerFloor.ContainsKey(ground))
            _dotsPerFloor[ground] = new List<GameObject>();

        if (_dotsPerFloor[ground].Count >= maxDotsPerFloor)
        {
            Debug.Log("Max dots reached on this floor");
            return;
        }

        // Spawn dot
        GameObject dot = Instantiate(dotPrefab, worldPosition, Quaternion.identity);
        var dotScript = dot.GetComponent<PlacementDot>();
        if (dotScript != null)
            dotScript.SetEnemyType(selectedEnemyType);

        _dotsPerFloor[ground].Add(dot);
    }

    public void RemoveDot(PlacementDot dot)
    {
        foreach (var kvp in _dotsPerFloor)
        {
            if (kvp.Value.Contains(dot.gameObject))
            {
                kvp.Value.Remove(dot.gameObject);
                break;
            }
        }
    }

    public List<GameObject> GetDots(Ground ground)
    {
        if (!_dotsPerFloor.ContainsKey(ground))
            return new List<GameObject>();

        return _dotsPerFloor[ground];
    }

    public Camera GetCamera()
    {
        return _PlacementCamera;
    }
}

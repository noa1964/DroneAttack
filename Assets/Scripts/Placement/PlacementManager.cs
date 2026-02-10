using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;
    public GameObject dotPrefab; // prefab of your dot
    public int maxDotsPerFloor = 3;

    [HideInInspector] public EnemyType selectedEnemyType; // Set from UI

    private Ground _activeFloor;
    private Building _selectedBuilding;

    [SerializeField] private PlacementUIHandler _PlacementUIHandler;
    [SerializeField] private Camera _PlacementCamera;
    [SerializeField] private Camera _MainCamera;
    // Track dots per floor
    private Dictionary<Ground, List<GameObject>> _dotsPerFloor = new Dictionary<Ground, List<GameObject>>();


    public static Action<Building> SelectedBuildingUpdate;

    private void OnEnable()
    {
        HumanSimulationManager.StartSimulation += StartSimulation;
    }
    private void OnDisable()
    {
        HumanSimulationManager.StartSimulation -= StartSimulation;
    }



    private void Awake()
    {
        Instance = this;
    }

    public void SetSelectedBuilding(Building building, Ground ground)
    {
        _selectedBuilding = building;
        SetActiveFloor(ground);
        SelectedBuildingUpdate?.Invoke(building);
        _MainCamera.gameObject.SetActive(false);
        _PlacementCamera.gameObject.SetActive(true);

        Transform target = _selectedBuilding.GetCamTransform();

        _PlacementCamera.transform.position = target.position;
        _PlacementCamera.transform.rotation = target.rotation;
    }

    public void SetActiveFloor(Ground ground)
    {
        if (_selectedBuilding == null || !_selectedBuilding.IsFloorOnTheBuilding(ground))
            return;

        _activeFloor = ground;
    }

    public void TryPlaceOnFloor(Ground ground, Vector3 worldPosition)
    {
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
            dotScript.Initialize(_PlacementCamera, _PlacementUIHandler.GetSelectedType(), _activeFloor.gameObject);

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

    private void StartSimulation()
    {
        _MainCamera.gameObject.SetActive(true);
        _PlacementCamera.gameObject.SetActive(false);
    }

    public List<GameObject> GetDots(Ground ground)
    {
        if (!_dotsPerFloor.ContainsKey(ground))
            return new List<GameObject>();

        return _dotsPerFloor[ground];
    }

    public List<GameObject> GetAllDotsPositions()
    {
        return _dotsPerFloor.Values.SelectMany(list => list).ToList();
    }

    public Dictionary<Ground, List<GameObject>> GetDictionaryDots()
    {
        return _dotsPerFloor;
    }



    public Camera GetCamera()
    {
        return _PlacementCamera;
    }
}

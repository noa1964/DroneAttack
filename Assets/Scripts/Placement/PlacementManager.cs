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
    [HideInInspector] public WeaponType selectedWeaponType; // Set from UI

    private Ground _activeFloor;
    private Building _selectedBuilding;

    [SerializeField] private PlacementUIHandler _PlacementUIHandler;
    [SerializeField] private Camera _PlacementCamera;
    [SerializeField] private Camera _MainCamera;
    // Track dots per floor
    private Dictionary<Ground, List<DotInfo>> _dotsPerFloor = new Dictionary<Ground, List<DotInfo>>();

    private List<GameObject> _displayDots;
    
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

    public void TryPlaceOnFloor(Ground ground, Vector3 worldPosition, Vector3 point)
    {
        if (_selectedBuilding == null || _activeFloor == null || ground != _activeFloor)
            return;

        if (!_dotsPerFloor.ContainsKey(ground))
            _dotsPerFloor[ground] = new List<DotInfo>();

        if (_dotsPerFloor[ground].Count >= maxDotsPerFloor)
        {
            Debug.Log("Max dots reached on this floor");
            return;
        }

        // Spawn dot
        //GameObject dotDisplay = Instantiate(dotPrefab, point, Quaternion.identity);
        GameObject dot = Instantiate(dotPrefab, point, Quaternion.identity);
        var dotScript = dot.GetComponent<PlacementDot>();
        if (dotScript != null)
            dotScript.Initialize(_PlacementCamera, _PlacementUIHandler.GetSelectedEnemyType(), _PlacementUIHandler.GetSelectedWeaponType(), _activeFloor.gameObject);

        //_displayDots.Add(dotDisplay);
        _dotsPerFloor[ground].Add(new DotInfo (dot, worldPosition));
    }

    public void RemoveDot(PlacementDot dot)
    {
        foreach (var kvp in _dotsPerFloor)
        {
            kvp.Value.RemoveAll(d => d.Dot == dot.gameObject);
        }
    }

    private void StartSimulation()
    {
        _MainCamera.gameObject.SetActive(true);
        _PlacementCamera.gameObject.SetActive(false);
    }

    public List<DotInfo> GetDots(Ground ground)
    {
        if (!_dotsPerFloor.ContainsKey(ground))
            return new List<DotInfo>();

        return _dotsPerFloor[ground];
    }

    public List<DotInfo> GetAllDotsPositions()
    {
        return _dotsPerFloor.Values.SelectMany(list => list).ToList();
    }

    public Dictionary<Ground, List<DotInfo>> GetDictionaryDots()
    {
        return _dotsPerFloor;
    }

    public bool HasAnyDot()
    {
        // Check each floor's list for at least one dot
        return _dotsPerFloor.Values.Any(list => list.Count > 0);
    }

    public Camera GetCamera()
    {
        return _PlacementCamera;
    }
}

public class DotInfo
{
    public GameObject Dot { get; private set; }
    public Vector3 Position { get; private set; }

    public DotInfo(GameObject dot, Vector3 position)
    {
        Dot = dot;
        Position = position;
    }

}

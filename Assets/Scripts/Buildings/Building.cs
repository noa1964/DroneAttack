using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Building : MonoBehaviour, IPointerClickHandler
{
    public BuildingConfig buildingConf;
    public List<Floor> _Floors;

    [SerializeField] private Transform _CamTransform;

    // Store the last clicked point on the building after simulation starts
    public static Vector3 lastClickedPoint;

    // Track whether simulation has started
    private bool simulationStarted = false;

    private void OnEnable()
    {
        PlacementManager.SelectedBuildingUpdate += SetActiveCollider;
        HumanSimulationManager.StartSimulation += StartSimulation;
    }

    private void OnDisable()
    {
        PlacementManager.SelectedBuildingUpdate -= SetActiveCollider;
        HumanSimulationManager.StartSimulation -= StartSimulation;
    }

    public void EnableOnlyFloor(int index)
    {
        for (int i = 0; i < _Floors.Count; i++)
        {
            _Floors[i].SetSelectable(i == index);
            _Floors[i].floorObj.SetActive(i == index);
        }
    }

    private void StartSimulation()
    {
        simulationStarted = true;

        // Disable the building collider so it won't block anything
        GetComponent<BoxCollider>().enabled = false;

        // Enable all floors and make them non-selectable
        for (int i = 0; i < _Floors.Count; i++)
        {
            _Floors[i].floorObj.SetActive(true);
            _Floors[i].SetSelectable(false);
            //_Floors[i].SetWallsCollider(true);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!simulationStarted)
        {
            // Normal behavior before simulation
            Debug.Log("Building clicked (pre-simulation): " + name);
            EnableOnlyFloor(0);
            PlacementManager.Instance.SetSelectedBuilding(this, _Floors[0]._Ground);
            HumanSimulationManager.Instance.SetSelectedBuilding(this);
        }
        else
        {
            // After simulation, store the clicked point on the building
            Ray ray = Camera.main.ScreenPointToRay(eventData.position);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
                {
                    DroneManager.Instance.SetTargetPoint(hit.point);
                    Debug.Log("Clicked on building at: " + hit.point);
                }
            }
        }
    }

    public void SetActiveCollider(Building building)
    {
        // Collider is only enabled if it's another building and simulation hasn't started
        GetComponent<BoxCollider>().enabled = !simulationStarted && (building != this);
    }

    public bool IsFloorOnTheBuilding(Ground ground)
    {
        for (int i = 0; i < _Floors.Count; i++)
        {
            if (_Floors[i]._Ground == ground)
                return true;
        }
        return false;
    }

    public Transform GetCamTransform()
    {
        return _CamTransform;
    }
}
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Building : MonoBehaviour, IPointerClickHandler
{
    public BuildingConfig buildingConf;
    //public List<Floor> _Floors = new List<Floor>();

    public List<Floor> _Floors;

    [SerializeField] private Transform _CamTransform;

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
        for (int i = 0; i < _Floors.Count; i++)
        {
            _Floors[i].floorObj.SetActive(true);
            _Floors[i].SetSelectable(false);

        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Building clicked: " + name);

        EnableOnlyFloor(0);
        PlacementManager.Instance.SetSelectedBuilding(this, _Floors[0]._Ground);
    }

    public void SetActiveCollider(Building building)
    {
        GetComponent<BoxCollider>().enabled = (building != this);
        
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



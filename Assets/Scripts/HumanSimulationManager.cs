using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanSimulationManager : MonoBehaviour
{
    public static HumanSimulationManager Instance;
    public static Action StartSimulation;
    private Building _selectedBuilding;
    private Floor _selectedFloor;
    [SerializeField] private LayerMask _UpViewLayer;
    [SerializeField] private GameObject _UpViewItems;
    private void OnEnable()
    {
        StartSimulation += StarHumantSimulation;
    }
    private void OnDisable()
    {
        StartSimulation -= StarHumantSimulation;
    }

    private void Start()
    {
        Instance = this;
    }

    private void StarHumantSimulation()
    {
        foreach(Floor floor in _selectedBuilding._Floors)
        {
            floor.SetWallsCollider(true);
        }
    }

    public void SetSelectedBuilding(Building building)
    {
        _selectedBuilding = building;
    }  
    
    public void SetSelectedFloor(Floor floor)
    {
        _selectedFloor = floor;
    }
    public Building GetSelectedBuilding()
    {
        return _selectedBuilding;
    }

    public void SetBuldingUpView()
    {
        int layerIndex = Mathf.RoundToInt(Mathf.Log(_UpViewLayer.value, 2));
        Debug.Log(layerIndex);
        _selectedFloor.gameObject.layer = layerIndex;
        _selectedFloor._Ground.gameObject.layer = layerIndex;
        Vector3 currentPos = _UpViewItems.transform.position;
        _UpViewItems.transform.position = new Vector3(currentPos.x, (currentPos.y + GetHeightUpViewItems()), currentPos.z);
    }

    public float GetHeightUpViewItems()
    {
        return (3 * (_selectedFloor.floorIndex - 1));
    }
}

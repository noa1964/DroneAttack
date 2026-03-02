using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanSimulationManager : MonoBehaviour
{
    public static HumanSimulationManager Instance;
    public static Action StartSimulation;
    private Building _selectedBuilding;

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
    public Building GetSelectedBuilding()
    {
        return _selectedBuilding;
    }
}

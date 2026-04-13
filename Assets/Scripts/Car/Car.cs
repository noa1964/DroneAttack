using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Car : MonoBehaviour, IPointerClickHandler
{
    // Store the last clicked point on the building after simulation starts

    [SerializeField] private GameObject _Windows;
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

    private void StartSimulation()
    {
        simulationStarted = true;

        // Disable the building collider so it won't block anything
        GetComponent<BoxCollider>().enabled = false;

    }

    public void OnPointerClick(PointerEventData eventData)
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

    public void SetActiveCollider(Building building)
    {
        // Collider is only enabled if it's another building and simulation hasn't started
        GetComponent<BoxCollider>().enabled = !simulationStarted && (building != this);
    }
}
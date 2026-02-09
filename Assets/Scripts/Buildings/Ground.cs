using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Ground : MonoBehaviour, IPointerClickHandler
{
    private Collider _collider;
    private Renderer _renderer;
    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _renderer = GetComponent<Renderer>();
    }

    public void SetSelectable(bool value)
    {
        _collider.enabled = value;

        if (_renderer != null)
        {
            _renderer.material.color = value ? Color.white : Color.green;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("click on ground");
        Ray ray = PlacementManager.Instance.GetCamera().ScreenPointToRay(eventData.position);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            PlacementManager.Instance.TryPlaceOnFloor(this, hit.point);
        }
    }
}

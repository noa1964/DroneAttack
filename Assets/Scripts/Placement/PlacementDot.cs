using UnityEngine;
using UnityEngine.EventSystems;

public enum EnemyType
{
    EnemyA,
    EnemyB,
    EnemyC
}

public class PlacementDot : MonoBehaviour, IPointerClickHandler, IDragHandler
{
    private EnemyType enemyType; // assigned at placement

    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            PlacementManager.Instance.RemoveDot(this);
            Destroy(gameObject);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Ray ray = _cam.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            transform.position = hit.point;
        }
    }

    public void SetEnemyType(EnemyType type)
    {
        enemyType = type;
    }
}

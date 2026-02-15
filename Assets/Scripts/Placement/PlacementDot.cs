using UnityEngine;
using UnityEngine.EventSystems;



public class PlacementDot : MonoBehaviour, IPointerClickHandler, IDragHandler
{
    private EnemyType _enemyType; // assigned at placement
    private WeaponType _weaponType; // assigned at placement

    private Camera _cam;

    private GameObject _ground;
    private void Awake()
    {
        
    }

    public void Initialize(Camera cam, EnemyType enemy, WeaponType weapon, GameObject ground)
    {
        _cam = cam;
        _enemyType = enemy;
        _weaponType = weapon;
        _ground = ground;
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
        if (_ground == null || _cam == null) return;

        Ray ray = _cam.ScreenPointToRay(eventData.position);

        // Get the ground collider
        Collider groundCollider = _ground.GetComponent<Collider>();
        if (groundCollider == null) return;

        // Raycast only against the ground collider
        if (groundCollider.Raycast(ray, out RaycastHit hit, 1000f))
        {
            Vector3 newPos = hit.point;

            // Keep original height
            newPos.y = transform.position.y;

            transform.position = newPos;
        }
    }


    public void SetEnemyType(EnemyType type)
    {
        _enemyType = type;
    }

    public EnemyType GetEnemyType()
    {
        return _enemyType; 
    }

    public WeaponType GetWeaponType()
    {
        return _weaponType;
    }

}

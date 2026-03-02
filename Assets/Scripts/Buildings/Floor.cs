using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Floor : MonoBehaviour
{
    public int floorIndex;

    public GameObject floorObj;
    public List<GameObject> windows;
    public List<GameObject> doors;
    public List<Transform> _PossibleEndPoints;
    public Ground _Ground;
    public Collider walls;

    private void Start()
    {
        _Ground.SetFloor(this);
    }

    public void SetSelectable(bool value)
    {
        _Ground.SetSelectable(value);
    
    }

    public void SetWallsCollider(bool isCollider)
    {
        Debug.Log(" building " + transform.parent.name);
        Debug.Log(" walls " + walls.transform.name);
        walls.enabled = isCollider;
    }
     

}
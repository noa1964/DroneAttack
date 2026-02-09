using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Floor : MonoBehaviour
{
    public int floorIndex;

    public GameObject floorObj;
    public List<GameObject> windows;
    public List<GameObject> doors;
    public Ground _Ground;



    public void SetSelectable(bool value)
    {
        _Ground.SetSelectable(value);
    
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Buildings/Building Config")]
public class BuildingConfig : ScriptableObject
{
    public BuildingType buildingType;
    public int floorCount;
}


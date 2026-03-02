using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiskController : MonoBehaviour
{
    [SerializeField] Renderer _Rend;

    private void OnEnable()
    {
        _Rend.material.SetFloat("_Animate", 1);
    }

    private void OnDisable()
    {
        _Rend.material.SetFloat("_Animate", 0);
    }
    
}

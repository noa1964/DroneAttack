using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanSimulationManager : MonoBehaviour
{

    public static Action StartSimulation;


    private void OnEnable()
    {
        StartSimulation += StartSimulation;
    }
    private void OnDisable()
    {
        StartSimulation -= StartSimulation;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void StarHumantSimulation()
    {

    }
}

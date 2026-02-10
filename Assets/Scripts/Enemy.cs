using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private EnemyType _enemyType;
    private Vector3 _targetPos;

    public void Initialize(EnemyType type, Vector3 pos)
    {
        _enemyType = type;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

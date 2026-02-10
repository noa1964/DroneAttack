using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private Enemy _EnemyPrefab;

    private Dictionary<Ground, List<GameObject>> _EnemyDotInfos = new Dictionary<Ground, List<GameObject>>();
    private void OnEnable()
    {
        HumanSimulationManager.StartSimulation += StartSimulation;
    }
    private void OnDisable()
    {
        HumanSimulationManager.StartSimulation -= StartSimulation;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void StartSimulation()
    {
        _EnemyDotInfos = PlacementManager.Instance.GetDictionaryDots();
        SpwanEnemies();
    }

    private void SpwanEnemies()
    {
        if (_EnemyPrefab == null)
        {
            Debug.LogWarning("Enemy prefab not assigned!");
            return;
        }

        foreach (var kvp in _EnemyDotInfos)
        {
            Ground ground = kvp.Key;
            List<GameObject> dotList = kvp.Value;

            // Get all possible end points on this ground
            List<Transform> possiblePoints = new List<Transform>(ground.GetPossibleEndPoints());

            if (dotList.Count > possiblePoints.Count)
            {
                Debug.LogWarning($"Not enough end points on ground {ground.name} for all enemies!");
            }

            // Shuffle the points so each enemy gets a random unique position
            Shuffle(possiblePoints);

            for (int i = 0; i < dotList.Count; i++)
            {
                GameObject dotObj = dotList[i];
                PlacementDot dot = dotObj.GetComponent<PlacementDot>();
                if (dot == null) continue;

                // Take the first available point
                Transform targetPoint = possiblePoints[i];

                // Instantiate enemy at the dot's world position (or at target point if you want)
                Enemy enemy = Instantiate(_EnemyPrefab, dotObj.transform.position, Quaternion.identity);

                // Initialize enemy with type and assigned position
                enemy.Initialize(dot.GetEnemyType(), targetPoint.position);
            }
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private List<EnemyInfo> _EnemiesPrefab;
    [SerializeField] private EnemyCamHandler _EnemyCamHandler;
    private Dictionary<Ground, List<GameObject>> _EnemyDotInfos = new Dictionary<Ground, List<GameObject>>();

    private List<Enemy> _enemies = new List<Enemy>();
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

        if (_EnemiesPrefab.Count <= 0)
        {
            Debug.LogWarning("Enemy prefab not assigned!");
            return;
        }

        int index = 1;

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

                EnemyType type = dot.GetEnemyType();
                // Instantiate enemy at the dot's world position (or at target point if you want)
                GameObject obj = Instantiate(GetEnemy(type), dotObj.transform.position, Quaternion.identity);

                Enemy enemy = obj.GetComponent<Enemy>();
                enemy.gameObject.name = "Enemy_" + index;
                // Initialize enemy with type and assigned position
                enemy.Initialize(type, dot.GetWeaponType(), targetPoint );

                _enemies.Add(enemy);
                dotObj.SetActive(false);
                index++;
            }
        }

        Debug.Log("done spwaining");
        _EnemyCamHandler.Initialize(_enemies);
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

    public GameObject GetEnemy(EnemyType type)
    {
        foreach (var enemy in _EnemiesPrefab)
        {
            if (enemy.type == type)
                return enemy.enemyObj;
        }

        Debug.LogWarning($"Enemy type {type} not found.");
        return null;
    }

}

[System.Serializable]
public class EnemyInfo
{
    public EnemyType type;
    public GameObject enemyObj;
}


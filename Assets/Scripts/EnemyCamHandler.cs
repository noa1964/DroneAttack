using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyCamHandler : MonoBehaviour
{
    [SerializeField] private GameObject _CamObj;

    private List<Enemy> _enemies = new List<Enemy>();

    [SerializeField] private TMP_Dropdown _EnemiesDropdown;

    [SerializeField] private CameraDragHandler _CameraDragHandler;

    [SerializeField] private GameObject _Panel;

    private void Awake()
    { 
        _EnemiesDropdown.onValueChanged.AddListener(OnEnemySelected);
        _Panel.SetActive(false);
    }

    public void Initialize(List<Enemy> enemies)
    {
        _enemies = enemies;
        _Panel.SetActive(true);
        SetupDropdown();
    }

    private void SetupDropdown()
    {
        _EnemiesDropdown.ClearOptions();

        List<string> options = new List<string>();

        foreach (Enemy enemy in _enemies)
        {
            options.Add(enemy.gameObject.name);
        }

        _EnemiesDropdown.AddOptions(options);

        if (_enemies.Count > 0)
        {
            _EnemiesDropdown.value = 0;
            OnEnemySelected(0);
        }
    }

    private void OnEnemySelected(int index)
    {
        if (index < 0 || index >= _enemies.Count)
            return;

        Enemy selectedEnemy = _enemies[index];

        AttachCameraToEnemy(selectedEnemy);
    }

    private void AttachCameraToEnemy(Enemy enemy)
    {
        _CamObj.transform.SetParent(enemy.GetCamTrans());
        _CamObj.transform.localPosition = new Vector3(0,0,0);
        _CamObj.transform.localRotation = Quaternion.identity;
        _CameraDragHandler.SetParent(enemy.gameObject.transform);
    }
}
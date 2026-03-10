using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyType
{
    EnemyA,
    EnemyB,
    EnemyC
}

public enum WeaponType
{
    WeaponA,
    WeaponB,
    WeaponC
}

public class Enemy : MonoBehaviour
{
    private EnemyType _enemyType;
    private WeaponType _weaponType;
    private Transform _targetTrans;
    private NavMeshAgent _agent;
    private bool _goingToFinalTarget = false;

    private bool _reachedFinalPoint = false;

    private Animator _animator;

    [SerializeField] private Transform _CamTrans;

    [Header("Weapon")]
    [SerializeField] private List<Weapon> Weapons;

    [Header("Random Walk Settings")]
    public float randomWalkRadius = 5f;
    public float minRandomTime = 2f;
    public float maxRandomTime = 5f;


    private bool _isDead = false;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    public void Initialize(EnemyType enemy, WeaponType weapon, Transform trans)
    {
        _enemyType = enemy;
        _targetTrans = trans;
        _weaponType = weapon;

        SetWeapon();
        StartCoroutine(RandomWalkThenGoToTarget());
    }

    private void Update()
    {
        if (_isDead) return;

        if (_goingToFinalTarget && !_reachedFinalPoint)
        {
            if (!_agent.pathPending &&
                _agent.remainingDistance <= _agent.stoppingDistance &&
                (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f))
            {
                OnReachedTarget();
            }
        }
    }

    private void SetWeapon()
    {
        Weapons.FirstOrDefault(w => w.type == _weaponType)?.weaponObj.SetActive(true);
    }

    private IEnumerator RandomWalkThenGoToTarget()
    {
        _animator.SetBool("Walking", true);
        float randomTime = Random.Range(minRandomTime, maxRandomTime);
        float timer = 0f;

        while (timer < randomTime)
        {
            if (!_agent.hasPath || _agent.remainingDistance < 0.5f)
            {
                Vector3 randomPoint = GetRandomPoint(transform.position, randomWalkRadius);
                _agent.SetDestination(randomPoint);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        _goingToFinalTarget = true;
        _agent.SetDestination(_targetTrans.position);
    }

    public Transform GetCamTrans()
    {
        return _CamTrans;
    }
    private Vector3 GetRandomPoint(Vector3 center, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += center;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return center;
    }

    private void OnReachedTarget()
    {
        _reachedFinalPoint = true;

        _agent.isStopped = true;
        _agent.updateRotation = false;

        StartCoroutine(RotateToTarget());

        if (_animator != null)
        {
            _animator.SetTrigger("Crouch");
        }
    }

    public void Die()
    {
        if (_isDead) return;

        _isDead = true;

        // Stop navigation
        if (_agent != null)
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            _agent.enabled = false;
        }

        // Stop all running coroutines (random walk etc)
        StopAllCoroutines();

        // Trigger death animation
        if (_animator != null)
        {
            _animator.SetTrigger("Dead");
        }

    }

    private IEnumerator RotateToTarget()
    {
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = _targetTrans.rotation;

        float t = 0f;
        float duration = 0.5f;

        while (t < duration)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t / duration);
            yield return null;
        }

        transform.rotation = targetRot;
    }

}

[System.Serializable]
public class Weapon
{
    public WeaponType type;
    public GameObject weaponObj;
}
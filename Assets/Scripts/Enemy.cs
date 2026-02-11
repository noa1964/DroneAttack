using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private EnemyType _enemyType;
    private Vector3 _targetPos;
    private NavMeshAgent _agent;
    private bool _goingToFinalTarget = false;

    private bool _reachedFinalPoint = false;

    private Animator _animator;

    [Header("Random Walk Settings")]
    public float randomWalkRadius = 5f;
    public float minRandomTime = 2f;
    public float maxRandomTime = 5f;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    public void Initialize(EnemyType type, Vector3 pos)
    {
        _enemyType = type;
        _targetPos = pos;

        StartCoroutine(RandomWalkThenGoToTarget());
    }

    private void Update()
    {
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
        _agent.SetDestination(_targetPos);
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

        if (_animator != null)
        {
            _animator.SetTrigger("Crouch");
        }
    }

}
﻿using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : MonoBehaviour
{
    public static List<EnemyUnit> enemies;
    public static Transform target;

    private Vector3 _velocity;
    private Vector3 _newVelocity;
    private Vector3 _newPosition;

    private List<EnemyUnit> _neighbors;
    private List<EnemyUnit> _collisionRisks;
    private EnemyUnit _closest;

    private void Awake()
    {
        if (enemies == null)
        {
            enemies = new List<EnemyUnit>();
        }
        enemies.Add(this);
        if (target == null)
            target = EnemyFleetManager.enemyFleetManager.target;

        Vector3 randomPos = Random.insideUnitSphere * EnemyFleetManager.enemyFleetManager.spawnRadius;
        randomPos.y = 1;
        transform.position = randomPos;
        _velocity = Random.onUnitSphere;
        _velocity *= EnemyFleetManager.enemyFleetManager.spawnVelocity;
        _velocity.y = 1;

        _neighbors = new List<EnemyUnit>();
        _collisionRisks = new List<EnemyUnit>();

        transform.SetParent(EnemyFleetManager.enemyFleetManager.transform);

    }

    private void Update()
    {
        List<EnemyUnit> tempNeighbors = GetNeighbors(this);
        _newVelocity = _velocity;
        _newPosition = transform.position;

        Vector3 neighborVel = GetAverageVelocity(_neighbors);
        _newVelocity += neighborVel * EnemyFleetManager.enemyFleetManager.velocityMatchingAmt;

        Vector3 neighborCenterOffset = GetAveragePosition(_neighbors) - transform.position;
        _newPosition += neighborCenterOffset * EnemyFleetManager.enemyFleetManager.flockCenteringAmt;

        Vector3 dist;
        if (_collisionRisks.Count > 0)
        {
            Vector3 collisionAveragePos = GetAveragePosition(_collisionRisks);
            dist = collisionAveragePos - transform.position;
            _newVelocity += dist * EnemyFleetManager.enemyFleetManager.cllisionAvoidanceAmt;
        }

        dist = target.position - transform.position;
        if (dist.magnitude > EnemyFleetManager.enemyFleetManager.targetAvoidanceDistance)
        {
            _newVelocity += dist * EnemyFleetManager.enemyFleetManager.targetAttractionAmount;
        }
        else
        {
            _newVelocity = Vector3.zero;
        }
    }

    private void LateUpdate()
    {
        _velocity = (1 - EnemyFleetManager.enemyFleetManager.velocityLerpAmt) * _velocity + EnemyFleetManager.enemyFleetManager.velocityLerpAmt * _newVelocity;
        if (_velocity.magnitude > EnemyFleetManager.enemyFleetManager.maxVelocity)
        {
            _velocity = _velocity.normalized * EnemyFleetManager.enemyFleetManager.maxVelocity;
        }
        if (_velocity.magnitude < EnemyFleetManager.enemyFleetManager.minVelocity)
        {
            _velocity = _velocity.normalized * EnemyFleetManager.enemyFleetManager.minVelocity;
        }
        _newPosition = transform.position + _velocity * Time.deltaTime;
        _newPosition.y = 1;

        transform.LookAt(_newPosition);
        transform.position = _newPosition;
    }

    private List<EnemyUnit> GetNeighbors(EnemyUnit enemyNeighbor)
    {
        float closestDistance = float.MaxValue;
        Vector3 delta;
        float dist;
        _neighbors.Clear();
        _collisionRisks.Clear();

        foreach (EnemyUnit enemy in enemies)
        {
            if (enemy == enemyNeighbor)
                continue;
            delta = enemy.transform.position - transform.position;
            dist = delta.magnitude;
            if (dist < closestDistance)
            {
                closestDistance = dist;
                _closest = enemy;
            }
            if (dist < EnemyFleetManager.enemyFleetManager.nearDistance)
            {
                _neighbors.Add(enemy);
            }
            if (dist < EnemyFleetManager.enemyFleetManager.collisionDistance)
            {
                _collisionRisks.Add(enemy);
            }
        }
        if (_neighbors.Count == 0)
        {
            _neighbors.Add(_closest);
        }
        return _neighbors;
    }

    public Vector3 GetAveragePosition(List<EnemyUnit> enemyAvgPos)
    {
        Vector3 sum = Vector3.zero;
        if (enemyAvgPos.Count == 0)
            return sum;
        foreach (EnemyUnit enemy in enemyAvgPos)
        {
            sum += enemy.transform.position;
        }
        Vector3 center = sum / enemyAvgPos.Count;
        return center;
    }

    public Vector3 GetAverageVelocity(List<EnemyUnit> enemyAvgVelocity)
    {
        Vector3 sum = Vector3.zero;
        if (enemyAvgVelocity.Count == 0)
            return sum;
        foreach (EnemyUnit enemy in enemyAvgVelocity)
        {
            sum += enemy._velocity;
        }
        Vector3 center = sum / enemyAvgVelocity.Count;
        return center;
    }
}

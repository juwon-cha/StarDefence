using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static event Action OnEnemyDestroyed;

    private float health;
    private float speed;
    
    private Transform endPoint;
    private List<Tile> path;
    private int waypointIndex = 0;

    /// <summary>
    /// 적을 초기화하고 스탯과 목표를 설정한 후 경로 찾음
    /// </summary>
    public void Initialize(EnemyDataSO data, Transform target)
    {
        this.health = data.health;
        this.speed = data.speed;
        this.endPoint = target;

        // Pathfinding 서비스에 경로 요청
        path = Pathfinding.Instance.FindPath(transform.position, endPoint.position);
        if (path == null || path.Count == 0)
        {
            Debug.LogError($"경로를 찾을 수 없습니다! {name}", this);
            // 경로가 없으면 바로 파괴하거나 다른 로직 수행?
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // 경로가 없으면 이동하지 않음
        if (path == null || waypointIndex >= path.Count)
        {
            return;
        }

        // 현재 목표 경유지(waypoint)를 향해 이동
        Vector3 currentWaypoint = path[waypointIndex].WorldPosition;
        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);

        // 현재 경유지에 도달하면 다음 경유지로 목표 변경
        if (Vector3.Distance(transform.position, currentWaypoint) < 0.01f)
        {
            waypointIndex++;
        }

        // 모든 경로를 통과하여 최종 목적지에 거의 도달했다면
        if (waypointIndex >= path.Count)
        {
             // 최종 목적지인 EndPoint로 마지막 이동
            transform.position = Vector3.MoveTowards(transform.position, endPoint.position, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, endPoint.position) < 0.01f)
            {
                // TODO: 지휘관 체력 감소 로직 추가
                OnEnemyDestroyed?.Invoke();
                Destroy(gameObject);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            // TODO: 적 사망 시 보상(골드 등) 지급 로직 추가
            OnEnemyDestroyed?.Invoke();
            Destroy(gameObject);
        }
    }
}

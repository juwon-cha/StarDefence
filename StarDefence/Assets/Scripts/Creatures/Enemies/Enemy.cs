using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Creature
{
    public enum EnemyState { Moving, Attacking }

    public static event Action<Enemy> OnEnemyDestroyed;

    private EnemyDataSO EnemyData => creatureData as EnemyDataSO;
    private EnemyState currentState;
    private Transform target;
    private Commander targetCommander;
    private List<Tile> path;
    private int waypointIndex = 0;
    private float attackTimer;

    private readonly string _healthBarUIPrefabPath = Constants.UI_ROOT_PATH + Constants.UI_POPUP_SUB_PATH + Constants.HEALTH_BAR_UI_PREFAB_NAME;
    private HealthBarUI _healthBarUIInstance;

    public bool IsBountyTarget { get; set; }

    private void OnDisable()
    {
        path = null;
        waypointIndex = 0;
        if (creatureData != null)
        {
            currentHealth = creatureData.maxHealth;
        }

        if (_healthBarUIInstance != null)
        {
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.Release(_healthBarUIInstance.gameObject);
            }
            _healthBarUIInstance = null;
        }

        targetCommander = null; // 풀에 반환될 때 캐시된 컴포넌트 초기화
        IsBountyTarget = false; // 풀에 반환될 때 플래그 초기화
    }

    public void Initialize(EnemyDataSO data, Transform targetTransform)
    {
        creatureData = data;
        currentHealth = EnemyData.maxHealth;
        target = targetTransform;
        currentState = EnemyState.Moving;

        // 타겟의 Commander 컴포넌트 캐싱
        if (target != null)
        {
            targetCommander = target.GetComponent<Commander>();
        }

        path = Pathfinding.Instance.FindPath(transform.position, target.position);
        if (path == null || path.Count == 0)
        {
            Debug.LogError($"Path not found for {name}!", this);
            PoolManager.Instance.Release(gameObject);
        }
    }

    void Update()
    {
        if (GameManager.Instance.Status == GameStatus.GameOver)
        {
            return;
        }

        switch (currentState)
        {
            case EnemyState.Moving:
                Move();
                break;
            case EnemyState.Attacking:
                PerformAttack();
                break;
        }
    }

    private void Move()
    {
        if (target == null)
        {
            // 지휘관이 파괴되었으므로 이동 중지
            return;
        }

        // 어떤 이동을 하기 전에 최종 목표의 공격 범위 내에 있는지 확인
        if (Vector3.Distance(transform.position, target.position) <= EnemyData.attackRange)
        {
            currentState = EnemyState.Attacking;
            return; // 모든 이동 중지
        }

        // 경로 따라가기 로직
        // 경로가 소진되었거나 존재하지 않으면 목표를 향해 직접 이동
        // 이는 지휘관이 움직일 수 있는 경우도 처리
        if (path == null || waypointIndex >= path.Count)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, EnemyData.speed * Time.deltaTime);
            return;
        }
        
        // 계산된 경로를 따라 이동
        Vector3 currentWaypoint = path[waypointIndex].WorldPosition;
        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, EnemyData.speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, currentWaypoint) < 0.01f)
        {
            waypointIndex++;
        }
    }

    private void PerformAttack()
    {
        if (target == null)
        {
            // 지휘관이 파괴되었으므로 공격 중지
            Debug.Log($"[Enemy: {name}] Target is null. Switching back to Moving state.");
            currentState = EnemyState.Moving; 
            return;
        }

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            attackTimer = EnemyData.attackInterval;
            Attack();
        }
    }

    private void Attack()
    {
        // 캐시된 Commander 컴포넌트를 사용하여 공격
        if (targetCommander != null)
        {
            //Debug.Log($"[Enemy: {name}] Attacking Commander for {EnemyData.attackDamage} damage."); // 이 로그는 너무 자주 발생하므로 주석 처리
            targetCommander.TakeDamage(EnemyData.attackDamage);
        }
        else
        {
            // 캐시된 컴포넌트가 없다면, 타겟이 파괴되었거나 Commander가 아닐 수 있음
            Debug.LogWarning($"[Enemy: {name}] Attack target does not have a valid Commander component. Switching back to Moving state.");
            
            // 타겟이 유효하지 않으므로 다시 타겟을 찾도록 상태를 변경
            currentState = EnemyState.Moving;
        }
    }

    public override void TakeDamage(float damage)
    {
        if (_healthBarUIInstance == null)
        {
            GameObject healthBarGO = PoolManager.Instance.Get(_healthBarUIPrefabPath);
            if (healthBarGO == null) return;

            healthBarGO.transform.SetParent(UIManager.Instance.WorldSpaceCanvas.transform);
            healthBarGO.transform.localScale = Vector3.one;
            
            _healthBarUIInstance = healthBarGO.GetComponent<HealthBarUI>();
            if (_healthBarUIInstance != null)
            {
                _healthBarUIInstance.Initialize(this);
            }
            else
            {
                Debug.LogError($"[Enemy] HealthBarUI component not found on prefab: {_healthBarUIPrefabPath}");
                PoolManager.Instance.Release(healthBarGO);
                return;
            }
        }

        base.TakeDamage(damage);
    }

    protected override void Die()
    {
        base.Die(); // 보상 지급 등 공통 로직 실행
        OnEnemyDestroyed?.Invoke(this);
        PoolManager.Instance.Release(gameObject);
    }
}

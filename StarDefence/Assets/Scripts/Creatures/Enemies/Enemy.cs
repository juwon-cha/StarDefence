using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static event Action<Enemy> OnEnemyDestroyed; // 어떤 적이 파괴되었는지 알 수 있도록 Enemy 인스턴스 전달

    // 체력 관련 이벤트
    /// <summary>현재 체력과 최대 체력 전달</summary>
    public event Action<float, float> OnHealthChanged;

    private float _maxHealth;    // 최대 체력
    private float _currentHealth; // 현재 체력
    private float speed;
    
    private Transform endPoint;
    private List<Tile> path;
    private int waypointIndex = 0;

    // 체력바 UI 관련
    private readonly string _healthBarUIPrefabPath = Constants.UI_ROOT_PATH + Constants.UI_POPUP_SUB_PATH + Constants.HEALTH_BAR_UI_PREFAB_NAME; // Constants 사용
    private HealthBarUI _healthBarUIInstance; // 현재 몬스터에게 표시되는 체력바 UI 인스턴스

    public float MaxHealth => _maxHealth;

    private void OnDisable()
    {
        // 오브젝트가 풀에 반환될 때 상태 초기화
        path = null;
        waypointIndex = 0;
        _currentHealth = _maxHealth; // 체력을 최대로 리셋

        // 체력바 UI가 있으면 풀에 반환하고 참조 해제
        if (_healthBarUIInstance != null)
        {
            // 어플리케이션 종료 시 PoolManager가 먼저 파괴될 수 있으므로 null 체크
            if (PoolManager.Instance != null)
            {
                // HealthBarUI가 구독 해제 책임을 가짐
                PoolManager.Instance.Release(_healthBarUIInstance.gameObject);
            }
            _healthBarUIInstance = null;
        }
    }

    /// <summary>
    /// 적을 초기화하고 스탯과 목표를 설정한 후 경로 찾음
    /// </summary>
    public void Initialize(EnemyDataSO data, Transform target)
    {
        _maxHealth = data.health;
        _currentHealth = data.health; // 초기 체력 설정
        speed = data.speed;
        endPoint = target;
        
        // Pathfinding 서비스에 경로 요청
        path = Pathfinding.Instance.FindPath(transform.position, endPoint.position);
        if (path == null || path.Count == 0)
        {
            Debug.LogError($"경로를 찾을 수 없습니다! {name}", this);
            PoolManager.Instance.Release(gameObject);
        }
    }

    void Update()
    {
        if (path == null || waypointIndex >= path.Count)
        {
            return;
        }

        Vector3 currentWaypoint = path[waypointIndex].WorldPosition;
        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, currentWaypoint) < 0.01f)
        {
            waypointIndex++;
        }

        if (waypointIndex >= path.Count)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPoint.position, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, endPoint.position) < 0.01f)
            {
                // TODO: 지휘관 체력 감소 로직 추가
                OnEnemyDestroyed?.Invoke(this); // 파괴된 적 인스턴스 전달
                PoolManager.Instance.Release(gameObject);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        // 체력바 UI가 아직 표시되지 않았다면 첫 피격 시 체력바를 표시
        if (_healthBarUIInstance == null)
        {
            GameObject healthBarGO = PoolManager.Instance.Get(_healthBarUIPrefabPath);
            if (healthBarGO == null) return;

            healthBarGO.transform.SetParent(UIManager.Instance.WorldSpaceCanvas.transform);
            healthBarGO.transform.localScale = Vector3.one; // 월드 스케일 캔버스에 맞춰 기본 스케일 설정
            
            _healthBarUIInstance = healthBarGO.GetComponent<HealthBarUI>();
            if (_healthBarUIInstance != null)
            {
                _healthBarUIInstance.Initialize(this); // 자신(Enemy)을 체력바 UI에 전달(HealthBarUI가 이벤트 구독)
            }
            else
            {
                Debug.LogError($"[Enemy] HealthBarUI component not found on prefab: {_healthBarUIPrefabPath}");
                PoolManager.Instance.Release(healthBarGO);
                return;
            }
        }

        _currentHealth -= damage;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth); // 체력 변화 이벤트 발생

        if (_currentHealth <= 0)
        {
            // TODO: 적 사망 시 보상(골드 등) 지급 로직 추가
            OnEnemyDestroyed?.Invoke(this); // 파괴된 적 인스턴스 전달
            PoolManager.Instance.Release(gameObject);
        }
    }
}

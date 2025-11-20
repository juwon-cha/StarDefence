using System.Collections;
using UnityEngine;

public abstract class Hero : Creature
{
    public HeroDataSO HeroData => creatureData as HeroDataSO;
    public Tile placedTile { get; private set; }

    protected Enemy currentTarget;
    private float attackTimer;
    private LayerMask enemyLayerMask;
    private Coroutine scanCoroutine;
    
    private const float SCAN_INTERVAL = 0.2f; // 적 탐색 주기

    #region 유니티 생명주기
    protected override void Start()
    {
        base.Start();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStatusChanged -= OnGameStatusChanged;
        }
    }

    protected virtual void Update()
    {
        // Wave 상태가 아니면 공격 로직을 실행하지 않음
        if (GameManager.Instance == null || GameManager.Instance.Status != GameStatus.Wave)
        {
            return;
        }
        
        // 유효한 타겟인지 확인(죽었거나 사거리를 벗어났는지)
        if (currentTarget != null && 
            (!currentTarget.gameObject.activeSelf || 
             Vector3.Distance(transform.position, currentTarget.transform.position) > HeroData.attackRange))
        {
            currentTarget = null;
        }
        
        // 공격 타이머 감소
        attackTimer -= Time.deltaTime;

        // 타겟이 있고 공격할 수 있다면 공격
        if (attackTimer <= 0f && currentTarget != null)
        {
            Attack();
            attackTimer = HeroData.attackInterval; // 타이머 리셋
        }
    }
    #endregion

    public virtual void Init(HeroDataSO data, Tile tile)
    {
        creatureData = data;
        placedTile = tile;
        transform.position = tile.transform.position;

        // "Enemy" 레이어만 탐지하도록 레이어 마스크 설정
        enemyLayerMask = LayerMask.GetMask("Enemy");
        attackTimer = 0;
        
        // 이벤트를 다시 구독하기 전에 이전 구독을 해제하여 중복 방지
        if(GameManager.Instance != null)
        {
            GameManager.Instance.OnStatusChanged -= OnGameStatusChanged;
        }
        GameManager.Instance.OnStatusChanged += OnGameStatusChanged;

        // 현재 게임 상태에 따라 초기 동작 결정
        OnGameStatusChanged(GameManager.Instance.Status);
    }
    
    /// <summary>
    /// 영웅이 풀에 반환되거나 제거될 때 호출될 정리 메서드
    /// </summary>
    public void Cleanup()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStatusChanged -= OnGameStatusChanged;
        }
        
        if (scanCoroutine != null)
        {
            StopCoroutine(scanCoroutine);
            scanCoroutine = null;
        }
        
        currentTarget = null;
    }
    
    /// <summary>
    /// 실제 공격 로직. 자식 클래스에서 반드시 구현
    /// </summary>
    protected abstract void Attack();
    
    /// <summary>
    /// 게임 상태 변경 시 호출될 이벤트 핸들러
    /// </summary>
    private void OnGameStatusChanged(GameStatus newStatus)
    {
        if (newStatus == GameStatus.Wave)
        {
            // Wave가 시작되면 적 탐색 코루틴 시작
            if (scanCoroutine == null)
            {
                scanCoroutine = StartCoroutine(ScanForEnemiesCoroutine());
            }
        }
        else
        {
            // Wave가 아니면 적 탐색 코루틴 중지 및 타겟 초기화
            if (scanCoroutine != null)
            {
                StopCoroutine(scanCoroutine);
                scanCoroutine = null;
            }
            currentTarget = null;
        }
    }

    /// <summary>
    /// 주기적으로 주변의 적을 탐색하여 가장 가까운 적을 타겟으로 설정
    /// </summary>
    private IEnumerator ScanForEnemiesCoroutine()
    {
        while (true)
        {
            // 현재 타겟이 없으면 새로운 타겟 탐색
            if (currentTarget == null)
            {
                FindClosestEnemy();
            }
            yield return new WaitForSeconds(SCAN_INTERVAL);
        }
    }

    private void FindClosestEnemy()
    {
        // 지정된 공격 사거리 내의 모든 Enemy 레이어 콜라이더를 가져옴
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, HeroData.attackRange, enemyLayerMask);

        float closestDistanceSqr = float.MaxValue;
        Enemy closestEnemy = null;

        foreach (var col in colliders)
        {
            // 콜라이더에서 Enemy 컴포넌트 가져오기
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy == null) continue;

            // 영웅과 적 사이의 거리 제곱 계산(Vector3.Distance보다 빠름)
            float distanceSqr = (transform.position - enemy.transform.position).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestEnemy = enemy;
            }
        }
        currentTarget = closestEnemy;
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (HeroData == null)
        {
            return;
        }
        
        // 공격 사거리를 시각적으로 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, HeroData.attackRange);
    }
    #endif
}

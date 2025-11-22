using System.Collections;
using System.Collections.Generic;
// using System.Linq; // LINQ 사용 제거
using UnityEngine;

public abstract class Hero : Creature
{
    public HeroDataSO HeroData => creatureData as HeroDataSO;
    public Tile placedTile { get; private set; }

    // 버프 시스템
    public float CurrentAttackInterval { get; private set; }
    private readonly List<BuffDataSO> activeBuffs = new List<BuffDataSO>();

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
            attackTimer = CurrentAttackInterval; // 버프가 적용된 공격 속도로 타이머 리셋
        }
    }
    #endregion

    public virtual void Init(HeroDataSO data, Tile tile)
    {
        creatureData = data;
        placedTile = tile;
        transform.position = tile.transform.position;

        enemyLayerMask = LayerMask.GetMask("Enemy");
        attackTimer = 0;
        
        RecalculateStats(); // 능력치 초기 계산
        
        if(GameManager.Instance != null)
        {
            GameManager.Instance.OnStatusChanged -= OnGameStatusChanged;
        }
        GameManager.Instance.OnStatusChanged += OnGameStatusChanged;
        
        OnGameStatusChanged(GameManager.Instance.Status);
    }
    
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
        
        activeBuffs.Clear();
        RecalculateStats();
        currentTarget = null;
    }
    
    protected abstract void Attack();
    
    #region 버프 시스템
    
    /// <summary>
    /// 영웅 버프 적용
    /// </summary>
    public void ApplyBuff(BuffDataSO buff)
    {
        if (buff == null || activeBuffs.Contains(buff)) return;
        
        activeBuffs.Add(buff);
        RecalculateStats();
    }

    /// <summary>
    /// 영웅 버프 제거
    /// </summary>
    public void RemoveBuff(BuffDataSO buff)
    {
        if (buff == null || !activeBuffs.Contains(buff)) return;

        activeBuffs.Remove(buff);
        RecalculateStats();
    }

    /// <summary>
    /// 현재 적용된 버프 목록을 기반으로 최종 능력치 다시 계산
    /// </summary>
    private void RecalculateStats()
    {
        // 공격 속도 계산
        float attackSpeedBonus = 0f;
        foreach (BuffDataSO buff in activeBuffs)
        {
            if (buff.buffType == BuffType.AttackSpeed)
            {
                attackSpeedBonus += buff.value;
            }
        }
        
        CurrentAttackInterval = HeroData.attackInterval * (1 - attackSpeedBonus);
        
        // TODO: 다른 버프들(공격력, 사거리 등) 계산 로직 추가
    }
    
    #endregion
    
    private void OnGameStatusChanged(GameStatus newStatus)
    {
        // 적 탐색 코루틴이 항상 실행되도록 보장
        if (scanCoroutine == null)
        {
            scanCoroutine = StartCoroutine(ScanForEnemiesCoroutine());
        }
        
        // 웨이브 상태가 아닐 때 현재 타겟이 일반 몬스터라면 타겟 초기화
        // 현상금 몬스터는 계속 공격해야 하므로 타겟 유지
        if (newStatus != GameStatus.Wave)
        {
            if (currentTarget != null && !currentTarget.IsBountyTarget)
            {
                currentTarget = null;
            }
        }
    }

    private IEnumerator ScanForEnemiesCoroutine()
    {
        while (true)
        {
            if (currentTarget == null)
            {
                FindClosestEnemy();
            }
            yield return new WaitForSeconds(SCAN_INTERVAL);
        }
    }

    private void FindClosestEnemy()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, HeroData.attackRange, enemyLayerMask);

        float closestDistanceSqr = float.MaxValue;
        Enemy closestEnemy = null;
        bool isWaveActive = GameManager.Instance.Status == GameStatus.Wave;

        foreach (var col in colliders)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy == null || !enemy.gameObject.activeSelf)
            {
                continue;
            }

            // 웨이브가 진행 중이 아닐 때 현상금 몬스터가 아니면 건너뜀
            if (!isWaveActive && !enemy.IsBountyTarget)
            {
                continue;
            }
            
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
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, HeroData.attackRange);
    }
    #endif
}

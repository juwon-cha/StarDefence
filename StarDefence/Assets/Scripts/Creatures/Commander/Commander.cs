using System.Collections;
using UnityEngine;

public abstract class Commander : Creature
{
    public CommanderDataSO CommanderData => creatureData as CommanderDataSO;

    protected Enemy currentTarget;
    private float attackTimer;
    private LayerMask enemyLayerMask;
    private Coroutine scanCoroutine;
    private Collider2D collider;

    private const float SCAN_INTERVAL = 0.2f;

    private readonly string healthBarUIPrefabPath = Constants.UI_ROOT_PATH + Constants.UI_POPUP_SUB_PATH + Constants.HEALTH_BAR_UI_PREFAB_NAME;
    private HealthBarUI healthBarUIInstance;

    protected override void Start()
    {
        base.Start();
        enemyLayerMask = LayerMask.GetMask("Enemy");
        attackTimer = 0;
        GameManager.Instance.SetCommander(this);
        scanCoroutine = StartCoroutine(ScanForEnemiesCoroutine());
        collider = GetComponent<Collider2D>();

        // 체력바 생성 및 초기화
        GameObject healthBarGO = PoolManager.Instance.Get(healthBarUIPrefabPath);
        if (healthBarGO == null) return;

        healthBarGO.transform.SetParent(UIManager.Instance.WorldSpaceCanvas.transform);
        healthBarGO.transform.localScale = Vector3.one;
        
        healthBarUIInstance = healthBarGO.GetComponent<HealthBarUI>();
        if (healthBarUIInstance != null)
        {
            healthBarUIInstance.Initialize(this);
        }
        else
        {
            Debug.LogError($"[Commander] HealthBarUI component not found on prefab: {healthBarUIPrefabPath}");
            PoolManager.Instance.Release(healthBarGO);
        }
    }

    protected virtual void Update()
    {
        if (currentTarget != null &&
            (!currentTarget.gameObject.activeSelf ||
             Vector3.Distance(transform.position, currentTarget.transform.position) > CommanderData.attackRange))
        {
            currentTarget = null;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f && currentTarget != null)
        {
            Attack();
            attackTimer = CommanderData.attackInterval;
        }
    }

    protected abstract void Attack();

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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, CommanderData.attackRange, enemyLayerMask);

        float closestDistanceSqr = float.MaxValue;
        Enemy closestEnemy = null;

        foreach (var col in colliders)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy == null)
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
    
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        Debug.Log($"[Commander] Took {damage} damage. Current health: {currentHealth}/{CreatureData.maxHealth}");
    }

    protected override void Die()
    {
        Debug.LogWarning("[Commander] Health reached zero. Calling Die().");
        GameManager.Instance.GameOver();
        
        // 게임 오브젝트 파괴를 막고 기능만 비활성화
        if (scanCoroutine != null)
        {
            StopCoroutine(scanCoroutine);
            scanCoroutine = null;
        }
        this.enabled = false; // Commander 스크립트 비활성화
        
        // 캐시된 콜라이더 비활성화
        if (collider != null)
        {
            collider.enabled = false;
        }

        // TODO: 패배 사망 애니메이션 재생하거나 스프라이트 변경?
        // 현재는 시각적으로만 유지됨
    }

    private void OnDrawGizmos()
    {
        if (CommanderData != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, CommanderData.attackRange);
        }
    }
}

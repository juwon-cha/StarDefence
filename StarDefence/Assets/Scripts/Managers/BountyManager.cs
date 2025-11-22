using UnityEngine;
using System.Collections.Generic;

public class BountyManager : Singleton<BountyManager>
{
    [SerializeField] private List<BountyDataSO> bountyDatas; // 현상금 몬스터 데이터 목록
    public const float BOUNTY_COOLDOWN = 30f; // 현상금 시스템 전체 쿨타임

    public event System.Action OnCooldownStarted;
    public event System.Action OnCooldownFinished;
    
    private float currentCooldown = 0f;
    public float CurrentCooldown => currentCooldown;
    public bool IsOnCooldown => currentCooldown > 0;

    void Update()
    {
        if (!IsOnCooldown) return;
        
        currentCooldown -= Time.deltaTime;
        if (currentCooldown <= 0)
        {
            currentCooldown = 0;
            OnCooldownFinished?.Invoke();
        }
    }

    /// <summary>
    /// 현상금 몬스터 스폰을 시도하는 메소드
    /// </summary>
    public bool TrySpawnBountyMonster(BountyDataSO data)
    {
        if (IsOnCooldown)
        {
            Debug.Log($"현상금을 사용하려면 {Mathf.CeilToInt(currentCooldown)}초를 더 기다려야 합니다.");
            return false;
        }

        Transform spawnPoint = GridManager.Instance.SpawnPoint;
        if (spawnPoint == null)
        {
            Debug.LogError("[BountyManager] 적 스폰 위치를 GridManager에서 찾을 수 없습니다!");
            return false;
        }

        if (data.enemyData == null)
        {
            Debug.LogError("[BountyManager] BountyDataSO에 EnemyDataSO가 연결되지 않았습니다.");
            return false;
        }

        // PoolManager를 통해 몬스터 스폰 및 초기화
        GameObject monsterObj = PoolManager.Instance.Get(data.enemyData.FullEnemyPrefabPath);
        if (monsterObj == null)
        {
            Debug.LogError($"[BountyManager] PoolManager에서 몬스터를 가져오지 못했습니다. 경로: {data.enemyData.FullEnemyPrefabPath}");
            return false;
        }
        monsterObj.transform.position = spawnPoint.position;
        monsterObj.transform.rotation = spawnPoint.rotation;
        
        // 몬스터 이동 목표(지휘관) 설정 및 현상금 플래그 설정
        Enemy enemy = monsterObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Initialize(data.enemyData, GameManager.Instance.Commander.transform);
            enemy.IsBountyTarget = true; // 현상금 몬스터 플래그 설정
        }
        else
        {
            Debug.LogError($"[BountyManager] 스폰된 몬스터 '{monsterObj.name}'에서 Enemy 컴포넌트를 찾을 수 없습니다!");
            // 실패 시 오브젝트를 풀에 반환해야 누수가 발생하지 않음
            PoolManager.Instance.Release(monsterObj);
            return false;
        }

        // 현상금 보상 설정(풀링을 고려하여 GetComponent 후 없으면 AddComponent)
        BountyTarget bountyTarget = monsterObj.GetComponent<BountyTarget>();
        if (bountyTarget == null)
        {
            bountyTarget = monsterObj.AddComponent<BountyTarget>();
        }
        bountyTarget.SetReward(data.bountyGold, data.bountyMineral);

        // 쿨타임 다시 설정 및 UI 갱신 이벤트 호출
        currentCooldown = BOUNTY_COOLDOWN;
        OnCooldownStarted?.Invoke();
        
        Debug.Log($"{data.enemyData.enemyPrefabName} 현상금 몬스터가 스폰되었습니다!");
        return true;
    }

    public List<BountyDataSO> GetBountyList()
    {
        return bountyDatas;
    }
}

/// <summary>
/// 현상금 몬스터에게 부착될 컴포넌트
/// </summary>
public class BountyTarget : MonoBehaviour
{
    private int gold;
    private int mineral;

    public void SetReward(int gold, int mineral)
    {
        this.gold = gold;
        this.mineral = mineral;
    }

    public void GrantReward()
    {
        if (GameManager.Instance != null)
        {
            if (gold > 0)
            {
                GameManager.Instance.AddGold(gold);
            }
            if (mineral > 0)
            {
                GameManager.Instance.AddMinerals(mineral);
            }
            Debug.Log($"현상금 보상으로 골드 {gold}, 미네랄 {mineral}을 획득했습니다.");
        }
    }
}

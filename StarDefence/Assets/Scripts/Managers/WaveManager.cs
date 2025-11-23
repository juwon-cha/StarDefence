using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : Singleton<WaveManager>
{
    [Header("Wave Settings")]
    [Tooltip("이 스테이지에서 진행할 웨이브 목록")]
    public List<WaveDataSO> waves;
    [Tooltip("다음 웨이브 시작 버튼을 누를 수 있는 제한 시간")]
    public float timeToPressButton = 15f;

    public List<Enemy> ActiveEnemies { get; private set; } = new List<Enemy>();

    private int currentWaveIndex = -1;
    private int enemiesAlive = 0;
    private bool isSpawning = false;

    #region 유니티 생명주기
    void OnEnable()
    {
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    void OnDisable()
    {
        if (Instance != null) // 싱글톤 인스턴스가 살아있을 때만 실행
        {
            Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
        }
    }

    void Start()
    {
        StartCoroutine(WaitForHUDAndStart());
    }
    #endregion

    #region 웨이브 관리
    public void PlayerStartsNextWave()
    {
        StartNextWaveFlow();
    }

    private void StartNextWaveFlow()
    {
        // 게임 상태 Wave로 변경하여 영웅들이 공격 시작
        GameManager.Instance.ChangeStatus(GameStatus.Wave);
        
        currentWaveIndex++;
        if (currentWaveIndex < waves.Count)
        {
            if (UIManager.Instance.MainHUD != null)
                UIManager.Instance.MainHUD.UpdateWaveText(currentWaveIndex, waves.Count);
            
            StartCoroutine(SpawnWave(waves[currentWaveIndex]));
        }
        else
        {
            if (UIManager.Instance.MainHUD != null)
                UIManager.Instance.MainHUD.ShowAllWavesCleared();
            
            Debug.Log("All waves cleared!");
        }
    }

    private void PrepareForNextWave()
    {
        // 마지막 웨이브가 방금 클리어되었는지 확인
        if (currentWaveIndex >= waves.Count - 1 && enemiesAlive <= 0 && !isSpawning)
        {
            GameManager.Instance.GameVictory();
            return;
        }

        // 다음 웨이브 준비를 위해 상태를 Build로 설정
        GameManager.Instance.ChangeStatus(GameStatus.Build);

        // 다음 웨이브가 더 있다면 다음 웨이브 버튼 표시
        if (currentWaveIndex + 1 < waves.Count)
        {
            NextWaveButtonUI button = UIManager.Instance.ShowWorldSpacePopup<NextWaveButtonUI>(Constants.NEXT_WAVE_BUTTON_UI_NAME);

            if (button != null)
            {
                Vector3 spawnPointPos = GridManager.Instance.SpawnPoint.position;
                Vector3 worldPosAbove = spawnPointPos + new Vector3(0, 2f, 0); 

                button.transform.position = worldPosAbove;
                button.Initialize(timeToPressButton, PlayerStartsNextWave);
            }
        }
    }
    #endregion

    #region 코루틴
    private IEnumerator WaitForHUDAndStart()
    {
        yield return new WaitUntil(() => UIManager.Instance != null && UIManager.Instance.MainHUD != null);

        // 첫 웨이브 정보 표시 후 다음 웨이브 준비
        if (currentWaveIndex + 1 < waves.Count)
        {
            UIManager.Instance.MainHUD.UpdateWaveText(currentWaveIndex + 1, waves.Count);
        }
        PrepareForNextWave();
    }

    private IEnumerator SpawnWave(WaveDataSO currentWave)
    {
        isSpawning = true;
        enemiesAlive = 0;

        foreach (var enemyInfo in currentWave.enemiesToSpawn)
        {
            for (int i = 0; i < enemyInfo.count; i++)
            {
                enemiesAlive++;
                
                Transform spawnPoint = GridManager.Instance.SpawnPoint;
                Transform commanderTransform = GameManager.Instance.Commander.transform;

                if (spawnPoint == null || commanderTransform == null)
                {
                    Debug.LogError("Spawn point or Commander not set.");
                    enemiesAlive--; 
                    yield break;
                }

                GameObject enemyGO = PoolManager.Instance.Get(enemyInfo.enemyData.FullEnemyPrefabPath);
                if(enemyGO == null) 
                {
                    enemiesAlive--;
                    continue;
                }
                
                Enemy enemy = enemyGO.GetComponent<Enemy>();
                if(enemy == null)
                {
                    enemiesAlive--;
                    PoolManager.Instance.Release(enemyGO);
                    continue;
                }
                
                ActiveEnemies.Add(enemy);
                
                enemy.transform.position = spawnPoint.position;
                enemy.transform.rotation = Quaternion.identity;
                
                // 초기화 성공 여부 확인
                bool initialized = enemy.Initialize(enemyInfo.enemyData, commanderTransform);
                if (!initialized)
                {
                    enemiesAlive--; // 카운트 복구
                    ActiveEnemies.Remove(enemy); // 리스트에서 제거
                }

                yield return new WaitForSeconds(currentWave.spawnInterval);
            }
        }

        isSpawning = false;
    }
    #endregion

    /// <summary>
    /// 웨이브 시스템 외부에서 생성된 적을 활성화 리스트에 등록(현상금 몬스터)
    /// </summary>
    public void RegisterEnemy(Enemy enemy)
    {
        if (enemy != null && !ActiveEnemies.Contains(enemy))
        {
            ActiveEnemies.Add(enemy);
        }
    }

    private void HandleEnemyDestroyed(Enemy enemy)
    {
        // 현상금 몬스터가 아닐 경우에만 웨이브 생존자 카운트 감소
        if (!enemy.IsBountyTarget)
        {
            enemiesAlive--;
        }
        
        // 활성화된 적 리스트에서는 항상 제거
        if (ActiveEnemies.Contains(enemy))
        {
            ActiveEnemies.Remove(enemy);
        }

        // 웨이브가 모두 클리어되었고 적이 더 이상 스폰되지 않으며 아직 Build 상태가 아니라면 다음 웨이브 준비
        if (!isSpawning && enemiesAlive <= 0 && GameManager.Instance.Status != GameStatus.Build)
        {
            PrepareForNextWave();
        }
    }
}
